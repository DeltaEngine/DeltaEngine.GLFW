using DeltaEngine;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Platforms;
using NUnit.Framework;

namespace Snake.Tests
{
	public class SnakeTests : TestWithMocksOrVisually
	{
		[SetUp]
		public void Init()
		{
			gridSize = 25;
			blockSize = 1.0f / gridSize;
			startPosition = blockSize * (int)(gridSize / 2.0f);
			moveSpeed = 0.15f;
		}

		private int gridSize;
		private float blockSize;
		private float startPosition;
		private float moveSpeed;

		[Test]
		public void CreateSnakeAtOrigin()
		{
			var snake = new Snake(gridSize);
			Assert.AreEqual(new Point(startPosition, startPosition),
				snake.Get<Body>().BodyParts[0].TopLeft);
		}

		[Test]
		public void SnakeHasTwoParts()
		{
			var game = new SnakeGame(Resolve<Window>());
			Assert.AreEqual(2, game.Snake.Get<Body>().BodyParts.Count);
		}

		[Test]
		public void AddToSnake()
		{
			var game = new SnakeGame(Resolve<Window>());
			Assert.AreEqual(2, game.Snake.Get<Body>().BodyParts.Count);
		}

		[Test]
		public void TouchTopBorder()
		{
			new SnakeGame(Resolve<Window>());
			AdvanceTimeAndUpdateEntities(moveSpeed * gridSize / 2);
		}

		[Test]
		public void TouchLeftBorder()
		{
			var game = new SnakeGame(Resolve<Window>());
			game.MoveLeft();
			AdvanceTimeAndUpdateEntities(moveSpeed * gridSize / 2);
		}

		[Test]
		public void TouchRightBorder()
		{
			var game = new SnakeGame(Resolve<Window>());
			game.MoveRight();
			AdvanceTimeAndUpdateEntities(moveSpeed * gridSize / 2);
		}

		[Test]
		public void TouchBottomBorder()
		{
			var game = new SnakeGame(Resolve<Window>());
			game.MoveLeft();
			AdvanceTimeAndUpdateEntities(moveSpeed);
			game.MoveDown();
			AdvanceTimeAndUpdateEntities(moveSpeed * gridSize / 2);
		}

		[Test]
		public void CheckTrailingVector()
		{
			var snake = new Snake(gridSize);
			Assert.AreEqual(new Point(0, blockSize), snake.Get<Body>().GetTrailingVector());
		}

		[Test]
		public void SnakeCollidingWithItselfWillRestart()
		{
			var game = new SnakeGame(Resolve<Window>());
			game.Snake.Get<Body>().AddSnakeBody();
			game.Snake.Get<Body>().AddSnakeBody();
			game.Snake.Get<Body>().AddSnakeBody();
			game.MoveLeft();
			AdvanceTimeAndUpdateEntities(moveSpeed);
			game.MoveDown();
			AdvanceTimeAndUpdateEntities(moveSpeed);
			game.MoveRight();
			AdvanceTimeAndUpdateEntities(moveSpeed);
		}

		[Test]
		public void DisposeSnake()
		{
			var snake = new Snake(gridSize) { IsActive = false };
			Assert.AreEqual(2, snake.Get<Body>().BodyParts.Count);
			snake.Dispose();
			Assert.Throws<Entity.ComponentNotFound>(() => snake.Get<Body>());
		}
	}
}