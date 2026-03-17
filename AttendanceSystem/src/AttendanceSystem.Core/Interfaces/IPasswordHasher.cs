namespace AttendanceSystem.Core.Interfaces
{
    // Abstracción para hashing de contraseñas
    public interface IPasswordHasher
    {
        string HashPassword(string plainPassword);
        bool VerifyPassword(string plainPassword, string storedHash);
    }
}
