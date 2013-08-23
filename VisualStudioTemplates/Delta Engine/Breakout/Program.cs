using DeltaEngine;
using DeltaEngine.Datatypes;
using DeltaEngine.Multimedia;
using DeltaEngine.Platforms;
using DeltaEngine.ScreenSpaces;

namespace $safeprojectname$
{
	public class Program : App
	{
		public Program()
		{
			Resolve<Settings>().UpdatesPerSecond = 60;
			new Game(Resolve<Window>(), Resolve<SoundDevice>());
		}

		public static void Main()
		{
			new Program().Run();
		}
	}
}