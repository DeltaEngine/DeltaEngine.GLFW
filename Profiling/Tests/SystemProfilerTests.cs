using DeltaEngine.Platforms;
using NUnit.Framework;

namespace DeltaEngine.Profiling.Tests
{
	public class SystemProfilerTests : TestWithMocksOrVisually
	{
		[SetUp]
		public void SetUpSystemInformation()
		{
			systemInformation = Resolve<SystemInformation>();
		}

		private SystemInformation systemInformation;

		[Test]
		public void VerifyDefaultProperties()
		{
			Assert.IsFalse(SystemProfiler.Current.IsActive);
			Assert.AreEqual(10, SystemProfiler.Current.MaximumPollsPerSecond);
			Assert.IsTrue(new SystemProfiler().IsActive);
			Assert.AreEqual(10, new SystemProfiler().MaximumPollsPerSecond);
		}

		[Test]
		public void ChangeMaximumPollsPerSecond()
		{
			var profiler = new SystemProfiler();
			profiler.MaximumPollsPerSecond = 2;
			Assert.AreEqual(2, profiler.MaximumPollsPerSecond);
		}

		[Test]
		public void LogInfo()
		{
			var profiler = new SystemProfiler();
			profiler.Log(ProfilingMode.Fps, systemInformation);
			SystemProfilerSection results = profiler.GetProfilingResults(ProfilingMode.Fps);
			Assert.IsTrue(results.TotalValue > 0.0f);
			Assert.AreEqual(1, results.Calls);
		}

		[Test]
		public void LogMultipleSetsOfInfo()
		{
			var profiler = new SystemProfiler();
			profiler.Log(ProfilingMode.Fps | ProfilingMode.AvailableRAM, systemInformation);
			Assert.AreEqual(1, profiler.GetProfilingResults(ProfilingMode.Fps).Calls);
			Assert.AreEqual(1, profiler.GetProfilingResults(ProfilingMode.AvailableRAM).Calls);
		}

		[Test]
		public void OnlyProfilesOnceIfTooShortATimeHasPassed()
		{
			int count = 0;
			var profiler = new SystemProfiler();
			profiler.Updated += () => count++;
			profiler.Log(ProfilingMode.Fps, systemInformation);
			profiler.Log(ProfilingMode.Fps, systemInformation);
			Assert.AreEqual(1, profiler.GetProfilingResults(ProfilingMode.Fps).Calls);
			Assert.AreEqual(1, count);
		}

		[Test]
		public void ProfilesTwiceIfEnoughTimeHasPassed()
		{
			int count = 0;
			var profiler = new SystemProfiler(1000);
			profiler.Updated += () => count++;
			profiler.Log(ProfilingMode.Fps, systemInformation);
			profiler.Log(ProfilingMode.Fps, systemInformation);
			Assert.AreEqual(2, profiler.GetProfilingResults(ProfilingMode.Fps).Calls);
			Assert.AreEqual(2, count);
		}

		[Test]
		public void ProfilingWhenInactiveDoesNothing()
		{
			int count = 0;
			var profiler = new SystemProfiler { IsActive = false };
			profiler.Updated += () => count++;
			profiler.Log(ProfilingMode.Fps, systemInformation);
			Assert.AreEqual(0, profiler.GetProfilingResults(ProfilingMode.Fps).Calls);
			Assert.AreEqual(0, count);
		}
	}
}