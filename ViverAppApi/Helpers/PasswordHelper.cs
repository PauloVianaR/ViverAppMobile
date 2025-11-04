using System;
using System.Security.Cryptography;
using System.Text;

namespace ViverAppApi.Helpers
{
    public static class PasswordHelper
    {
        private const string DefaultKeyString = "SXR6a178KS0pBkmhKrzFWQA";

        public static string EncryptPassword(int idUser, string password)
        {
            ArgumentNullException.ThrowIfNull(password);
            string salted = $"{idUser}:{password}";
            var cipher = EncryptToBytes(salted);
            return Convert.ToBase64String(cipher);
        }

        public static string DecryptPasswordFromBase64(string base64Cipher)
        {
            if (string.IsNullOrEmpty(base64Cipher)) throw new ArgumentNullException(nameof(base64Cipher));
            var bytes = Convert.FromBase64String(base64Cipher);
            return DecryptFromBytes(bytes);
        }

        public static string DecryptFromBytes(byte[] cipherBytes)
        {
            if (cipherBytes == null || cipherBytes.Length == 0) throw new ArgumentNullException(nameof(cipherBytes));
            var key = DeriveKeyBytesMySql(DefaultKeyString);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }

        private static byte[] EncryptToBytes(string plainText)
        {
            var key = DeriveKeyBytesMySql(DefaultKeyString);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        private static byte[] DeriveKeyBytesMySql(string keyString)
        {
            keyString ??= string.Empty;
            var keyBytes = Encoding.UTF8.GetBytes(keyString);
            var finalKey = new byte[16];
            for (int i = 0; i < keyBytes.Length; i++)
                finalKey[i % 16] ^= keyBytes[i];
            return finalKey;
        }

        public static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
