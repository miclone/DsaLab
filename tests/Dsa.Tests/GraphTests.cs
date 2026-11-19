namespace Dsa.Tests;

public class GraphTests
{
    // The classic 6-node example used in many Dijkstra explanations.
    static Graph ClassicGraph()
    {
        var g = new Graph(6, directed: false);
        g.AddEdge(0, 1, 7); g.AddEdge(0, 2, 9); g.AddEdge(0, 5, 14);
        g.AddEdge(1, 2, 10); g.AddEdge(1, 3, 15); g.AddEdge(2, 3, 11);
        g.AddEdge(2, 5, 2); g.AddEdge(3, 4, 6); g.AddEdge(4, 5, 9);
        return g;
    }

    // Connected undirected graph: a random spanning chain plus extra random edges.
    static Graph RandomConnectedGraph(int v, int extraEdges, int seed)
    {
        var rng = new Random(seed);
        var g = new Graph(v, directed: false);
        var order = Enumerable.Range(0, v).OrderBy(_ => rng.Next()).ToArray();
        for (int i = 1; i < v; i++) g.AddEdge(order[i - 1], order[i], rng.Next(1, 50));
        for (int i = 0; i < extraEdges; i++)
        {
            int a = rng.Next(v), b = rng.Next(v);
            if (a != b) g.AddEdge(a, b, rng.Next(1, 50));
        }
        return g;
    }

    [Fact]
    public void Dijkstra_ClassicGraph() =>
        Assert.Equal(new[] { 0, 7, 9, 20, 20, 11 }, GraphAlgorithms.Dijkstra(ClassicGraph(), 0));

    [Fact]
    public void Bfs_VisitsByLevel_Dfs_GoesDeep()
    {
        Assert.Equal(new[] { 0, 1, 2, 5, 3, 4 }, GraphAlgorithms.Bfs(ClassicGraph(), 0));
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, GraphAlgorithms.Dfs(ClassicGraph(), 0));
    }

    [Fact]
    public void Bfs_And_Dfs_OnlyReachConnectedVertices()
    {
        var g = new Graph(5, directed: false);
        g.AddEdge(0, 1); g.AddEdge(3, 4);
        Assert.Equal(new[] { 0, 1 }, GraphAlgorithms.Bfs(g, 0).Order());
        Assert.Equal(new[] { 3, 4 }, GraphAlgorithms.Dfs(g, 3).Order());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void ShortestPathAlgorithms_AgreeOnRandomGraphs(int seed)
    {
        var g = RandomConnectedGraph(30, 60, seed);
        var floyd = GraphAlgorithms.FloydWarshall(g);

        for (int source = 0; source < g.V; source += 7)
        {
            var dijkstra = GraphAlgorithms.Dijkstra(g, source);
            var bellman = GraphAlgorithms.BellmanFord(g, source);
            Assert.NotNull(bellman);
            Assert.Equal(dijkstra, bellman);
            for (int t = 0; t < g.V; t++) Assert.Equal(dijkstra[t], floyd[source, t]);
        }
    }

    [Fact]
    public void BellmanFord_HandlesNegativeEdges()
    {
        var g = new Graph(4, directed: true);
        g.AddEdge(0, 1, 4); g.AddEdge(0, 2, 5); g.AddEdge(2, 1, -3); g.AddEdge(1, 3, 2);
        Assert.Equal(new[] { 0, 2, 5, 4 }, GraphAlgorithms.BellmanFord(g, 0));
    }

    [Fact]
    public void BellmanFord_DetectsNegativeCycle()
    {
        var g = new Graph(3, directed: true);
        g.AddEdge(0, 1, 1); g.AddEdge(1, 2, -2); g.AddEdge(2, 1, 1);
        Assert.Null(GraphAlgorithms.BellmanFord(g, 0));
    }

    [Fact]
    public void Mst_ClassicGraph() =>
        Assert.Equal(33, GraphAlgorithms.KruskalMst(ClassicGraph()));

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(30)]
    public void Kruskal_And_Prim_Agree(int seed)
    {
        var g = RandomConnectedGraph(40, 120, seed);
        Assert.Equal(GraphAlgorithms.KruskalMst(g), GraphAlgorithms.PrimMst(g));
    }

    [Fact]
    public void TopologicalSort_RespectsEveryEdge()
    {
        var dag = new Graph(6, directed: true);
        dag.AddEdge(5, 2); dag.AddEdge(5, 0); dag.AddEdge(4, 0);
        dag.AddEdge(4, 1); dag.AddEdge(2, 3); dag.AddEdge(3, 1);

        var order = GraphAlgorithms.TopologicalSort(dag);
        Assert.Equal(dag.V, order.Count);

        var position = new int[dag.V];
        for (int i = 0; i < order.Count; i++) position[order[i]] = i;
        foreach (var (u, v, _) in dag.Edges()) Assert.True(position[u] < position[v], $"{u}->{v} out of order");
        Assert.False(GraphAlgorithms.HasCycleDirected(dag));
    }

    [Fact]
    public void CycleIsDetected_AndTopologicalSortIsIncomplete()
    {
        var g = new Graph(3, directed: true);
        g.AddEdge(0, 1); g.AddEdge(1, 2); g.AddEdge(2, 0);
        Assert.True(GraphAlgorithms.HasCycleDirected(g));
        Assert.True(GraphAlgorithms.TopologicalSort(g).Count < g.V);
    }
}
