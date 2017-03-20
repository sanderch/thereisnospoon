using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            var col1 = new Dictionary<int, List<string>>();
            // var col2 = new List<string>();

            var lst = GetWords(anagram);
            Console.WriteLine("got {0} words that can be a part of the phrase", lst.Count);

            foreach (var word1 in lst)
            {
                foreach (var word2 in lst)
                {

                    var phrase = $"{word1} {word2}";
                    var phraseSum = phrase.ToCharArray().Sum(f => f);
                    if (!col1.ContainsKey(phraseSum))
                    {
                        col1.Add(phraseSum, new List<string> { phrase } );
                    }
                    else
                    {
                        col1[phraseSum].Add(phrase);
                    }

                }
            }
            var t = 0;
            foreach (var tupleElement in col1)
            {
                t++;
                Console.Write($"tuple element {tupleElement.Key} - {t} - {tupleElement.Value.Count} x ");
                if (col1.ContainsKey(anagram.Sum(l => l) - (tupleElement.Key + " ".ToCharArray().Sum(l => l))))
                {
                    var tupleToCompare = col1[anagram.Sum(l => l) - (tupleElement.Key + " ".ToCharArray().Sum(l => l))];
                    Console.Write(tupleToCompare.Count);
                    var j = 0;
                    var tasks = new List<Task>();
                    
                    foreach (var phrase in tupleElement.Value)
                    {
                        j++;
                        if (j/(tupleElement.Value.Count/10+1) == j % (tupleElement.Value.Count / 10+1)) { Console.Write('.');}
                        tasks.Add(Task.Run(() =>
                        {
                            foreach (var phrase2 in tupleToCompare)
                            {
                                var r = Match($"{phrase} {phrase2}", md5, hashes);
                                if (r != null)
                                {
                                    Console.WriteLine($"GOTCHA: {phrase} {phrase2}");
                                }
                            }
                        }));
                    }
                    Task.WaitAll(tasks.ToArray());
                    Console.WriteLine();
                } else Console.WriteLine();
            }
            // four_foreaches(lst, anagram, md5, hashes, result);

            Console.WriteLine();
            result.ForEach(Console.WriteLine);
            Console.ReadLine();
        }

        private static void four_foreaches(IReadOnlyCollection<string> lst, char[] anagram, HashAlgorithm md5, List<string> hashes, List<string> result)
        {
            foreach (var word1 in lst)
            {
                Console.WriteLine(word1);
                foreach (var word2 in lst)
                {
                    Console.Write('.');
                    if (" ".ToCharArray().Sum(c => c) + word1.ToCharArray().Sum(c => c) + word2.ToCharArray().Sum(c => c) ==
                        anagram.Sum(c => c)
                    )
                    {
                        var res = Match($"{word1} {word2}", md5, hashes);
                        if (res != null)
                        {
                            result.Add(res);
                        }
                    }

                    foreach (var word3 in lst)
                    {
                        if ("  ".ToCharArray().Sum(c => c) + word1.ToCharArray().Sum(c => c) +
                            word2.ToCharArray().Sum(c => c) + word3.ToCharArray().Sum(c => c) == anagram.Sum(c => c)
                        )
                        {
                            var res = Match($"{word1} {word2} {word3}", md5, hashes);
                            if (res != null)
                            {
                                result.Add(res);
                            }
                        }
                    }
                }
            }
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