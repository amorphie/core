using Konscious.Security.Cryptography;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace amorphie.core.security.Cryptography
{
    public class Argon2
    {
        public const int BufferSize= 128;
        public const int MemorySize = 1024;

        public static byte[] CreateSalt()
        {
            var buffer = new byte[BufferSize];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);

            return buffer;
        }
        public static byte[] HashPassword(string password, byte[] salt)
        {
            var argon2id = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2id.Salt = salt;
            argon2id.DegreeOfParallelism = 8;
            argon2id.Iterations = 4;
            argon2id.MemorySize = MemorySize * MemorySize;
            return argon2id.GetBytes(16);
        }

        public static bool VerifyHash(string password, byte[] salt, byte[] hash)
        {
            var newHash = HashPassword(password, salt);
            return hash.SequenceEqual(newHash);
        }

    }
}