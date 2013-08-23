using DeltaEngine;
using DeltaEngine.Datatypes;
using DeltaEngine.Platforms;
using NUnit.Framework;

namespace Snake.Tests
{
	public class SnakeGameTests : TestWithMocksOrVisually
	{
		[SetUp]
		public void Init()
		{
			gridSize = 25;
			blockSize = 1.0f / gridSize;
			moveSpeed = 0.15f;
		}

		private float blockSize;
		private int gridSize;
		private float moveSpeed;

		[Test]
		public void StartGame()
		{
			new SnakeGame(Resolve<Window>());
		}

		[Test]
		public void RespawnChunkIfCollidingWithSnake()
		{
			var snakeGame = new SnakeGame(Resolve<Window>());
			snakeGame.Chunk.DrawArea = snakeGame.Snake.Get<Body>().BodyParts[0].DrawArea;
			Assert.IsTrue(snakeGame.Chunk.IsCollidingWithSnake(snakeGame.Snake.Get<Body>().BodyParts));
			snakeGame.RespawnChunk();
			Assert.IsFalse(snakeGame.Chunk.IsCollidingWithSnake(snakeGame.Snake.Get<Body>().BodyParts));
		}

		[Test]
		public void SnakeEatsChunk()
		{
			var game = new SnakeGame(Resolve<Window>());
			var snakeHead = game.Snake.Get<Body>().BodyParts[0].DrawArea;
			var direction = game.Snake.Get<Body>().Direction;
			var snakeBodyParts = game.Snake.Get<Body>().BodyParts;
			var oldTailTopLeftCorner = snakeBodyParts[snakeBodyParts.Count - 1].DrawArea.TopLeft;
			game.Chunk.DrawArea =
				new Rectangle(new Point(snakeHead.Left + direction.X, snakeHead.Top + direction.Y),
					new Size(blockSize));
			game.MoveUp();
			AdvanceTimeAndUpdateEntities(moveSpeed);
			Assert.AreEqual(3, game.Snake.Get<Body>().BodyParts.Count);
			var newTailTopLeftCorner = snakeBodyParts[snakeBodyParts.Count - 1].DrawArea.TopLeft;
			Assert.AreEqual(oldTailTopLeftCorner, newTailTopLeftCorner);
		}

		[Test]
		public void DisplayGameOver()
		{
			var game = new SnakeGame(Resolve<Window>());
			game.Reset();
		}
	}
}