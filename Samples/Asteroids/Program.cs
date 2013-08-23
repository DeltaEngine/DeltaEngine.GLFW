using DeltaEngine.Platforms;

namespace Asteroids
{
	internal class Program : App
	{
		public Program()
		{
			new Game();
		}

		public static void Main()
		{
			new Program().Run();
		}
	}
}