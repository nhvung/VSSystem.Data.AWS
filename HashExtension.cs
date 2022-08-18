using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VSSystem.Data.AWS
{
    static class HashExtension
    {
        public static byte[] GetSha1Hash(this byte[] input)
        {
            var sha1 = SHA1.Create();
            return sha1.ComputeHash(input);
        }
        public static byte[] GetMd5Hash(this byte[] input)
        {
            var md5 = MD5.Create();
            return md5.ComputeHash(input);
        }
        public static byte[] GetSha1Hash(this Stream input)
        {
            long position = input.Position;
            var sha1 = SHA1.Create();
            byte[] result = sha1.ComputeHash(input);
            input.Seek(position, SeekOrigin.Begin);
            return result;
        }
        public static byte[] GetMd5Hash(this Stream input)
        {
            long position = input.Position;
            var md5 = MD5.Create();
            byte[] result = md5.ComputeHash(input);
            input.Seek(position, SeekOrigin.Begin);
            return result;
        }
    }
}
