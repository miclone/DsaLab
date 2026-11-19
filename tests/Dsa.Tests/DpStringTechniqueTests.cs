using System.Numerics;

namespace Dsa.Tests;

public class DynamicProgrammingTests
{
    [Theory]
    [InlineData(0, 0L)]
    [InlineData(1, 1L)]
    [InlineData(10, 55L)]
    [InlineData(50, 12586269025L)]
    [InlineData(90, 2880067194370816120L)]
    public void Fibonacci_KnownValues(int n, long expected)
    {
        Assert.Equal(expected, DynamicProgramming.FibMemo(n));
        Assert.Equal(expected, DynamicProgramming.FibTab(n));
    }

    [Fact]
    public void Knapsack_KnownAndBruteForce()
    {
        Assert.Equal(9, DynamicProgramming.Knapsack([1, 3, 4, 5], [1, 4, 5, 7], 7));
        Assert.Equal(0, DynamicProgramming.Knapsack([5], [10], 4));

        var rng = new Random(9);
        for (int trial = 0; trial < 30; trial++)
        {
            int n = rng.Next(1, 12);
            var w = Enumerable.Range(0, n).Select(_ => rng.Next(1, 15)).ToArray();
            var v = Enumerable.Range(0, n).Select(_ => rng.Next(1, 30)).ToArray();
            int cap = rng.Next(0, 40);

            int best = 0; // try every subset
            for (int mask = 0; mask < 1 << n; mask++)
            {
                int tw = 0, tv = 0;
                for (int i = 0; i < n; i++) if ((mask >> i & 1) == 1) { tw += w[i]; tv += v[i]; }
                if (tw <= cap) best = Math.Max(best, tv);
            }
            Assert.Equal(best, DynamicProgramming.Knapsack(w, v, cap));
        }
    }

    [Theory]
    [InlineData("ABCBDAB", "BDCABA", 4)]
    [InlineData("", "abc", 0)]
    [InlineData("abc", "abc", 3)]
    [InlineData("abc", "def", 0)]
    public void Lcs(string a, string b, int expected) => Assert.Equal(expected, DynamicProgramming.Lcs(a, b));

    [Fact]
    public void Lis_MatchesQuadraticVersion()
    {
        Assert.Equal(4, DynamicProgramming.Lis([10, 9, 2, 5, 3, 7, 101, 18]));
        var rng = new Random(4);
        for (int trial = 0; trial < 50; trial++)
        {
            var a = Enumerable.Range(0, rng.Next(1, 40)).Select(_ => rng.Next(0, 30)).ToArray();
            var dp = new int[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                dp[i] = 1;
                for (int j = 0; j < i; j++) if (a[j] < a[i]) dp[i] = Math.Max(dp[i], dp[j] + 1);
            }
            Assert.Equal(dp.Max(), DynamicProgramming.Lis(a));
        }
    }

    [Theory]
    [InlineData("kitten", "sitting", 3)]
    [InlineData("", "abc", 3)]
    [InlineData("flaw", "lawn", 2)]
    [InlineData("same", "same", 0)]
    public void EditDistance(string a, string b, int expected)
    {
        Assert.Equal(expected, DynamicProgramming.EditDistance(a, b));
        Assert.Equal(expected, DynamicProgramming.EditDistance(b, a)); // symmetric
    }

    [Theory]
    [InlineData(new[] { 1, 2, 5 }, 11, 3)]
    [InlineData(new[] { 2 }, 3, -1)]
    [InlineData(new[] { 1 }, 0, 0)]
    [InlineData(new[] { 1, 3, 4 }, 6, 2)] // greedy would say 3 (4+1+1)
    public void CoinChange(int[] coins, int amount, int expected) =>
        Assert.Equal(expected, DynamicProgramming.CoinChange(coins, amount));

    [Fact]
    public void Kadane_MatchesBruteForce()
    {
        Assert.Equal(6, DynamicProgramming.MaxSubarray([-2, 1, -3, 4, -1, 2, 1, -5, 4]));
        Assert.Equal(-1, DynamicProgramming.MaxSubarray([-3, -1, -2]));

        var rng = new Random(6);
        for (int trial = 0; trial < 50; trial++)
        {
            var a = Enumerable.Range(0, rng.Next(1, 30)).Select(_ => rng.Next(-20, 20)).ToArray();
            int best = int.MinValue;
            for (int i = 0; i < a.Length; i++)
                for (int j = i, sum = 0; j < a.Length; j++) best = Math.Max(best, sum += a[j]);
            Assert.Equal(best, DynamicProgramming.MaxSubarray(a));
        }
    }
}

public class StringTests
{
    static List<int> NaiveSearch(string text, string pattern)
    {
        var result = new List<int>();
        for (int i = 0; i + pattern.Length <= text.Length; i++)
            if (string.CompareOrdinal(text, i, pattern, 0, pattern.Length) == 0) result.Add(i);
        return result;
    }

    [Fact]
    public void Kmp_And_RabinKarp_MatchNaiveSearch()
    {
        var rng = new Random(8);
        for (int trial = 0; trial < 200; trial++)
        {
            // A tiny alphabet produces lots of overlapping matches, which is where bugs hide.
            string text = new(Enumerable.Range(0, rng.Next(0, 60)).Select(_ => "ab"[rng.Next(2)]).ToArray());
            string pattern = new(Enumerable.Range(0, rng.Next(1, 5)).Select(_ => "ab"[rng.Next(2)]).ToArray());
            var expected = NaiveSearch(text, pattern);

            Assert.Equal(expected, Strings.KmpSearch(text, pattern));
            Assert.Equal(expected, Strings.RabinKarp(text, pattern));
        }
    }

    [Theory]
    [InlineData("A man, a plan, a canal: Panama", true)]
    [InlineData("race a car", false)]
    [InlineData("", true)]
    [InlineData("x", true)]
    public void IsPalindrome(string s, bool expected) => Assert.Equal(expected, Strings.IsPalindrome(s));

    [Theory]
    [InlineData("listen", "silent", true)]
    [InlineData("rat", "car", false)]
    [InlineData("aab", "abb", false)]
    [InlineData("a", "ab", false)]
    public void IsAnagram(string a, string b, bool expected) => Assert.Equal(expected, Strings.IsAnagram(a, b));
}

public class TechniqueTests
{
    [Fact]
    public void TwoSum_BothVersions()
    {
        Assert.Equal((2, 4), Techniques.TwoSumSorted([1, 2, 4, 7, 11], 15));
        Assert.Equal((-1, -1), Techniques.TwoSumSorted([1, 2, 3], 100));
        Assert.Equal((1, 2), Techniques.TwoSumHash([3, 2, 4], 6));
        Assert.Equal((0, 1), Techniques.TwoSumHash([3, 3], 6));
    }

    [Theory]
    [InlineData("abcabcbb", 3)]
    [InlineData("bbbbb", 1)]
    [InlineData("pwwkew", 3)]
    [InlineData("", 0)]
    [InlineData("abba", 2)] // checks the window never moves backwards
    public void LongestUniqueSubstring(string s, int expected) =>
        Assert.Equal(expected, Techniques.LongestUniqueSubstring(s));

    [Fact]
    public void PrefixSums_GiveRangeSums()
    {
        int[] a = [3, 1, 4, 1, 5];
        var p = Techniques.PrefixSums(a);
        for (int i = 0; i < a.Length; i++)
            for (int j = i; j < a.Length; j++)
                Assert.Equal(a[i..(j + 1)].Sum(), p[j + 1] - p[i]);
    }

    [Fact]
    public void Permutations_AreAllDistinct_AndCountIsFactorial()
    {
        var perms = Techniques.Permutations([1, 2, 3, 4]);
        Assert.Equal(24, perms.Count);
        Assert.Equal(24, perms.Select(p => string.Join(",", p)).Distinct().Count());
    }

    [Fact]
    public void Subsets_CountIsPowerOfTwo()
    {
        var subsets = Techniques.Subsets([1, 2, 3, 4]);
        Assert.Equal(16, subsets.Count);
        Assert.Contains(subsets, s => s.Count == 0);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(4, 2)]
    [InlineData(6, 4)]
    [InlineData(8, 92)]
    public void NQueens(int n, int expected) => Assert.Equal(expected, Techniques.NQueens(n));

    [Fact]
    public void MaxActivities_Greedy() =>
        Assert.Equal(3, Techniques.MaxActivities([(1, 4), (3, 5), (0, 6), (5, 7), (8, 9), (5, 9)]));

    [Theory]
    [InlineData(1L, true)]
    [InlineData(64L, true)]
    [InlineData(0L, false)]
    [InlineData(-8L, false)]
    [InlineData(96L, false)]
    public void IsPowerOfTwo(long x, bool expected) => Assert.Equal(expected, Techniques.IsPowerOfTwo(x));

    [Theory]
    [InlineData(48L, 18L, 6L)]
    [InlineData(17L, 5L, 1L)]
    [InlineData(0L, 9L, 9L)]
    public void Gcd(long a, long b, long expected) => Assert.Equal(expected, Techniques.Gcd(a, b));

    [Fact]
    public void Sieve_MatchesTrialDivision()
    {
        static bool IsPrime(int n)
        {
            if (n < 2) return false;
            for (int d = 2; d * d <= n; d++) if (n % d == 0) return false;
            return true;
        }
        Assert.Equal(Enumerable.Range(0, 1001).Where(IsPrime), Techniques.Sieve(1000));
    }

    [Fact]
    public void ModPow_MatchesBigInteger()
    {
        const long mod = 1_000_000_007;
        var rng = new Random(10);
        for (int trial = 0; trial < 100; trial++)
        {
            long b = rng.NextInt64(0, mod), e = rng.NextInt64(0, 1_000_000_000_000);
            Assert.Equal((long)BigInteger.ModPow(b, e, mod), Techniques.ModPow(b, e, mod));
        }
    }
}
