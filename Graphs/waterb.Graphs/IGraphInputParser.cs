namespace waterb.Graphs;

public interface IGraphInputParser<TNode, TData, in TGraphInputCollector>
	where TGraphInputCollector : IGraphInputCollector<TNode, TData>
{
	public bool TryParse(TGraphInputCollector destination, string[] input);
}