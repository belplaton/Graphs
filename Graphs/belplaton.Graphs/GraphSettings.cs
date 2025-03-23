namespace belplaton.Graphs
{
	[Flags]
	public enum GraphSettings
	{
		None = 0,
		IsDirected = 1 << 0,
		IsWeighed = 1 << 2
	}
}