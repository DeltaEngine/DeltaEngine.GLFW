using System.Collections.Generic;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Rendering.Fonts;

namespace DeltaEngine.Rendering.Graphs
{
	/// <summary>
	/// Renders the key to the graph lines below the graph.
	/// </summary>
	internal class RenderKey
	{
		public void Refresh(Graph graph)
		{
			ClearOldKeyLabels();
			if (graph.Visibility == Visibility.Show && Visibility == Visibility.Show)
				CreateNewKeyLabels(graph);
		}

		public Visibility Visibility { get; set; }

		private void ClearOldKeyLabels()
		{
			foreach (FontText keyLabel in KeyLabels)
				keyLabel.IsActive = false;
			KeyLabels.Clear();
		}

		public List<FontText> KeyLabels = new List<FontText>();

		private void CreateNewKeyLabels(Graph graph)
		{
			for (int i = 0; i < graph.Lines.Count; i++)
				if (graph.Lines[i].Key != "")
					CreateKeyLabel(graph, i);
		}

		private void CreateKeyLabel(Graph graph, int index)
		{
			int row = 1 + index / 6;
			float borderHeight = graph.DrawArea.Height * Graph.Border;
			float y = graph.DrawArea.Bottom + (4 * row) * borderHeight;
			float borderWidth = graph.DrawArea.Width * Graph.Border;
			float left = graph.DrawArea.Left + borderWidth;
			int column = index % 6;
			float interval = (graph.DrawArea.Width - 2 * borderWidth) / 6;
			float x = left + column * interval;
			KeyLabels.Add(new FontText(FontXml.Default, graph.Lines[index].Key,
				new Rectangle(x, y, 1.0f, 1.0f))
			{
				RenderLayer = graph.RenderLayer + RenderLayerOffset,
				Color = graph.Lines[index].Color,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			});
		}

		private const int RenderLayerOffset = 2;
	}
}