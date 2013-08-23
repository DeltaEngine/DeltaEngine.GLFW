﻿using DeltaEngine.Mocks;
using DeltaEngine.Platforms;
using NUnit.Framework;
using ProfilingMode = DeltaEngine.Core.ProfilingMode;

namespace DeltaEngine.Tests
{
	public class SettingsTests
	{
		[SetUp]
		public void Init()
		{
			settings = new MockSettings();
		}

		private Settings settings;

		[Test]
		public void CheckDefaultSettings()
		{
			Assert.AreEqual(false, settings.StartInFullscreen);
			Assert.AreEqual(1.0f, settings.SoundVolume);
			Assert.AreEqual(0.75f, settings.MusicVolume);
			Assert.AreEqual(24, settings.DepthBufferBits);
			Assert.AreEqual(32, settings.ColorBufferBits);
			Assert.AreEqual(4, settings.AntiAliasingSamples);
			Assert.AreEqual(0, settings.LimitFramerate);
			Assert.AreEqual(20, settings.UpdatesPerSecond);
			Assert.AreEqual(60, settings.RapidUpdatesPerSecond);
		}

		[Test]
		public void ChangeAndSaveSettings()
		{
			settings.PlayerName = ModifiedPlayerName;
			settings.TwoLetterLanguageName = ModifiedTwoLetterLanguageName;
			Assert.AreEqual(settings.TwoLetterLanguageName, ModifiedTwoLetterLanguageName);
			Assert.AreEqual(ModifiedPlayerName, settings.PlayerName);
			settings.Save();
		}

		private const string ModifiedPlayerName = "John Doe";
		private const string ModifiedTwoLetterLanguageName = "de";

		[Test]
		public void EditAndCheckSettings()
		{
			EditSettings();
			CheckSettings();
		}

		private void EditSettings()
		{
			settings.SoundVolume = 2.0f;
			settings.MusicVolume = 1.0f;
			settings.DepthBufferBits = 16;
			settings.ColorBufferBits = 16;
			settings.AntiAliasingSamples = 2;
			settings.LimitFramerate = 20;
			settings.UpdatesPerSecond = 30;
			settings.RapidUpdatesPerSecond = 60;
			settings.ProfilingModes = ProfilingMode.Rendering;
		}

		private void CheckSettings()
		{
			Assert.AreEqual(2.0f, settings.SoundVolume);
			Assert.AreEqual(1.0f, settings.MusicVolume);
			Assert.AreEqual(16, settings.DepthBufferBits);
			Assert.AreEqual(16, settings.ColorBufferBits);
			Assert.AreEqual(2, settings.AntiAliasingSamples);
			Assert.AreEqual(20, settings.LimitFramerate);
			Assert.AreEqual(30, settings.UpdatesPerSecond);
			Assert.AreEqual(60, settings.RapidUpdatesPerSecond);
			Assert.AreEqual(ProfilingMode.Rendering, settings.ProfilingModes);
		}

		[Test]
		public void ChangeFileSettings()
		{
			var fileSettings = new FileSettings();
			fileSettings.StartInFullscreen = true;
			Assert.AreEqual(true, fileSettings.StartInFullscreen);
			fileSettings.Dispose();
		}
	}
}