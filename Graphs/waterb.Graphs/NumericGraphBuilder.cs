namespace waterb.Graphs;

#nullable disable

public class NumericGraphBuilder : IGraphBuilder<GraphMatrix<int, int>, int, int>, INumericGraphInputCollector
{
	private readonly HashSet<
		(NodeDataPair<int, int> from, NodeDataPair<int, int> to, double? weight)> _ribs = [];
	private readonly HashSet<
		(NodeDataPair<int, int> from, NodeDataPair<int, int> to, double? weight)> _missingReverseRibs = [];
	private GraphSettings _desiredSettings;
	private int _maxVertex;
	private bool _isAnyWeighted;
	private bool _isRequireNodeOneOffset = true;

	private GraphSettings _settings => _desiredSettings |
		(_isAnyWeighted ? GraphSettings.IsWeighted : GraphSettings.None) |
		(_missingReverseRibs.Count != 0 ? GraphSettings.IsDirected : GraphSettings.None);

	public NumericGraphBuilder SetDesiredSettings(GraphSettings settings)
	{
		_desiredSettings = settings;
		return this;
	}
	
	public NumericGraphBuilder AddRib(NodeDataPair<int, int> from, NodeDataPair<int, int> to, double? weight)
	{
		if (weight != null) _isAnyWeighted = true;
		if (from.node == 0 || to.node == 0) _isRequireNodeOneOffset = false;

		var data = (from, to, weight);
		var reverseData = (to, from, weight);
		if (!_ribs.Contains(reverseData)) _missingReverseRibs.Add(reverseData);
		
		_missingReverseRibs.Remove(data);
		_ribs.Add((from, to, weight));
		
		_maxVertex = Math.Max(_maxVertex, Math.Max(to.node, from.node));
		return this;
	}
	
	public NumericGraphBuilder Clear()
	{
		_ribs.Clear();
		_missingReverseRibs.Clear();
		_desiredSettings = GraphSettings.None;
		_maxVertex = 0;
		_isAnyWeighted = false;
		_isRequireNodeOneOffset = true;
		return this;
	}

	public NumericGraphBuilder ParsePayloadInput<TParser>(string[] input)
		where TParser : INumericGraphInputParser, new()
	{
		var parser = new TParser();
		parser.TryParse(this, input);
		
		return this;
	}
	
	public void Collect((NodeDataPair<int, int> from, NodeDataPair<int, int> to, double? weight) data)
	{
		AddRib(data.from, data.to, data.weight);
	}

	public void ClearInput() => Clear();
	
	public GraphMatrix<int, int> CreateGraph()
	{
		var graph = new GraphMatrix<int, int>(_settings);

		var offset = _isRequireNodeOneOffset ? 1 : 0;
		for (var i = 0; i < _maxVertex; i++)
		{
			graph.AddNode(i + offset, default);
		}

		foreach (var rib in _ribs)
		{
			graph.SetData(rib.from.node, rib.from.data);
			graph.SetData(rib.to.node, rib.to.data);
			graph.SetEdge(rib.from.node, rib.to.node, rib.weight);
		}

		return graph;
	}
}