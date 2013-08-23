using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DeltaEngine.Content;
using DeltaEngine.Content.Online;
using DeltaEngine.Content.Xml;
using DeltaEngine.Core;
using DeltaEngine.Entities;
using DeltaEngine.Extensions;
using DeltaEngine.Graphics;
using DeltaEngine.Logging;
using DeltaEngine.Networking.Tcp;
using Microsoft.Win32;

namespace DeltaEngine.Platforms
{
	/// <summary>
	/// Starts an application on demand by registering, resolving and running it (via EntitiesRunner)
	/// </summary>
	public abstract class AppRunner : ApproveFirstFrameScreenshot
	{
		protected void RegisterCommonEngineSingletons()
		{
			CreateDefaultLoggers();
			CreateContentLoader();
			LoadSettingsAndCommands();
			RegisterInstance(new StopwatchTime());
			RegisterSingleton<Drawing>();
			CreateEntitySystem();
			RegisterMediaTypes();
		}

		private void CreateContentLoader()
		{
			if (ContentLoader.current == null)
				ContentLoader.current = new DeveloperOnlineContentLoader(ConnectToOnlineService());
			ContentLoader.current.resolver = new AutofacContentDataResolver(this);
			instancesToDispose.Add(ContentLoader.current);
		}

		private OnlineServiceConnection ConnectToOnlineService()
		{
			var connection = OnlineServiceConnection.CreateForAppRunner(GetApiKey(), OnTimeout, OnError,
				OnReady);
			instancesToDispose.Add(connection);
			instancesToDispose.Add(new NetworkLogger(connection));
			return connection;
		}

		private readonly List<IDisposable> instancesToDispose = new List<IDisposable>();

		private string GetApiKey()
		{
			string apiKey = "";
			using (var key = Registry.CurrentUser.OpenSubKey(@"Software\DeltaEngine\Editor", false))
				if (key != null)
					apiKey = (string)key.GetValue("ApiKey");
			if (string.IsNullOrEmpty(apiKey))
			{
				OnConnectionError("ApiKey not set. Please login with Editor to set it up.");
				if (failedToGetContent && !StackTraceExtensions.StartedFromNCrunch)
					Environment.Exit((int)ExitCode.ContentMissingAndApiKeyNotSet);
			}
			return apiKey;
		}

		internal enum ExitCode
		{
			InitializationError = -1,
			UpdateAndDrawTickFailed = -2,
			ContentMissingAndApiKeyNotSet = -3
		}

		private void OnConnectionError(string errorMessage)
		{
			Logger.Warning(errorMessage);
			int timeout = 1250;
			while (timeout > 0 && ContentLoader.current == null)
			{
				if (cachedWindow != null && cachedWindow.IsClosing)
					return;
				Thread.Sleep(10);
				timeout -= 10;
			}
			if (!(ContentLoader.current is DeveloperOnlineContentLoader))
				return;
			if (ContentLoader.HasValidContentForStartup())
			{
				(ContentLoader.current as DeveloperOnlineContentLoader).OnLoadContentMetaData();
				return;
			}
			failedToGetContent = true;
			timeout += 2250;
			while (timeout > 0 && !IsAlreadyInitialized)
			{
				Thread.Sleep(10);
				timeout -= 10;
			}
			Window.ShowMessageBox("Developer mode error", "No content available. " + errorMessage,
				new[] { "OK" });
		}

		private bool failedToGetContent;

		private void OnTimeout()
		{
			OnConnectionError("Connection to OnlineService timed out.");
		}

		private void OnError(string serverMessage)
		{
			OnConnectionError("Server Error: " + serverMessage);
		}

		private void OnReady()
		{
			onlineServiceReadyReceived = true;
		}

		private bool onlineServiceReadyReceived;

		private void LoadSettingsAndCommands()
		{
			instancesToDispose.Add(settings = new FileSettings());
			RegisterInstance(settings);
			ContentIsReady += () => ContentLoader.Load<InputCommands>("DefaultCommands");
		}

		protected internal static event Action ContentIsReady;

		private void CreateDefaultLoggers()
		{
			instancesToDispose.Add(new TextFileLogger());
			if (ExceptionExtensions.IsDebugMode)
				instancesToDispose.Add(new ConsoleLogger());
		}

		private void CreateEntitySystem()
		{
			instancesToDispose.Add(
				entities = new EntitiesRunner(new AutofacHandlerResolver(this), settings));
			RegisterInstance(entities);
		}

		protected virtual void RegisterMediaTypes()
		{
			Register<Material>();
			Register<ImageAnimation>();
			Register<SpriteSheetAnimation>();
		}

		internal override void MakeSureContentManagerIsReady()
		{
			if (alreadyCheckedContentManagerReady)
				return;
			alreadyCheckedContentManagerReady = true;
			if (ContentLoader.current is DeveloperOnlineContentLoader && !IsEditorContentLoader())
				WaitUntilContentFromOnlineServiceIsReady();
			if (ContentIsReady != null)
				ContentIsReady();
		}

		private bool alreadyCheckedContentManagerReady;

		private static bool IsEditorContentLoader()
		{
			return ContentLoader.current.GetType().FullName == "DeltaEngine.Editor.EditorContentLoader";
		}

		private void WaitUntilContentFromOnlineServiceIsReady()
		{
			while (!onlineServiceReadyReceived)
			{
				if (failedToGetContent)
					throw new FailedToLoadAnyContentUnableToInitializeApp();
				Thread.Sleep(10);
				if (onlineServiceReadyReceived || !(ContentLoader.current is DeveloperOnlineContentLoader))
					return;
				if (warnedAlready)
					continue;
				Logger.Info("No content available. Waiting until OnlineService sends it to us ...");
				warnedAlready = true;
			}
		}

		private bool warnedAlready;

		private class FailedToLoadAnyContentUnableToInitializeApp : Exception { }

		protected class AutofacContentDataResolver : ContentDataResolver
		{
			public AutofacContentDataResolver(Resolver resolver)
			{
				this.resolver = resolver;
			}

			private readonly Resolver resolver;

			public override ContentData Resolve(Type contentType, string contentName)
			{
				return resolver.Resolve(contentType, contentName) as ContentData;
			}

			public override ContentData Resolve(Type contentType, ContentCreationData data)
			{
				return resolver.Resolve(contentType, data) as ContentData;
			}

			public override void MakeSureResolverIsInitializedAndContentIsReady()
			{
				resolver.MakeSureContentManagerIsReady();
			}
		}

		protected Settings settings;
		protected EntitiesRunner entities;

		protected class AutofacHandlerResolver : BehaviorResolver
		{
			public AutofacHandlerResolver(Resolver resolver)
			{
				this.resolver = resolver;
			}

			private readonly Resolver resolver;

			public UpdateBehavior ResolveUpdateBehavior(Type handlerType)
			{
				return resolver.Resolve(handlerType) as UpdateBehavior;
			}

			public DrawBehavior ResolveDrawBehavior(Type handlerType)
			{
				return resolver.Resolve(handlerType) as DrawBehavior;
			}
		}

		public virtual void Run()
		{
			do
				RunTick();
			while (!Window.IsClosing);
			Dispose();
		}

		internal void RunTick()
		{
			Device.Clear();
			GlobalTime.Current.Update();
			TryUpdateAndDrawEntities();
			ExecuteTestCodeAndMakeScreenshotAfterFirstFrame();
			Device.Present();
			Window.Present();
		}

		private Device Device
		{
			get { return cachedDevice ?? (cachedDevice = Resolve<Device>()); }
		}
		private Device cachedDevice;

		private Window Window
		{
			get { return cachedWindow ?? (cachedWindow = Resolve<Window>()); }
		}
		private Window cachedWindow;

		private Drawing Drawing
		{
			get { return cachedDrawing ?? (cachedDrawing = Resolve<Drawing>()); }
		}
		private Drawing cachedDrawing;

		/// <summary>
		/// When debugging or testing crash where the actual exception happens, not here.
		/// </summary>
		private void TryUpdateAndDrawEntities()
		{
			Drawing.NumberOfDynamicVerticesDrawnThisFrame = 0;
			Drawing.NumberOfDynamicDrawCallsThisFrame = 0;
			if (Debugger.IsAttached || StackTraceExtensions.StartedFromNCrunch)
			{
				entities.UpdateAndDrawAllEntities(Drawing.DrawEverythingInCurrentLayer);
				return;
			}

			try
			{
				entities.UpdateAndDrawAllEntities(Drawing.DrawEverythingInCurrentLayer);
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
				if (exception.IsWeak())
					return;
				DisplayMessageBoxAndCloseApp(exception, "Fatal Runtime Error");
			}
		}

		private void DisplayMessageBoxAndCloseApp(Exception exception, string title)
		{
			var window = Resolve<Window>();
			window.CopyTextToClipboard(exception.ToString());
			if (window.ShowMessageBox(title, "Unable to continue: " + exception,
				new[] { "Abort", "Ignore" }) == "Ignore")
				return;
			Dispose();
			if (!StackTraceExtensions.StartedFromNCrunch)
				Environment.Exit((int)ExitCode.UpdateAndDrawTickFailed);
		}

		public override void Dispose()
		{
			foreach (var instance in instancesToDispose)
				instance.Dispose();
			instancesToDispose.Clear();
			base.Dispose();
		}
	}
}