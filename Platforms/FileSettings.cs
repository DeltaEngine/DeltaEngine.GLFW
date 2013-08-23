using System;
using System.IO;
using System.Threading;
using DeltaEngine.Content;
using DeltaEngine.Content.Xml;
using DeltaEngine.Extensions;

namespace DeltaEngine.Platforms
{
	/// <summary>
	/// App settings loaded from and saved to file.
	/// </summary>
	public class FileSettings : Settings
	{
		public FileSettings()
		{
			filePath = Path.Combine(AssemblyExtensions.GetMyDocumentsAppFolder(), SettingsFilename);
			if (File.Exists(filePath))
				data = new XmlFile(filePath).Root;
			else
				TryToGenerateSettingsFromContentDefaultSettings();
		}

		private readonly string filePath;
		private XmlData data;

		private void TryToGenerateSettingsFromContentDefaultSettings()
		{
			SetFallbackSettings();
			AppRunner.ContentIsReady += LoadDefaultSettings;
		}

		protected void SetFallbackSettings()
		{
			data = new XmlData("Settings");
			data.AddChild("Resolution", DefaultResolution);
			data.AddChild("StartInFullscreen", false);
			data.AddChild("PlayerName", Environment.UserName);
			data.AddChild("Language", Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
			data.AddChild("SoundVolume", 1.0f);
			data.AddChild("MusicVolume", 0.75f);
			data.AddChild("DepthBufferBits", 24);
			data.AddChild("ColorBufferBits", 32);
			data.AddChild("AntiAliasingSamples", 4);
			data.AddChild("LimitFramerate", 0);
			data.AddChild("UpdatesPerSecond", DefaultUpdatesPerSecond);
			data.AddChild("ProfilingModes", ProfilingMode.None);
			wasChanged = true;
		}

		private void LoadDefaultSettings()
		{
			data = ContentLoader.Load<XmlContent>("DefaultSettings").Data;
			wasChanged = true;
		}

		public override void Save()
		{
			new XmlFile(data).Save(filePath);
		}

		protected override T GetValue<T>(string key, T defaultValue)
		{
			return data.GetChildValue(key, defaultValue);
		}

		protected override void SetValue(string key, object value)
		{
			if (data.GetChild(key) == null)
				data.AddChild(key, StringExtensions.ToInvariantString(value));
			else
				data.GetChild(key).Value = StringExtensions.ToInvariantString(value);
			wasChanged = true;
		}
	}
}