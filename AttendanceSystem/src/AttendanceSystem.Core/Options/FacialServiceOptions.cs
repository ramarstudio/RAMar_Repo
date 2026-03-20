using System;

namespace AttendanceSystem.Core.Options
{
    public sealed class FacialServiceOptions
    {
        public string VerifyPath  { get; }
        public string EncodePath  { get; }

        public FacialServiceOptions(string verifyPath = "/api/verify", string encodePath = "/api/encode")
        {
            if (string.IsNullOrWhiteSpace(verifyPath))
                throw new ArgumentException("VerifyPath es obligatorio.", nameof(verifyPath));
            if (string.IsNullOrWhiteSpace(encodePath))
                throw new ArgumentException("EncodePath es obligatorio.", nameof(encodePath));

            VerifyPath = verifyPath;
            EncodePath = encodePath;
        }
    }
}
