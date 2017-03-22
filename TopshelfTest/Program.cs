using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ThereIsNoSpoon
{
    internal class Program
    {
        static void Main()
        {
            var anagram = "poultry outwits ants".ToCharArray();
            var hashes = new List<string> { "e4820b45d2277f3844eac66c903e84be", "23170acc097c24edb98fc5488ab033fe", "665e5bcb0c20062fe8abaaf4628bb154" };
            
            var wordsByStringLetters = GetWordsByStringLetters(anagram);
            Console.WriteLine("got {0} words that can be a part of the phrase", wordsByStringLetters.Count);

            var pairsCollection = MapPairs(wordsByStringLetters);

            var result = FindTriples(wordsByStringLetters, anagram, hashes);
            result.AddRange(FindQuarters(pairsCollection, anagram, hashes));

            result.ForEach(Console.WriteLine);
            Console.ReadLine();
        }

        private static List<string> FindQuarters(IReadOnlyDictionary<int, List<string>> pairsCollection, IEnumerable<char> anagram, ICollection<string> hashes)
        {
            var t = 0;
            var result = new List<string>();
            var anagramSum = anagram.Sum(l => l);
            foreach (var tupleElement in pairsCollection)
            {
                t++;
                Console.Write($"tuple element {tupleElement.Key} - {t} - {tupleElement.Value.Count} x ");
                if (pairsCollection.ContainsKey(anagramSum - tupleElement.Key))
                {
                    var tupleToCompare = pairsCollection[anagramSum - tupleElement.Key];
                    Console.Write(tupleToCompare.Count);
                    var j = 0;
                    var tasks = new List<Task>();

                    foreach (var phrase in tupleElement.Value)
                    {
                        j++;
                        if (j / (tupleElement.Value.Count / 10 + 1) == j % (tupleElement.Value.Count / 10 + 1))
                        {
                            Console.Write('.');
                        }
                        tasks.Add(Task.Run(() =>
                        {
                            foreach (var phrase2 in tupleToCompare)
                            {
                                var r = Match($"{phrase} {phrase2}", hashes);
                                if (r != null)
                                {
                                    result.Add(r);
                                }
                            }
                        }));
                    }
                    Task.WaitAll(tasks.ToArray());
                }
                Console.WriteLine();
            }
            return result;
        }

        private static Dictionary<int, List<string>> MapPairs(IReadOnlyCollection<string> lst)
        {
            var col1 = new Dictionary<int, List<string>>();
            foreach (var word1 in lst)
            {
                foreach (var word2 in lst)
                {
                    var phrase = $"{word1} {word2}";
                    var phraseSum = phrase.ToCharArray().Sum(f => f);
                    if (!col1.ContainsKey(phraseSum))
                    {
                        col1.Add(phraseSum, new List<string> {phrase});
                    }
                    else
                    {
                        col1[phraseSum].Add(phrase);
                    }
                }
            }
            return col1;
        }

        private static List<string> FindTriples(IReadOnlyCollection<string> lst, IEnumerable<char> anagram, ICollection<string> hashes)
        {
            var result = new List<string>();
            var anagramSum = anagram.Sum(c => c);
            var spaceCharSum = " ".ToCharArray().Sum(c => c);
            foreach (var word1 in lst)
            {

                Console.WriteLine(word1);
                var word1Sum = word1.ToCharArray().Sum(c => c);
                if (word1Sum == anagramSum - "  ".ToCharArray().Sum(c => c)
                )
                {
                    var res = Match($"{word1}", hashes);
                    if (res != null)
                    {
                        result.Add(res);
                    }
                }
                foreach (var word2 in lst)
                {
                    var word2Sum = word2.ToCharArray().Sum(c => c);
                    if (spaceCharSum + word1Sum + word2Sum == anagramSum - spaceCharSum)
                    {
                        var res = Match($"{word1} {word2}", hashes);
                        if (res != null)
                        {
                            result.Add(res);
                        }
                    }

                    foreach (var word3 in lst)
                    {
                        if (spaceCharSum*2 + word1Sum + word2Sum + word3.ToCharArray().Sum(c => c) == anagramSum
                        )
                        {
                            var res = Match($"{word1} {word2} {word3}", hashes);
                            if (res != null)
                            {
                                result.Add(res);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static string Match(string phrase, ICollection<string> hashes)
        {
            var md5 = MD5.Create();
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

        private static List<string> GetWordsByStringLetters(char[] anagram)
        {
            var assembly = Assembly.GetEntryAssembly();
            const string resourceName = "ThereIsNoSpoon.resources.wordlist";
            var lst = new List<string>();
            using (var fileStream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128))
                {
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
                        }
                    }
                }
            }
            return lst;
        }
    }
}