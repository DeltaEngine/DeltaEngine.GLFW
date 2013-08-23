using System.Collections.Generic;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Rendering.Shapes;

namespace DeltaEngine.Rendering.Graphs
{
	/// <summary>
	/// Horizontal lines at fixed intervals - eg. if there were five percentiles there'd be 
	/// six lines at 0%, 20%, 40%, 60%, 80% and 100% of the maximum value.
	/// </summary>
	internal class RenderPercentiles
	{
		public void Refresh(Graph graph)
		{
			ClearOldPercentiles();
			if (graph.Visibility == Visibility.Show && Visibility == Visibility.Show)
				CreateNewPercentiles(graph);
		}

		public Visibility Visibility { get; set; }

		private void ClearOldPercentiles()
		{
			foreach (Line2D percentile in Percentiles)
			{
				percentile.Visibility = Visibility.Hide;
				line2DPool.Add(percentile);
			}
			Percentiles.Clear();
		}

		public List<Line2D> Percentiles = new List<Line2D>();
		private readonly List<Line2D> line2DPool = new List<Line2D>();

		private void CreateNewPercentiles(Graph graph)
		{
			for (int i = 0; i <= NumberOfPercentiles; i++)
				CreatePercentile(graph, i);
		}

		public int NumberOfPercentiles { get; set; }

		private void CreatePercentile(Graph graph, int index)
		{
			Line2D percentile = CreateBlankPercentile();
			float borderHeight = graph.DrawArea.Height * Graph.Border;
			float interval = (graph.DrawArea.Height - 2 * borderHeight) / NumberOfPercentiles;
			float bottom = graph.DrawArea.Bottom - borderHeight;
			float y = bottom - index * interval;
			float borderWidth = graph.DrawArea.Width * Graph.Border;
			float startX = graph.DrawArea.Left + borderWidth;
			float endX = graph.DrawArea.Right - borderWidth;
			percentile.StartPoint = new Point(startX, y);
			percentile.EndPoint = new Point(endX, y);
			percentile.Color = PercentileColor;
			percentile.RenderLayer = graph.RenderLayer + RenderLayerOffset;
			percentile.Visibility = Visibility.Show;
			Percentiles.Add(percentile);
		}

		public Color PercentileColor = Color.Grey;

		private Line2D CreateBlankPercentile()
		{
			Line2D percentile;
			if (line2DPool.Count > 0)
			{
				percentile = line2DPool[0];
				line2DPool.RemoveAt(0);
			}
			else
				percentile = new Line2D(Point.Zero, Point.Zero, Color.Black);
			return percentile;
		}

		private const int RenderLayerOffset = 1;
	}
}