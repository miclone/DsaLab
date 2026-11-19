namespace Dsa.Tests;

public class SortingTests
{
    static readonly Dictionary<string, Action<int[]>> Sorts = new()
    {
        ["Bubble"] = Sorting.BubbleSort,
        ["Selection"] = Sorting.SelectionSort,
        ["Insertion"] = Sorting.InsertionSort,
        ["Shell"] = Sorting.ShellSort,
        ["Merge"] = Sorting.MergeSort,
        ["Quick"] = Sorting.QuickSort,
        ["QuickSimple"] = Sorting.QuickSortSimple,
        ["QuickAdaptive"] = Sorting.QuickSortAdaptive,
        ["Heap"] = Sorting.HeapSort,
        ["Counting"] = Sorting.CountingSort,
        ["Radix"] = Sorting.RadixSort, // non-negative integers only
        ["RadixAllocating"] = Sorting.RadixSortAllocating, // non-negative integers only
        ["RadixBase256"] = Sorting.RadixSortBase256,       // handles negatives too
        ["RadixBase256FourScans"] = Sorting.RadixSortBase256FourScans,
    };

    static readonly Dictionary<string, Func<int[]>> Inputs = new()
    {
        ["empty"] = () => [],
        ["single"] = () => [42],
        ["sorted"] = () => Enumerable.Range(0, 300).ToArray(),
        ["reversed"] = () => Enumerable.Range(0, 300).Reverse().ToArray(),
        ["all equal"] = () => Enumerable.Repeat(7, 100).ToArray(),
        ["many duplicates"] = () => RandomArray(500, 0, 10, seed: 1),
        ["random"] = () => RandomArray(1000, 0, 100_000, seed: 2),
    };

    public static TheoryData<string, string> AllCombinations()
    {
        var data = new TheoryData<string, string>();
        foreach (var sort in Sorts.Keys)
            foreach (var input in Inputs.Keys) data.Add(sort, input);
        return data;
    }

    public static TheoryData<string> SortsThatHandleNegatives()
    {
        var data = new TheoryData<string>();
        foreach (var name in Sorts.Keys.Where(k => k is not ("Radix" or "RadixAllocating"))) data.Add(name);
        return data;
    }

    static int[] RandomArray(int n, int min, int max, int seed)
    {
        var rng = new Random(seed);
        return Enumerable.Range(0, n).Select(_ => rng.Next(min, max)).ToArray();
    }

    [Theory]
    [MemberData(nameof(AllCombinations))]
    public void Sort_MatchesArraySort(string sortName, string inputName)
    {
        var input = Inputs[inputName]();
        var expected = (int[])input.Clone();
        Array.Sort(expected);

        Sorts[sortName](input);

        Assert.Equal(expected, input);
    }

    [Theory]
    [MemberData(nameof(SortsThatHandleNegatives))]
    public void Sort_HandlesNegativeNumbers(string sortName)
    {
        var input = RandomArray(500, -1000, 1000, seed: 3);
        var expected = input.Order().ToArray();

        Sorts[sortName](input);

        Assert.Equal(expected, input);
    }
}

public class SearchingTests
{
    [Fact]
    public void Linear_FindsFirstOccurrence()
    {
        int[] a = [4, 2, 7, 2];
        Assert.Equal(1, Searching.Linear(a, 2));
        Assert.Equal(-1, Searching.Linear(a, 9));
    }

    [Fact]
    public void Binary_FindsEveryElement_AndRejectsMissing()
    {
        var a = Enumerable.Range(0, 500).Select(i => i * 2).ToArray(); // even numbers
        for (int i = 0; i < a.Length; i++) Assert.Equal(i, Searching.Binary(a, a[i]));
        Assert.Equal(-1, Searching.Binary(a, 3));
        Assert.Equal(-1, Searching.Binary(a, -1));
        Assert.Equal(-1, Searching.Binary(a, 10_000));
        Assert.Equal(-1, Searching.Binary([], 1));
    }

    [Fact]
    public void LowerAndUpperBound_MatchBruteForce()
    {
        var rng = new Random(5);
        for (int trial = 0; trial < 50; trial++)
        {
            var a = Enumerable.Range(0, rng.Next(0, 60)).Select(_ => rng.Next(0, 20)).Order().ToArray();
            for (int target = -1; target <= 21; target++)
            {
                int lower = Array.FindIndex(a, x => x >= target); if (lower < 0) lower = a.Length;
                int upper = Array.FindIndex(a, x => x > target); if (upper < 0) upper = a.Length;
                Assert.Equal(lower, Searching.LowerBound(a, target));
                Assert.Equal(upper, Searching.UpperBound(a, target));
            }
        }
    }

    [Theory]
    [InlineData(2.0)]
    [InlineData(-3.5)]
    [InlineData(7.25)]
    public void TernaryMax_FindsPeak(double peak)
    {
        double x = Searching.TernaryMax(v => -(v - peak) * (v - peak), -10, 10);
        Assert.Equal(peak, x, precision: 6);
    }
}