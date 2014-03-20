using System;
using System.IO;
using System.Linq;
using DeltaEngine.Content.Json;
using DeltaEngine.Graphics.GLFW3;
using DeltaEngine.Input.GLFW3;
using DeltaEngine.Multimedia.GLFW;
using DeltaEngine.Physics2D;
using DeltaEngine.Physics2D.Farseer;
using DeltaEngine.Physics3D;
using DeltaEngine.Physics3D.Jitter;
using DeltaEngine.Platforms.Windows;
using DeltaEngine.Content.Xml;
using DeltaEngine.Graphics;
using DeltaEngine.Rendering2D;
using DeltaEngine.Rendering3D;
using DeltaEngine.Core;
using DeltaEngine.Extensions;

namespace DeltaEngine.Platforms
{
	public class GLFW3Resolver : AppRunner
	{
		private readonly string[] glfwDllsNeeded = { "glfw3.dll", "glfw3winxpvista.dll", "openal32.dll", "wrap_oal.dll" };

		public GLFW3Resolver()
		{
			try
			{
				TryInitializeGLFW();
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
				if (StackTraceExtensions.StartedFromNCrunchOrNunitConsole)
					throw;
				DisplayMessageBoxAndCloseApp("Fatal GLFW Initialization Error", exception);
			}
		}

		private void TryInitializeGLFW()
		{
			RegisterCommonEngineSingletons();
			MakeSureGlfwDllsAreAvailable();
			UseWinXpVistaVersionOfGlfwIfOnThatPlatform();
			RegisterSingleton<WindowsSystemInformation>();
			RegisterSingleton<GLFW3Window>();
			RegisterSingleton<GLFW3Device>();
			RegisterSingleton<Drawing>();
			RegisterSingleton<BatchRenderer2D>();
			RegisterSingleton<BatchRenderer3D>();
			RegisterSingleton<OpenGL20ScreenshotCapturer>();
			RegisterSingleton<GLFWSoundDevice>();
			RegisterSingleton<GLFWMouse>();
			RegisterSingleton<GLFWTouch>();
			RegisterSingleton<GLFWGamePad>();
			RegisterSingleton<GLFWKeyboard>();
			Register<InputCommands>();
			if (IsAlreadyInitialized)
				throw new UnableToRegisterMoreTypesAppAlreadyStarted();
		}

		protected override void RegisterMediaTypes()
		{
			base.RegisterMediaTypes();
			Register<OpenGL20Image>();
			Register<GLFW3Shader>();
			Register<OpenGL20Geometry>();
			Register<OpenALSound>();
			Register<GLFWMusic>();
			Register<GLFWVideo>();
			Register<XmlContent>();
			Register<JsonContent>();
		}

		protected override void RegisterPhysics()
		{
			RegisterSingleton<FarseerPhysics>();
			RegisterSingleton<JitterPhysics>();
			Register<AffixToPhysics2D>();
			Register<AffixToPhysics3D>();
		}

		private void MakeSureGlfwDllsAreAvailable()
		{
			if (AreNativeDllsMissing())
				TryCopyNativeDlls();
		}

		private bool AreNativeDllsMissing()
		{
			return glfwDllsNeeded.Any((string dll) => !File.Exists(dll));
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
			string nuGetPackagesPath = FindNuGetPackagesPath();
			string nativeBinariesPath = Path.Combine(nuGetPackagesPath, "packages", "Pencil.Gaming.GLFW3.1.0.4955", "NativeBinaries", "x86");
			if (!Directory.Exists(nativeBinariesPath))
				return false;
			CopyNativeDllsFromPath(nativeBinariesPath);
			return true;
		}

		private static string FindNuGetPackagesPath()
		{
			const int MaxPathLength = 18;
			string path = Path.Combine("..", "..");
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
			foreach (string nativeDll in glfwDllsNeeded)
				File.Copy(Path.Combine(nativeBinariesPath, nativeDll), nativeDll, true);
		}

		private static bool IsPackagesDirectory(string path)
		{
			return Directory.Exists(Path.Combine(path, "packages"));
		}

		private bool TryCopyNativeDllsFromDeltaEnginePath()
		{
			string enginePath = Environment.GetEnvironmentVariable("DeltaEnginePath");
			if (enginePath == null || !Directory.Exists(Path.Combine(enginePath, "GLFW")))
				return false;
			CopyNativeDllsFromPath(Path.Combine(enginePath, "GLFW"));
			return true;
		}

		private static void UseWinXpVistaVersionOfGlfwIfOnThatPlatform()
		{
			Version version = Environment.OSVersion.Version;
			bool isWin7OrHigher = version.Major >= 6 && version.Minor > 0;
			if (isWin7OrHigher || File.Exists("glfw3win7.dll"))
				return;
			File.Move("glfw3.dll", "glfw3win7.dll");
			File.Copy("glfw3winxpvista.dll", "glfw3.dll", true);
		}

		private class FailedToCopyNativeGlfwDllFiles : Exception
		{
			public FailedToCopyNativeGlfwDllFiles(string message)
				: base(message) {}
		}
	}
}