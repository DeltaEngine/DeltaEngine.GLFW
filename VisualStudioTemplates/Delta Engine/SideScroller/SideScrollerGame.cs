using System.Collections.Generic;
using DeltaEngine.Content;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Rendering;
using DeltaEngine.ScreenSpaces;

namespace $safeprojectname$
{
	internal class SideScrollerGame : Entity2D
	{
		public SideScrollerGame() : base(Rectangle.Zero)
		{
			interact = new InteractionLogics();
			playerTexture = new Material(Shader.Position2DColorUv, "PlayerPlane");
			enemyTexture = new Material(Shader.Position2DColorUv, "EnemyPlane");
			player = new PlayerPlane(playerTexture, new Point(0.15f, 0.5f));
			controls = new GameControls();
			BindPlayerToControls();
			BindPlayerAndInteraction();
			Start<EnemySpawner>();
		}

		internal readonly PlayerPlane player;
		internal readonly GameControls controls;
		internal readonly InteractionLogics interact;
		internal readonly Material playerTexture;
		internal readonly Material enemyTexture;

		public void CreateEnemyAtPosition(Point position)
		{
			var enemy = new EnemyPlane(enemyTexture, position);
			player.PlayerFiredShot += point => enemy.CheckIfHitAndReact(point);
			enemy.EnemyFiredShot += point => interact.FireShotByEnemy(point);
		}

		private void BindPlayerToControls()
		{
			controls.Ascend += () => player.AccelerateVertically(-Time.Delta);
			controls.Sink += () => player.AccelerateVertically(Time.Delta);
			controls.VerticalStop += () => player.StopVertically();
			controls.Fire += () => 
			{
				player.IsFireing = true;
			};
			controls.HoldFire += () => 
			{
				player.IsFireing = false;
			};
			controls.SlowDown += () => 
			{
			};
			controls.Accelerate += () => 
			{
			};
		}

		private void BindPlayerAndInteraction()
		{
			player.PlayerFiredShot += point => 
			{
				interact.FireShotByPlayer(point);
			};
		}
		private class EnemySpawner : UpdateBehavior
		{
			public override void Update(IEnumerable<Entity> entities)
			{
				foreach (var entity in entities)
				{
					if (!(GlobalTime.Current.Milliseconds - timeLastOneSpawned > 2000))
						continue;

					var game = entity as SideScrollerGame;
					game.CreateEnemyAtPosition(new Point(ScreenSpace.Current.Viewport.Right, 
						ScreenSpace.Current.Viewport.Center.Y + alternating * 0.1f));
					timeLastOneSpawned = GlobalTime.Current.Milliseconds;
					alternating *= -1;
				}
			}

			private float timeLastOneSpawned;
			private int alternating = 1;
		}
	}
}