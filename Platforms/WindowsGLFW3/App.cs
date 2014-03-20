using DeltaEngine.Core;

namespace DeltaEngine.Platforms
{
	public abstract class App
	{
		private readonly GLFW3Resolver resolver = new GLFW3Resolver();

		protected App() {}

		protected App(Window windowToRegister)
		{
			resolver.RegisterInstance(windowToRegister);
		}

		protected void Run()
		{
			resolver.Run();
			resolver.Dispose();
		}

		protected T Resolve<T>()
			where T : class
		{
			return resolver.Resolve<T>();
		}
	}
}