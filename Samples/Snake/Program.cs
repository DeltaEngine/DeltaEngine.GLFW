using DeltaEngine;
using DeltaEngine.Platforms;
using DeltaEngine.ScreenSpaces;

namespace Snake
{
	internal class Program : App
	{
		public Program()
		{
			new SnakeGame(Resolve<Window>());
		}

		public static void Main()
		{
			new Program().Run();
		}
	}
}