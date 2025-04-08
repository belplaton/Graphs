namespace waterb.Graphs;

#nullable disable

public class NumericGraphBuilder<TNode> : IGraphBuilder<GraphMatrix<int, TNode>, int, TNode>, INumericGraphInputCollector
{
	private int _maxVertex;
	private readonly List<(int from, int to, double? weight)> _ribsPayloadData = new();
	
	public NumericGraphBuilder<TNode> AddRib(int from, int to, double? weight)
	{
		_ribsPayloadData.Add((from, to, weight));
		_maxVertex = Math.Max(_maxVertex, Math.Max(to, from));
		return this;
	}
	
	public NumericGraphBuilder<TNode> Clear()
	{
		_ribsPayloadData.Clear();
		_maxVertex = 0;
		return this;
	}

	public NumericGraphBuilder<TNode> ParsePayloadInput<TParser>(string[] input, bool isWeighted)
		where TParser : INumericGraphInputParser, new()
	{
		var parser = new TParser();
		parser.TryParse(this, input, (_settings & GraphSettings.IsWeighted) != 0);
		
		return this;
	}
	
	public void Collect((int from, int to, double? weight) data)
	{
		AddRib(data.from, data.to, data.weight);
	}

	public void ClearInput() => Clear();
	
	public GraphMatrix<int, TNode> CreateGraph()
	{
		var graph = new GraphMatrix<int, TNode>(_settings);

		for (var i = graph.Size; i < _maxVertex; i++)
		{
			graph.AddNode(i + 1, default);
		}
		
		for (var i = 0; _ribsPayloadData != null && i < _ribsPayloadData.Count; i++)
		{
			graph.SetData(_ribsPayloadData[i].from, default);
			graph.SetData(_ribsPayloadData[i].to, default);
			graph.SetEdgeData(_ribsPayloadData[i].from, _ribsPayloadData[i].to,
				(_settings & GraphSettings.IsWeighted) != 0 ? _ribsPayloadData[i].weight : null);
		}

		return graph;
	}
}