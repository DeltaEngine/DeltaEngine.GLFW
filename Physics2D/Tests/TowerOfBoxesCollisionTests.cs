﻿using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Input;
using DeltaEngine.Physics2D.Farseer;
using DeltaEngine.Platforms;
using DeltaEngine.Rendering2D.Shapes;
using DeltaEngine.ScreenSpaces;
using NUnit.Framework;

namespace DeltaEngine.Physics2D.Tests
{
	public class TowerOfBoxesCollisionTests : TestWithMocksOrVisually
	{
		[SetUp]
		public void SetUp()
		{
			physics = new FarseerPhysics();
		}

		private Physics physics;

		[Test]
		public void TowerOfBoxesCollision()
		{
			Rectangle viewport = ScreenSpace.Current.Viewport;
			physics.CreateEdge(viewport.BottomLeft, viewport.BottomRight);
			physics.CreateEdge(viewport.BottomLeft, viewport.TopLeft);
			physics.CreateEdge(viewport.TopRight, viewport.BottomRight);
			physics.CreateEdge(viewport.TopLeft, viewport.TopRight);
			CreateTowerOfBoxes();
			new Command(AddCircleOnClick).Add(new MouseButtonTrigger());
		}

		private void CreateTowerOfBoxes()
		{
			Rectangle viewport = ScreenSpace.Current.Viewport;
			for (int y = 0; y < 6; y++)
				for (int x = 0; x < 4; x++)
				{
					PhysicsBody box = physics.CreateRectangle(new Size(0.08f));
					box.Restitution = 0.5f;
					box.Friction = 1f;
					box.Position = new Vector2D(0.6f + x * 0.09f, viewport.Bottom - 0.04f - (y * 0.09f));
					new FilledRect(new Rectangle(0f, 0f, 0.08f, 0.08f),
						Color.GetRandomBrightColor()).AffixToPhysics(box);
				}
		}

		//ncrunch: no coverage start
		private void AddCircleOnClick()
		{
			PhysicsBody circle = physics.CreateCircle(0.03f);
			circle.Restitution = 1f;
			circle.Friction = 0.8f;
			circle.Position = new Vector2D(ScreenSpace.Current.Viewport.Left + 0.04f, 0.6f);
			circle.LinearVelocity = new Vector2D(17f, 0f);
			new Ellipse(Vector2D.Zero, 0.03f, 0.03f, Color.GetRandomBrightColor()).AffixToPhysics(circle);
		}
	}
}