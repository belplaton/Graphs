namespace waterb.Graphs;

public static partial class GraphTester
{
    public static void PrepareResults<TNumericGraphInputParser>(
        string pathInput, string pathOutput, GraphTestType graphTestType)
        where TNumericGraphInputParser : INumericGraphInputParser
    {
        if (!File.Exists(pathInput))
        {
            Console.WriteLine($"file'{pathInput}' is not found!");
            return;
        }
		
        var lines = File.ReadAllLines(pathInput);
        if (GraphTestType.Graph)
        var builder = new NumericGraphBuilder<int>();
        builder
            .SetSettings(GraphSettings.IsDirected | GraphSettings.IsWeighted)
            .ParsePayloadInput<RibsListNumericGraphInputParser>(lines, true);
    }
}