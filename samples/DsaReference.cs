// ============================================================================
//  Data Structures & Algorithms in C# / .NET 10 — runnable reference
//
//  Run with the .NET 10 SDK (file-based apps, no .csproj needed):
//      dotnet run DsaReference.cs
//
//  Contents
//   PART 1  Built-in .NET collections (what to use in real code)
//   PART 2  Hand-written data structures (how they work)
//           LinkedList, Stack, Queue, MinHeap, BST, AVL tree, Trie,
//           Union-Find, Graph, LRU cache, Segment tree, Fenwick tree, HashMap
//   PART 3  Sorting       bubble, selection, insertion, merge, quick, heap,
//                         counting, radix, shell
//   PART 4  Searching     linear, binary, lower/upper bound, ternary
//   PART 5  Graphs        BFS, DFS, Dijkstra, Bellman-Ford, Floyd-Warshall,
//                         topological sort, Kruskal, Prim, cycle detection
//   PART 6  Dynamic prog. Fibonacci, 0/1 knapsack, LCS, LIS, edit distance,
//                         coin change, max subarray (Kadane)
//   PART 7  Strings       KMP, Rabin-Karp, palindrome, anagram
//   PART 8  Techniques    two pointers, sliding window, prefix sums,
//                         backtracking (permutations, subsets, N-Queens),
//                         greedy, bit manipulation, GCD/primes
// ============================================================================

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

Demo.Run();

static class Demo
{
    static void Section(string title) => Console.WriteLine($"\n===== {title} =====");

    public static void Run()
    {
        // -------------------------------------------------------------- PART 1
        Section("PART 1: Built-in collections");

        // List<T> — dynamic array. O(1) index, amortised O(1) Add, O(n) Insert/Remove.
        var list = new List<int> { 5, 3, 8 };
        list.Add(1); list.Sort();
        Console.WriteLine($"List<T>: {string.Join(",", list)}");

        // Dictionary<K,V> — hash table. O(1) average lookup/insert/remove.
        var dict = new Dictionary<string, int> { ["apple"] = 3, ["pear"] = 5 };
        dict.TryAdd("kiwi", 1);
        if (dict.TryGetValue("pear", out var pears)) Console.WriteLine($"Dictionary: pear={pears}");

        // HashSet<T> — unique items, O(1) Contains; set operations.
        var a = new HashSet<int> { 1, 2, 3 };
        a.IntersectWith([2, 3, 4]);
        Console.WriteLine($"HashSet intersect: {string.Join(",", a)}");

        // SortedSet<T> / SortedDictionary<K,V> — red-black trees, O(log n), kept in order.
        var sortedSet = new SortedSet<int> { 9, 1, 5 };
        Console.WriteLine($"SortedSet min={sortedSet.Min} max={sortedSet.Max}");
        var sortedDict = new SortedDictionary<string, int> { ["b"] = 2, ["a"] = 1 };
        Console.WriteLine($"SortedDictionary first key: {sortedDict.First().Key}");

        // SortedList<K,V> — sorted arrays; less memory, O(n) insert.
        // OrderedDictionary<K,V> (generic, .NET 9+) — keeps insertion order + index access.
        var ordered = new OrderedDictionary<string, int> { ["z"] = 26, ["a"] = 1 };
        Console.WriteLine($"OrderedDictionary index 0: {ordered.GetAt(0).Key}");

        // Stack<T> (LIFO) and Queue<T> (FIFO) — O(1) push/pop, enqueue/dequeue.
        var stack = new Stack<int>([1, 2, 3]);
        var queue = new Queue<int>([1, 2, 3]);
        Console.WriteLine($"Stack.Pop={stack.Pop()}  Queue.Dequeue={queue.Dequeue()}");

        // LinkedList<T> — doubly linked; O(1) insert/remove given a node.
        var linked = new LinkedList<int>([2, 3]);
        linked.AddFirst(1); linked.AddLast(4);
        Console.WriteLine($"LinkedList<T>: {string.Join("<->", linked)}");

        // PriorityQueue<TElement,TPriority> — binary min-heap. O(log n) enqueue/dequeue.
        var pq = new PriorityQueue<string, int>();
        pq.Enqueue("low", 10); pq.Enqueue("urgent", 1); pq.Enqueue("mid", 5);
        Console.WriteLine($"PriorityQueue first out: {pq.Dequeue()}");

        // Immutable collections — thread-safe snapshots; "changes" return new instances.
        var imm = ImmutableList.Create(1, 2, 3);
        var imm2 = imm.Add(4);
        Console.WriteLine($"Immutable: original={imm.Count} new={imm2.Count}");

        // Frozen collections (.NET 8+) — build once, read very fast.
        var frozen = new Dictionary<string, int> { ["x"] = 1, ["y"] = 2 }.ToFrozenDictionary();
        Console.WriteLine($"FrozenDictionary y={frozen["y"]}");

        // Concurrent collections — safe for multi-threaded access.
        var concurrent = new ConcurrentDictionary<string, int>();
        concurrent.AddOrUpdate("hits", 1, (_, old) => old + 1);
        concurrent.AddOrUpdate("hits", 1, (_, old) => old + 1);
        Console.WriteLine($"ConcurrentDictionary hits={concurrent["hits"]}");
        // Also: ConcurrentQueue, ConcurrentStack, ConcurrentBag, BlockingCollection, Channels.

        // Span<T> / ArrayPool<T> — allocation-free slicing and buffer reuse.
        Span<int> span = [10, 20, 30, 40, 50];
        Console.WriteLine($"Span slice: {string.Join(",", span.Slice(1, 3).ToArray())}");
        int[] rented = ArrayPool<int>.Shared.Rent(16);
        ArrayPool<int>.Shared.Return(rented);

        // BitArray, Dictionary with custom comparer, etc. also live in System.Collections.

        // -------------------------------------------------------------- PART 2
        Section("PART 2: Hand-written data structures");

        var sll = new SinglyLinkedList<int>();
        sll.AddLast(1); sll.AddLast(2); sll.AddFirst(0); sll.Reverse();
        Console.WriteLine($"SinglyLinkedList reversed: {string.Join("->", sll)}");

        var myStack = new ArrayStack<char>();
        foreach (var c in "abc") myStack.Push(c);
        Console.WriteLine($"ArrayStack pop: {myStack.Pop()}");

        var myQueue = new CircularQueue<int>(3);
        myQueue.Enqueue(1); myQueue.Enqueue(2); myQueue.Dequeue(); myQueue.Enqueue(3); myQueue.Enqueue(4);
        Console.WriteLine($"CircularQueue peek: {myQueue.Peek()} count={myQueue.Count}");

        var heap = new MinHeap<int>();
        foreach (var x in new[] { 7, 2, 9, 1, 5 }) heap.Push(x);
        Console.Write("MinHeap pops: ");
        while (heap.Count > 0) Console.Write(heap.Pop() + " ");
        Console.WriteLine();

        var bst = new BinarySearchTree<int>();
        foreach (var x in new[] { 50, 30, 70, 20, 40, 60, 80 }) bst.Insert(x);
        bst.Remove(30);
        Console.WriteLine($"BST in-order: {string.Join(",", bst.InOrder())}  contains 40? {bst.Contains(40)}");

        var avl = new AvlTree<int>();
        for (int i = 1; i <= 7; i++) avl.Insert(i); // would be a linked list in a plain BST
        Console.WriteLine($"AVL height after inserting 1..7: {avl.Height} (balanced)");

        var trie = new Trie();
        foreach (var w in new[] { "car", "cart", "care", "dog" }) trie.Insert(w);
        Console.WriteLine($"Trie words with 'car': {string.Join(",", trie.WordsWithPrefix("car"))}");

        var uf = new UnionFind(5);
        uf.Union(0, 1); uf.Union(3, 4);
        Console.WriteLine($"UnionFind connected(0,1)={uf.Connected(0, 1)} connected(1,3)={uf.Connected(1, 3)}");

        var lru = new LruCache<string, int>(2);
        lru.Put("a", 1); lru.Put("b", 2); lru.Get("a"); lru.Put("c", 3); // evicts "b"
        Console.WriteLine($"LRU has b? {lru.TryGet("b", out _)}  has a? {lru.TryGet("a", out _)}");

        var seg = new SegmentTree([2, 1, 5, 3, 4]);
        seg.Update(1, 10);
        Console.WriteLine($"SegmentTree sum[0..2]={seg.Query(0, 2)}");

        var fen = new FenwickTree(5);
        int[] fenData = [2, 1, 5, 3, 4];
        for (int i = 0; i < fenData.Length; i++) fen.Add(i, fenData[i]);
        Console.WriteLine($"FenwickTree sum[1..3]={fen.RangeSum(1, 3)}");

        var hm = new SimpleHashMap<string, int>();
        hm.Put("one", 1); hm.Put("two", 2); hm.Put("one", 11);
        Console.WriteLine($"SimpleHashMap one={hm.Get("one")} count={hm.Count}");

        // -------------------------------------------------------------- PART 3
        Section("PART 3: Sorting");
        int[] data = [64, 34, 25, 12, 22, 11, 90, 5];
        void Show(string name, Action<int[]> sort)
        {
            var copy = (int[])data.Clone();
            sort(copy);
            Console.WriteLine($"{name,-15} {string.Join(",", copy)}");
        }
        Show("Bubble", Sorting.BubbleSort);
        Show("Selection", Sorting.SelectionSort);
        Show("Insertion", Sorting.InsertionSort);
        Show("Shell", Sorting.ShellSort);
        Show("Merge", Sorting.MergeSort);
        Show("Quick", Sorting.QuickSort);
        Show("Heap", Sorting.HeapSort);
        Show("Counting", Sorting.CountingSort);
        Show("Radix", Sorting.RadixSort);
        Show("Built-in", arr => Array.Sort(arr)); // introsort — use this in real code

        // -------------------------------------------------------------- PART 4
        Section("PART 4: Searching");
        int[] sorted = [1, 3, 3, 3, 7, 9, 11];
        Console.WriteLine($"Linear(7)={Searching.Linear(sorted, 7)}  Binary(9)={Searching.Binary(sorted, 9)}");
        Console.WriteLine($"LowerBound(3)={Searching.LowerBound(sorted, 3)}  UpperBound(3)={Searching.UpperBound(sorted, 3)}");
        Console.WriteLine($"Built-in Array.BinarySearch(11)={Array.BinarySearch(sorted, 11)}");
        double peak = Searching.TernaryMax(x => -(x - 2) * (x - 2) + 4, -10, 10);
        Console.WriteLine($"Ternary search peak of -(x-2)^2+4 at x≈{peak:F3}");

        // -------------------------------------------------------------- PART 5
        Section("PART 5: Graphs");
        var g = new Graph(6, directed: false);
        g.AddEdge(0, 1, 7); g.AddEdge(0, 2, 9); g.AddEdge(0, 5, 14);
        g.AddEdge(1, 2, 10); g.AddEdge(1, 3, 15); g.AddEdge(2, 3, 11);
        g.AddEdge(2, 5, 2); g.AddEdge(3, 4, 6); g.AddEdge(4, 5, 9);

        Console.WriteLine($"BFS from 0: {string.Join(",", GraphAlgorithms.Bfs(g, 0))}");
        Console.WriteLine($"DFS from 0: {string.Join(",", GraphAlgorithms.Dfs(g, 0))}");
        var dist = GraphAlgorithms.Dijkstra(g, 0);
        Console.WriteLine($"Dijkstra dist from 0: {string.Join(",", dist)}");
        var bf = GraphAlgorithms.BellmanFord(g, 0);
        Console.WriteLine($"Bellman-Ford from 0: {(bf is null ? "negative cycle" : string.Join(",", bf))}");
        var fw = GraphAlgorithms.FloydWarshall(g);
        Console.WriteLine($"Floyd-Warshall dist(0,4)={fw[0, 4]}");
        Console.WriteLine($"Kruskal MST weight={GraphAlgorithms.KruskalMst(g)}  Prim MST weight={GraphAlgorithms.PrimMst(g)}");

        var dag = new Graph(6, directed: true);
        dag.AddEdge(5, 2); dag.AddEdge(5, 0); dag.AddEdge(4, 0);
        dag.AddEdge(4, 1); dag.AddEdge(2, 3); dag.AddEdge(3, 1);
        Console.WriteLine($"Topological sort (Kahn): {string.Join(",", GraphAlgorithms.TopologicalSort(dag))}");
        Console.WriteLine($"DAG has cycle? {GraphAlgorithms.HasCycleDirected(dag)}");

        // -------------------------------------------------------------- PART 6
        Section("PART 6: Dynamic programming");
        Console.WriteLine($"Fib(50) memo={DynamicProgramming.FibMemo(50)}  tab={DynamicProgramming.FibTab(50)}");
        Console.WriteLine($"0/1 Knapsack={DynamicProgramming.Knapsack([1, 3, 4, 5], [1, 4, 5, 7], 7)}");
        Console.WriteLine($"LCS(ABCBDAB, BDCABA)={DynamicProgramming.Lcs("ABCBDAB", "BDCABA")}");
        Console.WriteLine($"LIS length={DynamicProgramming.Lis([10, 9, 2, 5, 3, 7, 101, 18])}");
        Console.WriteLine($"EditDistance(kitten,sitting)={DynamicProgramming.EditDistance("kitten", "sitting")}");
        Console.WriteLine($"CoinChange([1,2,5],11)={DynamicProgramming.CoinChange([1, 2, 5], 11)}");
        Console.WriteLine($"Kadane max subarray={DynamicProgramming.MaxSubarray([-2, 1, -3, 4, -1, 2, 1, -5, 4])}");

        // -------------------------------------------------------------- PART 7
        Section("PART 7: Strings");
        Console.WriteLine($"KMP 'abab' in 'ababcabab': {string.Join(",", Strings.KmpSearch("ababcabab", "abab"))}");
        Console.WriteLine($"Rabin-Karp 'cab': {string.Join(",", Strings.RabinKarp("ababcabab", "cab"))}");
        Console.WriteLine($"IsPalindrome('A man, a plan, a canal: Panama')={Strings.IsPalindrome("A man, a plan, a canal: Panama")}");
        Console.WriteLine($"IsAnagram(listen,silent)={Strings.IsAnagram("listen", "silent")}");

        // -------------------------------------------------------------- PART 8
        Section("PART 8: Techniques");
        Console.WriteLine($"TwoSum sorted [1,2,4,7,11] target 15 -> {Techniques.TwoSumSorted([1, 2, 4, 7, 11], 15)}");
        Console.WriteLine($"TwoSum hash [3,2,4] target 6 -> {Techniques.TwoSumHash([3, 2, 4], 6)}");
        Console.WriteLine($"Longest substring w/o repeat 'abcabcbb'={Techniques.LongestUniqueSubstring("abcabcbb")}");
        var prefix = Techniques.PrefixSums([3, 1, 4, 1, 5]);
        Console.WriteLine($"Prefix sums: {string.Join(",", prefix)}  sum[1..3]={prefix[4] - prefix[1]}");
        Console.WriteLine($"Permutations of [1,2,3]: {Techniques.Permutations([1, 2, 3]).Count}");
        Console.WriteLine($"Subsets of [1,2,3]: {Techniques.Subsets([1, 2, 3]).Count}");
        Console.WriteLine($"N-Queens(8) solutions={Techniques.NQueens(8)}");
        Console.WriteLine($"Activity selection (greedy)={Techniques.MaxActivities([(1, 4), (3, 5), (0, 6), (5, 7), (8, 9), (5, 9)])}");
        Console.WriteLine($"Bits: popcount(29)={System.Numerics.BitOperations.PopCount(29u)}  isPow2(64)={Techniques.IsPowerOfTwo(64)}");
        Console.WriteLine($"GCD(48,18)={Techniques.Gcd(48, 18)}  primes<30: {string.Join(",", Techniques.Sieve(30))}");
        Console.WriteLine($"FastPow(3,13) mod 1e9+7={Techniques.ModPow(3, 13, 1_000_000_007)}");

        var swapList = new List<int> { 1, 2, 3 };
        swapList.Swap(0, 2); // C# 14 extension member, see ListExtensions below
        Console.WriteLine($"C# 14 extension Swap: {string.Join(",", swapList)}");
    }
}

// ============================================================================
// C# 14 (.NET 10) extension members
// ============================================================================
static class ListExtensions
{
    extension<T>(IList<T> list)
    {
        public void Swap(int i, int j) => (list[i], list[j]) = (list[j], list[i]);
    }
}

// ============================================================================
// PART 2 — Data structures
// ============================================================================

/// <summary>Singly linked list. AddFirst O(1), AddLast O(1) with tail, search O(n).</summary>
class SinglyLinkedList<T> : IEnumerable<T>
{
    class Node(T value) { public T Value = value; public Node? Next; }
    Node? _head, _tail;
    public int Count { get; private set; }

    public void AddFirst(T value)
    {
        var node = new Node(value) { Next = _head };
        _head = node;
        _tail ??= node;
        Count++;
    }

    public void AddLast(T value)
    {
        var node = new Node(value);
        if (_tail is null) _head = _tail = node;
        else { _tail.Next = node; _tail = node; }
        Count++;
    }

    public bool Remove(T value)
    {
        Node? prev = null, cur = _head;
        while (cur is not null)
        {
            if (EqualityComparer<T>.Default.Equals(cur.Value, value))
            {
                if (prev is null) _head = cur.Next; else prev.Next = cur.Next;
                if (cur == _tail) _tail = prev;
                Count--;
                return true;
            }
            prev = cur; cur = cur.Next;
        }
        return false;
    }

    /// <summary>Classic in-place reversal — a common interview question.</summary>
    public void Reverse()
    {
        Node? prev = null, cur = _head;
        _tail = _head;
        while (cur is not null)
        {
            var next = cur.Next;
            cur.Next = prev;
            prev = cur;
            cur = next;
        }
        _head = prev;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var n = _head; n is not null; n = n.Next) yield return n.Value;
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Stack on a growable array. Push/Pop O(1) amortised.</summary>
class ArrayStack<T>
{
    T[] _items = new T[4];
    public int Count { get; private set; }

    public void Push(T item)
    {
        if (Count == _items.Length) Array.Resize(ref _items, _items.Length * 2);
        _items[Count++] = item;
    }

    public T Pop()
    {
        if (Count == 0) throw new InvalidOperationException("Stack empty");
        var item = _items[--Count];
        _items[Count] = default!;
        return item;
    }

    public T Peek() => Count == 0 ? throw new InvalidOperationException("Stack empty") : _items[Count - 1];
}

/// <summary>Queue on a ring buffer. Enqueue/Dequeue O(1) amortised.</summary>
class CircularQueue<T>(int capacity = 4)
{
    T[] _items = new T[Math.Max(1, capacity)];
    int _head, _tail;
    public int Count { get; private set; }

    public void Enqueue(T item)
    {
        if (Count == _items.Length) Grow();
        _items[_tail] = item;
        _tail = (_tail + 1) % _items.Length;
        Count++;
    }

    public T Dequeue()
    {
        if (Count == 0) throw new InvalidOperationException("Queue empty");
        var item = _items[_head];
        _items[_head] = default!;
        _head = (_head + 1) % _items.Length;
        Count--;
        return item;
    }

    public T Peek() => Count == 0 ? throw new InvalidOperationException("Queue empty") : _items[_head];

    void Grow()
    {
        var bigger = new T[_items.Length * 2];
        for (int i = 0; i < Count; i++) bigger[i] = _items[(_head + i) % _items.Length];
        _items = bigger; _head = 0; _tail = Count;
    }
}

/// <summary>Binary min-heap. Push/Pop O(log n), Peek O(1).</summary>
class MinHeap<T> where T : IComparable<T>
{
    readonly List<T> _data = [];
    public int Count => _data.Count;
    public T Peek() => _data[0];

    public void Push(T item)
    {
        _data.Add(item);
        int i = _data.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_data[i].CompareTo(_data[parent]) >= 0) break;
            (_data[i], _data[parent]) = (_data[parent], _data[i]);
            i = parent;
        }
    }

    public T Pop()
    {
        if (_data.Count == 0) throw new InvalidOperationException("Heap empty");
        var top = _data[0];
        int last = _data.Count - 1;
        _data[0] = _data[last];
        _data.RemoveAt(last);

        int i = 0;
        while (true)
        {
            int l = 2 * i + 1, r = l + 1, smallest = i;
            if (l < _data.Count && _data[l].CompareTo(_data[smallest]) < 0) smallest = l;
            if (r < _data.Count && _data[r].CompareTo(_data[smallest]) < 0) smallest = r;
            if (smallest == i) break;
            (_data[i], _data[smallest]) = (_data[smallest], _data[i]);
            i = smallest;
        }
        return top;
    }
}

/// <summary>Unbalanced binary search tree. O(log n) average, O(n) worst case.</summary>
class BinarySearchTree<T> where T : IComparable<T>
{
    class Node(T value) { public T Value = value; public Node? Left, Right; }
    Node? _root;

    public void Insert(T value) => _root = Insert(_root, value);
    static Node Insert(Node? node, T value)
    {
        if (node is null) return new Node(value);
        int cmp = value.CompareTo(node.Value);
        if (cmp < 0) node.Left = Insert(node.Left, value);
        else if (cmp > 0) node.Right = Insert(node.Right, value);
        return node;
    }

    public bool Contains(T value)
    {
        var n = _root;
        while (n is not null)
        {
            int cmp = value.CompareTo(n.Value);
            if (cmp == 0) return true;
            n = cmp < 0 ? n.Left : n.Right;
        }
        return false;
    }

    public void Remove(T value) => _root = Remove(_root, value);
    static Node? Remove(Node? node, T value)
    {
        if (node is null) return null;
        int cmp = value.CompareTo(node.Value);
        if (cmp < 0) node.Left = Remove(node.Left, value);
        else if (cmp > 0) node.Right = Remove(node.Right, value);
        else
        {
            if (node.Left is null) return node.Right;
            if (node.Right is null) return node.Left;
            var successor = node.Right;               // smallest in right subtree
            while (successor.Left is not null) successor = successor.Left;
            node.Value = successor.Value;
            node.Right = Remove(node.Right, successor.Value);
        }
        return node;
    }

    public IEnumerable<T> InOrder() => InOrder(_root);
    static IEnumerable<T> InOrder(Node? n)
    {
        if (n is null) yield break;
        foreach (var v in InOrder(n.Left)) yield return v;
        yield return n.Value;
        foreach (var v in InOrder(n.Right)) yield return v;
    }
    // Pre-order: node, left, right.  Post-order: left, right, node.
    // Level-order: BFS with a Queue.
}

/// <summary>Self-balancing AVL tree. Guaranteed O(log n) insert/search.</summary>
class AvlTree<T> where T : IComparable<T>
{
    class Node(T value) { public T Value = value; public Node? Left, Right; public int Height = 1; }
    Node? _root;
    public int Height => H(_root);

    static int H(Node? n) => n?.Height ?? 0;
    static void Fix(Node n) => n.Height = 1 + Math.Max(H(n.Left), H(n.Right));
    static int Balance(Node n) => H(n.Left) - H(n.Right);

    static Node RotateRight(Node y)
    {
        var x = y.Left!;
        y.Left = x.Right; x.Right = y;
        Fix(y); Fix(x);
        return x;
    }

    static Node RotateLeft(Node x)
    {
        var y = x.Right!;
        x.Right = y.Left; y.Left = x;
        Fix(x); Fix(y);
        return y;
    }

    public void Insert(T value) => _root = Insert(_root, value);
    static Node Insert(Node? node, T value)
    {
        if (node is null) return new Node(value);
        int cmp = value.CompareTo(node.Value);
        if (cmp < 0) node.Left = Insert(node.Left, value);
        else if (cmp > 0) node.Right = Insert(node.Right, value);
        else return node;

        Fix(node);
        int bal = Balance(node);
        if (bal > 1 && value.CompareTo(node.Left!.Value) < 0) return RotateRight(node);            // LL
        if (bal < -1 && value.CompareTo(node.Right!.Value) > 0) return RotateLeft(node);           // RR
        if (bal > 1) { node.Left = RotateLeft(node.Left!); return RotateRight(node); }             // LR
        if (bal < -1) { node.Right = RotateRight(node.Right!); return RotateLeft(node); }          // RL
        return node;
    }
}

/// <summary>Prefix tree for strings. Insert/search O(word length).</summary>
class Trie
{
    class Node { public Dictionary<char, Node> Children = new(); public bool IsWord; }
    readonly Node _root = new();

    public void Insert(string word)
    {
        var n = _root;
        foreach (var c in word)
        {
            if (!n.Children.TryGetValue(c, out var next)) n.Children[c] = next = new Node();
            n = next;
        }
        n.IsWord = true;
    }

    public bool Search(string word) => Find(word)?.IsWord ?? false;
    public bool StartsWith(string prefix) => Find(prefix) is not null;

    public List<string> WordsWithPrefix(string prefix)
    {
        var results = new List<string>();
        var start = Find(prefix);
        if (start is not null) Collect(start, new StringBuilder(prefix), results);
        return results;
    }

    Node? Find(string s)
    {
        var n = _root;
        foreach (var c in s)
        {
            if (!n.Children.TryGetValue(c, out var next)) return null;
            n = next;
        }
        return n;
    }

    static void Collect(Node n, StringBuilder sb, List<string> results)
    {
        if (n.IsWord) results.Add(sb.ToString());
        foreach (var (c, child) in n.Children)
        {
            sb.Append(c);
            Collect(child, sb, results);
            sb.Length--;
        }
    }
}

/// <summary>Disjoint-set union with path compression + union by rank. ~O(1) per op.</summary>
class UnionFind
{
    readonly int[] _parent, _rank;
    public int Components { get; private set; }

    public UnionFind(int n)
    {
        _parent = Enumerable.Range(0, n).ToArray();
        _rank = new int[n];
        Components = n;
    }

    public int Find(int x)
    {
        if (_parent[x] != x) _parent[x] = Find(_parent[x]); // path compression
        return _parent[x];
    }

    public bool Union(int a, int b)
    {
        int ra = Find(a), rb = Find(b);
        if (ra == rb) return false;
        if (_rank[ra] < _rank[rb]) (ra, rb) = (rb, ra);
        _parent[rb] = ra;
        if (_rank[ra] == _rank[rb]) _rank[ra]++;
        Components--;
        return true;
    }

    public bool Connected(int a, int b) => Find(a) == Find(b);
}

/// <summary>Weighted graph as an adjacency list.</summary>
class Graph(int vertices, bool directed)
{
    public int V { get; } = vertices;
    public bool Directed { get; } = directed;
    public List<(int To, int Weight)>[] Adj { get; } =
        Enumerable.Range(0, vertices).Select(_ => new List<(int, int)>()).ToArray();

    public void AddEdge(int from, int to, int weight = 1)
    {
        Adj[from].Add((to, weight));
        if (!Directed) Adj[to].Add((from, weight));
    }

    public IEnumerable<(int From, int To, int Weight)> Edges()
    {
        for (int u = 0; u < V; u++)
            foreach (var (v, w) in Adj[u])
                if (Directed || u < v) yield return (u, v, w);
    }
}

/// <summary>Least-recently-used cache: Dictionary + LinkedList gives O(1) get/put.</summary>
class LruCache<TKey, TValue>(int capacity) where TKey : notnull
{
    readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _map = new();
    readonly LinkedList<(TKey Key, TValue Value)> _order = new();

    public bool TryGet(TKey key, out TValue value)
    {
        if (_map.TryGetValue(key, out var node))
        {
            _order.Remove(node); _order.AddFirst(node);
            value = node.Value.Value;
            return true;
        }
        value = default!;
        return false;
    }

    public TValue? Get(TKey key) => TryGet(key, out var v) ? v : default;

    public void Put(TKey key, TValue value)
    {
        if (_map.TryGetValue(key, out var existing)) _order.Remove(existing);
        else if (_map.Count == capacity)
        {
            var lru = _order.Last!;
            _order.RemoveLast();
            _map.Remove(lru.Value.Key);
        }
        _map[key] = _order.AddFirst((key, value));
    }
}

/// <summary>Segment tree for range-sum queries + point updates, both O(log n).</summary>
class SegmentTree
{
    readonly int _n;
    readonly long[] _tree;

    public SegmentTree(int[] values)
    {
        _n = values.Length;
        _tree = new long[4 * _n];
        Build(values, 1, 0, _n - 1);
    }

    void Build(int[] v, int node, int l, int r)
    {
        if (l == r) { _tree[node] = v[l]; return; }
        int m = (l + r) / 2;
        Build(v, 2 * node, l, m); Build(v, 2 * node + 1, m + 1, r);
        _tree[node] = _tree[2 * node] + _tree[2 * node + 1];
    }

    public void Update(int index, int value) => Update(1, 0, _n - 1, index, value);
    void Update(int node, int l, int r, int i, int value)
    {
        if (l == r) { _tree[node] = value; return; }
        int m = (l + r) / 2;
        if (i <= m) Update(2 * node, l, m, i, value); else Update(2 * node + 1, m + 1, r, i, value);
        _tree[node] = _tree[2 * node] + _tree[2 * node + 1];
    }

    public long Query(int from, int to) => Query(1, 0, _n - 1, from, to);
    long Query(int node, int l, int r, int from, int to)
    {
        if (to < l || r < from) return 0;
        if (from <= l && r <= to) return _tree[node];
        int m = (l + r) / 2;
        return Query(2 * node, l, m, from, to) + Query(2 * node + 1, m + 1, r, from, to);
    }
}

/// <summary>Fenwick / Binary Indexed Tree: prefix sums with O(log n) updates.</summary>
class FenwickTree(int n)
{
    readonly long[] _tree = new long[n + 1];

    public void Add(int index, long delta)
    {
        for (int i = index + 1; i < _tree.Length; i += i & -i) _tree[i] += delta;
    }

    public long PrefixSum(int index) // sum of [0..index]
    {
        long sum = 0;
        for (int i = index + 1; i > 0; i -= i & -i) sum += _tree[i];
        return sum;
    }

    public long RangeSum(int from, int to) => PrefixSum(to) - (from > 0 ? PrefixSum(from - 1) : 0);
}

/// <summary>Hash map with separate chaining — shows how Dictionary works underneath.</summary>
class SimpleHashMap<TKey, TValue> where TKey : notnull
{
    List<(TKey Key, TValue Value)>[] _buckets = NewBuckets(8);
    public int Count { get; private set; }

    static List<(TKey, TValue)>[] NewBuckets(int n) =>
        Enumerable.Range(0, n).Select(_ => new List<(TKey, TValue)>()).ToArray();

    int IndexFor(TKey key, int size) => (key.GetHashCode() & int.MaxValue) % size;

    public void Put(TKey key, TValue value)
    {
        var bucket = _buckets[IndexFor(key, _buckets.Length)];
        for (int i = 0; i < bucket.Count; i++)
            if (EqualityComparer<TKey>.Default.Equals(bucket[i].Key, key)) { bucket[i] = (key, value); return; }
        bucket.Add((key, value));
        if (++Count > _buckets.Length * 3 / 4) Resize();
    }

    public TValue Get(TKey key)
    {
        foreach (var (k, v) in _buckets[IndexFor(key, _buckets.Length)])
            if (EqualityComparer<TKey>.Default.Equals(k, key)) return v;
        throw new KeyNotFoundException(key.ToString());
    }

    void Resize()
    {
        var old = _buckets;
        _buckets = NewBuckets(old.Length * 2);
        foreach (var bucket in old)
            foreach (var entry in bucket) _buckets[IndexFor(entry.Key, _buckets.Length)].Add(entry);
    }
}

// ============================================================================
// PART 3 — Sorting
// ============================================================================
static class Sorting
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

    // O(n log n) average, O(n²) worst. In-place. Lomuto partition, middle pivot.
    public static void QuickSort(int[] a) => QuickSort(a, 0, a.Length - 1);
    static void QuickSort(int[] a, int lo, int hi)
    {
        if (lo >= hi) return;
        int mid = lo + (hi - lo) / 2;
        (a[mid], a[hi]) = (a[hi], a[mid]);
        int pivot = a[hi], i = lo;
        for (int j = lo; j < hi; j++)
            if (a[j] < pivot) { (a[i], a[j]) = (a[j], a[i]); i++; }
        (a[i], a[hi]) = (a[hi], a[i]);
        QuickSort(a, lo, i - 1);
        QuickSort(a, i + 1, hi);
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
    public static void RadixSort(int[] a)
    {
        if (a.Length == 0) return;
        int max = a.Max();
        var output = new int[a.Length];
        for (int exp = 1; max / exp > 0; exp *= 10)
        {
            var count = new int[10];
            foreach (var x in a) count[x / exp % 10]++;
            for (int i = 1; i < 10; i++) count[i] += count[i - 1];
            for (int i = a.Length - 1; i >= 0; i--) output[--count[a[i] / exp % 10]] = a[i];
            Array.Copy(output, a, a.Length);
        }
    }
}

// ============================================================================
// PART 4 — Searching
// ============================================================================
static class Searching
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

// ============================================================================
// PART 5 — Graph algorithms
// ============================================================================
static class GraphAlgorithms
{
    public const int Inf = int.MaxValue / 2;

    // Breadth-first search — level by level; gives shortest paths in unweighted graphs.
    public static List<int> Bfs(Graph g, int start)
    {
        var order = new List<int>();
        var seen = new bool[g.V];
        var q = new Queue<int>();
        q.Enqueue(start); seen[start] = true;
        while (q.Count > 0)
        {
            int u = q.Dequeue();
            order.Add(u);
            foreach (var (v, _) in g.Adj[u])
                if (!seen[v]) { seen[v] = true; q.Enqueue(v); }
        }
        return order;
    }

    // Depth-first search — iterative with an explicit stack.
    public static List<int> Dfs(Graph g, int start)
    {
        var order = new List<int>();
        var seen = new bool[g.V];
        var stack = new Stack<int>();
        stack.Push(start);
        while (stack.Count > 0)
        {
            int u = stack.Pop();
            if (seen[u]) continue;
            seen[u] = true;
            order.Add(u);
            for (int i = g.Adj[u].Count - 1; i >= 0; i--)
                if (!seen[g.Adj[u][i].To]) stack.Push(g.Adj[u][i].To);
        }
        return order;
    }

    // Dijkstra — shortest paths, non-negative weights. O((V + E) log V).
    public static int[] Dijkstra(Graph g, int source)
    {
        var dist = Enumerable.Repeat(Inf, g.V).ToArray();
        var pq = new PriorityQueue<int, int>();
        dist[source] = 0;
        pq.Enqueue(source, 0);
        while (pq.TryDequeue(out int u, out int d))
        {
            if (d > dist[u]) continue; // stale entry
            foreach (var (v, w) in g.Adj[u])
                if (dist[u] + w < dist[v]) { dist[v] = dist[u] + w; pq.Enqueue(v, dist[v]); }
        }
        return dist;
    }

    // Bellman-Ford — handles negative weights, detects negative cycles. O(V·E).
    public static int[]? BellmanFord(Graph g, int source)
    {
        var dist = Enumerable.Repeat(Inf, g.V).ToArray();
        dist[source] = 0;
        var edges = new List<(int u, int v, int w)>();
        for (int u = 0; u < g.V; u++) foreach (var (v, w) in g.Adj[u]) edges.Add((u, v, w));

        for (int i = 0; i < g.V - 1; i++)
            foreach (var (u, v, w) in edges)
                if (dist[u] != Inf && dist[u] + w < dist[v]) dist[v] = dist[u] + w;

        foreach (var (u, v, w) in edges)
            if (dist[u] != Inf && dist[u] + w < dist[v]) return null; // negative cycle
        return dist;
    }

    // Floyd-Warshall — all-pairs shortest paths. O(V³).
    public static int[,] FloydWarshall(Graph g)
    {
        var d = new int[g.V, g.V];
        for (int i = 0; i < g.V; i++)
            for (int j = 0; j < g.V; j++) d[i, j] = i == j ? 0 : Inf;
        for (int u = 0; u < g.V; u++)
            foreach (var (v, w) in g.Adj[u]) d[u, v] = Math.Min(d[u, v], w);

        for (int k = 0; k < g.V; k++)
            for (int i = 0; i < g.V; i++)
                for (int j = 0; j < g.V; j++)
                    if (d[i, k] + d[k, j] < d[i, j]) d[i, j] = d[i, k] + d[k, j];
        return d;
    }

    // Kruskal — minimum spanning tree via sorted edges + Union-Find. O(E log E).
    public static int KruskalMst(Graph g)
    {
        var uf = new UnionFind(g.V);
        int total = 0;
        foreach (var (u, v, w) in g.Edges().OrderBy(e => e.Weight))
            if (uf.Union(u, v)) total += w;
        return total;
    }

    // Prim — minimum spanning tree grown from one vertex with a priority queue.
    public static int PrimMst(Graph g)
    {
        var inTree = new bool[g.V];
        var pq = new PriorityQueue<int, int>();
        pq.Enqueue(0, 0);
        int total = 0;
        while (pq.TryDequeue(out int u, out int w))
        {
            if (inTree[u]) continue;
            inTree[u] = true;
            total += w;
            foreach (var (v, wt) in g.Adj[u]) if (!inTree[v]) pq.Enqueue(v, wt);
        }
        return total;
    }

    // Kahn's algorithm — topological order of a DAG. O(V + E).
    public static List<int> TopologicalSort(Graph g)
    {
        var inDegree = new int[g.V];
        for (int u = 0; u < g.V; u++) foreach (var (v, _) in g.Adj[u]) inDegree[v]++;
        var q = new Queue<int>(Enumerable.Range(0, g.V).Where(v => inDegree[v] == 0));
        var order = new List<int>();
        while (q.Count > 0)
        {
            int u = q.Dequeue();
            order.Add(u);
            foreach (var (v, _) in g.Adj[u]) if (--inDegree[v] == 0) q.Enqueue(v);
        }
        return order; // if order.Count < V, the graph has a cycle
    }

    // Cycle detection in a directed graph using DFS colouring (white/grey/black).
    public static bool HasCycleDirected(Graph g)
    {
        var state = new int[g.V]; // 0 = unvisited, 1 = on stack, 2 = done
        bool Visit(int u)
        {
            state[u] = 1;
            foreach (var (v, _) in g.Adj[u])
                if (state[v] == 1 || (state[v] == 0 && Visit(v))) return true;
            state[u] = 2;
            return false;
        }
        for (int u = 0; u < g.V; u++) if (state[u] == 0 && Visit(u)) return true;
        return false;
    }
}

// ============================================================================
// PART 6 — Dynamic programming
// ============================================================================
static class DynamicProgramming
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

// ============================================================================
// PART 7 — String algorithms
// ============================================================================
static class Strings
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

// ============================================================================
// PART 8 — Common techniques
// ============================================================================
static class Techniques
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