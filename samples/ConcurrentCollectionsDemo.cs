// ============================================================================
//  Concurrent collections in .NET 10: one working example of each
//
//  Run (file-based app, no project needed):
//      dotnet run ConcurrentCollectionsDemo.cs
//
//  Which one to use:
//  ┌──────────────────────────┬──────────────────────────────────────────────┐
//  │ ConcurrentDictionary     │ shared lookup / cache / counters             │
//  │ ConcurrentQueue          │ FIFO hand-off between threads (no waiting)   │
//  │ ConcurrentStack          │ LIFO, e.g. "most recent first" work          │
//  │ ConcurrentBag            │ unordered; best when the SAME thread adds    │
//  │                          │ and takes, e.g. an object pool               │
//  │ BlockingCollection       │ producer/consumer where consumers WAIT, with │
//  │                          │ a size limit (older, thread-blocking API)    │
//  │ Channel<T>               │ modern async producer/consumer pipelines;    │
//  │                          │ waits without blocking threads (prefer this) │
//  └──────────────────────────┴──────────────────────────────────────────────┘
//  Numbers marked "varies" depend on thread timing and differ run to run.
// ============================================================================


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;

Console.WriteLine($"Concurrent collections demo: {Environment.ProcessorCount} logical CPUs\n");

WhyConcurrentCollections.Run();
ConcurrentDictionaryDemo.Run();
ConcurrentQueueDemo.Run();
ConcurrentStackDemo.Run();
ConcurrentBagDemo.Run();
BlockingCollectionDemo.Run();
await ChannelDemo.RunAsync();

static class Report
{
    public static void Title(string text) => Console.WriteLine($"===== {text} =====");

    public static void Check(string what, object expected, object actual) =>
        Console.WriteLine($"  {(Equals(expected, actual) ? "OK  " : "FAIL")} {what}: expected {expected:N0}, got {actual:N0}");

    public static void Info(string text) => Console.WriteLine($"  {text}");
}

// ----------------------------------------------------------------------------
// 0. Why they exist: a normal List<T> breaks when many threads add at once.
// ----------------------------------------------------------------------------
static class WhyConcurrentCollections
{
    public static void Run()
    {
        Report.Title("0. Why: List<T> vs ConcurrentBag<T> with 8 threads adding");
        const int N = 100_000;

        var list = new List<int>();
        int exceptions = 0;
        Parallel.For(0, N, i =>
        {
            try { list.Add(i); }                            // NOT thread-safe
            catch { Interlocked.Increment(ref exceptions); }
        });
        Report.Info($"List<int>:     {list.Count:N0} of {N:N0} items, {exceptions} exceptions (varies: usually items are lost)");

        var bag = new ConcurrentBag<int>();
        Parallel.For(0, N, bag.Add);                        // thread-safe
        Report.Check("ConcurrentBag count", N, bag.Count);
        Console.WriteLine();
    }
}

// ----------------------------------------------------------------------------
// 1. ConcurrentDictionary: counting words from many threads, plus a cache.
// ----------------------------------------------------------------------------
static class ConcurrentDictionaryDemo
{
    public static void Run()
    {
        Report.Title("1. ConcurrentDictionary<TKey, TValue>");

        // 10,000 copies of a sentence that contains "the" 3 times and "fox" twice.
        string[] sentence = "the quick brown fox jumps over the lazy dog the fox".Split(' ');
        string[] words = Enumerable.Repeat(sentence, 10_000).SelectMany(w => w).ToArray();

        // RIGHT: AddOrUpdate does the read-modify-write as one atomic step.
        var counts = new ConcurrentDictionary<string, int>();
        Parallel.ForEach(words, w => counts.AddOrUpdate(w, 1, (_, old) => old + 1));
        Report.Check("count of \"the\" with AddOrUpdate", 30_000, counts["the"]);
        Report.Check("count of \"fox\" with AddOrUpdate", 20_000, counts["fox"]);

        // WRONG: dict[key]++ is a read, then a separate write. Two threads can read
        // the same old value and both write old + 1, so one update is lost. Each
        // call is thread-safe on its own; the combination is not.
        var broken = new ConcurrentDictionary<string, int>();
        Parallel.ForEach(words, w => { broken.TryAdd(w, 0); broken[w]++; });
        Report.Info($"count of \"the\" with dict[key]++: {broken["the"]:N0} of 30,000 (varies: lost updates)");

        // GetOrAdd's factory can run MORE than once for the same key when threads
        // race, though only one result is kept. Fine for cheap values, a problem
        // for expensive ones (database calls, big allocations).
        int factoryCalls = 0;
        var cache = new ConcurrentDictionary<int, string>();
        Parallel.For(0, 1_000, i => cache.GetOrAdd(i % 10, key =>
        {
            Interlocked.Increment(ref factoryCalls);
            Thread.SpinWait(20_000);                        // pretend this is slow
            return $"value {key}";
        }));
        Report.Info($"plain GetOrAdd: 10 keys, factory ran {factoryCalls} times (varies: often more than 10)");

        // Fix: store Lazy<T>. Racing threads may create several cheap Lazy wrappers,
        // but only the one that gets stored is ever evaluated, exactly once per key.
        int lazyCalls = 0;
        var lazyCache = new ConcurrentDictionary<int, Lazy<string>>();
        Parallel.For(0, 1_000, i => _ = lazyCache.GetOrAdd(i % 10, key => new Lazy<string>(() =>
        {
            Interlocked.Increment(ref lazyCalls);
            Thread.SpinWait(20_000);
            return $"value {key}";
        })).Value);
        Report.Check("GetOrAdd + Lazy<T>: expensive factory calls", 10, lazyCalls);

        // Other atomic operations.
        counts.TryUpdate("fox", 0, comparisonValue: 20_000);   // only if still 20,000
        counts.TryRemove("dog", out int dogs);
        Report.Info($"TryUpdate set fox to {counts["fox"]}; TryRemove took dog = {dogs:N0}; keys left: {counts.Count}");
        Console.WriteLine();
    }
}

// ----------------------------------------------------------------------------
// 2. ConcurrentQueue: several producers and consumers passing work items FIFO.
// ----------------------------------------------------------------------------
static class ConcurrentQueueDemo
{
    public static void Run()
    {
        Report.Title("2. ConcurrentQueue<T>: 4 producers, 4 consumers");
        const int Producers = 4, PerProducer = 25_000, Total = Producers * PerProducer;

        var queue = new ConcurrentQueue<int>();
        int producersDone = 0, consumedCount = 0;
        long consumedSum = 0;

        var producers = Enumerable.Range(0, Producers).Select(p => Task.Run(() =>
        {
            for (int i = 0; i < PerProducer; i++) queue.Enqueue(p * PerProducer + i);
            Interlocked.Increment(ref producersDone);
        })).ToArray();

        var consumers = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
        {
            while (true)
            {
                if (queue.TryDequeue(out int item))
                {
                    Interlocked.Add(ref consumedSum, item);
                    Interlocked.Increment(ref consumedCount);
                }
                // Check "producers done" BEFORE "queue empty": once all producers
                // are done, nothing new can arrive, so empty really means finished.
                else if (Volatile.Read(ref producersDone) == Producers && queue.IsEmpty) break;
                else Thread.Yield();    // nothing yet: let other threads run
            }
        })).ToArray();

        Task.WaitAll(producers.Concat(consumers).ToArray());

        Report.Check("items consumed", Total, consumedCount);
        Report.Check("sum of items (each consumed exactly once)", (long)Total * (Total - 1) / 2, consumedSum);
        Report.Info("Note: idle consumers spin in a loop. BlockingCollection and Channel (below) let them sleep instead.");
        Console.WriteLine();
    }
}

// ----------------------------------------------------------------------------
// 3. ConcurrentStack: LIFO, with atomic multi-item push and pop.
// ----------------------------------------------------------------------------
static class ConcurrentStackDemo
{
    public static void Run()
    {
        Report.Title("3. ConcurrentStack<T>");
        const int N = 10_000;

        var stack = new ConcurrentStack<int>();
        Parallel.For(0, N, stack.Push);

        // PushRange is atomic: these three land together, with no other thread's
        // items in between. They pop back out in reverse order (LIFO).
        stack.PushRange([-1, -2, -3]);
        var top = new int[3];
        int popped = stack.TryPopRange(top);
        Report.Info($"PushRange(-1, -2, -3) then TryPopRange -> {popped} items: {string.Join(", ", top)}");

        if (stack.TryPeek(out int peek)) Report.Info($"TryPeek sees {peek} (varies: whichever thread pushed last)");

        // Drain it from 4 threads at once; every item must come out exactly once.
        var seen = new ConcurrentDictionary<int, byte>();
        int duplicates = 0;
        var drainers = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
        {
            while (stack.TryPop(out int item))
                if (!seen.TryAdd(item, 0)) Interlocked.Increment(ref duplicates);
        })).ToArray();
        Task.WaitAll(drainers);

        Report.Check("distinct items popped by 4 threads", N, seen.Count);
        Report.Check("duplicates", 0, duplicates);
        Console.WriteLine();
    }
}

// ----------------------------------------------------------------------------
// 4. ConcurrentBag: an object pool. Each thread mostly gets back the object it
//    returned itself, because the bag keeps a separate list per thread.
// ----------------------------------------------------------------------------
static class ConcurrentBagDemo
{
    public static void Run()
    {
        Report.Title("4. ConcurrentBag<T>: StringBuilder pool");
        const int Jobs = 10_000;

        var pool = new ConcurrentBag<StringBuilder>();
        int created = 0;

        StringBuilder Rent()
        {
            if (pool.TryTake(out var sb)) return sb.Clear();   // reuse one
            Interlocked.Increment(ref created);
            return new StringBuilder(capacity: 256);            // or make a new one
        }

        int totalLength = 0;
        Parallel.For(0, Jobs, i =>
        {
            var sb = Rent();
            sb.Append("job #").Append(i).Append(" done");
            Interlocked.Add(ref totalLength, sb.Length);
            pool.Add(sb);                                       // give it back
        });

        Report.Info($"{Jobs:N0} jobs needed only {created} StringBuilders (varies: about one per thread)");
        Report.Check("objects back in the pool", created, pool.Count);
        Report.Info("Note: a bag has no order. Use it when any item will do.");
        Console.WriteLine();
    }
}

// ----------------------------------------------------------------------------
// 5. BlockingCollection: bounded producer/consumer. The producer is made to
//    wait when 100 items are queued (back-pressure), and consumers sleep
//    instead of spinning when there is nothing to do.
// ----------------------------------------------------------------------------
static class BlockingCollectionDemo
{
    public static void Run()
    {
        Report.Title("5. BlockingCollection<T>: fast producer, slow consumers, capacity 100");
        const int N = 10_000;

        using var work = new BlockingCollection<int>(boundedCapacity: 100); // wraps a ConcurrentQueue
        int maxQueued = 0, consumed = 0;
        long sum = 0;

        var producer = Task.Run(() =>
        {
            for (int i = 0; i < N; i++)
            {
                work.Add(i);                                    // waits while 100 items are queued
                maxQueued = Math.Max(maxQueued, work.Count);
            }
            work.CompleteAdding();                              // "no more items": ends the consumers' loops
        });

        var consumers = Enumerable.Range(0, 3).Select(_ => Task.Run(() =>
        {
            foreach (int item in work.GetConsumingEnumerable())  // sleeps when empty
            {
                Thread.SpinWait(2_000);                         // simulate slow work
                Interlocked.Add(ref sum, item);
                Interlocked.Increment(ref consumed);
            }
        })).ToArray();

        Task.WaitAll(consumers.Append(producer).ToArray());

        Report.Check("items consumed", N, consumed);
        Report.Check("sum of items", (long)N * (N - 1) / 2, sum);
        Report.Info($"most items ever queued: {maxQueued} (never more than 100: the producer had to wait)");
        Console.WriteLine();
    }
}

// ----------------------------------------------------------------------------
// 6. Channel<T>: the modern async version. A 3-stage pipeline:
//    produce numbers -> 3 workers square them -> one reader adds them up.
//    Waiting uses await, so no thread is blocked while nothing is happening.
// ----------------------------------------------------------------------------
static class ChannelDemo
{
    public static async Task RunAsync()
    {
        Report.Title("6. Channel<T>: async pipeline");
        const int N = 10_000;

        var numbers = Channel.CreateBounded<int>(new BoundedChannelOptions(capacity: 100)
        {
            FullMode = BoundedChannelFullMode.Wait,             // writer awaits when full
            SingleWriter = true,                                // lets the channel optimise
        });
        var squares = Channel.CreateUnbounded<long>();

        // Stage 1: one producer.
        var producer = Task.Run(async () =>
        {
            for (int i = 1; i <= N; i++) await numbers.Writer.WriteAsync(i);
            numbers.Writer.Complete();
        });

        // Stage 2: three workers share the input channel; each number goes to one worker.
        var workers = Enumerable.Range(0, 3).Select(_ => Task.Run(async () =>
        {
            await foreach (int n in numbers.Reader.ReadAllAsync())
                await squares.Writer.WriteAsync((long)n * n);
        })).ToArray();

        // When all workers have finished, close the second channel.
        var closeSquares = Task.Run(async () =>
        {
            await Task.WhenAll(workers);
            squares.Writer.Complete();
        });

        // Stage 3: add up the results as they arrive.
        long total = 0;
        int count = 0;
        await foreach (long sq in squares.Reader.ReadAllAsync())
        {
            total += sq;
            count++;
        }
        await Task.WhenAll(producer, closeSquares);

        Report.Check("results received", N, count);
        Report.Check("sum of squares 1..10,000 = n(n+1)(2n+1)/6", (long)N * (N + 1) * (2L * N + 1) / 6, total);
        Report.Info("Prefer Channel<T> for new async code; BlockingCollection blocks real threads while waiting.");
        Console.WriteLine();
    }
}