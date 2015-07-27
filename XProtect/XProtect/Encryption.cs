using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;
    class Encryption
    {
        private static readonly byte[] salt = new byte[] { 4, 8, 15, 16, 23, 42, 6, 120 };
        private static readonly int keySize = 256;

        public static byte[] Encrypt(byte[] data, string password)
        {
            byte[] result = null;
            byte[] passwordBytes = Encoding.Default.GetBytes(password);

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = keySize;
                    AES.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(passwordBytes, salt, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;
                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    result = ms.ToArray();
                }

            }
            return result;
        }
        public static byte[] Decrypt(byte[] data, string password)
        {
            byte[] result = null;
            byte[] passwordBytes = Encoding.Default.GetBytes(password);

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = keySize;
                    AES.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(passwordBytes, salt, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;
                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    result = ms.ToArray();
                }
            }

            return result;
        }
      
        public static void EncryptFile(string inPath, string outPath, string password)
        {
            byte[] encBytes = File.ReadAllBytes(inPath);
            encBytes = Encrypt(encBytes, password);
            File.WriteAllBytes(outPath, encBytes);
        }
        public static void DecryptFile(string inPath, string outPath, string password)
        {
            byte[] encBytes = File.ReadAllBytes(inPath);
            encBytes = Decrypt(encBytes, password);
            File.WriteAllBytes(outPath, encBytes);
        }
    }