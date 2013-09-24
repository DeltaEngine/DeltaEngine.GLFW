using System;
using System.Collections.Generic;
using DeltaEngine.Core;
using DeltaEngine.Graphics.Vertices;

namespace DeltaEngine.Graphics.GLFW3
{
	/// <summary>
	/// Functionality for all OpenGL based circular buffers to render small batches quickly.
	/// </summary>
	public class GLFW3CircularBuffer : CircularBuffer
	{
		public GLFW3CircularBuffer(Device device, ShaderWithFormat shader, BlendMode blendMode,
			VerticesMode drawMode = VerticesMode.Triangles)
			: base(device, shader, blendMode, drawMode) {}

		protected override void CreateNative()
		{
			glDevice = (GLFW3Device)device;
			nativeVertexBuffer = glDevice.CreateVertexBuffer(maxNumberOfVertices * vertexSize,
				GLFW3BufferMode.Stream);
			if (nativeVertexBuffer == GLFW3Device.InvalidHandle)
				throw new UnableToCreateOpenGLGeometry();
			if (UsesIndexBuffer)
				nativeIndexBuffer = glDevice.CreateIndexBuffer(maxNumberOfIndices * indexSize,
					GLFW3BufferMode.Stream);
		}

		private class UnableToCreateOpenGLGeometry : Exception {}

		private GLFW3Device glDevice;
		private int nativeVertexBuffer;
		private int nativeIndexBuffer;

		protected override void DisposeNextFrame()
		{
			buffersToDisposeNextFrame.Add(nativeVertexBuffer);
			if (UsesIndexBuffer)
				buffersToDisposeNextFrame.Add(nativeIndexBuffer);
		}

		private readonly List<int> buffersToDisposeNextFrame = new List<int>();

		protected override void AddDataNative<VertexType>(Chunk textureChunk, VertexType[] vertexData,
			short[] indices, int numberOfVertices, int numberOfIndices)
		{
			glDevice.BindVertexBuffer(nativeVertexBuffer);
			glDevice.LoadVertexData(totalVertexOffsetInBytes, vertexData, vertexSize * numberOfVertices);
			if (!UsesIndexBuffer)
				return;
			if (indices == null)
				indices = ComputeIndices(textureChunk.NumberOfVertices, numberOfVertices);
			else if (totalIndicesCount > 0)
				indices = RemapIndices(indices, numberOfIndices);
			glDevice.BindIndexBuffer(nativeIndexBuffer);
			glDevice.LoadIndices(totalIndexOffsetInBytes, indices, indexSize * numberOfIndices);
		}

		public override void DisposeUnusedBuffersFromPreviousFrame()
		{
			if (buffersToDisposeNextFrame.Count <= 0)
				return;
			foreach (var buffer in buffersToDisposeNextFrame)
				glDevice.DeleteBuffer(buffer);
			buffersToDisposeNextFrame.Clear();
		}

		public override void DrawAllTextureChunks()
		{
			glDevice.BindVertexBuffer(nativeVertexBuffer);
			if (UsesIndexBuffer)
				glDevice.BindIndexBuffer(nativeIndexBuffer);
			base.DrawAllTextureChunks();
		}

		protected override void DrawChunk(Chunk chunk)
		{
			if (Is3D && blendMode == BlendMode.Additive)
				device.DisableDepthTest();
			if (UsesIndexBuffer)
			{
				if (chunk.Texture != null)
					shader.SetDiffuseTexture(chunk.Texture);
				glDevice.DrawTriangles(chunk.FirstIndexOffsetInBytes, chunk.NumberOfIndices);
			}
			else
				glDevice.DrawLines(chunk.FirstVertexOffsetInBytes / vertexSize, chunk.NumberOfVertices);
		}

		protected override void DisposeNative()
		{
			glDevice.DeleteBuffer(nativeVertexBuffer);
			if (UsesIndexBuffer)
				glDevice.DeleteBuffer(nativeIndexBuffer);
		}
	}
}