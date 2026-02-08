using System.Collections.Generic;
using System.Text;

namespace Server.Services
{
    public class DecryptionService
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

        public string Decrypt(string ciphertext, string keyword)
        {
            if (ciphertext == null) return "";

            string cipherUpper = BuildCipherAlphabetUpper(keyword);

            
            var invUpper = new Dictionary<char, char>(26);
            for (int i = 0; i < 26; i++)
                invUpper[cipherUpper[i]] = AlphabetUpper[i];

            var invLower = new Dictionary<char, char>(26);
            for (int i = 0; i < 26; i++)
                invLower[char.ToLowerInvariant(cipherUpper[i])] = AlphabetLower[i];

            var sb = new StringBuilder(ciphertext.Length);

            foreach (char ch in ciphertext)
            {
                if (ch >= 'A' && ch <= 'Z')
                {
                    
                    sb.Append(invUpper.TryGetValue(ch, out char p) ? p : ch);
                }
                else if (ch >= 'a' && ch <= 'z')
                {
                    sb.Append(invLower.TryGetValue(ch, out char p) ? p : ch);
                }
                else
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }
    }
}
