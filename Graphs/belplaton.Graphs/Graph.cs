namespace belplaton.Graphs
{
	public interface IGraph<TKey, TNode> where TKey : notnull
	{
		public int Size { get; }
		public int Capacity { get; }
		public GraphSettings Settings { get; }
		public IReadOnlyList<TNode> Nodes { get; }
		
		public (double?[,] adjacency, TKey[] keys, TNode[] nodeData) GetRawAdjacencyMatrix();
		public (List<(int index, double weight)> adjacency, TKey[] keys, TNode[] nodeData)? GetRawAdjacencyList(TKey key);
		
		public TNode? GetNode(TKey key);
		public bool TrySetNode(TKey key, TNode node);
		public void SetNode(TKey key, TNode node);
		
		public bool? GetEdge(TKey from, TKey to);
		public double? GetWeight(TKey from, TKey to);

		public bool TrySetEdgeData(TKey from, TKey to, double? weight);
		public void SetEdgeData(TKey from, TKey to, double? weight);
		
		public bool TryClearEdge(TKey from, TKey to);
		public void ClearEdge(TKey from, TKey to);
	}

	public interface IIndexedGraph<TNode> : IGraph<int, TNode>
	{
		
	}
}