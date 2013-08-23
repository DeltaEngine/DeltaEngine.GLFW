﻿using DeltaEngine;
using DeltaEngine.Commands;
using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Input;
using DeltaEngine.Rendering.Fonts;
using DeltaEngine.Rendering.Shapes;
using DeltaEngine.ScreenSpaces;

namespace Snake
{
	/// <summary>
	/// This is the main game class holding the control of the entire snake game, user input and the 
	/// interaction of the snake with the borders and the chunks
	/// </summary>
	public class SnakeGame
	{
		public SnakeGame(Window window)
		{
			new RelativeScreenSpace(window);
			gridSize = 25;
			blockSize = 1.0f / gridSize;
			this.window = window;
			SetupPlayArea();
			SetInput();
			InitializeSnake();
			SpawnFirstChunk();
		}

		private readonly int gridSize;
		private readonly float blockSize;
		private readonly Window window;

		private void SetupPlayArea()
		{
			window.Title = "Snake - Let's go";
			window.BackgroundColor = Color.Red;
			new FilledRect(CalculateBackgroundDrawArea(), Color.Black);
		}

		private Rectangle CalculateBackgroundDrawArea()
		{
			return new Rectangle(blockSize, blockSize, blockSize * (gridSize - 2),
				blockSize * (gridSize - 2));
		}

		private void SetInput()
		{
			new Command(MoveLeft).Add(new KeyTrigger(Key.CursorLeft));
			new Command(MoveRight).Add(new KeyTrigger(Key.CursorRight));
			new Command(MoveUp).Add(new KeyTrigger(Key.CursorUp));
			new Command(MoveDown).Add(new KeyTrigger(Key.CursorDown));
			new Command(MoveAccordingToTouchPosition).Add(new TouchPressTrigger()).Add(
				new MouseButtonTrigger());
		}

		public void MoveLeft()
		{
			if (GetDirection().X > 0)
				return;
			snakeBody.Direction = new Point(-blockSize, 0);
		}

		private Point GetDirection()
		{
			if (snakeBody.BodyParts.Count == 0)
				return Point.Zero;

			var snakeHead = snakeBody.BodyParts[0];
			var partNextToSnakeHead = snakeBody.BodyParts[1];
			var direction = new Point(snakeHead.DrawArea.Left - partNextToSnakeHead.DrawArea.Left,
				snakeHead.DrawArea.Top - partNextToSnakeHead.DrawArea.Top);
			return direction;
		}

		public void MoveRight()
		{
			if (GetDirection().X < 0)
				return;
			snakeBody.Direction = new Point(blockSize, 0);
		}

		public void MoveUp()
		{
			if (GetDirection().Y > 0)
				return;
			snakeBody.Direction = new Point(0, -blockSize);
		}

		public void MoveDown()
		{
			if (GetDirection().Y < 0)
				return;
			snakeBody.Direction = new Point(0, blockSize);
		}

		private void MoveAccordingToTouchPosition(Point position)
		{
			var comparison = snakeBody.HeadPosition;
			CheckTouchHorizontal(position,comparison);
			CheckTouchVertical(position,comparison);
		}

		private void CheckTouchVertical(Point position, Point comparison)
		{
			if (GetDirection().X != 0)
			{
				var deltaY = position.Y - comparison.Y;
				if (deltaY > 0)
					MoveDown();
				if (deltaY < 0)
					MoveUp();
			}
		}

		private void CheckTouchHorizontal(Point position, Point comparison)
		{
			if (GetDirection().Y != 0)
			{
				var deltaX = position.X - comparison.X;
				if (deltaX > 0)
					MoveRight();
				if (deltaX < 0)
					MoveLeft();
			}
		}

		private void InitializeSnake()
		{
			Snake = new Snake(gridSize);
			snakeBody = Snake.Get<Body>();
			AddEventListeners();
		}

		private Body snakeBody;
		public Snake Snake { get; private set; }

		private void AddEventListeners()
		{
			snakeBody.DetectSnakeCollisionWithChunk += SnakeCollisionWithChunk;
			snakeBody.SnakeCollidesWithBorderOrItself += Reset;
		}

		private void SnakeCollisionWithChunk(Point trailingVector)
		{
			if (Chunk.TopLeft == snakeBody.BodyParts[0].TopLeft)
			{
				Chunk.SpawnAtRandomLocation();
				GrowSnakeInSize(trailingVector);
			}
		}

		private void GrowSnakeInSize(Point trailingVector)
		{
			var snakeBodyParts = snakeBody.BodyParts;
			var tail = snakeBodyParts[snakeBodyParts.Count - 1].DrawArea.TopLeft;
			var newBodyPart = new FilledRect(CalculateTrailDrawArea(trailingVector, tail), Color.Teal);
			snakeBodyParts.Add(newBodyPart);
			window.Title = "Snake - Length: " + snakeBodyParts.Count;
		}

		private Rectangle CalculateTrailDrawArea(Point trailingVector, Point tail)
		{
			return new Rectangle(new Point(tail.X + trailingVector.X, tail.Y + trailingVector.Y),
				new Size(blockSize));
		}

		public void Reset()
		{
			RemoveEventListeners();
			Snake.Dispose();
			DisplayGameOverMessage();
		}

		private void DisplayGameOverMessage()
		{
			Chunk.IsActive = false;
			var fontGameOverText = ContentLoader.Load<FontXml>("Tahoma30");
			var fontReplayText = ContentLoader.Load<FontXml>("Verdana12");
			gameOverMsg = new FontText(fontGameOverText, "Game Over",
				Rectangle.FromCenter(Point.Half, new Size(0.6f, 0.3f))) { Color = Color.Red };
			restartMsg = new FontText(fontReplayText, "Do you want to continue (Y/N)",
				Rectangle.FromCenter(new Point(0.5f, 0.7f), new Size(0.6f, 0.3f))) { Color = Color.Yellow };
			yesCommand = new Command(RestartGame);
			yesCommand.Add(new KeyTrigger(Key.Y));
			noCommand = new Command(CloseGame);
			noCommand.Add(new KeyTrigger(Key.N));
		}

		private Command yesCommand;
		private Command noCommand;
		private FontText gameOverMsg;
		private FontText restartMsg;

		private void RestartGame()
		{
			yesCommand.IsActive = false;
			noCommand.IsActive = false;
			gameOverMsg.IsActive = false;
			restartMsg.IsActive = false;
			InitializeSnake();
			SpawnFirstChunk();
		}

		private void CloseGame()
		{
			window.CloseAfterFrame();
		}

		private void SpawnFirstChunk()
		{
			Chunk = new Chunk(gridSize, blockSize);
			RespawnChunk();
		}

		public Chunk Chunk { get; private set; }

		public void RespawnChunk()
		{
			while (Chunk.IsCollidingWithSnake(snakeBody.BodyParts))
				Chunk.SpawnAtRandomLocation();
		}

		private void RemoveEventListeners()
		{
			snakeBody.DetectSnakeCollisionWithChunk -= SnakeCollisionWithChunk;
			snakeBody.SnakeCollidesWithBorderOrItself -= Reset;
		}
	}
}