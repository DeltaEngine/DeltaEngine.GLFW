using DeltaEngine;
using DeltaEngine.Platforms;

namespace $safeprojectname$
{
	internal class Program : App
	{
		public Program()
		{
			Resolve<Window>();
			new SideScrollerGame();
		}

		public static void Main()
		{
			new Program().Run();
		}
	}
}