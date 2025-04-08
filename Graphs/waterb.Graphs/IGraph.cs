namespace waterb.Graphs
{
	public interface IGraph<TNode, TData>
	{
		public int Size { get; }
		public GraphSettings Settings { get; }
		public IReadOnlyList<TNode> Nodes { get; }
		public IReadOnlyList<TData> NodeData { get; }
		
		public IReadOnlyList<IReadOnlyList<double?>> GetRawAdjacencyMatrix();
		public List<RibData<TNode>> GetIncidentRibs();
		public List<RibData<TNode>>? GetIncidentRibs(TNode node);
		
		public IReadOnlyList<double?> this[TNode node] { get; }
		public IReadOnlyList<double?> this[int index] { get; }
		
		public TData? GetData(TNode node);
		public int? GetIndex(TNode node);
		public void SetData(TNode node, TData data);
		
		public bool TryAddNode(TNode node, TData data);
		public void AddNode(TNode node, TData data);

		public bool RemoveNode(TNode node);
		
		public bool? GetEdge(TNode from, TNode to);
		public double? GetWeight(TNode from, TNode to);

		public bool TrySetEdge(TNode from, TNode to, double? weight);
		public void SetEdge(TNode from, TNode to, double? weight);
		
		public bool TryClearEdge(TNode from, TNode to);
		public void ClearEdge(TNode from, TNode to);
	}
}