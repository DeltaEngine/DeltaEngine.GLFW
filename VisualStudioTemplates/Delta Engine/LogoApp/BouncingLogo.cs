using DeltaEngine;
using DeltaEngine.Commands;
using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Multimedia;
using DeltaEngine.Physics2D;
using DeltaEngine.Rendering.Sprites;

namespace $safeprojectname$
{
	public class BouncingLogo : Sprite
	{
		public BouncingLogo() : base(ContentLoader.Load<Material>("Logo"), Point.Half)
		{
			Color = Color.GetRandomColor();
			this.StartRotating(random.Get(-50, 50));
			this.StartBouncingOffScreenEdges(new Point(random.Get(-0.4f, 0.4f), random.Get(-0.4f, 
				0.4f)), () => sound.Play(0.03f));
			new Command(Command.Click, position => Center = position);
		}

		private readonly Randomizer random = Randomizer.Current;
		private readonly Sound sound = ContentLoader.Load<Sound>("BorderHit");
	}
}