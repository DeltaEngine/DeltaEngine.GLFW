namespace DeltaEngine.Graphics.Mocks
{
	/// <summary>
	/// Mock geometry used in unit tests.
	/// </summary>
	public class MockGeometry : Geometry
	{
		public MockGeometry(string contentName, Device device)
			: base(contentName) { }

		private MockGeometry(GeometryCreationData creationData, Device device)
			: base(creationData) { }

		public override void Draw() {}
		protected override void SetNativeData(byte[] vertexData, short[] indices) {}
		protected override void DisposeData() { }
	}
}