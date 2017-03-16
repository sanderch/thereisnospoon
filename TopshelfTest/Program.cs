using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TopshelfTest
{
    class Program
    {
        static void Main()
        {
            var anagram = "poultry outwits ants".ToCharArray();
            var result = new List<string>();
            var hashes = new List<string> { "e4820b45d2277f3844eac66c903e84be", "23170acc097c24edb98fc5488ab033fe", "665e5bcb0c20062fe8abaaf4628bb154" };
            var md5 = MD5.Create();

            var lst = GetWords(anagram); // .OrderBy(f => f.Length).ToList();
            Console.WriteLine("got {0} words that can be a part of the phrase", lst.Count);

            foreach (var word1 in lst)
            {
                Console.WriteLine(word1);
                foreach (var word2 in lst)
                {
                    Console.Write('.');
                    if (" ".ToCharArray().Sum(c => c) + word1.ToCharArray().Sum(c => c) + word2.ToCharArray().Sum(c => c)  == anagram.Sum(c => c)
                    )
                    {
                        var res = Match($"{word1} {word2}", md5, hashes);
                        if (res != null) { result.Add(res); }
                    }

                    foreach (var word3 in lst)
                    {
                        if ("  ".ToCharArray().Sum(c => c) + word1.ToCharArray().Sum(c => c) +
                            word2.ToCharArray().Sum(c => c) + word3.ToCharArray().Sum(c => c) == anagram.Sum(c => c)
                            )
                        {
                            var res = Match($"{word1} {word2} {word3}", md5, hashes);
                            if (res != null) { result.Add(res); }
                        }
                        if ($"{word1} {word2} {word3}".Length > 20) continue;
                        foreach (var word4 in lst)
                        {
                            if ("   ".ToCharArray().Sum(c => c) + word1.ToCharArray().Sum(c => c) +
                                word2.ToCharArray().Sum(c => c) + word3.ToCharArray().Sum(c => c) + 
                                word4.ToCharArray().Sum(c => c) == anagram.Sum(c => c)
                                )
                            {
                                var res = Match($"{word1} {word2} {word3} {word4}", md5, hashes);
                                if (res != null) { result.Add(res); }
                            }
                        }
                    }
                }
            }
            Console.WriteLine();
            result.ForEach(Console.WriteLine);
            Console.ReadLine();
        }

        private static string Match(string phrase, HashAlgorithm md5, List<string> hashes)
        {
            var hashstring = GetStringHash(md5.ComputeHash(Encoding.ASCII.GetBytes(phrase)));
            return hashes.Contains(hashstring) ? $"GOTCHA: {phrase}" : null;
        }

        private static string GetStringHash(byte[] hash)
        {
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString().ToLower();
        }

        private static List<string> GetWords(char[] anagram)
        {
            List<string> lst;
            using (var fileStream = File.OpenRead("C:\\Users\\chea\\Downloads\\wordlist"))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128))
                {
                    lst = new List<string>();

                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var takeIt = true;
                        var lineChar = line.ToCharArray();
                        var dic = new Dictionary<char, int>();
                        foreach (var c in lineChar)
                        {
                            takeIt = takeIt && anagram.Contains(c);
                            if (!dic.ContainsKey(c)) dic.Add(c, 1);
                            else dic[c]++;
                            takeIt = takeIt && dic[c] <= anagram.Count(ch => ch == c);
                        }

                        if (takeIt)
                        {
                            lst.Add(line);
                            // Console.WriteLine(line);
                        }
                    }
                }
            }
            return lst;
        }
    }
}