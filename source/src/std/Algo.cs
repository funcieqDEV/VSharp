using System;
using System.Collections.Generic;
using VSharp;
using CSMath = System.Math;

namespace VSharpLib
{
    [Module]
    public class algo
    {
        /// <summary>
        /// Knuth-Morris-Pratt (KMP) Algorithm for substring search.
        /// </summary>
        /// <param name="text">The main text to search within.</param>
        /// <param name="pattern">The pattern to find.</param>
        /// <returns>List of starting indices where the pattern appears in the text.</returns>
        public List<int> KMPSearch(string text, string pattern)
        {
            int m = pattern.Length, n = text.Length;
            List<int> result = new List<int>();
            int[] lps = computeLPS(pattern).ToArray();

            int i = 0, j = 0;
            while (i < n)
            {
                if (pattern[j] == text[i])
                {
                    i++; j++;
                }
                if (j == m)
                {
                    result.Add(i - j);
                    j = lps[j - 1];
                }
                else if (i < n && pattern[j] != text[i])
                {
                    j = (j != 0) ? lps[j - 1] : 0;
                    i += (j == 0) ? 1 : 0;
                }
            }
            return result;
        }

        private List<int> computeLPS(string pattern)
        {
            int m = pattern.Length, len = 0;
            int[] lps = new int[m];
            for (int i = 1; i < m;)
            {
                if (pattern[i] == pattern[len])
                    lps[i++] = ++len;
                else
                    len = (len != 0) ? lps[len - 1] : 0;
            }
            return lps.ToList();
        }

        /// <summary>
        /// Rabin-Karp Algorithm for substring search using rolling hash.
        /// </summary>
        /// <param name="text">The main text to search within.</param>
        /// <param name="pattern">The pattern to find.</param>
        /// <param name="prime">A prime number used for hashing.</param>
        /// <returns>List of starting indices where the pattern appears in the text.</returns>
        public List<int> rabinKarpSearch(string text, string pattern, int prime = 101)
        {
            int m = pattern.Length, n = text.Length;
            int patternHash = 0, textHash = 0, h = 1, d = 256;
            List<int> result = new List<int>();

            for (int i = 0; i < m - 1; i++)
                h = (h * d) % prime;

            for (int i = 0; i < m; i++)
            {
                patternHash = (d * patternHash + pattern[i]) % prime;
                textHash = (d * textHash + text[i]) % prime;
            }

            for (int i = 0; i <= n - m; i++)
            {
                if (patternHash == textHash && text.Substring(i, m) == pattern)
                    result.Add(i);

                if (i < n - m)
                {
                    textHash = (d * (textHash - text[i] * h) + text[i + m]) % prime;
                    if (textHash < 0) textHash += prime;
                }
            }
            return result;
        }

        /// <summary>
        /// Z-Function for finding prefix matches in a string.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <returns>An array where each index contains the length of the longest prefix matching the substring starting at that index.</returns>
        public List<int>ZFunction(string s)
        {
            int n = s.Length;
            int[] z = new int[n];
            int l = 0, r = 0;

            for (int i = 1; i < n; i++)
            {
                if (i <= r)
                    z[i] = CSMath.Min(r - i + 1, z[i - l]);

                while (i + z[i] < n && s[z[i]] == s[i + z[i]])
                    z[i]++;

                if (i + z[i] - 1 > r)
                {
                    l = i;
                    r = i + z[i] - 1;
                }
            }
            return z.ToList();
        }

        /// <summary>
        /// Suffix Array Construction using sorting.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <returns>A sorted suffix array.</returns>
        public List<int> buildSuffixArray(string s)
        {
            int n = s.Length;
            Tuple<string, int>[] suffixes = new Tuple<string, int>[n];

            for (int i = 0; i < n; i++)
                suffixes[i] = Tuple.Create(s.Substring(i), i);

            System.Array.Sort(suffixes, (a, b) => string.Compare(a.Item1, b.Item1, StringComparison.Ordinal));

            int[] suffixArray = new int[n];
            for (int i = 0; i < n; i++)
                suffixArray[i] = suffixes[i].Item2;

            return suffixArray.ToList();
        }

        /// <summary>
        /// Computes the Levenshtein distance between two strings.
        /// </summary>
        /// <param name="s1">The first string.</param>
        /// <param name="s2">The second string.</param>
        /// <returns>The minimum number of operations required to transform one string into the other.</returns>
        public int levenshteinDistance(string s1, string s2)
        {
            int len1 = s1.Length, len2 = s2.Length;
            int[,] dp = new int[len1 + 1, len2 + 1];

            for (int i = 0; i <= len1; i++)
                for (int j = 0; j <= len2; j++)
                {
                    if (i == 0) dp[i, j] = j;
                    else if (j == 0) dp[i, j] = i;
                    else
                        dp[i, j] = CSMath.Min(
                            dp[i - 1, j - 1] + (s1[i - 1] == s2[j - 1] ? 0 : 1),
                            CSMath.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1));
                }

            return dp[len1, len2];
        }

        /// <summary>
        /// Finds all prime numbers up to a given limit using the Sieve of Eratosthenes algorithm.
        /// </summary>
        /// <param name="n">The upper limit (inclusive) to find prime numbers.</param>
        /// <returns>A list of prime numbers up to the specified limit.</returns>
        public List<int> sieveOfEratosthenes(int n)
        {
            if (n < 2)
                return new List<int>();

            bool[] isPrime = new bool[n + 1];

         
            for (int i = 2; i <= n; i++)
                isPrime[i] = true;

            for (int p = 2; p * p <= n; p++)
            {
                if (isPrime[p])
                {
                    for (int i = p * p; i <= n; i += p)
                        isPrime[i] = false;
                }
            }

            List<int> primes = new List<int>();
            for (int i = 2; i <= n; i++)
            {
                if (isPrime[i])
                    primes.Add(i);
            }
            return primes;
        }
    }
}
