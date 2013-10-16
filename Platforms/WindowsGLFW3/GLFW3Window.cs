using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Extensions;
using Pencil.Gaming;
using Color = DeltaEngine.Datatypes.Color;
using Orientation = DeltaEngine.Core.Orientation;
using Size = DeltaEngine.Datatypes.Size;

namespace DeltaEngine.Platforms
{
	/// <summary>
	/// GLFW3 window implementation for the Delta Engine to run applications in.
	/// </summary>
	public class GLFW3Window : Window
	{
		public GLFW3Window(Settings settings)
		{
			Glfw.SetErrorCallback(ErrorCallback);
			if (!Glfw.Init())
				throw new UnableToInitializeGLFW();
			if (GetGlfwMajorVersion() < 3)
				throw new UnableToInitializeShadersForGLFW();
			this.settings = settings;
			CreateWindow(settings.Resolution, settings.StartInFullscreen);
			new GLFW3Time();
			Title = StackTraceExtensions.GetEntryName();
			BackgroundColor = Color.Black;
			hwnd = GetForegroundWindow();
			Icon appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			if (appIcon != null && hwnd != IntPtr.Zero)
				SetIcon(appIcon);
		}

		private static void ErrorCallback(GlfwError code, string desc)
		{
			Logger.Warning("Glfw error: " + desc);
		}

		private class UnableToInitializeGLFW : Exception {}

		private static int GetGlfwMajorVersion()
		{
			int majorVersion;
			int minorVersion;
			int revision;
			Glfw.GetVersion(out majorVersion, out minorVersion, out revision);
			return majorVersion;
		}

		private class UnableToInitializeShadersForGLFW : Exception {}

		private readonly Settings settings;

		private void CreateWindow(Size resolution, bool startInFullscreen)
		{
			viewportSize = resolution;
			IsFullscreen = startInFullscreen;
			var width = (int)resolution.Width;
			var height = (int)resolution.Height;
			OpenWindow(startInFullscreen, width, height);
		}

		private void OpenWindow(bool startInFullscreen, int width, int height)
		{
			var monitor = startInFullscreen ? Glfw.GetPrimaryMonitor() : GlfwMonitorPtr.Null;
			SetupWindow();
			nativeWindow = Glfw.CreateWindow(width, height, "GLFW3", monitor, GlfwWindowPtr.Null);
			Glfw.MakeContextCurrent(nativeWindow);
			Glfw.SetWindowSizeCallback(nativeWindow, OnWindowResize);
		}

		private void SetupWindow()
		{
			var colorBits = GetColorBits();
			Glfw.WindowHint(WindowHint.RedBits, colorBits[0]);
			Glfw.WindowHint(WindowHint.GreenBits, colorBits[1]);
			Glfw.WindowHint(WindowHint.BlueBits, colorBits[2]);
			Glfw.WindowHint(WindowHint.AlphaBits, colorBits[3]);
			Glfw.WindowHint(WindowHint.DepthBits, settings.DepthBufferBits);
			Glfw.WindowHint(WindowHint.Samples, settings.AntiAliasingSamples);
		}

		private int[] GetColorBits()
		{
			if (settings.ColorBufferBits >= 24)
				return new[] { 8, 8, 8, settings.ColorBufferBits >= 32 ? 8 : 0 };
			if (settings.ColorBufferBits >= 16)
				return new[] { 5, 6, 5, 0 };
			throw new UnsupportedFramebufferFormat();
		}

		private class UnsupportedFramebufferFormat : Exception {}

		private GlfwWindowPtr nativeWindow;

		private void OnWindowResize(GlfwWindowPtr window, int width, int height)
		{
			if (width == 0 || height == 0)
				return;
			viewportSize = new Size(width, height);
			Orientation = width > height ? Orientation.Landscape : Orientation.Portrait;
			if (ViewportSizeChanged != null)
				ViewportSizeChanged(ViewportPixelSize);
			if (OrientationChanged != null)
				OrientationChanged(Orientation);
		}

		public string Title
		{
			get { return title; }
			set
			{
				title = value;
				Glfw.SetWindowTitle(nativeWindow, title);
			}
		}

		private string title;

		public Orientation Orientation { get; private set; }

		public event Action<Size> ViewportSizeChanged;
		public event Action<Orientation> OrientationChanged;

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern int DrawMenuBar(int currentWindow);

		private const int WMSeticon = 0x80;
		private const int IconSmall = 0;
		private readonly IntPtr hwnd;

		private void SetIcon(Icon icon)
		{
			SendMessage(hwnd, WMSeticon, (IntPtr)IconSmall, icon.Handle);
			DrawMenuBar((int)hwnd);
		}

		public bool IsVisible
		{
			get { return true; }
		}

		public object Handle
		{
			get { return nativeWindow; }
		}

		public Size ViewportPixelSize
		{
			get { return viewportSize; }
			set
			{
				viewportSize = value;
				Glfw.SetWindowSize(nativeWindow, (int)value.Width, (int)value.Height);
				OnWindowResize(nativeWindow, (int)value.Width, (int)value.Height);
			}
		}

		private Size viewportSize;

		public Size TotalPixelSize
		{
			get { return viewportSize; }
		}

		public Vector2D PixelPosition
		{
			get { return position; }
			set
			{
				position = value;
				Glfw.SetWindowPos(nativeWindow, (int)value.X, (int)value.Y);
			}
		}

		private Vector2D position;

		public Color BackgroundColor { get; set; }

		public virtual void SetFullscreen(Size setFullscreenViewportSize)
		{
			windowedSize = viewportSize;
			Glfw.DestroyWindow(nativeWindow);
			CreateWindow(setFullscreenViewportSize, true);
			if (FullscreenChanged != null)
				FullscreenChanged(setFullscreenViewportSize, true);
		}

		private Size windowedSize;

		public void SetWindowed()
		{
			Glfw.DestroyWindow(nativeWindow);
			CreateWindow(windowedSize, false);
			if (FullscreenChanged != null)
				FullscreenChanged(windowedSize, false);
		}

		public bool IsFullscreen { get; private set; }
		public event Action<Size, bool> FullscreenChanged;

		public virtual bool IsClosing
		{
			get { return rememberToClose || Glfw.WindowShouldClose(nativeWindow); }
		}
		private bool rememberToClose;

		public bool ShowCursor
		{
			get { return !remDisabledShowCursor; }
			set
			{
				if (remDisabledShowCursor != value)
					return;

				remDisabledShowCursor = !value;
				if (remDisabledShowCursor)
					Cursor.Hide();
				else
					Cursor.Show();
			}
		}

		public string ShowMessageBox(string caption, string message, string[] buttons)
		{
			var buttonCombination = MessageBoxButtons.OK;
			if (buttons.Contains("Cancel"))
				buttonCombination = MessageBoxButtons.OKCancel;
			if (buttons.Contains("Ignore") || buttons.Contains("Abort") || buttons.Contains("Retry"))
				buttonCombination = MessageBoxButtons.AbortRetryIgnore;
			if (buttons.Contains("Yes") || buttons.Contains("No"))
				buttonCombination = MessageBoxButtons.YesNo;
			return MessageBox.Show(message, Title + " " + caption, buttonCombination).ToString();
		}

		/// <summary>
		/// Clipboard.SetText must be executed on a STA thread, which we are not, create extra thread!
		/// </summary>
		public void CopyTextToClipboard(string text)
		{
			var staThread = new Thread(() => TrySetClipboardText(text));
			staThread.SetApartmentState(ApartmentState.STA);
			staThread.Start();
		}

		private static void TrySetClipboardText(string text)
		{
			try
			{
				Clipboard.SetText(text, TextDataFormat.Text);
			}
			catch (Exception)
			{
				Logger.Warning("Failed to set clipboard text: " + text);
			}
		}

		private bool remDisabledShowCursor;

		public void Present()
		{
			Glfw.PollEvents();
			AllowAltF4ToCloseWindow();
		}

		private void AllowAltF4ToCloseWindow()
		{
			if (Glfw.GetKey(nativeWindow, Key.LeftAlt) && Glfw.GetKey(nativeWindow, Key.F4))
				CloseAfterFrame();
		}

		public void CloseAfterFrame()
		{
			rememberToClose = true;
		}

		public void Dispose()
		{
			CloseAfterFrame();
			Glfw.DestroyWindow(nativeWindow);
			Glfw.Terminate();
		}
	}
}