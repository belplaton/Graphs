using System.Text;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public struct FloydWarshallData<TNode> where TNode : notnull
	{
		public readonly Dictionary<TNode, int>  nodesDegrees = new();
		public readonly Dictionary<TNode, double> nodesEccentricity = new();
		public readonly List<(int index, TNode node)> peripheralNodes = new();
		public readonly List<(int index, TNode node)> centralNodes = new();
		public double radius = 0;
		public double diameter = 0;

		public FloydWarshallData()
		{
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine("Vertices degrees:");
			sb.Append('[');
			var appendCounter = 0;
			using var degreesEnumerator = nodesDegrees.GetEnumerator();
			while (degreesEnumerator.MoveNext())
			{
				sb.Append(appendCounter + 1 < nodesDegrees.Count ? 
					$"{degreesEnumerator.Current.Value}, " : $"{degreesEnumerator.Current.Value}");
				appendCounter++;
			}

			sb.Append("]\n");
			sb.AppendLine("Eccentricity:");
			sb.Append('[');
			appendCounter = 0;
			using var eccentricityEnumerator = nodesEccentricity.GetEnumerator();
			while (eccentricityEnumerator.MoveNext())
			{
				sb.Append(appendCounter + 1 < nodesEccentricity.Count ? 
					$"{eccentricityEnumerator.Current.Value}, " : $"{eccentricityEnumerator.Current.Value}");
				appendCounter++;
			}
			
			sb.Append("]\n");
			sb.AppendLine($"R = {radius}");
			sb.AppendLine("Central vertices:");
			sb.Append('[');
			for (var i = 0; i < centralNodes.Count; i++)
			{
				sb.Append(i + 1 < centralNodes.Count ? $"{centralNodes[i]}, " : $"{centralNodes[i]}");
			}

			sb.Append("]\n");
			sb.AppendLine($"D = {diameter}");
			sb.AppendLine("Peripheral vertices");
			sb.Append('[');
			for (var i = 0; i < peripheralNodes.Count; i++)
			{
				sb.Append(i + 1 < peripheralNodes.Count ? $"{peripheralNodes[i]}, " : $"{peripheralNodes[i]}");
			}

			sb.Append("]\n");
			return sb.ToString();
		}
	}
}