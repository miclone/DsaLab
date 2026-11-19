// PART 3 — Sorting
using System.Buffers;
using System.Text;

namespace Dsa;

public static class Sorting
{
    // O(n²) — swap adjacent out-of-order pairs; stops early if already sorted.
    public static void BubbleSort(int[] a)
    {
        for (int i = 0; i < a.Length - 1; i++)
        {
            bool swapped = false;
            for (int j = 0; j < a.Length - 1 - i; j++)
                if (a[j] > a[j + 1]) { (a[j], a[j + 1]) = (a[j + 1], a[j]); swapped = true; }
            if (!swapped) break;
        }
    }

    // O(n²) — repeatedly select the minimum of the unsorted part.
    public static void SelectionSort(int[] a)
    {
        for (int i = 0; i < a.Length - 1; i++)
        {
            int min = i;
            for (int j = i + 1; j < a.Length; j++) if (a[j] < a[min]) min = j;
            (a[i], a[min]) = (a[min], a[i]);
        }
    }

    // O(n²) worst, O(n) on nearly-sorted data. Stable.
    public static void InsertionSort(int[] a)
    {
        for (int i = 1; i < a.Length; i++)
        {
            int key = a[i], j = i - 1;
            while (j >= 0 && a[j] > key) { a[j + 1] = a[j]; j--; }
            a[j + 1] = key;
        }
    }

    // Insertion sort over shrinking gaps. ~O(n^1.3) in practice.
    public static void ShellSort(int[] a)
    {
        for (int gap = a.Length / 2; gap > 0; gap /= 2)
            for (int i = gap; i < a.Length; i++)
            {
                int temp = a[i], j = i;
                while (j >= gap && a[j - gap] > temp) { a[j] = a[j - gap]; j -= gap; }
                a[j] = temp;
            }
    }

    // O(n log n) always, O(n) extra space. Stable. Divide & conquer.
    public static void MergeSort(int[] a)
    {
        if (a.Length < 2) return;
        var buffer = new int[a.Length];
        MergeSort(a, buffer, 0, a.Length - 1);
    }

    static void MergeSort(int[] a, int[] buf, int lo, int hi)
    {
        if (lo >= hi) return;
        int mid = lo + (hi - lo) / 2;
        MergeSort(a, buf, lo, mid);
        MergeSort(a, buf, mid + 1, hi);
        int i = lo, j = mid + 1, k = lo;
        while (i <= mid && j <= hi) buf[k++] = a[i] <= a[j] ? a[i++] : a[j++];
        while (i <= mid) buf[k++] = a[i++];
        while (j <= hi) buf[k++] = a[j++];
        Array.Copy(buf, lo, a, lo, hi - lo + 1);
    }

    // O(n log n) average AND worst case. In-place. Three-way ("Dutch national flag")
    // partition: splits the range into  < pivot | == pivot | > pivot  so values equal
    // to the pivot are finished in one pass and never partitioned again. That keeps
    // arrays with many repeated values fast (all-equal input becomes one O(n) pass).
    // Also: median-of-three pivot; recurses only into the smaller side and loops on
    // the larger one, so stack depth stays O(log n).
    // Introsort safety net: some inputs (e.g. "organ pipe": 0,1,2..,2,1,0) fool
    // median-of-three every time and push plain quicksort towards O(n²). If the
    // recursion goes deeper than 2·log2(n), the remaining range is heap sorted
    // instead, which is always O(n log n). Array.Sort uses the same trick.
    public static void QuickSort(int[] a) => QuickSort3Way(a, 0, a.Length - 1, DepthLimit(a.Length));

    static void QuickSort3Way(int[] a, int lo, int hi, int depthLimit)
    {
        while (lo < hi)
        {
            if (depthLimit-- == 0) { HeapSortRange(a, lo, hi); return; }

            int pivot = MedianOfThree(a[lo], a[lo + (hi - lo) / 2], a[hi]);
            int lt = lo, i = lo, gt = hi;
            while (i <= gt)
            {
                if (a[i] < pivot) { (a[lt], a[i]) = (a[i], a[lt]); lt++; i++; }
                else if (a[i] > pivot) { (a[i], a[gt]) = (a[gt], a[i]); gt--; }
                else i++;
            }
            // Now a[lo..lt-1] < pivot, a[lt..gt] == pivot, a[gt+1..hi] > pivot.
            if (lt - lo < hi - gt)
            {
                QuickSort3Way(a, lo, lt - 1, depthLimit);
                lo = gt + 1;
            }
            else
            {
                QuickSort3Way(a, gt + 1, hi, depthLimit);
                hi = lt - 1;
            }
        }
    }

    // Adaptive quicksort: the duplicate trick from pdqsort ("pattern-defeating
    // quicksort", used by Go's standard library).
    // Normally it does a plain two-way partition (< pivot | pivot | >= pivot), which
    // is cheaper than three-way when values are all different. It only groups equal
    // values when it detects repeats, using this observation:
    //   every element in a[lo..hi] is >= a[lo - 1]  (a[lo - 1] is an earlier pivot
    //   or part of an already-placed block on the left).
    // So if the new pivot EQUALS a[lo - 1], the pivot is the smallest value in the
    // range, and one pass can move all copies of it to the front and drop them.
    // That costs one extra comparison per partition when values don't repeat.
    // Also: median-of-three pivot, smaller-side recursion, introsort depth limit.
    public static void QuickSortAdaptive(int[] a) => QuickSortAdaptive(a, 0, a.Length - 1, DepthLimit(a.Length));

    static void QuickSortAdaptive(int[] a, int lo, int hi, int depthLimit)
    {
        while (lo < hi)
        {
            int p = MedianOfThreeIndex(a, lo, lo + (hi - lo) / 2, hi);
            int pivot = a[p];

            if (lo > 0 && a[lo - 1] == pivot)
            {
                // pivot is the minimum of this range: gather all copies at the front.
                int eq = lo;
                for (int j = lo; j <= hi; j++)
                    if (a[j] == pivot) { (a[eq], a[j]) = (a[j], a[eq]); eq++; }
                lo = eq; // at least one copy (the pivot itself), so this always progresses
                continue;
            }

            if (depthLimit-- == 0) { HeapSortRange(a, lo, hi); return; }

            // Lomuto partition: a[lo..i-1] < pivot, a[i] == pivot, a[i+1..hi] >= pivot.
            (a[p], a[hi]) = (a[hi], a[p]);
            int i = lo;
            for (int j = lo; j < hi; j++)
                if (a[j] < pivot) { (a[i], a[j]) = (a[j], a[i]); i++; }
            (a[i], a[hi]) = (a[hi], a[i]);

            // Both sides keep the "everything >= a[lo - 1]" property:
            // the right side starts just after the pivot, which is <= all of it.
            if (i - lo < hi - i)
            {
                QuickSortAdaptive(a, lo, i - 1, depthLimit);
                lo = i + 1;
            }
            else
            {
                QuickSortAdaptive(a, i + 1, hi, depthLimit);
                hi = i - 1;
            }
        }
    }

    static int DepthLimit(int n) => n < 2 ? 0 : 2 * int.Log2(n);

    static int MedianOfThree(int x, int y, int z) =>
        Math.Max(Math.Min(x, y), Math.Min(Math.Max(x, y), z));

    // Index (i, j or k) of the median of a[i], a[j], a[k].
    static int MedianOfThreeIndex(int[] a, int i, int j, int k)
    {
        if (a[i] > a[j]) (i, j) = (j, i);   // now a[i] <= a[j]
        if (a[j] > a[k])
        {
            j = k;                          // median is the larger of a[i] and a[k]
            if (a[i] > a[j]) j = i;
        }
        return j;
    }

    // Heap sort of just a[lo..hi]; the introsort fallback.
    static void HeapSortRange(int[] a, int lo, int hi)
    {
        int n = hi - lo + 1;
        for (int i = n / 2 - 1; i >= 0; i--) SiftDownRange(a, lo, n, i);
        for (int end = n - 1; end > 0; end--)
        {
            (a[lo], a[lo + end]) = (a[lo + end], a[lo]);
            SiftDownRange(a, lo, end, 0);
        }
    }

    static void SiftDownRange(int[] a, int lo, int size, int i)
    {
        while (true)
        {
            int l = 2 * i + 1, r = l + 1, largest = i;
            if (l < size && a[lo + l] > a[lo + largest]) largest = l;
            if (r < size && a[lo + r] > a[lo + largest]) largest = r;
            if (largest == i) return;
            (a[lo + i], a[lo + largest]) = (a[lo + largest], a[lo + i]);
            i = largest;
        }
    }

    // The original version, kept for comparison in tests and benchmarks.
    // O(n log n) average, O(n²) worst. Lomuto partition, middle pivot.
    // Weakness: every value equal to the pivot goes to the same side, so arrays with
    // many repeated values degrade towards O(n²), and recursion can get very deep.
    public static void QuickSortSimple(int[] a) => QuickSortSimple(a, 0, a.Length - 1);
    static void QuickSortSimple(int[] a, int lo, int hi)
    {
        if (lo >= hi) return;
        int mid = lo + (hi - lo) / 2;
        (a[mid], a[hi]) = (a[hi], a[mid]);
        int pivot = a[hi], i = lo;
        for (int j = lo; j < hi; j++)
            if (a[j] < pivot) { (a[i], a[j]) = (a[j], a[i]); i++; }
        (a[i], a[hi]) = (a[hi], a[i]);
        QuickSortSimple(a, lo, i - 1);
        QuickSortSimple(a, i + 1, hi);
    }

    // O(n log n), in-place, not stable.
    public static void HeapSort(int[] a)
    {
        int n = a.Length;
        for (int i = n / 2 - 1; i >= 0; i--) SiftDown(a, n, i);
        for (int end = n - 1; end > 0; end--)
        {
            (a[0], a[end]) = (a[end], a[0]);
            SiftDown(a, end, 0);
        }
    }

    static void SiftDown(int[] a, int size, int i)
    {
        while (true)
        {
            int l = 2 * i + 1, r = l + 1, largest = i;
            if (l < size && a[l] > a[largest]) largest = l;
            if (r < size && a[r] > a[largest]) largest = r;
            if (largest == i) return;
            (a[i], a[largest]) = (a[largest], a[i]);
            i = largest;
        }
    }

    // O(n + k) where k = value range. Non-comparison sort for small integer ranges.
    public static void CountingSort(int[] a)
    {
        if (a.Length == 0) return;
        int min = a.Min(), max = a.Max();
        var count = new int[max - min + 1];
        foreach (var x in a) count[x - min]++;
        int idx = 0;
        for (int v = 0; v < count.Length; v++)
            while (count[v]-- > 0) a[idx++] = v + min;
    }

    // O(d·(n + 10)) for non-negative ints; sorts digit by digit (LSD).
    // Allocation-free: the output buffer is borrowed from ArrayPool and the digit
    // counts live on the stack. A fresh 100,000-item buffer would be 400 KB, which
    // goes on the Large Object Heap and forces expensive Gen2 garbage collections.
    public static void RadixSort(int[] a)
    {
        if (a.Length < 2) return;
        int max = 0;
        foreach (var x in a)
        {
            if (x < 0) throw new ArgumentException("RadixSort only supports non-negative integers.", nameof(a));
            if (x > max) max = x;
        }

        int[] rented = ArrayPool<int>.Shared.Rent(a.Length); // may be longer than a.Length
        try
        {
            Span<int> output = rented.AsSpan(0, a.Length);
            Span<int> count = stackalloc int[10];
            // long, not int: with values near int.MaxValue, exp * 10 would overflow an int.
            for (long exp = 1; max / exp > 0; exp *= 10)
            {
                count.Clear();
                foreach (var x in a) count[(int)(x / exp % 10)]++;
                for (int i = 1; i < 10; i++) count[i] += count[i - 1];
                for (int i = a.Length - 1; i >= 0; i--) output[--count[(int)(a[i] / exp % 10)]] = a[i];
                output.CopyTo(a);
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(rented);
        }
    }

    // LSD radix sort in base 256: how production radix sorts work.
    //  * Each pass sorts by one byte, extracted with a shift and a mask:
    //    (key >> shift) & 0xFF. No division at all, unlike base 10.
    //  * An int has 4 bytes, so there are at most 4 passes, and nothing can overflow.
    //  * Handles negative numbers: flipping the sign bit (x ^ 0x80000000, read as an
    //    unsigned number) maps int.MinValue..int.MaxValue onto 0..uint.MaxValue in
    //    the same order, so negatives sort before positives.
    //  * ONE read of the data counts all four bytes at once. This works because a
    //    pass only reorders the items, so the byte counts never change between
    //    passes. (Counting per pass would re-read the whole array four times.)
    //  * Skips passes where every item has the same byte (for values 0..999, the top
    //    two bytes are always 0, so only 2 of the 4 passes move anything).
    //  * Ping-pongs between the array and one pooled buffer instead of copying back
    //    after every pass. Allocation-free.
    // O(n) for one counting read + at most 4 moving passes. Stable, which LSD requires.
    public static void RadixSortBase256(int[] a)
    {
        int n = a.Length;
        if (n < 2) return;

        // counts[pass * 256 + byteValue]; 4 KB on the stack.
        Span<int> counts = stackalloc int[4 * 256];
        foreach (int x in a)
        {
            uint k = (uint)x ^ 0x8000_0000u;
            counts[(int)(k & 0xFF)]++;
            counts[256 + (int)((k >> 8) & 0xFF)]++;
            counts[512 + (int)((k >> 16) & 0xFF)]++;
            counts[768 + (int)(k >> 24)]++;
        }

        int[] rented = ArrayPool<int>.Shared.Rent(n);
        try
        {
            Span<int> src = a;
            Span<int> dst = rented.AsSpan(0, n);
            bool dataIsInOriginalArray = true;

            for (int pass = 0; pass < 4; pass++)
            {
                int shift = pass * 8;
                Span<int> count = counts.Slice(pass * 256, 256);

                // Every item has the same byte here, so this pass would change nothing.
                if (count[ByteKey(a[0], shift)] == n) continue;

                // Turn counts into starting positions (exclusive prefix sum).
                int position = 0;
                for (int b = 0; b < 256; b++)
                {
                    int c = count[b];
                    count[b] = position;
                    position += c;
                }

                // Scan forwards so equal bytes keep their order (stable).
                foreach (int x in src) dst[count[ByteKey(x, shift)]++] = x;

                Span<int> temp = src; src = dst; dst = temp;
                dataIsInOriginalArray = !dataIsInOriginalArray;
            }

            // After an odd number of real passes the sorted data is in the pooled buffer.
            if (!dataIsInOriginalArray) src.CopyTo(a);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(rented);
        }
    }

    // Previous base-256 version, kept for comparison: it re-reads the whole array
    // to count bytes at the start of every pass, even passes it ends up skipping.
    // LSD radix sort in base 256.
    //  * Each pass sorts by one byte, extracted with a shift and a mask:
    //    (key >> shift) & 0xFF. No division at all, unlike base 10.
    //  * An int has 4 bytes, so there are at most 4 passes, and nothing can overflow.
    //  * Handles negative numbers: flipping the sign bit (x ^ 0x80000000, read as an
    //    unsigned number) maps int.MinValue..int.MaxValue onto 0..uint.MaxValue in
    //    the same order, so negatives sort before positives.
    //  * Skips passes where every item has the same byte (for values 0..999, the top
    //    two bytes are always 0, so only 2 of the 4 passes do any moving).
    //  * Ping-pongs between the array and one pooled buffer instead of copying back
    //    after every pass. Allocation-free.
    // O(4·n + 4·256) for any int input. Stable within each pass, which LSD requires.
    public static void RadixSortBase256FourScans(int[] a)
    {
        int n = a.Length;
        if (n < 2) return;

        int[] rented = ArrayPool<int>.Shared.Rent(n);
        try
        {
            Span<int> src = a;
            Span<int> dst = rented.AsSpan(0, n);
            Span<int> count = stackalloc int[256];
            bool dataIsInOriginalArray = true;

            for (int shift = 0; shift < 32; shift += 8)
            {
                count.Clear();
                foreach (int x in src) count[ByteKey(x, shift)]++;

                // Every item has the same byte here, so this pass would change nothing.
                if (count[ByteKey(src[0], shift)] == n) continue;

                // Turn counts into starting positions (exclusive prefix sum).
                int position = 0;
                for (int b = 0; b < 256; b++)
                {
                    int c = count[b];
                    count[b] = position;
                    position += c;
                }

                // Scan forwards so equal bytes keep their order (stable).
                foreach (int x in src) dst[count[ByteKey(x, shift)]++] = x;

                Span<int> temp = src; src = dst; dst = temp;
                dataIsInOriginalArray = !dataIsInOriginalArray;
            }

            // After an odd number of real passes the sorted data is in the pooled buffer.
            if (!dataIsInOriginalArray) src.CopyTo(a);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(rented);
        }
    }

    static int ByteKey(int x, int shift) => (int)((((uint)x ^ 0x8000_0000u) >> shift) & 0xFF);

    // The original version, kept for comparison: allocates a new output buffer and
    // a new count array on every pass. (Also fixed: exp is now a long, see above.)
    public static void RadixSortAllocating(int[] a)
    {
        if (a.Length == 0) return;
        int max = a.Max();
        var output = new int[a.Length];
        for (long exp = 1; max / exp > 0; exp *= 10)
        {
            var count = new int[10];
            foreach (var x in a) count[x / exp % 10]++;
            for (int i = 1; i < 10; i++) count[i] += count[i - 1];
            for (int i = a.Length - 1; i >= 0; i--) output[--count[a[i] / exp % 10]] = a[i];
            Array.Copy(output, a, a.Length);
        }
    }
}