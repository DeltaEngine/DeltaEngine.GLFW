namespace DeltaEngine.Platforms
{
	/// <summary>
	/// Initializes the GLFW3 resolver and the window to get started. To execute the app call Run.
	/// </summary>
	public abstract class App
	{
		protected void Run()
		{
			resolver.Run();
		}

		private readonly GLFW3Resolver resolver = new GLFW3Resolver();

		protected T Resolve<T>() where T : class
		{
			return resolver.Resolve<T>();
		}
	}
}