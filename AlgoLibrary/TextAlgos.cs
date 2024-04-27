namespace AlgoLibrary
{
    public class TextAlgos
    {
        /// <summary>
        /// return the Levenshtein distance (INT) between two strings
        /// distance = 0 : perfect match
        /// operations: insertion, deletion, substitution
        /// > https://en.wikipedia.org/wiki/Levenshtein_distance
        /// </summary>
        public static int LevenshteinDistance(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
            {
                d[i, 0] = i;
            }

            for (int j = 0; j <= t.Length; j++)
            {
                d[0, j] = j;
            }

            for (int j = 1; j <= t.Length; j++)
            {
                for (int i = 1; i <= s.Length; i++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(Math.Min(
                        d[i - 1, j] + 1,    // deletion
                        d[i, j - 1] + 1),   // insertion
                        d[i - 1, j - 1] + cost); // substitution
                }
            }

            return d[s.Length, t.Length];
        }
    
        /// <summary>
        /// return (bool) true if KeyValue-pairs match better when swapped
        /// : Key1 =~ Value2 and Key2 =~ Value1
        /// </summary>
        /// <param name="kv">First str KeyValue pair</param>
        /// <param name="kv2">Second str KeyValue pair</param>
        /// <returns></returns>
        public static bool KV_IsSwapped(KeyValuePair<string, string> kv, KeyValuePair<string, string> kv2) // 
        {
            int distance_KK =  LevenshteinDistance(kv.Key, kv2.Key); // smaller = better match, 0 = perfect match
            int distance_VV =  LevenshteinDistance(kv.Value, kv2.Value);
            int sum_KK_VV = distance_KK + distance_VV;

            // __swapped
            int distance__KV =  LevenshteinDistance(kv.Key, kv2.Value);
            int distance__VK =  LevenshteinDistance(kv.Value, kv2.Key);
            int sum__KV_VK = distance__KV + distance__VK;

            Console.WriteLine($"Distance between: {kv.Key} and {kv2.Key} is {distance_KK}");
            Console.WriteLine($"Distance between: {kv.Value} and {kv2.Value} is {distance_VV}");

            Console.WriteLine($"(Swapped) Distance between: {kv.Key} and {kv2.Value} is {distance__KV}");
            Console.WriteLine($"(Swapped) Distance between: {kv.Value} and {kv2.Key} is {distance__VK}");

            Console.WriteLine($"Sum straight: {sum_KK_VV} \t swapped: {sum__KV_VK}");

            return sum__KV_VK < sum_KK_VV;
        }
    
    }
}
