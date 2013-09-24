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
			if (TryCopyNativeDllsFromNuGetPackage())
				return;
			if (TryCopyNativeDllsFromDeltaEnginePath())
				return;
			throw new FailedToCopyNativeGlfwDllFiles("GLFW dlls not found inside the application " + "output directory nor inside the %DeltaEnginePath% environment variable. Make sure it's " + "set and containing the required files: " + string.Join(",", glfwDllsNeeded));
		}

		private bool TryCopyNativeDllsFromNuGetPackage()
		{
			var nuGetPackagesPath = FindNuGetPackagesPath();
			string nativeBinariesPath = Path.Combine(nuGetPackagesPath, "packages", "Pencil.Gaming.GLFW3.1.0.4953", "NativeBinaries", "x86");
			if (!Directory.Exists(nativeBinariesPath))
				return false;
			CopyNativeDllsFromPath(nativeBinariesPath);
			return true;
		}

		private static string FindNuGetPackagesPath()
		{
			int MaxPathLength = 18;
			var path = Path.Combine("..", "..");
			while (!IsPackagesDirectory(path))
			{
				path = Path.Combine(path, "..");
				if (path.Length > MaxPathLength)
					break;
			}
			return path;
		}

		private void CopyNativeDllsFromPath(string nativeBinariesPath)
		{
			foreach (var nativeDll in glfwDllsNeeded)
				File.Copy(Path.Combine(nativeBinariesPath, nativeDll), nativeDll, true);
		}

		private static bool IsPackagesDirectory(string path)
		{
			return Directory.Exists(Path.Combine(path, "packages"));
		}

		private bool TryCopyNativeDllsFromDeltaEnginePath()
		{
			string enginePath = Environment.GetEnvironmentVariable("DeltaEnginePath");
			if (enginePath == null || !Directory.Exists(enginePath))
				return false;
			CopyNativeDllsFromPath(Path.Combine(enginePath, "GLFW"));
			return true;
		}

		private class FailedToCopyNativeGlfwDllFiles : Exception
		{
			public FailedToCopyNativeGlfwDllFiles(string message)
				: base(message)
			{
			}
		}
	}
}