namespace Dsa.Tests;

public class SinglyLinkedListTests
{
    [Fact]
    public void AddFirst_AddLast_KeepOrder()
    {
        var list = new SinglyLinkedList<int>();
        list.AddLast(2); list.AddLast(3); list.AddFirst(1);
        Assert.Equal(new[] { 1, 2, 3 }, list);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void Reverse_ReversesAndKeepsTailUsable()
    {
        var list = new SinglyLinkedList<int>();
        foreach (var x in new[] { 1, 2, 3 }) list.AddLast(x);
        list.Reverse();
        list.AddLast(0); // tail must point at the old head after reversing
        Assert.Equal(new[] { 3, 2, 1, 0 }, list);
    }

    [Fact]
    public void Remove_HeadMiddleTail_AndMissing()
    {
        var list = new SinglyLinkedList<int>();
        foreach (var x in new[] { 1, 2, 3, 4 }) list.AddLast(x);
        Assert.True(list.Remove(1));
        Assert.True(list.Remove(3));
        Assert.True(list.Remove(4));
        Assert.False(list.Remove(99));
        list.AddLast(5); // tail must have been updated when 4 was removed
        Assert.Equal(new[] { 2, 5 }, list);
    }
}

public class StackAndQueueTests
{
    [Fact]
    public void ArrayStack_IsLifo_AndGrows()
    {
        var stack = new ArrayStack<int>();
        for (int i = 0; i < 100; i++) stack.Push(i);
        Assert.Equal(99, stack.Peek());
        for (int i = 99; i >= 0; i--) Assert.Equal(i, stack.Pop());
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
    }

    [Fact]
    public void CircularQueue_IsFifo_AcrossWrapAroundAndGrowth()
    {
        var queue = new CircularQueue<int>(3);
        var expected = new Queue<int>();
        var rng = new Random(1);
        int next = 0;
        for (int step = 0; step < 1000; step++)
        {
            if (expected.Count == 0 || rng.Next(3) > 0)
            {
                queue.Enqueue(next); expected.Enqueue(next); next++;
            }
            else
            {
                Assert.Equal(expected.Dequeue(), queue.Dequeue());
            }
            Assert.Equal(expected.Count, queue.Count);
        }
    }

    [Fact]
    public void CircularQueue_EmptyDequeue_Throws() =>
        Assert.Throws<InvalidOperationException>(() => new CircularQueue<int>().Dequeue());
}

public class HeapTests
{
    [Fact]
    public void MinHeap_PopsInSortedOrder()
    {
        var rng = new Random(42);
        var values = Enumerable.Range(0, 1000).Select(_ => rng.Next(-500, 500)).ToArray();
        var heap = new MinHeap<int>();
        foreach (var v in values) heap.Push(v);

        var popped = new List<int>();
        while (heap.Count > 0) popped.Add(heap.Pop());

        Assert.Equal(values.Order(), popped);
    }

    [Fact]
    public void MinHeap_MatchesBuiltInPriorityQueue()
    {
        var rng = new Random(7);
        var mine = new MinHeap<int>();
        var builtIn = new PriorityQueue<int, int>();
        for (int i = 0; i < 2000; i++)
        {
            if (mine.Count == 0 || rng.Next(2) == 0)
            {
                int v = rng.Next(1000);
                mine.Push(v); builtIn.Enqueue(v, v);
            }
            else Assert.Equal(builtIn.Dequeue(), mine.Pop());
        }
    }
}

public class TreeTests
{
    [Fact]
    public void Bst_InOrderIsSorted_AndContainsWorks()
    {
        var rng = new Random(3);
        var values = Enumerable.Range(0, 500).Select(_ => rng.Next(1000)).ToArray();
        var bst = new BinarySearchTree<int>();
        foreach (var v in values) bst.Insert(v);

        Assert.Equal(values.Distinct().Order(), bst.InOrder());
        foreach (var v in values) Assert.True(bst.Contains(v));
        Assert.False(bst.Contains(-1));
    }

    [Theory]
    [InlineData(20)] // leaf
    [InlineData(30)] // node with two children (20, 40)
    [InlineData(50)] // root with two children
    [InlineData(80)] // leaf on the right
    public void Bst_Remove_KeepsTreeValid(int toRemove)
    {
        int[] values = [50, 30, 70, 20, 40, 60, 80];
        var bst = new BinarySearchTree<int>();
        foreach (var v in values) bst.Insert(v);

        bst.Remove(toRemove);

        Assert.False(bst.Contains(toRemove));
        Assert.Equal(values.Where(v => v != toRemove).Order(), bst.InOrder());
    }

    [Fact]
    public void Bst_Remove_NodeWithOneChild()
    {
        var bst = new BinarySearchTree<int>();
        foreach (var v in new[] { 10, 5, 3 }) bst.Insert(v); // 5 has only a left child
        bst.Remove(5);
        Assert.Equal(new[] { 3, 10 }, bst.InOrder());
    }

    [Theory]
    [InlineData(7)]
    [InlineData(1023)]
    [InlineData(10_000)]
    public void Avl_StaysBalanced_OnSortedInput(int n)
    {
        var avl = new AvlTree<int>();
        for (int i = 0; i < n; i++) avl.Insert(i);
        // An AVL tree's height is at most ~1.44 * log2(n + 2).
        Assert.True(avl.Height <= 1.45 * Math.Log2(n + 2), $"height {avl.Height} too tall for n={n}");
    }

    [Fact]
    public void Trie_SearchAndPrefix()
    {
        var trie = new Trie();
        foreach (var w in new[] { "car", "cart", "care", "dog" }) trie.Insert(w);

        Assert.True(trie.Search("car"));
        Assert.False(trie.Search("ca"));
        Assert.True(trie.StartsWith("ca"));
        Assert.False(trie.StartsWith("x"));
        Assert.Equal(new[] { "car", "care", "cart" }, trie.WordsWithPrefix("car").Order());
        Assert.Empty(trie.WordsWithPrefix("zebra"));
    }
}

public class UnionFindTests
{
    [Fact]
    public void Union_MergesComponents()
    {
        var uf = new UnionFind(6);
        Assert.Equal(6, uf.Components);
        Assert.True(uf.Union(0, 1));
        Assert.True(uf.Union(1, 2));
        Assert.False(uf.Union(0, 2)); // already joined
        Assert.True(uf.Union(4, 5));
        Assert.Equal(3, uf.Components);
        Assert.True(uf.Connected(0, 2));
        Assert.False(uf.Connected(2, 4));
    }
}

public class LruCacheTests
{
    [Fact]
    public void EvictsLeastRecentlyUsed()
    {
        var cache = new LruCache<string, int>(2);
        cache.Put("a", 1);
        cache.Put("b", 2);
        cache.Get("a");      // a is now most recent
        cache.Put("c", 3);   // evicts b

        Assert.False(cache.TryGet("b", out _));
        Assert.True(cache.TryGet("a", out var a)); Assert.Equal(1, a);
        Assert.True(cache.TryGet("c", out var c)); Assert.Equal(3, c);
    }

    [Fact]
    public void UpdatingExistingKey_DoesNotEvict()
    {
        var cache = new LruCache<string, int>(2);
        cache.Put("a", 1);
        cache.Put("b", 2);
        cache.Put("a", 10);

        Assert.True(cache.TryGet("b", out _));
        Assert.True(cache.TryGet("a", out var a)); Assert.Equal(10, a);
    }
}

public class RangeQueryTests
{
    [Fact]
    public void SegmentTree_MatchesBruteForce()
    {
        var rng = new Random(11);
        var values = Enumerable.Range(0, 200).Select(_ => rng.Next(-100, 100)).ToArray();
        var seg = new SegmentTree(values);

        for (int step = 0; step < 2000; step++)
        {
            if (rng.Next(3) == 0)
            {
                int i = rng.Next(values.Length), v = rng.Next(-100, 100);
                values[i] = v; seg.Update(i, v);
            }
            else
            {
                int l = rng.Next(values.Length), r = rng.Next(l, values.Length);
                Assert.Equal(values[l..(r + 1)].Sum(), seg.Query(l, r));
            }
        }
    }

    [Fact]
    public void FenwickTree_MatchesBruteForce()
    {
        var rng = new Random(12);
        var values = new long[200];
        var fen = new FenwickTree(values.Length);

        for (int step = 0; step < 2000; step++)
        {
            if (rng.Next(2) == 0)
            {
                int i = rng.Next(values.Length), d = rng.Next(-50, 50);
                values[i] += d; fen.Add(i, d);
            }
            else
            {
                int l = rng.Next(values.Length), r = rng.Next(l, values.Length);
                Assert.Equal(values[l..(r + 1)].Sum(), fen.RangeSum(l, r));
            }
        }
    }
}

public class SimpleHashMapTests
{
    [Fact]
    public void PutGet_SurvivesResizing()
    {
        var map = new SimpleHashMap<int, string>();
        for (int i = 0; i < 1000; i++) map.Put(i, $"v{i}");
        for (int i = 0; i < 1000; i++) Assert.Equal($"v{i}", map.Get(i));
        Assert.Equal(1000, map.Count);
    }

    [Fact]
    public void Put_OverwritesExistingKey()
    {
        var map = new SimpleHashMap<string, int>();
        map.Put("x", 1);
        map.Put("x", 2);
        Assert.Equal(2, map.Get("x"));
        Assert.Equal(1, map.Count);
    }

    [Fact]
    public void Get_MissingKey_Throws() =>
        Assert.Throws<KeyNotFoundException>(() => new SimpleHashMap<string, int>().Get("nope"));
}

public class ExtensionMemberTests
{
    [Fact]
    public void Swap_SwapsElements()
    {
        var list = new List<int> { 1, 2, 3 };
        list.Swap(0, 2);
        Assert.Equal(new[] { 3, 2, 1 }, list);
    }
}
