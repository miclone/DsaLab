// PART 6 — Dynamic programming
using System.Buffers;
using System.Text;

namespace Dsa;

public static class DynamicProgramming
{
    // Top-down (memoisation).
    public static long FibMemo(int n, Dictionary<int, long>? memo = null)
    {
        memo ??= new();
        if (n <= 1) return n;
        if (memo.TryGetValue(n, out var cached)) return cached;
        return memo[n] = FibMemo(n - 1, memo) + FibMemo(n - 2, memo);
    }

    // Bottom-up (tabulation) with O(1) space.
    public static long FibTab(int n)
    {
        long a = 0, b = 1;
        for (int i = 0; i < n; i++) (a, b) = (b, a + b);
        return a;
    }

    // 0/1 knapsack — max value within capacity. O(n·W).
    public static int Knapsack(int[] weights, int[] values, int capacity)
    {
        var dp = new int[capacity + 1];
        for (int i = 0; i < weights.Length; i++)
            for (int w = capacity; w >= weights[i]; w--) // backwards = each item once
                dp[w] = Math.Max(dp[w], dp[w - weights[i]] + values[i]);
        return dp[capacity];
    }

    // Longest common subsequence. O(n·m).
    public static int Lcs(string a, string b)
    {
        var dp = new int[a.Length + 1, b.Length + 1];
        for (int i = 1; i <= a.Length; i++)
            for (int j = 1; j <= b.Length; j++)
                dp[i, j] = a[i - 1] == b[j - 1] ? dp[i - 1, j - 1] + 1 : Math.Max(dp[i - 1, j], dp[i, j - 1]);
        return dp[a.Length, b.Length];
    }

    // Longest increasing subsequence — patience sorting, O(n log n).
    public static int Lis(int[] nums)
    {
        var tails = new List<int>();
        foreach (var x in nums)
        {
            int idx = tails.BinarySearch(x);
            if (idx < 0) idx = ~idx;
            if (idx == tails.Count) tails.Add(x); else tails[idx] = x;
        }
        return tails.Count;
    }

    // Levenshtein distance. O(n·m).
    public static int EditDistance(string a, string b)
    {
        var dp = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) dp[0, j] = j;
        for (int i = 1; i <= a.Length; i++)
            for (int j = 1; j <= b.Length; j++)
                dp[i, j] = a[i - 1] == b[j - 1]
                    ? dp[i - 1, j - 1]
                    : 1 + Math.Min(dp[i - 1, j - 1], Math.Min(dp[i - 1, j], dp[i, j - 1]));
        return dp[a.Length, b.Length];
    }

    // Fewest coins to make an amount (-1 if impossible).
    public static int CoinChange(int[] coins, int amount)
    {
        var dp = Enumerable.Repeat(int.MaxValue, amount + 1).ToArray();
        dp[0] = 0;
        for (int x = 1; x <= amount; x++)
            foreach (var c in coins)
                if (c <= x && dp[x - c] != int.MaxValue) dp[x] = Math.Min(dp[x], dp[x - c] + 1);
        return dp[amount] == int.MaxValue ? -1 : dp[amount];
    }

    // Kadane's algorithm — max subarray sum. O(n).
    public static int MaxSubarray(int[] nums)
    {
        int best = nums[0], current = nums[0];
        for (int i = 1; i < nums.Length; i++)
        {
            current = Math.Max(nums[i], current + nums[i]);
            best = Math.Max(best, current);
        }
        return best;
    }
}
