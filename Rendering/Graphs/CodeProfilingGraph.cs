using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Input;
using DeltaEngine.Platforms;
using DeltaEngine.Profiling;

namespace DeltaEngine.Rendering.Graphs
{
	/// <summary>
	/// Draws an overlay showing code profiling information - eg. how long Rendering took to run.
	/// </summary>
	public abstract class CodeProfilingGraph : Graph
	{
		protected CodeProfilingGraph(Rectangle drawArea, ProfilingMode profilingMode)
			: base(drawArea)
		{
			this.profilingMode = profilingMode;
			RenderLayer = int.MaxValue - 10;
			// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Visibility = Visibility.Hide;
			// ReSharper restore DoNotCallOverridableMethodsInConstructor
			NumberOfPercentiles = 5;
			MaximumNumberOfPoints = 100;
			PercentileSuffix = "%";
			CodeProfiler.Current.IsActive = true;
			CodeProfiler.Current.Updated += Updated;
			new Command(ToggleVisibility).Add(new KeyTrigger(Key.F11));
		}

		private readonly ProfilingMode profilingMode;

		private void Updated()
		{
			if (Visibility == Visibility.Hide)
				return;

			CodeProfilingResults results = CodeProfiler.Current.GetProfilingResults(profilingMode);
			UpdateLinesAndColors(results);
			AddValuesToLines(results);
		}

		private void UpdateLinesAndColors(CodeProfilingResults results)
		{
			for (int index = 0; index < Lines.Count; index++)
				Lines[index].Color = Color.GetHeatmapColor(index / (results.Sections.Count - 1.0f));

			for (int index = Lines.Count; index < results.Sections.Count; index++)
				CreateLine(GetKey(results.Sections[index].Name),
					Color.GetHeatmapColor(index / (results.Sections.Count - 1.0f)));
		}

		private static string GetKey(string name)
		{
			if (name.Contains("+"))
				return name.Substring(name.IndexOf('+') + 1);

			return name.Contains(".") ? name.Substring(name.LastIndexOf('.') + 1) : name;
		}

		private void AddValuesToLines(CodeProfilingResults results)
		{
			for (int index = 0; index < results.Sections.Count; index++)
				Lines[index].AddValue(GetSectionPercentage(results, index));
		}

		private static float GetSectionPercentage(CodeProfilingResults results, int index)
		{
			return 100.0f * results.Sections[index].TotalTime / results.TotalSectionTime;
		}
	}
}