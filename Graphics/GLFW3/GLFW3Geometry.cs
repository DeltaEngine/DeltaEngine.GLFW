using System;

namespace DeltaEngine.Graphics.GLFW3
{
	/// <summary>
	/// GPU geometry data in OpenGL
	/// </summary>
	public class GLFW3Geometry : Geometry
	{
		protected GLFW3Geometry(string contentName, GLFW3Device device)
			: base(contentName)
		{
			this.device = device;
		}

		private GLFW3Geometry(GeometryCreationData creationData, GLFW3Device device)
			: base(creationData)
		{
			this.device = device;
		}

		private readonly GLFW3Device device;

		protected override void SetNativeData(byte[] vertexData, short[] indices)
		{
			if (vertexBufferHandle == GLFW3Device.InvalidHandle)
				CreateBuffers();
			device.LoadVertexData(0, vertexData, vertexData.Length);
			device.LoadIndices(0, indices, indices.Length * sizeof(short));
		}

		private int vertexBufferHandle = GLFW3Device.InvalidHandle;

		private void CreateBuffers()
		{
			int vertexDataSize = NumberOfVertices * Format.Stride;
			vertexBufferHandle = device.CreateVertexBuffer(vertexDataSize, GLFW3BufferMode.Static);
			if (vertexBufferHandle == GLFW3Device.InvalidHandle)
				throw new UnableToCreateOpenGLGeometry();
			indexBufferHandle = device.CreateIndexBuffer(NumberOfIndices * sizeof(short),
				GLFW3BufferMode.Static);
		}

		private class UnableToCreateOpenGLGeometry : Exception {}

		private int indexBufferHandle = GLFW3Device.InvalidHandle;

		public override void Draw()
		{
			if (vertexBufferHandle == GLFW3Device.InvalidHandle)
				throw new UnableToDrawDynamicGeometrySetDataNeedsToBeCalledFirst();
			device.BindVertexBuffer(vertexBufferHandle);
			device.BindIndexBuffer(indexBufferHandle);
			device.DrawTriangles(0, NumberOfIndices);
		}

		private class UnableToDrawDynamicGeometrySetDataNeedsToBeCalledFirst : Exception {}

		protected override void DisposeData()
		{
			if (vertexBufferHandle == GLFW3Device.InvalidHandle)
				return;
			device.DeleteBuffer(vertexBufferHandle);
			device.DeleteBuffer(indexBufferHandle);
			vertexBufferHandle = GLFW3Device.InvalidHandle;
		}
	}
}