namespace waterb.Graphs
{
	[Flags]
	public enum GraphSettings
	{
		None = 0,
		IsDirected = 1 << 0,
		IsWeighted = 1 << 1
	}
}