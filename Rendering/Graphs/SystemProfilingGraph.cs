using System.Collections.Generic;
using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Input;
using DeltaEngine.Platforms;
using DeltaEngine.Profiling;

namespace DeltaEngine.Rendering.Graphs
{
	internal abstract class SystemProfilingGraph : Graph
	{
		protected SystemProfilingGraph(Rectangle drawArea, ProfilingMode profilingMode, Color color)
			: base(drawArea)
		{
			Add(profilingMode);
			RenderLayer = int.MaxValue - 10;
			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Visibility = Visibility.Hide;
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
			NumberOfPercentiles = 5;
			MaximumNumberOfPoints = 100;
			ArePercentileLabelsInteger = true;
			CreateLine(profilingMode.ToString(), color);
			Start<LogProfilingData>();
			SystemProfiler.Current.IsActive = true;
			SystemProfiler.Current.Updated += Updated;
			new Command(ToggleVisibility).Add(new KeyTrigger(Key.F11));
		}

		private class LogProfilingData : UpdateBehavior
		{
			public LogProfilingData(SystemInformation systemInformation)
			{
				this.systemInformation = systemInformation;
			}

			private readonly SystemInformation systemInformation;

			public override void Update(IEnumerable<Entity> entities)
			{
				SystemProfiler.Current.Log(ProfilingMode.Fps | ProfilingMode.AvailableRAM,
					systemInformation);
			}
		}

		private void Updated()
		{
			if (Visibility == Visibility.Hide)
				return;
			SystemProfilerSection results =
				SystemProfiler.Current.GetProfilingResults(Get<ProfilingMode>());
			Lines[0].AddValue(results.TotalValue / results.Calls);
		}
	}
}