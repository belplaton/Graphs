#nullable disable
namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public interface IDFSEnumerator<TNode, TData, out TDFSNode> : IEnumerator<TDFSNode>
		where TDFSNode : struct, IDFSEnumerator<TNode, TData, TDFSNode>.IDFSNode
	{
		public interface IDFSNode
		{
			public TNode Node { get; init; }
			public int Depth { get; init; }
		}
	}
}