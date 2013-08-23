using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;

namespace DeltaEngine.Rendering.Graphs
{
	internal class AvailableRamProfilingGraph : SystemProfilingGraph
	{
		public AvailableRamProfilingGraph()
			: base(Rectangle.FromCenter(0.6f, 0.5f, 0.3f, 0.2f), ProfilingMode.AvailableRAM, Color.Green)
		{
			Viewport = new Rectangle(0.0f, 0.0f, 100.0f, 1.0f);
			IsAutogrowing = true;
		}
	}
}