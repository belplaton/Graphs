using System.Globalization;

namespace waterb.Graphs;

public static class GraphMapParser
{
	public static bool TryCreateGraphMap(string[] input, out GraphMap? map)
	{
		map = null;
		try
		{
			if (input.Length == 0) return false;

			var firstLine = input[0].Trim();
			var firstParts = firstLine.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
			var targetHeight = default(int?);
			var targetWidth = default(int?);
			var isUseOneDigitOffset = false;
			
			for (var y = 0; y < input.Length; y++)
			{
				var line = input[y].Trim();
				var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
				if (map == null)
				{
					if (targetHeight != null && targetWidth != null)
					{
						map = new GraphMap(targetHeight.Value, targetWidth.Value);
					}
					else
					{
						map = new GraphMap(input.Length, parts.Length);
					}
				}
				else if (map.Width != parts.Length)
				{
					map = null;
					if (firstParts.Length == 2)
					{
						targetHeight = int.Parse(firstParts[0], CultureInfo.InvariantCulture);
						targetWidth = int.Parse(firstParts[1], CultureInfo.InvariantCulture);
						isUseOneDigitOffset = true;
						y = 0;
						continue;
					}
					
					return false;
				}

				for (var x = 0; x < parts.Length; x++)
				{
					var offsetY = y - (isUseOneDigitOffset ? 1 : 0);
					map[x, offsetY] = double.Parse(parts[x], CultureInfo.InvariantCulture);
					if (map[x, offsetY] == 0) map[x, offsetY] = null;
				}
			}
			
			return map != null;
		}
		catch
		{
			return false;
		}
	}
}