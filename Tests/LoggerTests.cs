using System;
using System.IO;
using System.Runtime.InteropServices;
using DeltaEngine.Mocks;
using NUnit.Framework;

namespace DeltaEngine.Tests
{
	public class LoggerTests
	{
		[Test]
		public void LogInfoMessage()
		{
			using (var logger = new MockLogger())
			{
				Assert.IsNull(logger.LastMessage);
				Logger.Info("Hello");
				Assert.AreEqual("Hello", logger.LastMessage);
				Assert.AreEqual(0, logger.NumberOfRepeatedMessagesIgnored);
			}
		}
		
		[Test]
		public void LogWarning()
		{
			using (var logger = new MockLogger())
			{
				Logger.Warning("Ohoh");
				Assert.AreEqual("Ohoh", logger.LastMessage);
				Logger.Warning(new NullReferenceException());
				Assert.IsTrue(logger.LastMessage.Contains("NullReferenceException"));
			}
		}

		[Test]
		public void LogError()
		{
			using (var logger = new MockLogger())
			{
				Logger.Error(new ExternalException());
				Assert.IsTrue(logger.LastMessage.Contains(new ExternalException().Message),
					logger.LastMessage);
			}
		}

		[Test]
		public void RegisteringLoggerInTheSameThreadTwiceIsNotAllowed()
		{
			using (new SameThreadLogger())
			{
				Logger.Info("La la la");
				Assert.Throws<Logger.LoggerWasAlreadyAttached>(() => { using (new SameThreadLogger()) {} });
			}
		}

		public class SameThreadLogger : Logger
		{
			public SameThreadLogger()
				: base(true) { }

			public override void Write(MessageType messageType, string message)
			{
				Assert.AreEqual("La la la", message);
			}
		}

		[Test]
		public void RegisteringTheSameLoggerTwiceIsNotAllowed()
		{
			using (new AnotherLogger())
			{
				Assert.Throws<Logger.LoggerWasAlreadyAttached>(() => { using (new AnotherLogger()) {} });
			}
		}

		public class AnotherLogger : MockLogger {}

		[Test]
		public void IfNoLoggerIsAttachedWeGetAWarn()
		{
			const string Warning = "Ohoh";
			const string Message = "No loggers have been created for this message: " + Warning;
			var defaultOut = Console.Out;
			var console = new StringWriter();
			Console.SetOut(console);
			Logger.Warning(Warning);
			Assert.IsTrue(console.ToString().Contains(Message));
			Console.SetOut(defaultOut);
			console.Dispose();
		}
	}
}