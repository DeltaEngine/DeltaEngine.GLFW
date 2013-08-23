using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;

namespace DeltaEngine.Rendering.Graphs
{
	internal class FpsProfilingGraph : SystemProfilingGraph
	{
		public FpsProfilingGraph()
			: base(Rectangle.FromCenter(0.2f, 0.5f, 0.3f, 0.2f), ProfilingMode.Fps, Color.Yellow)
		{
			Viewport = new Rectangle(0.0f, 0.0f, 100.0f, 60.0f);
		}
	}
}