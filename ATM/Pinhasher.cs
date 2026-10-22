using System.Security.Cryptography;

namespace ATM
{
    public static class PinHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public static byte[] HashPin(string pin)
        {
            ValidatePin(pin);

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                pin,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            byte[] storedValue = new byte[SaltSize + HashSize];
            Buffer.BlockCopy(salt, 0, storedValue, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, storedValue, SaltSize, HashSize);

            return storedValue;
        }

        public static bool VerifyPin(string pin, byte[] storedValue)
        {
            if (!IsValidPin(pin) || storedValue.Length != SaltSize + HashSize)
            {
                return false;
            }

            byte[] salt = storedValue[..SaltSize];
            byte[] savedHash = storedValue[SaltSize..];
            byte[] enteredHash = Rfc2898DeriveBytes.Pbkdf2(
                pin,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return CryptographicOperations.FixedTimeEquals(savedHash, enteredHash);
        }

        private static void ValidatePin(string pin)
        {
            if (!IsValidPin(pin))
            {
                throw new ArgumentException("PIN must contain exactly four digits.", nameof(pin));
            }
        }

        private static bool IsValidPin(string pin)
        {
            return pin is not null
                && pin.Length == 4
                && pin.All(char.IsDigit);
        }
    }
}