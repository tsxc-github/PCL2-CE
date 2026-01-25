using System;
using System.Text;
using System.Security.Cryptography;

namespace PCL.Core.MZMC.Helper
{
    public class Converter
    {
        public static string ComputeSHA256(string s)
        {
            string hash = String.Empty;

            // 初始化一个 SHA256 哈希对象
            using (SHA256 sha256 = SHA256.Create())
            {
                // 计算给定字符串的哈希值
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));

                // 将字节数组转换为字符串格式
                foreach (byte b in hashValue) {
                    hash += $"{b:X2}";
                }
            }

            return hash;
        }
    }
}