using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Gen.Networking
{
    public sealed class LoginInfo
    {
        /// <summary>
        /// Currently used login info.
        /// </summary>
        public static LoginInfo Active { get; set; }

        // Local Parameters:
        public string Username { get; set; }
        public string Password { get; set; }
        public string UserID { get; set; }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Encryption Methods
        ///                Can be modified to use data, that server cannot normally get.
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        public string Encrypt(string data)
        {
            using Aes aes = Aes.Create();
            aes.Key = EncryptionKey;
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using MemoryStream ms = new();
            ms.Write(iv, 0, iv.Length);

            using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(data);
                cs.Write(plainBytes, 0, plainBytes.Length);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string data)
        {
            byte[] cipherBytes = Convert.FromBase64String(data);
            using Aes aes = Aes.Create();
            aes.Key = EncryptionKey;

            using MemoryStream ms = new(cipherBytes);
            byte[] iv = new byte[16];
            ms.Read(iv, 0, iv.Length);
            aes.IV = iv;

            using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader sr = new(cs, Encoding.UTF8);
            return sr.ReadToEnd();
        }


        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- <![CDATA[
        /// 
        ///                                     Private Data
        /// 
        /// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ]]>
        private byte[] EncryptionKey => lastKey ??= GenerateKey();
        private byte[] lastKey;
        private byte[] GenerateKey()
        {
            using SHA256 sha256 = SHA256.Create();
            string combined = Password + Username;
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        }
    }
}
