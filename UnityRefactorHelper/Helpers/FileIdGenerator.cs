using System.Security.Cryptography;
using System.Text;

namespace UnityRefactorHelper.Helpers
{
    public static class FileIdGenerator
    {
        public static int Compute(string namespaceName, string className)
        {
            if (string.IsNullOrEmpty(namespaceName))
                namespaceName = string.Empty;

            var toBeHashed = "s\0\0\0" + namespaceName + className;

            using (HashAlgorithm hash = new Md4())
            {
                var hashed = hash.ComputeHash(Encoding.UTF8.GetBytes(toBeHashed));

                var result = 0;

                for (var i = 3; i >= 0; --i)
                {
                    result <<= 8;
                    result |= hashed[i];
                }

                return result;
            }
        }
    }
}