namespace AttendanceSystem.Core.Interfaces
{
    // Abstracción para cifrado simétrico de datos sensibles
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
