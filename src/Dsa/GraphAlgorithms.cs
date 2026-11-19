// PART 5 — Graph algorithms
using System.Buffers;
using System.Text;

namespace Dsa;

public static class GraphAlgorithms
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
