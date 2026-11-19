// PART 7 — String algorithms
using System.Buffers;
using System.Text;

namespace Dsa;

public static class Strings
{
    // Knuth-Morris-Pratt — all matches in O(n + m).
    public static List<int> KmpSearch(string text, string pattern)
    {
        var lps = new int[pattern.Length]; // longest proper prefix that is also a suffix
        for (int i = 1, len = 0; i < pattern.Length;)
        {
            if (pattern[i] == pattern[len]) lps[i++] = ++len;
            else if (len > 0) len = lps[len - 1];
            else lps[i++] = 0;
        }

        var matches = new List<int>();
        for (int i = 0, j = 0; i < text.Length;)
        {
            if (text[i] == pattern[j])
            {
                i++; j++;
                if (j == pattern.Length) { matches.Add(i - j); j = lps[j - 1]; }
            }
            else if (j > 0) j = lps[j - 1];
            else i++;
        }
        return matches;
    }

    // Rabin-Karp — rolling hash; average O(n + m).
    public static List<int> RabinKarp(string text, string pattern)
    {
        const long Base = 256, Mod = 1_000_000_007;
        int m = pattern.Length, n = text.Length;
        var matches = new List<int>();
        if (m > n) return matches;

        long high = 1, ph = 0, th = 0;
        for (int i = 0; i < m - 1; i++) high = high * Base % Mod;
        for (int i = 0; i < m; i++) { ph = (ph * Base + pattern[i]) % Mod; th = (th * Base + text[i]) % Mod; }

        for (int i = 0; i <= n - m; i++)
        {
            if (ph == th && string.CompareOrdinal(text, i, pattern, 0, m) == 0) matches.Add(i);
            if (i < n - m)
            {
                th = (th - text[i] * high % Mod + Mod) % Mod;
                th = (th * Base + text[i + m]) % Mod;
            }
        }
        return matches;
    }

    public static bool IsPalindrome(string s)
    {
        int l = 0, r = s.Length - 1;
        while (l < r)
        {
            if (!char.IsLetterOrDigit(s[l])) { l++; continue; }
            if (!char.IsLetterOrDigit(s[r])) { r--; continue; }
            if (char.ToLowerInvariant(s[l++]) != char.ToLowerInvariant(s[r--])) return false;
        }
        return true;
    }

    public static bool IsAnagram(string a, string b)
    {
        if (a.Length != b.Length) return false;
        Span<int> counts = stackalloc int[26];
        foreach (var c in a) counts[c - 'a']++;
        foreach (var c in b) if (--counts[c - 'a'] < 0) return false;
        return true;
    }
}
