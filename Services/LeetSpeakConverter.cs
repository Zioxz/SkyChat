using System.Collections.Generic;
using System.Text;

namespace Coflnet.Sky.Chat.Services
{
    public class LeetSpeakConverter
    {
        static readonly Dictionary<char, char> dictionary = new Dictionary<char, char>
        {
            {'4', 'a'},
            {'3', 'e'},
            {'1', 'l'},
            {'0', 'o'},
            {'@', 'a'},
            {'$', 's'},
            {'7', 't'},
            {'9', 'g'},
            {'8', 'b'},
            {'[', 'c'},
            {'|', 'i'},
            {'2', 'z'},
            {'6', 'b'},
            {'5', 's'},
            {'+', 't'}
        };

        /// <summary>
        /// Normalize leetspeak
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string Normalize(string original)
        {
            var builder = new StringBuilder(original.Length);
            foreach (var c in original)
            {
                if(dictionary.TryGetValue(c, out var newc))
                    builder.Append(newc);
                else
                    builder.Append(c);
            }
            return builder.ToString();
        }
    }
}
