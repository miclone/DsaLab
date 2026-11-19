using System.Diagnostics;

namespace Dsa.Tests;

/// <summary>
/// Tests for the two improvements suggested by the benchmarks:
/// three-way partitioning in QuickSort and the allocation-free RadixSort.
/// </summary>
public class QuickSortThreeWayTests
{
    static void AssertSortedLikeArraySort(int[] input, Action<int[]> sort)
    {
        var expected = (int[])input.Clone();
        Array.Sort(expected);
        sort(input);
        Assert.Equal(expected, input);
    }

    [Fact]
    public void AllEqual_OneMillion_IsFastAndDoesNotOverflowTheStack()
    {
        // The simple Lomuto version would recurse ~1,000,000 levels deep here and crash
        // the test run with a StackOverflowException, so it is deliberately not tested.
        var a = Enumerable.Repeat(7, 1_000_000).ToArray();

        var sw = Stopwatch.StartNew();
        Sorting.QuickSort(a);
        sw.Stop();

        Assert.All(a, x => Assert.Equal(7, x));
        Assert.True(sw.ElapsedMilliseconds < 2_000, $"took {sw.ElapsedMilliseconds} ms; three-way partition should make this one O(n) pass");
    }

    [Theory]
    [InlineData(2)]      // only 0 and 1
    [InlineData(4)]
    [InlineData(1_000)]  // the SmallRangeSortingBenchmarks case
    public void FewDistinctValues_LargeArray(int distinctValues)
    {
        var rng = new Random(distinctValues);
        var a = Enumerable.Range(0, 200_000).Select(_ => rng.Next(distinctValues)).ToArray();
        AssertSortedLikeArraySort(a, Sorting.QuickSort);
    }

    [Fact]
    public void AlreadySortedAndReversed_OneMillion()
    {
        // Median-of-three pivots keep these O(n log n);
        // recursing into the smaller side keeps the stack shallow.
        AssertSortedLikeArraySort(Enumerable.Range(0, 1_000_000).ToArray(), Sorting.QuickSort);
        AssertSortedLikeArraySort(Enumerable.Range(0, 1_000_000).Reverse().ToArray(), Sorting.QuickSort);
    }

    [Fact]
    public void OrganPipeAndSawtooth_Patterns()
    {
        var organPipe = Enumerable.Range(0, 50_000).Concat(Enumerable.Range(0, 50_000).Reverse()).ToArray();
        var sawtooth = Enumerable.Range(0, 100_000).Select(i => i % 100).ToArray();
        AssertSortedLikeArraySort(organPipe, Sorting.QuickSort);
        AssertSortedLikeArraySort(sawtooth, Sorting.QuickSort);
    }

    [Fact]
    public void ExtremeValues()
    {
        int[] a = [int.MaxValue, int.MinValue, 0, -1, 1, int.MaxValue, int.MinValue];
        AssertSortedLikeArraySort(a, Sorting.QuickSort);
    }

    [Fact]
    public void BothVersions_AgreeOnRandomInputs()
    {
        var rng = new Random(123);
        for (int trial = 0; trial < 200; trial++)
        {
            var a = Enumerable.Range(0, rng.Next(0, 300)).Select(_ => rng.Next(-50, 50)).ToArray();
            var b = (int[])a.Clone();
            Sorting.QuickSort(a);
            Sorting.QuickSortSimple(b);
            Assert.Equal(b, a);
        }
    }
}

public class RadixSortPooledTests
{
    static int[] RandomNonNegative(int n, int max, int seed)
    {
        var rng = new Random(seed);
        return Enumerable.Range(0, n).Select(_ => rng.Next(max)).ToArray();
    }

    static long AllocatedBytes(Action action)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [Fact]
    public void RadixSort_DoesNotAllocate()
    {
        var a = RandomNonNegative(100_000, 1_000, seed: 1);
        Sorting.RadixSort((int[])a.Clone()); // warm-up: the pool creates its buffer once

        long allocated = AllocatedBytes(() => Sorting.RadixSort(a));

        Assert.True(allocated < 1_024, $"allocated {allocated:N0} bytes; expected ~0 with ArrayPool + stackalloc");
    }

    [Fact]
    public void RadixSortAllocating_DoesAllocate_ForContrast()
    {
        var a = RandomNonNegative(100_000, 1_000, seed: 2);
        Sorting.RadixSortAllocating((int[])a.Clone());

        long allocated = AllocatedBytes(() => Sorting.RadixSortAllocating(a));

        Assert.True(allocated >= 100_000 * sizeof(int), $"allocated only {allocated:N0} bytes");
    }

    [Fact]
    public void RadixSort_RepeatedCalls_ReusePooledBufferCorrectly()
    {
        // The pooled buffer comes back with old data in it and may be longer than
        // needed; results must not depend on either.
        var rng = new Random(3);
        foreach (var n in new[] { 100_000, 10, 50_000, 1, 100_000, 7 })
        {
            var a = Enumerable.Range(0, n).Select(_ => rng.Next(1_000_000)).ToArray();
            var expected = a.Order().ToArray();
            Sorting.RadixSort(a);
            Assert.Equal(expected, a);
        }
    }

    [Fact]
    public void RadixSort_HandlesValuesNearIntMaxValue()
    {
        // With an int exponent, exp * 10 overflowed after 1,000,000,000.
        int[] a = [int.MaxValue, 0, 5, int.MaxValue - 1, 1_000_000_000, 2_000_000_000];
        var expected = a.Order().ToArray();

        Sorting.RadixSort(a);
        Assert.Equal(expected, a);

        int[] b = [int.MaxValue, 0, 5, int.MaxValue - 1, 1_000_000_000, 2_000_000_000];
        Sorting.RadixSortAllocating(b);
        Assert.Equal(expected, b);
    }

    [Fact]
    public void RadixSort_RejectsNegativeNumbers() =>
        Assert.Throws<ArgumentException>(() => Sorting.RadixSort([3, -1, 2]));

    [Fact]
    public void BothVersions_AgreeOnRandomInputs()
    {
        for (int seed = 0; seed < 50; seed++)
        {
            var a = RandomNonNegative(1_000, 100_000, seed);
            var b = (int[])a.Clone();
            Sorting.RadixSort(a);
            Sorting.RadixSortAllocating(b);
            Assert.Equal(b, a);
        }
    }
}

public class RadixSortBase256Tests
{
    static void AssertSortsLikeArraySort(int[] input)
    {
        var expected = (int[])input.Clone();
        Array.Sort(expected);
        Sorting.RadixSortBase256(input);
        Assert.Equal(expected, input);
    }

    static int[] RandomInRange(int n, int min, int max, int seed)
    {
        var rng = new Random(seed);
        return Enumerable.Range(0, n).Select(_ => rng.Next(min, max)).ToArray();
    }

    [Fact]
    public void ExtremeAndNegativeValues()
    {
        // The sign-bit flip must put int.MinValue first and int.MaxValue last.
        AssertSortsLikeArraySort([int.MaxValue, int.MinValue, 0, -1, 1, -256, 256, int.MinValue + 1, int.MaxValue - 1, -1]);
    }

    [Fact]
    public void FullIntRange_Random()
    {
        var rng = new Random(77);
        var a = Enumerable.Range(0, 100_000).Select(_ => rng.Next(int.MinValue, int.MaxValue)).ToArray();
        AssertSortsLikeArraySort(a);
    }

    // Different value ranges make a different number of byte positions vary, so the
    // sort does 1, 2, 3 or 4 real passes. An odd count leaves the result in the pooled
    // buffer and exercises the copy-back at the end.
    [Theory]
    [InlineData(0, 256)]                 // 1 pass  (only the lowest byte varies)
    [InlineData(0, 1_000)]               // 2 passes (the SmallRange benchmark case)
    [InlineData(0, 1 << 20)]             // 3 passes
    [InlineData(0, int.MaxValue)]        // 4 passes (sign-flipped top byte varies)
    [InlineData(-1_000, 1_000)]          // crosses zero: top byte differs for negatives
    [InlineData(int.MinValue, -1)]       // negatives only
    public void DifferentNumbersOfPasses(int min, int max) =>
        AssertSortsLikeArraySort(RandomInRange(20_000, min, max, seed: min ^ max));

    [Fact]
    public void ValuesThatDifferOnlyInTheTopByte()
    {
        // The three lower passes are all skipped; only the top byte decides the order.
        var a = Enumerable.Range(0, 100).Select(i => (i * 37 % 100) << 24).ToArray();
        AssertSortsLikeArraySort(a);
    }

    [Fact]
    public void AllEqual_SkipsEveryPass() => AssertSortsLikeArraySort(Enumerable.Repeat(-5, 10_000).ToArray());

    [Fact]
    public void TinyInputs()
    {
        AssertSortsLikeArraySort([]);
        AssertSortsLikeArraySort([1]);
        AssertSortsLikeArraySort([2, 1]);
    }

    [Fact]
    public void DoesNotAllocate()
    {
        var a = RandomInRange(100_000, int.MinValue, int.MaxValue, seed: 5);
        Sorting.RadixSortBase256((int[])a.Clone()); // warm-up

        long before = GC.GetAllocatedBytesForCurrentThread();
        Sorting.RadixSortBase256(a);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.True(allocated < 1_024, $"allocated {allocated:N0} bytes");
    }

    [Fact]
    public void AgreesWithBase10Radix_OnNonNegativeInputs()
    {
        for (int seed = 0; seed < 50; seed++)
        {
            var a = RandomInRange(2_000, 0, 1_000_000, seed);
            var b = (int[])a.Clone();
            Sorting.RadixSortBase256(a);
            Sorting.RadixSort(b);
            Assert.Equal(b, a);
        }
    }
}

/// <summary>
/// Hard inputs for the two robust quicksorts (three-way and adaptive). Each one runs
/// against both. The timing limits are far above what O(n log n) needs even in a
/// Debug build, but far below what an O(n²) slide would take, so a broken depth
/// limit or duplicate check shows up as a failure instead of a silent slowdown.
/// </summary>
public class RobustQuickSortTests
{
    static readonly Dictionary<string, Action<int[]>> Sorts = new()
    {
        ["ThreeWay"] = Sorting.QuickSort,
        ["Adaptive"] = Sorting.QuickSortAdaptive,
    };

    public static TheoryData<string> SortNames()
    {
        var data = new TheoryData<string>();
        foreach (var name in Sorts.Keys) data.Add(name);
        return data;
    }

    static void AssertSortsQuickly(string sortName, int[] input, int maxMilliseconds)
    {
        var expected = (int[])input.Clone();
        Array.Sort(expected);

        var sw = Stopwatch.StartNew();
        Sorts[sortName](input);
        sw.Stop();

        Assert.Equal(expected, input);
        Assert.True(sw.ElapsedMilliseconds < maxMilliseconds,
            $"{sortName} took {sw.ElapsedMilliseconds} ms (limit {maxMilliseconds} ms): likely an O(n²) slide");
    }

    // Organ pipe (0,1,2,...,2,1,0) fools median-of-three on every partition.
    // Before the introsort depth limit, simulations showed 35-80x the normal work
    // at 20,000 items and growing with n.
    [Theory]
    [MemberData(nameof(SortNames))]
    public void OrganPipe_200k_UsesDepthLimitFallback(string sortName)
    {
        var a = Enumerable.Range(0, 100_000).Concat(Enumerable.Range(0, 100_000).Reverse()).ToArray();
        AssertSortsQuickly(sortName, a, maxMilliseconds: 3_000);
    }

    [Theory]
    [MemberData(nameof(SortNames))]
    public void AllEqual_OneMillion(string sortName) =>
        AssertSortsQuickly(sortName, Enumerable.Repeat(42, 1_000_000).ToArray(), maxMilliseconds: 2_000);

    [Theory]
    [MemberData(nameof(SortNames))]
    public void SortedAndReversed_OneMillion(string sortName)
    {
        AssertSortsQuickly(sortName, Enumerable.Range(0, 1_000_000).ToArray(), maxMilliseconds: 3_000);
        AssertSortsQuickly(sortName, Enumerable.Range(0, 1_000_000).Reverse().ToArray(), maxMilliseconds: 3_000);
    }

    [Theory]
    [MemberData(nameof(SortNames))]
    public void ManyDuplicates_200k(string sortName)
    {
        var rng = new Random(21);
        AssertSortsQuickly(sortName, Enumerable.Range(0, 200_000).Select(_ => rng.Next(3)).ToArray(), maxMilliseconds: 2_000);
    }

    [Theory]
    [MemberData(nameof(SortNames))]
    public void Sawtooth_200k(string sortName) =>
        AssertSortsQuickly(sortName, Enumerable.Range(0, 200_000).Select(i => i % 100).ToArray(), maxMilliseconds: 3_000);

    [Fact]
    public void Adaptive_AgreesWithOtherQuickSorts_OnSmallRandomInputs()
    {
        // Small sizes and a tiny value range hit every branch: the duplicate skip,
        // the depth-limit fallback on short ranges, and ranges of 0, 1 and 2 items.
        var rng = new Random(99);
        for (int trial = 0; trial < 2_000; trial++)
        {
            var a = Enumerable.Range(0, rng.Next(0, 40)).Select(_ => rng.Next(-3, 4)).ToArray();
            var b = (int[])a.Clone();
            var c = (int[])a.Clone();
            Sorting.QuickSortAdaptive(a);
            Sorting.QuickSort(b);
            Sorting.QuickSortSimple(c);
            Assert.Equal(c, a);
            Assert.Equal(c, b);
        }
    }

    [Fact]
    public void Adaptive_ExtremeValues()
    {
        int[] a = [int.MaxValue, int.MinValue, 0, -1, 1, int.MaxValue, int.MinValue, 0];
        var expected = a.Order().ToArray();
        Sorting.QuickSortAdaptive(a);
        Assert.Equal(expected, a);
    }
}

public class RadixSortSingleReadTests
{
    [Fact]
    public void SingleRead_AgreesWithFourScanVersion()
    {
        var rng = new Random(31);
        foreach (var (min, max) in new[] { (0, 256), (0, 1_000), (0, 1 << 20), (int.MinValue, int.MaxValue), (-1_000, 1_000) })
        {
            for (int trial = 0; trial < 20; trial++)
            {
                var a = Enumerable.Range(0, rng.Next(0, 3_000)).Select(_ => rng.Next(min, max)).ToArray();
                var b = (int[])a.Clone();
                Sorting.RadixSortBase256(a);
                Sorting.RadixSortBase256FourScans(b);
                Assert.Equal(b, a);
            }
        }
    }

    [Fact]
    public void FourScanVersion_IsAlsoAllocationFree()
    {
        var rng = new Random(32);
        var a = Enumerable.Range(0, 100_000).Select(_ => rng.Next()).ToArray();
        Sorting.RadixSortBase256FourScans((int[])a.Clone());

        long before = GC.GetAllocatedBytesForCurrentThread();
        Sorting.RadixSortBase256FourScans(a);
        Assert.True(GC.GetAllocatedBytesForCurrentThread() - before < 1_024);
    }
}