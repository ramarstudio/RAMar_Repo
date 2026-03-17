using System;
using System.Security.Cryptography;
using System.Text;
using AttendanceSystem.Core.Interfaces;

namespace AttendanceSystem.Security
{
    // Configuración de cifrado (evita primitivas sueltas)
    public sealed class EncryptionOptions
    {
        public byte[] EncryptionKey { get; }
        public byte[] HmacKey { get; }

        public EncryptionOptions(string encryptionKeyBase64, string hmacKeyBase64)
        {
            EncryptionKey = ParseKey(encryptionKeyBase64, nameof(encryptionKeyBase64));
            HmacKey = ParseKey(hmacKeyBase64, nameof(hmacKeyBase64));
        }

        private static byte[] ParseKey(string base64, string paramName)
        {
            if (string.IsNullOrEmpty(base64))
                throw new ArgumentException("La clave no puede estar vacía.", paramName);

            byte[] key = Convert.FromBase64String(base64);
            if (key.Length != 32)
                throw new ArgumentException($"La clave debe ser de 32 bytes. Se recibieron {key.Length}.", paramName);

            return key;
        }
    }

    // AES-256-CBC + HMAC-SHA256 (Encrypt-then-MAC). Formato: [IV][Cipher][HMAC] → Base64
    public sealed class EncryptionService : IEncryptionService
    {
        private const int IvSize = 16;
        private const int HmacSize = 32;

        private readonly EncryptionOptions _options;

        public EncryptionService(EncryptionOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string Encrypt(string plainText)
        {
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));

            using (var aes = CreateAes())
            {
                aes.GenerateIV();

                byte[] cipherBytes;
                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }

                // IV + CipherText → HMAC → [IV + CipherText + HMAC]
                byte[] ivAndCipher = CryptoHelper.Combine(aes.IV, cipherBytes);
                byte[] hmac = ComputeHmac(ivAndCipher);

                return Convert.ToBase64String(CryptoHelper.Assemble(ivAndCipher, hmac));
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentException("El texto cifrado no puede estar vacío.", nameof(cipherText));

            byte[] fullData = Convert.FromBase64String(cipherText);
            if (fullData.Length < IvSize + HmacSize + 1)
                throw new CryptographicException("Datos cifrados corruptos.");

            int dataLength = fullData.Length - HmacSize;
            byte[] ivAndCipher = CryptoHelper.Slice(fullData, 0, dataLength);
            byte[] storedHmac = CryptoHelper.Slice(fullData, dataLength, HmacSize);

            // Verificar integridad ANTES de descifrar
            if (!CryptoHelper.ConstantTimeEquals(storedHmac, ComputeHmac(ivAndCipher)))
                throw new CryptographicException("Integridad comprometida: HMAC no coincide.");

            byte[] iv = CryptoHelper.Slice(ivAndCipher, 0, IvSize);
            byte[] cipherBytes = CryptoHelper.Slice(ivAndCipher, IvSize, ivAndCipher.Length - IvSize);

            using (var aes = CreateAes())
            {
                aes.IV = iv;
                using (var decryptor = aes.CreateDecryptor())
                {
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }

        // Genera par de claves para configuración inicial
        public static (string EncryptionKey, string HmacKey) GenerateKeyPair()
        {
            return (Convert.ToBase64String(CryptoHelper.GenerateRandomBytes(32)),
                    Convert.ToBase64String(CryptoHelper.GenerateRandomBytes(32)));
        }

        // Fábrica de AES configurado (DRY)
        private Aes CreateAes()
        {
            var aes = Aes.Create();
            aes.Key = _options.EncryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }

        private byte[] ComputeHmac(byte[] data)
        {
            using (var hmac = new HMACSHA256(_options.HmacKey)) { return hmac.ComputeHash(data); }
        }
    }
}
