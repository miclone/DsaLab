// PART 4 — Searching
using System.Buffers;
using System.Text;

namespace Dsa;

public static class Searching
{
    public static int Linear(int[] a, int target)
    {
        for (int i = 0; i < a.Length; i++) if (a[i] == target) return i;
        return -1;
    }

    // O(log n) on sorted input.
    public static int Binary(int[] a, int target)
    {
        int lo = 0, hi = a.Length - 1;
        while (lo <= hi)
        {
            int mid = lo + (hi - lo) / 2; // avoids overflow of (lo + hi)
            if (a[mid] == target) return mid;
            if (a[mid] < target) lo = mid + 1; else hi = mid - 1;
        }
        return -1;
    }

    // First index with a[i] >= target.
    public static int LowerBound(int[] a, int target)
    {
        int lo = 0, hi = a.Length;
        while (lo < hi) { int m = (lo + hi) / 2; if (a[m] < target) lo = m + 1; else hi = m; }
        return lo;
    }

    // First index with a[i] > target.
    public static int UpperBound(int[] a, int target)
    {
        int lo = 0, hi = a.Length;
        while (lo < hi) { int m = (lo + hi) / 2; if (a[m] <= target) lo = m + 1; else hi = m; }
        return lo;
    }

    // Finds the maximum of a unimodal function.
    public static double TernaryMax(Func<double, double> f, double lo, double hi)
    {
        for (int i = 0; i < 200; i++)
        {
            double m1 = lo + (hi - lo) / 3, m2 = hi - (hi - lo) / 3;
            if (f(m1) < f(m2)) lo = m1; else hi = m2;
        }
        return (lo + hi) / 2;
    }
}
