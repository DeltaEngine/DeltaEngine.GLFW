using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Rendering.Particles;
using DeltaEngine.Rendering.Sprites;

namespace GhostWars
{
	/// <summary>
	/// Collection of methods to create effects used in the GhostWars game.
	/// </summary>
	public static class Effects
	{
		public static Sprite CreateArrow(Point start, Point target)
		{
			var material = new Material(Shader.Position2DColorUv, "Arrow");
			var newSprite = new Sprite(material, CalculateArrowDrawArea(material, start, target));
			newSprite.Rotation = target.RotationTo(start);
			return newSprite;
		}

		private static Rectangle CalculateArrowDrawArea(Material material, Point start, Point target)
		{
			start += Point.Normalize(start.DirectionTo(target)) * 0.033f;
			target -= Point.Normalize(start.DirectionTo(target)) * 0.033f;
			var distance = start.DistanceTo(target);
			var size = new Size(distance, distance / material.DiffuseMap.PixelSize.AspectRatio);
			return Rectangle.FromCenter((start + target) / 2, size);
		}

		public static ParticleEmitter CreateDeathEffect(Point position)
		{
			var material = new Material(Shader.Position2DColorUv, "DeathSkull");
			var deathEffect = new ParticleEffectData
			{
				ParticleMaterial = material,
				MaximumNumberOfParticles = 1,
				SpawnInterval = 0,
				Size = new RangeGraph<Size>(new Size(0.02f), new Size(0.015f)),
				Force = new RangeGraph<Point>(new Point(0, -0.04f), new Point(0, -0.04f)),
				LifeTime = 2f,
				StartVelocity = new RangeGraph<Point>(Point.Zero, new Point(0.01f, 0.01f))
			};
			return new ParticleEmitter(deathEffect, position);
		}

		public static ParticleEmitter CreateHitEffect(Point position)
		{
			var material = new Material(Shader.Position2DColorUv, "Hit");
			var deathEffect = new ParticleEffectData
			{
				ParticleMaterial = material,
				MaximumNumberOfParticles = 1,
				SpawnInterval = 0,
				Size = new RangeGraph<Size>(new Size(0.06f), new Size(0.09f)),
				LifeTime = 0.5f
			};
			return new ParticleEmitter(deathEffect, position);
		}

		public static ParticleEmitter CreateSparkleEffect(Team team, Point position, int sparkles)
		{
			/*creates too many particles, needs to be handled better and allow changing numbers
			var sparkleMaterial = new Material(Shader.Position2DColorUv, "CoronaAdditive");
			var sparkleCreator = new ParticleEmitterData
			{
				ParticleMaterial = sparkleMaterial,
				MaximumNumberOfParticles = 10,
				SpawnInterval = 0.45f,
				Size = new Range<Size>(new Size(0.02f), new Size(0.015f)),
				Force = new Point(0, 0.02f),
				LifeTime = 1.5f,
				StartVelocityVariance = new Point(0.05f, 0.05f),
				StartColor = team.ToColor()
			};
			return new ParticleEmitter(sparkleCreator, position);
			 */
			/*create and update particles
			if (Team == Team.None)
				return;
			var sparkleMaterial = new Material(ContentLoader.Load<Shader>(Shader.Position2DColorUv),
				images.GetCoronaImage(Team));
			sparkleEmitter = new ParticleEmitter(GetSparkleCreator(sparkleMaterial), new Point(Center.X, Center.Y - 0.04f));
			sparkleEmitter.RenderLayer = 4;
			/*use ParticleEmitter
			if (corona == null)
			{
				corona = new PhysicsSprite(images.GetCoronaImage(currentTeam), DrawArea);
				corona.BlendMode = BlendMode.Additive;
				corona.RenderLayer = -1;
				corona.RotationSpeed = 10;
				corona.Add<RotateSpriteByRotationSpeed>();
				entities.Add(corona);
			}
			else
				corona.Image = images.GetCoronaImage(currentTeam);
			corona.DrawArea = Rectangle.FromCenter(Center + new Point(0.0035f, 0.0055f),
				new Size(Size.Height));
			 */
			/*private ParticleEmitter sparkleEmitter;

			private ParticleEmitterData GetSparkleCreator(Material sparkleMaterial)
			{
				return new ParticleEmitterData
				{
					ParticleMaterial = sparkleMaterial,
					MaximumNumberOfParticles = 20,
					SpawnInterval = 0.2f,
					Size = new Range<Size>(new Size(0.02f), new Size(0.015f)),
					Force = new Point(0, 0.02f),
					LifeTime = 2f,
					StartVelocityVariance = new Point(0.05f, 0.05f)
				};
			}
			*/
			return null;
		}
	}
}