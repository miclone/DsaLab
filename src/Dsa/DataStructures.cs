// PART 2 — Data structures
using System.Buffers;
using System.Text;

namespace Dsa;

/// <summary>Singly linked list. AddFirst O(1), AddLast O(1) with tail, search O(n).</summary>
public class SinglyLinkedList<T> : IEnumerable<T>
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
public class ArrayStack<T>
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
public class CircularQueue<T>(int capacity = 4)
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
public class MinHeap<T> where T : IComparable<T>
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
public class BinarySearchTree<T> where T : IComparable<T>
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
public class AvlTree<T> where T : IComparable<T>
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
public class Trie
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
public class UnionFind
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
public class Graph(int vertices, bool directed)
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
public class LruCache<TKey, TValue>(int capacity) where TKey : notnull
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
public class SegmentTree
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
public class FenwickTree(int n)
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
public class SimpleHashMap<TKey, TValue> where TKey : notnull
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
