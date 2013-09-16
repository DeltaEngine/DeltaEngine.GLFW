using System;
using System.IO;
using System.Linq;
using DeltaEngine.Graphics.GLFW3;
using DeltaEngine.Input.GLFW3;
using DeltaEngine.Multimedia.GLFW;
using DeltaEngine.Platforms.Windows;

namespace DeltaEngine.Platforms
{
	internal class GLFW3Resolver : AppRunner
	{
		private readonly string[] glfwDllsNeeded = { "glfw3.dll", "openal32.dll", "wrap_oal.dll" };

		public GLFW3Resolver()
		{
			RegisterCommonEngineSingletons();
			MakeSureGlfwDllsAreAvailable();
			RegisterSingleton<WindowsSystemInformation>();
			RegisterSingleton<GLFW3Window>();
			RegisterSingleton<GLFW3Device>();
			RegisterSingleton<GLFW3ScreenshotCapturer>();
			RegisterSingleton<GLFWSoundDevice>();
			RegisterSingleton<GLFWMouse>();
			RegisterSingleton<GLFWTouch>();
			RegisterSingleton<GLFWGamePad>();
			RegisterSingleton<GLFWKeyboard>();
			if (IsAlreadyInitialized)
				throw new UnableToRegisterMoreTypesAppAlreadyStarted();
		}

		protected override void RegisterMediaTypes()
		{
			base.RegisterMediaTypes();
			Register<GLFW3Image>();
			Register<GLFW3Shader>();
			Register<GLFW3Geometry>();
			Register<GLFWSound>();
			Register<GLFWMusic>();
			Register<GLFWVideo>();
		}

		private void MakeSureGlfwDllsAreAvailable()
		{
			if (AreNativeDllsMissing())
				TryCopyNativeDlls();
		}

		private bool AreNativeDllsMissing()
		{
			return glfwDllsNeeded.Any(dll => !File.Exists(dll));
		}

		private void TryCopyNativeDlls()
		{
			try
			{
				CopyNativeDlls();
			}
			catch (Exception ex)
			{
				throw new FailedToCopyNativeGlfwDllFiles("Please provide the glfw .dll files!", ex);
			}
		}

		private void CopyNativeDlls()
		{
			var path = Path.Combine("..", "..");
			while (!IsPackagesDirectory(path))
			{
				path = Path.Combine(path, "..");
				if (path.Length > 3 * 6)
					break;
			}
			CopyGlfwDlls(path);
		}

		private void CopyGlfwDlls(string path)
		{
			foreach (var dll in glfwDllsNeeded)
				File.Copy(Path.Combine(path, "packages", "Pencil.Gaming.GLFW3.1.0.4953", "NativeBinaries", "x86", dll), dll, true);
		}

		private static bool IsPackagesDirectory(string path)
		{
			return Directory.Exists(Path.Combine(path, "packages"));
		}

		private class FailedToCopyNativeGlfwDllFiles : Exception
		{
			public FailedToCopyNativeGlfwDllFiles(string message, Exception innerException)
				: base(message,innerException)
			{
			}
		}
	}
}