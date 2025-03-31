using System.Text;

namespace waterb.Graphs.GraphAlgorithms;

public static partial class GraphAlgorithms
{
	public class FloydWarshallData
	{
		public readonly double?[][] dist;
		public readonly int[] degreesVector;
		public readonly double[] eccentricity;
		public readonly List<int> peripheralIndexes;
		public readonly List<int> centralIndexes;
		public double radius;
		public double diameter;
        
		public FloydWarshallData(int size)
		{
			dist = new double?[size][];
			for (var i = 0; i < size; i++)
			{
				dist[i] = new double?[size];
			}

			degreesVector = new int [size];
			eccentricity = new double[size];
			peripheralIndexes = new List<int>();
			centralIndexes = new List<int>();
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine("Vertices degrees:");
			sb.Append('[');
			for (var i = 0; i < degreesVector.Length; i++)
			{
				sb.Append(i + 1 < degreesVector.Length ? $"{degreesVector[i]}, " : $"{degreesVector[i]}");
			}

			sb.Append("]\n");
            
			sb.AppendLine("Eccentricity:");
			sb.Append('[');
			for (var i = 0; i < eccentricity.Length; i++)
			{
				sb.Append(i + 1 < eccentricity.Length ? $"{eccentricity[i]}, " : $"{eccentricity[i]}");
			}

			sb.Append("]\n");
			
			sb.AppendLine($"R = {radius}");
			sb.AppendLine("Central vertices:");
			sb.Append('[');
			for (var i = 0; i < centralIndexes.Count; i++)
			{
				sb.Append(i + 1 < centralIndexes.Count ? $"{centralIndexes[i]}, " : $"{centralIndexes[i]}");
			}

			sb.Append("]\n");
			
			sb.AppendLine($"D = {diameter}");
			sb.AppendLine("Peripheral vertices");
			sb.Append('[');
			for (var i = 0; i < peripheralIndexes.Count; i++)
			{
				sb.Append(i + 1 < peripheralIndexes.Count ? $"{peripheralIndexes[i]}, " : $"{peripheralIndexes[i]}");
			}

			sb.Append("]\n");
			return sb.ToString();
		}
	}
}