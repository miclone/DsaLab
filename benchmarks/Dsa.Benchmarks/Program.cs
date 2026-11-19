using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

// Pick benchmarks from a menu, or filter from the command line, e.g.
//   dotnet run -c Release -- --filter *Sorting*
//   dotnet run -c Release -- --filter *Collection*
//   dotnet run -c Release -- --filter *Quick* *Radix* *ArraySort*   (skips the slow O(n²) sorts)
BenchmarkSwitcher.FromAssembly(typeof(SortingBenchmarks).Assembly).Run(args);

/// <summary>
/// Compares every sort on random input. Each call copies one of 64 different
/// random arrays into a working buffer, cycling through them.
///
/// Why 64 arrays and not one? If the same array is sorted thousands of times in a
/// row, the CPU's branch predictor memorises the pattern of if/else outcomes and
/// comparison-based sorts look unrealistically fast (the first version of this
/// benchmark showed exactly that at N = 1,000). Cycling through many different
/// inputs gives the predictor far too much to memorise, like real-world data.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
public class SortingBenchmarks
{
    const int InputCount = 64; // power of two so "& (InputCount - 1)" works as a cheap modulo
    int[][] _sources = [];
    int[] _work = [];
    int _next;

    // O(n²) sorts get slow quickly: 10,000 items is ~50 million comparisons.
    [Params(1_000, 10_000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _sources = Enumerable.Range(0, InputCount)
            .Select(i => Enumerable.Range(0, N).Select(_ => rng.Next(0, 1_000_000)).ToArray())
            .ToArray();
        _work = new int[N];
    }

    int[] Fresh()
    {
        Array.Copy(_sources[_next++ & (InputCount - 1)], _work, N);
        return _work;
    }

    // Measures only the copy, so you can see how much of each result is setup cost.
    [Benchmark] public int[] CopyOnly_Overhead() => Fresh();

    [Benchmark(Baseline = true)] public void BuiltIn_ArraySort() => Array.Sort(Fresh());
    [Benchmark] public void Quick() => Sorting.QuickSort(Fresh());
    [Benchmark] public void QuickSimple() => Sorting.QuickSortSimple(Fresh());
    [Benchmark] public void QuickAdaptive() => Sorting.QuickSortAdaptive(Fresh());
    [Benchmark] public void Merge() => Sorting.MergeSort(Fresh());
    [Benchmark] public void Heap() => Sorting.HeapSort(Fresh());
    [Benchmark] public void Shell() => Sorting.ShellSort(Fresh());
    [Benchmark] public void Radix() => Sorting.RadixSort(Fresh());
    [Benchmark] public void RadixBase256() => Sorting.RadixSortBase256(Fresh());
    [Benchmark] public void RadixBase256FourScans() => Sorting.RadixSortBase256FourScans(Fresh());
    [Benchmark] public void Counting() => Sorting.CountingSort(Fresh());
    [Benchmark] public void Insertion() => Sorting.InsertionSort(Fresh());
    [Benchmark] public void Selection() => Sorting.SelectionSort(Fresh());
    [Benchmark] public void Bubble() => Sorting.BubbleSort(Fresh());
}

/// <summary>
/// The case Counting sort is designed for: lots of items, small range of values
/// (think ages, exam scores, star ratings). Here the values are 0–999, so Counting
/// sort only needs 1,000 counters (4 KB) instead of the 1,000,000 (4 MB) it had to
/// allocate in SortingBenchmarks.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
public class SmallRangeSortingBenchmarks
{
    const int InputCount = 64;
    int[][] _sources = [];
    int[] _work = [];
    int _next;

    [Params(10_000, 100_000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _sources = Enumerable.Range(0, InputCount)
            .Select(i => Enumerable.Range(0, N).Select(_ => rng.Next(0, 1_000)).ToArray())
            .ToArray();
        _work = new int[N];
    }

    int[] Fresh()
    {
        Array.Copy(_sources[_next++ & (InputCount - 1)], _work, N);
        return _work;
    }

    [Benchmark] public int[] CopyOnly_Overhead() => Fresh();
    [Benchmark(Baseline = true)] public void BuiltIn_ArraySort() => Array.Sort(Fresh());
    [Benchmark] public void Quick() => Sorting.QuickSort(Fresh());               // three-way partition
    [Benchmark] public void QuickSimple() => Sorting.QuickSortSimple(Fresh());   // original Lomuto
    [Benchmark] public void QuickAdaptive() => Sorting.QuickSortAdaptive(Fresh()); // pdqsort-style duplicate check
    [Benchmark] public void Radix() => Sorting.RadixSort(Fresh());               // ArrayPool, no allocations
    [Benchmark] public void RadixAllocating() => Sorting.RadixSortAllocating(Fresh()); // original
    [Benchmark] public void RadixBase256() => Sorting.RadixSortBase256(Fresh());       // one counting read
    [Benchmark] public void RadixBase256FourScans() => Sorting.RadixSortBase256FourScans(Fresh()); // previous version
    [Benchmark] public void Counting() => Sorting.CountingSort(Fresh());
}

/// <summary>Hand-written structures against their built-in .NET equivalents.</summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class CollectionBenchmarks
{
    int[] _values = [];

    [Params(10_000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _values = Enumerable.Range(0, N).Select(_ => rng.Next()).ToArray();
    }

    // ---- Heap: push N, pop N ----------------------------------------------
    [Benchmark(Baseline = true), BenchmarkCategory("Heap")]
    public int BuiltIn_PriorityQueue()
    {
        var pq = new PriorityQueue<int, int>();
        foreach (var v in _values) pq.Enqueue(v, v);
        int last = 0;
        while (pq.Count > 0) last = pq.Dequeue();
        return last;
    }

    [Benchmark, BenchmarkCategory("Heap")]
    public int Custom_MinHeap()
    {
        var heap = new MinHeap<int>();
        foreach (var v in _values) heap.Push(v);
        int last = 0;
        while (heap.Count > 0) last = heap.Pop();
        return last;
    }

    // ---- Hash map: insert N, look up N ------------------------------------
    [Benchmark(Baseline = true), BenchmarkCategory("HashMap")]
    public int BuiltIn_Dictionary()
    {
        var map = new Dictionary<int, int>();
        foreach (var v in _values) map[v] = v;
        int sum = 0;
        foreach (var v in _values) sum += map[v];
        return sum;
    }

    [Benchmark, BenchmarkCategory("HashMap")]
    public int Custom_SimpleHashMap()
    {
        var map = new SimpleHashMap<int, int>();
        foreach (var v in _values) map.Put(v, v);
        int sum = 0;
        foreach (var v in _values) sum += map.Get(v);
        return sum;
    }

    // ---- Stack: push N, pop N ---------------------------------------------
    [Benchmark(Baseline = true), BenchmarkCategory("Stack")]
    public int BuiltIn_Stack()
    {
        var stack = new Stack<int>();
        foreach (var v in _values) stack.Push(v);
        int last = 0;
        while (stack.Count > 0) last = stack.Pop();
        return last;
    }

    [Benchmark, BenchmarkCategory("Stack")]
    public int Custom_ArrayStack()
    {
        var stack = new ArrayStack<int>();
        foreach (var v in _values) stack.Push(v);
        int last = 0;
        while (stack.Count > 0) last = stack.Pop();
        return last;
    }

    // ---- Queue: enqueue N, dequeue N --------------------------------------
    [Benchmark(Baseline = true), BenchmarkCategory("Queue")]
    public int BuiltIn_Queue()
    {
        var queue = new Queue<int>();
        foreach (var v in _values) queue.Enqueue(v);
        int last = 0;
        while (queue.Count > 0) last = queue.Dequeue();
        return last;
    }

    [Benchmark, BenchmarkCategory("Queue")]
    public int Custom_CircularQueue()
    {
        var queue = new CircularQueue<int>();
        foreach (var v in _values) queue.Enqueue(v);
        int last = 0;
        while (queue.Count > 0) last = queue.Dequeue();
        return last;
    }

    // ---- Balanced tree: insert N (SortedSet is a red-black tree) -----------
    [Benchmark(Baseline = true), BenchmarkCategory("BalancedTree")]
    public int BuiltIn_SortedSet_InsertOnly()
    {
        var set = new SortedSet<int>();
        foreach (var v in _values) set.Add(v);
        return set.Count;
    }

    [Benchmark, BenchmarkCategory("BalancedTree")]
    public int Custom_AvlTree_InsertOnly()
    {
        var avl = new AvlTree<int>();
        foreach (var v in _values) avl.Insert(v);
        return avl.Height;
    }
}

/// <summary>Shortest-path algorithms on the same random connected graph.</summary>
[MemoryDiagnoser]
public class GraphBenchmarks
{
    Graph _graph = new(1, false);

    [Params(200)]
    public int V;

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _graph = new Graph(V, directed: false);
        for (int i = 1; i < V; i++) _graph.AddEdge(i - 1, i, rng.Next(1, 100));
        for (int i = 0; i < V * 4; i++)
        {
            int a = rng.Next(V), b = rng.Next(V);
            if (a != b) _graph.AddEdge(a, b, rng.Next(1, 100));
        }
    }

    [Benchmark(Baseline = true)] public int[] Dijkstra() => GraphAlgorithms.Dijkstra(_graph, 0);
    [Benchmark] public int[]? BellmanFord() => GraphAlgorithms.BellmanFord(_graph, 0);
    [Benchmark] public int[,] FloydWarshall_AllPairs() => GraphAlgorithms.FloydWarshall(_graph);
    [Benchmark] public int Kruskal() => GraphAlgorithms.KruskalMst(_graph);
    [Benchmark] public int Prim() => GraphAlgorithms.PrimMst(_graph);
}