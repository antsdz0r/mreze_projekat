using System.Collections.Generic;
using System.Text;

namespace Client.Services
{
    internal static class KeywordCipher
    {
        private const string AlphabetUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string AlphabetLower = "abcdefghijklmnopqrstuvwxyz";

        private static string BuildCipherAlphabetUpper(string keyword)
        {
            if (keyword == null) keyword = "";

            var seen = new HashSet<char>();
            var sb = new StringBuilder(26);

           
            foreach (char c in keyword.ToUpperInvariant())
            {
                if (c < 'A' || c > 'Z') continue;
                if (seen.Add(c)) sb.Append(c);
            }

             
            foreach (char c in AlphabetUpper)
            {
                if (seen.Add(c)) sb.Append(c);
            }

            return sb.ToString(); 
        }

        public static string Encrypt(string plaintext, string keyword)
        {
            if (plaintext == null) return "";

            string cipherUpper = BuildCipherAlphabetUpper(keyword);

           
            var mapUpper = new Dictionary<char, char>(26);
            for (int i = 0; i < 26; i++)
                mapUpper[AlphabetUpper[i]] = cipherUpper[i];

            var mapLower = new Dictionary<char, char>(26);
            for (int i = 0; i < 26; i++)
                mapLower[AlphabetLower[i]] = char.ToLowerInvariant(cipherUpper[i]);

            var sb = new StringBuilder(plaintext.Length);

            foreach (char ch in plaintext)
            {
                if (ch >= 'A' && ch <= 'Z')
                    sb.Append(mapUpper[ch]);
                
                else if (ch >= 'a' && ch <= 'z')
                    sb.Append(mapLower[ch]);

                else
                    sb.Append(ch);
            }

            return sb.ToString();
        }
    }
}
