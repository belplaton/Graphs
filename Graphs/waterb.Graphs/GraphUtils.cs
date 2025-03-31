namespace waterb.Graphs;

public static class GraphUtils
{
	public static double GetWeight(this double? value) => value ?? double.PositiveInfinity;
}