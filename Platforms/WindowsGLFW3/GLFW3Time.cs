using DeltaEngine.Core;
using Pencil.Gaming;

namespace DeltaEngine.Platforms
{
	public class GLFW3Time : GlobalTime
	{
		private const long Precision = 1000000;

		protected override long TicksPerSecond
		{
			get
			{
				return Precision;
			}
		}

		public GLFW3Time()
		{
			Glfw.SetTime(0.0);
		}

		protected override long GetTicks()
		{
			return (long)(Precision * Glfw.GetTime());
		}
	}
}