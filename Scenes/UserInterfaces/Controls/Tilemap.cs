﻿using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Extensions;
using DeltaEngine.Input;
using DeltaEngine.Rendering;
using DeltaEngine.Rendering.Sprites;

namespace DeltaEngine.Scenes.UserInterfaces.Controls
{
	/// <summary>
	/// A smoothly scrolling grid of colored images.
	/// </summary>
	public abstract class Tilemap : Control
	{
		protected Tilemap(Size world, Size map)
			: base(Rectangle.Zero)
		{
			Add(data = new Data(world, map));
			new Command(ScrollLeft).Add(new KeyTrigger(Key.A, Input.State.Pressed));
			new Command(ScrollRight).Add(new KeyTrigger(Key.D, Input.State.Pressed));
			new Command(ScrollUp).Add(new KeyTrigger(Key.W, Input.State.Pressed));
			new Command(ScrollDown).Add(new KeyTrigger(Key.S, Input.State.Pressed));
		}

		private readonly Data data;

		public class Data
		{
			public Data(Size world, Size map)
			{
				CreateWorld(world);
				CreateMap(map);
			}

			private void CreateWorld(Size world)
			{
				WorldWidth = (int)world.Width;
				WorldHeight = (int)world.Height;
				World = new Tile[WorldWidth,WorldHeight];
			}

			public int WorldWidth { get; private set; }
			public int WorldHeight { get; private set; }
			public Tile[,] World { get; private set; }

			private void CreateMap(Size map)
			{
				MapWidth = (int)map.Width;
				MapHeight = (int)map.Height;
				Map = new Sprite[MapWidth,MapHeight];
			}

			public int MapWidth { get; private set; }
			public int MapHeight { get; private set; }
			public Sprite[,] Map { get; private set; }
			public Point RenderingTopLeft = new Point(0.0001f, 0.0001f);
			public Point TargetTopLeft = Point.Zero;
		}

		public override void Update()
		{
			base.Update();
			ProcessAnyDragging();
			UpdateTilePositions();
		}

		private void ProcessAnyDragging()
		{
			if (State.DragDelta == Point.Zero)
				return;
			data.TargetTopLeft.X -= State.DragDelta.X * data.MapWidth / DrawArea.Width;
			data.TargetTopLeft.Y -= State.DragDelta.Y * data.MapHeight / DrawArea.Height;
			RestrictScrollingToWithinWorld();
		}

		private void RestrictScrollingToWithinWorld()
		{
			if (data.TargetTopLeft.X < 0)
				data.TargetTopLeft.X = 0;
			if (data.TargetTopLeft.X > data.WorldWidth - data.MapWidth)
				data.TargetTopLeft.X = data.WorldWidth - data.MapWidth;
			if (data.TargetTopLeft.Y < 0)
				data.TargetTopLeft.Y = 0;
			if (data.TargetTopLeft.Y > data.WorldHeight - data.MapHeight)
				data.TargetTopLeft.Y = data.WorldHeight - data.MapHeight;
		}

		private void UpdateTilePositions()
		{
			if (data.RenderingTopLeft == data.TargetTopLeft)
				return;
			UpdateTopLeft();
			UpdateMapSprites();
		}

		private void UpdateTopLeft()
		{
			float percentage = ScrollingStiffness * Time.Delta;
			data.RenderingTopLeft =
				new Point(data.RenderingTopLeft.X.Lerp(data.TargetTopLeft.X, percentage),
					data.RenderingTopLeft.Y.Lerp(data.TargetTopLeft.Y, percentage));
		}

		private const float ScrollingStiffness = 2.0f;

		private void UpdateMapSprites()
		{
			offset = new Point(data.RenderingTopLeft.X % 1.0f, data.RenderingTopLeft.Y % 1.0f);
			for (int x = 0; x < data.MapWidth; x++)
				for (int y = 0; y < data.MapHeight; y++)
					UpdateTileSprite(x, y);
			lastRenderingTopLeft = GetIntPoint(data.RenderingTopLeft);
		}

		private Point offset;
		private Point lastRenderingTopLeft;

		private static Point GetIntPoint(Point point)
		{
			return new Point((int)point.X, (int)point.Y);
		}

		private void UpdateTileSprite(int tileX, int tileY)
		{
			var worldX = (int)data.RenderingTopLeft.X + tileX;
			var worldY = (int)data.RenderingTopLeft.Y + tileY;
			data.Map[tileX, tileY].Material = data.World[worldX, worldY].Material;
			data.Map[tileX, tileY].Color = data.World[worldX, worldY].Color;
			data.Map[tileX, tileY].DrawArea =
				new Rectangle(DrawArea.Left + (tileX - offset.X) * TileWidth,
					DrawArea.Top + (tileY - offset.Y) * TileHeight, TileWidth, TileHeight);
			if (lastRenderingTopLeft != GetIntPoint(data.RenderingTopLeft))
				AdjustLastFrameComponents(data.Map[tileX, tileY]);
		}

		private void AdjustLastFrameComponents(Entity2D tile)
		{
			float lastPositionX = tile.LastDrawArea.Left -
				(lastRenderingTopLeft.X - (int)data.RenderingTopLeft.X) * TileWidth;
			float lastPositionY = tile.LastDrawArea.Top -
				(lastRenderingTopLeft.Y - (int)data.RenderingTopLeft.Y) * TileHeight;
			tile.LastDrawArea = new Rectangle(lastPositionX, lastPositionY, tile.Size.Width,
				tile.Size.Height);
			tile.LastColor = tile.Color;
		}

		private void ScrollLeft()
		{
			data.TargetTopLeft.X -= Time.Delta * ScrollSpeed;
			RestrictScrollingToWithinWorld();
		}

		private const float ScrollSpeed = 4.0f;

		private void ScrollRight()
		{
			data.TargetTopLeft.X += Time.Delta * ScrollSpeed;
			RestrictScrollingToWithinWorld();
		}

		private void ScrollUp()
		{
			data.TargetTopLeft.Y -= Time.Delta * ScrollSpeed;
			RestrictScrollingToWithinWorld();
		}

		private void ScrollDown()
		{
			data.TargetTopLeft.Y += Time.Delta * ScrollSpeed;
			RestrictScrollingToWithinWorld();
		}

		public float TileWidth
		{
			get { return DrawArea.Width / Get<Data>().MapWidth; }
		}

		public float TileHeight
		{
			get { return DrawArea.Height / Get<Data>().MapHeight; }
		}
	}
}