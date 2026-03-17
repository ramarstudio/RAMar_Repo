using System;
using System.Security.Cryptography;
using System.Text;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Security
{
    // Configuración de hashing (evita primitivas sueltas en el constructor)
    public sealed class HashingOptions
    {
        public string Pepper { get; }
        public int Iterations { get; }
        public int SaltSize { get; }
        public int HashSize { get; }

        public HashingOptions(string pepper, int iterations = 100_000, int saltSize = 16, int hashSize = 32)
        {
            if (string.IsNullOrEmpty(pepper))
                throw new ArgumentException("El pepper del servidor es obligatorio.", nameof(pepper));
            if (iterations < 10_000)
                throw new ArgumentOutOfRangeException(nameof(iterations), "Mínimo 10,000 iteraciones.");

            Pepper = pepper;
            Iterations = iterations;
            SaltSize = saltSize;
            HashSize = hashSize;
        }
    }

    // PBKDF2-SHA512 + Pepper. Formato: $AS$v1${iter}${salt}${hash}
    public sealed class PasswordHasher : IPasswordHasher
    {
        private const string FormatHeader = "$AS$v1$";
        private const char Separator = '$';

        private readonly byte[] _pepper;
        private readonly HashingOptions _options;

        public PasswordHasher(HashingOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _pepper = Convert.FromBase64String(options.Pepper);
        }

        public string HashPassword(string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(plainPassword));

            byte[] salt = CryptoHelper.GenerateRandomBytes(_options.SaltSize);
            byte[] hash = DeriveKey(plainPassword, salt, _options.Iterations, _options.HashSize);

            return string.Concat(
                FormatHeader,
                _options.Iterations, Separator,
                Convert.ToBase64String(salt), Separator,
                Convert.ToBase64String(hash));
        }

        public bool VerifyPassword(string plainPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            if (!storedHash.StartsWith(FormatHeader))
                return false;

            string[] parts = storedHash.Substring(FormatHeader.Length).Split(Separator);
            if (parts.Length != 3 || !int.TryParse(parts[0], out int iterations))
                return false;

            byte[] salt, originalHash;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                originalHash = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException) { return false; }

            byte[] computedHash = DeriveKey(plainPassword, salt, iterations, originalHash.Length);
            return CryptoHelper.ConstantTimeEquals(originalHash, computedHash);
        }

        // Combina password + pepper y deriva con PBKDF2-SHA512
        private byte[] DeriveKey(string password, byte[] salt, int iterations, int outputSize)
        {
            byte[] peppered = CryptoHelper.Combine(Encoding.UTF8.GetBytes(password), _pepper);

            using (var pbkdf2 = new Rfc2898DeriveBytes(peppered, salt, iterations, HashAlgorithmName.SHA512))
            {
                return pbkdf2.GetBytes(outputSize);
            }
        }
    }

    // Utilidades criptográficas compartidas (DRY, reutilizable)
    internal static class CryptoHelper
    {
        public static byte[] GenerateRandomBytes(int size)
        {
            byte[] buffer = new byte[size];
            using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(buffer); }
            return buffer;
        }

        // Comparación en tiempo constante (previene timing attacks)
        public static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        // Combina dos arreglos en uno solo (evita repetir Buffer.BlockCopy)
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
            return result;
        }

        // Ensambla múltiples bloques de bytes en un solo arreglo
        public static byte[] Assemble(params byte[][] blocks)
        {
            int totalLength = 0;
            for (int i = 0; i < blocks.Length; i++) totalLength += blocks[i].Length;

            byte[] result = new byte[totalLength];
            int offset = 0;
            for (int i = 0; i < blocks.Length; i++)
            {
                Buffer.BlockCopy(blocks[i], 0, result, offset, blocks[i].Length);
                offset += blocks[i].Length;
            }
            return result;
        }

        // Extrae un segmento de bytes de un arreglo
        public static byte[] Slice(byte[] source, int offset, int length)
        {
            byte[] result = new byte[length];
            Buffer.BlockCopy(source, offset, result, 0, length);
            return result;
        }
    }
}
