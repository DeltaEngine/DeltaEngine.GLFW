using DeltaEngine.Commands;
using DeltaEngine.Datatypes;
using DeltaEngine.Graphics;
using DeltaEngine.Input;
using DeltaEngine.Platforms;
using DeltaEngine.Rendering.Cameras;
using DeltaEngine.Rendering.Shapes3D;
using DeltaEngine.ScreenSpaces;
using NUnit.Framework;

namespace DeltaEngine.Rendering.Models.Tests
{
	public class ModelTests : TestWithMocksOrVisually
	{
		[Test]
		public void RenderFromZPositive()
		{
			RenderBoxFrom(Vector.UnitZ);
		}

		[Test]
		public void RenderFromZNegative()
		{
			RenderBoxFrom(-Vector.UnitZ);
		}

		[Test]
		public void RenderFromXPositive()
		{
			RenderBoxFrom(Vector.UnitX);
		}

		[Test]
		public void RenderFromXNegative()
		{
			RenderBoxFrom(-Vector.UnitX);
		}

		private void RenderBoxFrom(Vector vector)
		{
			Resolve<Window>().BackgroundColor = Color.Grey;
			new LookAtCamera(Resolve<Device>(), Resolve<Window>(), vector * 5.0f, Vector.Zero);
			new Model(new ModelData(new Box(Vector.One, Color.Red)), Vector.Zero);
			RenderCoordinateSystemCross();
		}

		private static void RenderCoordinateSystemCross()
		{
			new Line3D(-Vector.UnitX * 0.25f, Vector.UnitX, Color.Red);
			new Line3D(-Vector.UnitY * 0.25f, Vector.UnitY, Color.Green);
			new Line3D(-Vector.UnitZ * 0.25f, Vector.UnitZ, Color.Blue);
		}

		[Test]
		public void RayPick()
		{
			var camera = new LookAtCamera(Resolve<Device>(), Resolve<Window>(),
				new Vector(4.0f, 8.0f, 4.0f), Vector.Zero);
			var cube = new Model(new ModelData(new Box(Vector.One, Color.Red)), Vector.Zero);
			var floor = new Plane(Vector.UnitY, 0.0f);
			new Command(point =>
			{
				var ray = camera.ScreenPointToRay(ScreenSpace.Current.ToPixelSpace(point));
				cube.Position = floor.Intersect(ray);
			}).Add(new MouseButtonTrigger(MouseButton.Left, State.Pressed));
		}
	}
}