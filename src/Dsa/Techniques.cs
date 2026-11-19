// PART 8 — Common techniques
using System.Buffers;
using System.Text;

namespace Dsa;

public static class Techniques
{
    // Two pointers on a sorted array.
    public static (int, int) TwoSumSorted(int[] a, int target)
    {
        int l = 0, r = a.Length - 1;
        while (l < r)
        {
            int sum = a[l] + a[r];
            if (sum == target) return (l, r);
            if (sum < target) l++; else r--;
        }
        return (-1, -1);
    }

    // Hash map lookup — unsorted input, O(n).
    public static (int, int) TwoSumHash(int[] a, int target)
    {
        var seen = new Dictionary<int, int>();
        for (int i = 0; i < a.Length; i++)
        {
            if (seen.TryGetValue(target - a[i], out int j)) return (j, i);
            seen[a[i]] = i;
        }
        return (-1, -1);
    }

    // Sliding window.
    public static int LongestUniqueSubstring(string s)
    {
        var lastSeen = new Dictionary<char, int>();
        int best = 0, start = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (lastSeen.TryGetValue(s[i], out int prev) && prev >= start) start = prev + 1;
            lastSeen[s[i]] = i;
            best = Math.Max(best, i - start + 1);
        }
        return best;
    }

    // Prefix sums: sum of [i..j] = p[j+1] - p[i] in O(1).
    public static long[] PrefixSums(int[] a)
    {
        var p = new long[a.Length + 1];
        for (int i = 0; i < a.Length; i++) p[i + 1] = p[i] + a[i];
        return p;
    }

    // Backtracking: all permutations.
    public static List<List<int>> Permutations(int[] nums)
    {
        var result = new List<List<int>>();
        var used = new bool[nums.Length];
        var current = new List<int>();
        void Backtrack()
        {
            if (current.Count == nums.Length) { result.Add([.. current]); return; }
            for (int i = 0; i < nums.Length; i++)
            {
                if (used[i]) continue;
                used[i] = true; current.Add(nums[i]);
                Backtrack();
                used[i] = false; current.RemoveAt(current.Count - 1);
            }
        }
        Backtrack();
        return result;
    }

    // Backtracking: all subsets (power set).
    public static List<List<int>> Subsets(int[] nums)
    {
        var result = new List<List<int>>();
        var current = new List<int>();
        void Backtrack(int start)
        {
            result.Add([.. current]);
            for (int i = start; i < nums.Length; i++)
            {
                current.Add(nums[i]);
                Backtrack(i + 1);
                current.RemoveAt(current.Count - 1);
            }
        }
        Backtrack(0);
        return result;
    }

    // Backtracking: count N-Queens solutions.
    public static int NQueens(int n)
    {
        var cols = new bool[n];
        var diag1 = new bool[2 * n];
        var diag2 = new bool[2 * n];
        int count = 0;
        void Place(int row)
        {
            if (row == n) { count++; return; }
            for (int c = 0; c < n; c++)
            {
                if (cols[c] || diag1[row + c] || diag2[row - c + n]) continue;
                cols[c] = diag1[row + c] = diag2[row - c + n] = true;
                Place(row + 1);
                cols[c] = diag1[row + c] = diag2[row - c + n] = false;
            }
        }
        Place(0);
        return count;
    }

    // Greedy: pick the activity that finishes first.
    public static int MaxActivities((int Start, int End)[] activities)
    {
        int count = 0, lastEnd = int.MinValue;
        foreach (var (s, e) in activities.OrderBy(a => a.End))
            if (s >= lastEnd) { count++; lastEnd = e; }
        return count;
    }

    // Bit manipulation.
    public static bool IsPowerOfTwo(long x) => x > 0 && (x & (x - 1)) == 0;

    // Euclid's GCD.
    public static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);

    // Sieve of Eratosthenes. O(n log log n).
    public static List<int> Sieve(int limit)
    {
        var composite = new bool[limit + 1];
        var primes = new List<int>();
        for (int i = 2; i <= limit; i++)
        {
            if (composite[i]) continue;
            primes.Add(i);
            for (long j = (long)i * i; j <= limit; j += i) composite[j] = true;
        }
        return primes;
    }

    // Fast exponentiation by squaring. O(log exp).
    public static long ModPow(long b, long exp, long mod)
    {
        long result = 1;
        b %= mod;
        while (exp > 0)
        {
            if ((exp & 1) == 1) result = result * b % mod;
            b = b * b % mod;
            exp >>= 1;
        }
        return result;
    }
}
