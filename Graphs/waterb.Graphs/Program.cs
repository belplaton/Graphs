using waterb.Graphs;
using waterb.Graphs.MapAlgorithms;

internal static class Program
{
	public static void Main(string[] args)
	{
		const string filePath = "graph_input.txt";
		if (!File.Exists(filePath))
		{
			Console.WriteLine($"file'{filePath}' is not found!");
			return;
		}
		
		var lines = File.ReadAllLines(filePath);
		if (GraphMapParser.TryCreateGraphMap(lines, out var map))
		{
			var metrics = Enum.GetValues<DistanceMetric>();
			for (var i = 0; i < metrics.Length; i++)
			{
				var path = map!.FindPathAStar((13, 14), (6, 14), metrics[i]);
				if (path != null)
				{
					Console.WriteLine($"Metrics: {metrics[i]}");
					Console.Write(map?.PrepareMapInfoWithRoute(path));
					Console.WriteLine();
				}
			}
		}

		Console.WriteLine("any key to clos...");
		Console.ReadKey();
	}
}