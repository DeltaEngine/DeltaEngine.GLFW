using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Rendering;
using DeltaEngine.Rendering.Sprites;
using DeltaEngine.ScreenSpaces;

namespace SideScroller
{
	internal class ParallaxBackground : Entity2D
	{
		public ParallaxBackground(float baseScrollSpeed = 1)
			: base(Rectangle.Zero)
		{
			CreateLayers();
			//Start<ParallaxScroller>();
			BaseScrollSpeed = baseScrollSpeed;
		}

		private void CreateLayers()
		{
			//layerAlpha = new BackgroundLayer(contentLoader, "BgLowest", screenSpace, 0.2f);
			layerBeta = new BackgroundLayer("BgMiddle", 0.4f);
			//layerGamma = new BackgroundLayer(contentLoader, "BgForemost", screenSpace,0.8f);
		}

		internal BackgroundLayer /*layerAlpha,*/ layerBeta /*, layerGamma*/;
		public float BaseScrollSpeed { get; set; }

		internal class BackgroundLayer
		{
			public BackgroundLayer(string layerMaterialName, float factorToBaseSpeed)
			{
				FactorToBaseSpeed = factorToBaseSpeed;
				var layerMaterial = new Material(Shader.Position2DColorUv, layerMaterialName);
				var alphaDrawArea = new Rectangle(ScreenSpace.Current.Viewport.TopLeft,
					ScreenSpace.Current.Viewport.Size);
				var betaDrawArea = new Rectangle(ScreenSpace.Current.Viewport.TopRight,
					ScreenSpace.Current.Viewport.Size);
				SpriteAlpha = new Sprite(layerMaterial, alphaDrawArea);
				SpriteBeta = new Sprite(layerMaterial, betaDrawArea);
				SpriteAlpha.RenderLayer = (int)DefRenderLayer.Background + 1;
				SpriteBeta.RenderLayer = (int)DefRenderLayer.Background + 1;
			}

			internal Sprite SpriteAlpha, SpriteBeta;
			internal float FactorToBaseSpeed;

			public void MoveLayer(float offset)
			{
				var pointAlpha = GetFuturePointForLayer(SpriteAlpha.TopLeft, offset);
				var pointBeta = GetFuturePointForLayer(SpriteBeta.TopLeft, offset);

				if (pointAlpha.X < ScreenSpace.Current.Viewport.Left)
					pointAlpha.X = pointBeta.X + SpriteBeta.Size.Width;
				else if (pointBeta.X < ScreenSpace.Current.Viewport.Left)
					pointBeta.X = pointAlpha.X + SpriteAlpha.Size.Width;

				SpriteAlpha.TopLeft = pointAlpha;
				SpriteBeta.TopLeft = pointBeta;
			}

			private Point GetFuturePointForLayer(Point currentPoint, float offset)
			{
				return new Point(currentPoint.X - offset * FactorToBaseSpeed * Time.Delta, currentPoint.Y);
			}
		}
	}
}