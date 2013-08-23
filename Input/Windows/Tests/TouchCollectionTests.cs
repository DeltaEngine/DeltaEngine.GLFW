using System.Collections.Generic;
using DeltaEngine.Datatypes;
using DeltaEngine.Platforms.Mocks;
using DeltaEngine.ScreenSpaces;
using NUnit.Framework;

namespace DeltaEngine.Input.Windows.Tests
{
	public class TouchCollectionTests
	{
		public TouchCollectionTests()
		{
			resolver = new MockResolver();
		}

		private readonly MockResolver resolver;

		[Test]
		public void FindIndexByIdOrGetFreeIndex()
		{
			var touchCollection = new TouchCollection(null);
			Assert.AreEqual(0, touchCollection.FindIndexByIdOrGetFreeIndex(478));
		}

		[Test]
		public void FindIndexByIdWithExistingId()
		{
			var touchCollection = new TouchCollection(null);
			touchCollection.ids[5] = 5893;
			Assert.AreEqual(5, touchCollection.FindIndexByIdOrGetFreeIndex(5893));
		}

		[Test]
		public void FindFreeIndex()
		{
			var touchCollection = new TouchCollection(null);
			Assert.AreEqual(0, touchCollection.FindIndexByIdOrGetFreeIndex(456));
		}

		[Test]
		public void FindFreeIndexWithoutAnyFreeIndices()
		{
			var touchCollection = new TouchCollection(null);
			for (int index = 0; index < touchCollection.ids.Length; index++)
				touchCollection.ids[index] = 1;
			Assert.AreEqual(-1, touchCollection.FindIndexByIdOrGetFreeIndex(546));
		}

		[Test]
		public void IsTouchDown()
		{
			Assert.True(TouchCollection.IsTouchDown(NativeTouchInput.FlagTouchDown));
			Assert.True(TouchCollection.IsTouchDown(NativeTouchInput.FlagTouchDownOrMoved));
			Assert.True(TouchCollection.IsTouchDown(NativeTouchInput.FlagTouchMove));
			Assert.False(TouchCollection.IsTouchDown(0x0008));
		}

		[Test]
		public void UpdateTouchState()
		{
			var touchCollection = new TouchCollection(null);
			touchCollection.UpdateTouchState(0, NativeTouchInput.FlagTouchDown);
			Assert.AreEqual(State.Pressing, touchCollection.states[0]);
			touchCollection.UpdateTouchState(0, 0);
			Assert.AreEqual(State.Releasing, touchCollection.states[0]);
		}

		[Test]
		public void CalculateQuadraticPosition()
		{
			TouchCollection touchCollection = CreateCollection();
			Point quadPosition = touchCollection.CalculateQuadraticPosition(400 * 100, 300 * 100);
			Assert.AreEqual(ScreenSpace.Current.FromPixelSpace(new Point(400, 300)), quadPosition);
		}

		[Test]
		public void ProcessNewTouches()
		{
			TouchCollection touchCollection = CreateCollection();
			var newTouches = new List<NativeTouchInput> { GetTestTouchInput() };
			touchCollection.ProcessNewTouches(newTouches);
			Assert.AreEqual(15, touchCollection.ids[0]);
			Assert.AreEqual(ScreenSpace.Current.FromPixelSpace(new Point(400, 300)),
				touchCollection.locations[0]);
			Assert.AreEqual(State.Pressing, touchCollection.states[0]);
		}

		private static NativeTouchInput GetTestTouchInput()
		{
			return new NativeTouchInput(NativeTouchInput.FlagTouchDown, 15, 40000, 30000);
		}

		[Test]
		public void UpdateTouchStateWithoutNewData()
		{
			TouchCollection touchCollection = CreateCollection();
			touchCollection.ids[0] = 15;
			touchCollection.states[0] = State.Releasing;
			touchCollection.UpdateTouchStateWithoutNewData(0);
			Assert.AreEqual(State.Released, touchCollection.states[0]);
			Assert.AreEqual(15, touchCollection.ids[0]);
			touchCollection.states[0] = State.Released;
			touchCollection.UpdateTouchStateWithoutNewData(0);
			Assert.AreEqual(State.Released, touchCollection.states[0]);
			Assert.AreEqual(-1, touchCollection.ids[0]);
		}

		[Test]
		public void UpdateAllTouches()
		{
			TouchCollection touchCollection = CreateCollection();
			var newTouches = new List<NativeTouchInput> { GetTestTouchInput() };
			touchCollection.ids[0] = 15;
			touchCollection.states[0] = State.Pressing;
			touchCollection.UpdateAllTouches(newTouches);
			Assert.AreEqual(0, newTouches.Count);
			Assert.AreEqual(15, touchCollection.ids[0]);
			Assert.AreEqual(ScreenSpace.Current.FromPixelSpace(new Point(400, 300)),
				touchCollection.locations[0]);
			Assert.AreEqual(State.Pressed, touchCollection.states[0]);
		}

		[Test]
		public void UpdateTouchWithUpdatedActiveTouch()
		{
			TouchCollection touchCollection = CreateCollection();
			var newTouches = new List<NativeTouchInput> { GetTestTouchInput() };
			touchCollection.ids[0] = 15;
			touchCollection.states[0] = State.Pressing;
			touchCollection.UpdatePreviouslyActiveTouches(newTouches);
			Assert.AreEqual(0, newTouches.Count);
			Assert.AreEqual(15, touchCollection.ids[0]);
			Assert.AreEqual(ScreenSpace.Current.FromPixelSpace(new Point(400, 300)),
				touchCollection.locations[0]);
			Assert.AreEqual(State.Pressed, touchCollection.states[0]);
		}

		[Test]
		public void UpdateTouchWithoutAnyActiveTouch()
		{
			TouchCollection touchCollection = CreateCollection();
			var newTouches = new List<NativeTouchInput>();
			touchCollection.ids[0] = 15;
			touchCollection.states[0] = State.Releasing;
			touchCollection.UpdateTouchBy(0, newTouches);
			Assert.AreEqual(15, touchCollection.ids[0]);
			Assert.AreEqual(State.Released, touchCollection.states[0]);
			touchCollection.UpdateTouchBy(0, newTouches);
			Assert.AreEqual(-1, touchCollection.ids[0]);
			Assert.AreEqual(State.Released, touchCollection.states[0]);
		}

		[Test]
		public void UpdateTouchIfPreviouslyPresentWithMultipleNewTouches()
		{
			TouchCollection touchCollection = CreateCollection();
			var newTouches = new List<NativeTouchInput>
			{
				new NativeTouchInput(3, 0, 0, 0),
				new NativeTouchInput(15, 0, 0, 0),
			};
			touchCollection.ids[0] = 15;
			touchCollection.states[0] = State.Releasing;
			touchCollection.UpdateTouchBy(0, newTouches);
			Assert.AreEqual(15, touchCollection.ids[0]);
			Assert.AreEqual(State.Released, touchCollection.states[0]);
		}

		private TouchCollection CreateCollection()
		{
			var window = resolver.Window;
			var positionTranslator = new CursorPositionTranslater(window);
			return new TouchCollection(positionTranslator);
		}
	}
}