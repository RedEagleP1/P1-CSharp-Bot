using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.AccountCommandSpecific
{
    public static class Encryptor
    {
        const string key = "f93nsbd";

        public static string Encrypt(string text)
        {
            string xorEncrypted = XOREncryption(text);
            byte[] byteEncoded = Encoding.UTF8.GetBytes(xorEncrypted);
            string base64 = Convert.ToBase64String(byteEncoded);

            return base64;
        }
        public static string Decrypt(string text)
        {
            byte[] byteEncoded = Convert.FromBase64String(text);
            string xorEncrypted = Encoding.UTF8.GetString(byteEncoded);
            string decryptedText = XORDecryption(xorEncrypted);

            return decryptedText;
        }
        static string XOREncryption(string text)
        {
            var result = new StringBuilder();
            for(int i = 0; i < text.Length; i++)
            {
                result.Append((char)(text[i] ^ key[i % key.Length]));
            }

            return result.ToString();
        }

        static string XORDecryption(string text)
        {
            return XOREncryption(text);
        }
    }
}
