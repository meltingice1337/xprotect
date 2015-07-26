using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.IO.Compression;
    class Encryption
    {
        public static int keySize = 256;
        public static int iterations = 10000;

        public static byte[] Encrypt(byte[] data, string password)
        {
            byte[] key = Encoding.ASCII.GetBytes(password);
            byte[] encBytes;
            byte[] IV = { 25, 32, 59, 24, 54, 12, 56, 23, 68, 35, 41, 65, 78, 32, 13, 89 };
            using (RijndaelManaged encryptor = new RijndaelManaged())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, IV, iterations);
                encryptor.Key = pdb.GetBytes(keySize / 8);
                encryptor.IV = IV;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    encBytes = ms.ToArray();
                    return encBytes;
                }
            }
        }
        public static byte[] Decrypt(byte[] data,string password)
        {
            byte[] key = Encoding.ASCII.GetBytes(password);
            byte[] IV = { 25, 32, 59, 24, 54, 12, 56, 23, 68, 35, 41, 65, 78, 32, 13, 89 };
            using (RijndaelManaged encryptor = new RijndaelManaged())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, IV, iterations);
                encryptor.Key = pdb.GetBytes(keySize / 8);
                encryptor.IV = IV;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    return ms.ToArray();
                }
            }
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