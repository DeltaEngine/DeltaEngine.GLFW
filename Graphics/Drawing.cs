using System;
using System.Collections.Generic;
using DeltaEngine.Content;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Graphics.Vertices;

namespace DeltaEngine.Graphics
{
	/// <summary>
	/// Draws shapes, images and geometries. Framework independant, but needs a graphic device.
	/// </summary>
	public sealed class Drawing : IDisposable
	{
		public Drawing(Device device, Window window)
		{
			this.device = device;
			this.window = window;
		}

		private readonly Device device;
		private readonly Window window;

		public void Dispose()
		{
			foreach (var pair in buffersPerBlendMode)
				foreach (var buffer in pair.Value)
					buffer.Dispose();
			foreach (var buffer in lineBuffers)
				buffer.Dispose();
		}

		public void AddGeometry(Geometry geometry, Material material)
		{
			foreach (var sorted in sortedShaderGeometries)
				if (sorted.shader == material.Shader)
				{
					sorted.AddGeometry(geometry, material);
					return;
				}
			sortedShaderGeometries.Add(new GeometryPerShader(geometry, material, device));
		}

		private readonly List<GeometryPerShader> sortedShaderGeometries =
			new List<GeometryPerShader>();

		private class GeometryPerShader
		{
			public GeometryPerShader(Geometry geometry, Material material, Device device)
			{
				shader = material.Shader;
				this.device = device;
				AddGeometry(geometry, material);
			}

			private readonly Device device;

			public readonly Shader shader;

			public void AddGeometry(Geometry geometry, Material material)
			{
				foreach (var sorted in sortedTextureGeometries)
					if (sorted.texture == material.DiffuseMap)
					{
						sorted.geometries.Add(geometry);
						return;
					}
				sortedTextureGeometries.Add(new GeometryPerTexture(geometry, material));
			}

			private readonly List<GeometryPerTexture> sortedTextureGeometries =
				new List<GeometryPerTexture>();

			private class GeometryPerTexture
			{
				public GeometryPerTexture(Geometry geometry, Material material)
				{
					texture = material.DiffuseMap;
					geometries.Add(geometry);
				}

				public readonly Image texture;
				public readonly List<Geometry> geometries = new List<Geometry>();

				public void Draw()
				{
					foreach (var geometry in geometries)
						geometry.Draw();
				}
			}

			public void Draw()
			{
				shader.Bind();
				shader.SetModelViewProjectionMatrix(device.ModelViewProjectionMatrix);
				foreach (var textureGeometries in sortedTextureGeometries)
				{
					if (textureGeometries.texture != null)
						shader.SetDiffuseTexture(textureGeometries.texture);
					textureGeometries.Draw();
				}
			}
		}

		/// <summary>
		/// Adds presorted material DrawableEntities calls. Rendering happens after all vertices have
		/// been added at the end of the frame in <see cref="DrawEverythingInCurrentLayer"/>.
		/// </summary>
		public void Add<T>(Shader shader, Image image, T[] vertices, short[] indices = null,
			int numberOfVerticesUsed = 0, int numberOfIndicesUsed = 0) where T : struct, Vertex
		{
			GetDrawBuffer(shader, image.BlendMode).Add(image, vertices, indices, numberOfVerticesUsed,
				numberOfIndicesUsed);
		}

		public void Add<T>(Material material, BlendMode blendMode, T[] vertices,
			short[] indices = null, int numberOfVerticesUsed = 0, int numberOfIndicesUsed = 0)
			where T : struct, Vertex
		{
			GetDrawBuffer(material.Shader, blendMode).Add(material.DiffuseMap, vertices, indices,
				numberOfVerticesUsed, numberOfIndicesUsed);
		}

		private CircularBuffer GetDrawBuffer(Shader shader, BlendMode blendMode)
		{
			foreach (var pair in buffersPerBlendMode)
				if (pair.Key == blendMode)
				{
					foreach (var buffer in pair.Value)
						if (buffer.shader == shader)
							return buffer;
					var newBuffer = device.CreateCircularBuffer(shader as ShaderWithFormat, blendMode);
					pair.Value.Add(newBuffer);
					return newBuffer;
				}
			var initialBuffer = device.CreateCircularBuffer(shader as ShaderWithFormat, blendMode);
			buffersPerBlendMode.Add(blendMode, new List<CircularBuffer> { initialBuffer });
			return initialBuffer;
		}

		private readonly Dictionary<BlendMode, List<CircularBuffer>> buffersPerBlendMode =
			new Dictionary<BlendMode, List<CircularBuffer>>();

		public void AddLines<T>(Material material, T[] vertices) where T : struct, Vertex
		{
			if (material.DiffuseMap != null)
				throw new LineMaterialShouldNotUseDiffuseMap(material);
			GetDrawBufferForLines(material.Shader).Add(null, vertices);
		}

		public class LineMaterialShouldNotUseDiffuseMap : Exception
		{
			public LineMaterialShouldNotUseDiffuseMap(Material material)
				: base(material.ToString()) {}
		}

		private CircularBuffer GetDrawBufferForLines(Shader shader)
		{
			foreach (var buffer in lineBuffers)
				if (buffer.shader == shader)
					return buffer;
			var newBuffer = device.CreateCircularBuffer(shader as ShaderWithFormat, BlendMode.Normal,
				VerticesMode.Lines);
			lineBuffers.Add(newBuffer);
			return newBuffer;
		}

		private readonly List<CircularBuffer> lineBuffers = new List<CircularBuffer>();

		internal void DrawEverythingInCurrentLayer()
		{
			if (Has3DData())
				Draw3DData();
			Draw2DData();
		}

		private bool Has3DData()
		{
			if (sortedShaderGeometries.Count > 0)
				return true;
			foreach (var lineBuffer in lineBuffers)
				if (lineBuffer.Is3D && lineBuffer.NumberOfActiveVertices > 0)
					return true;
			foreach (var pair in buffersPerBlendMode)
				foreach (var buffer in pair.Value)
					if (buffer.Is3D && buffer.NumberOfActiveVertices > 0)
						return true;
			return false;
		}

		private void Draw3DData()
		{
			device.Set3DMode();
			foreach (var sortedGeometry in sortedShaderGeometries)
				sortedGeometry.Draw();
			sortedShaderGeometries.Clear();
			foreach (var lineBuffer in lineBuffers)
				if (lineBuffer.Is3D && lineBuffer.NumberOfActiveVertices > 0)
					DrawBufferAndIncreaseStatisticsNumbers(lineBuffer);
			foreach (var pair in buffersPerBlendMode)
				foreach (var buffer in pair.Value)
					if (buffer.Is3D && buffer.NumberOfActiveVertices > 0)
						DrawBufferAndIncreaseStatisticsNumbers(buffer);
			device.Set2DMode();
		}

		private void DrawBufferAndIncreaseStatisticsNumbers(CircularBuffer buffer)
		{
			NumberOfDynamicDrawCallsThisFrame++;
			NumberOfDynamicVerticesDrawnThisFrame += buffer.NumberOfActiveVertices;
			buffer.DrawAllTextureChunks();
		}

		private void Draw2DData()
		{
			foreach (var buffer in lineBuffers)
				if (!buffer.Is3D && buffer.NumberOfActiveVertices > 0)
					DrawBufferAndIncreaseStatisticsNumbers(buffer);
			foreach (var pair in buffersPerBlendMode)
				foreach (var buffer in pair.Value)
					if (!buffer.Is3D && buffer.NumberOfActiveVertices > 0)
						DrawBufferAndIncreaseStatisticsNumbers(buffer);
		}

		public int NumberOfDynamicVerticesDrawnThisFrame { get; internal set; }
		public int NumberOfDynamicDrawCallsThisFrame { get; internal set; }

		public Size ViewportPixelSize
		{
			get { return window.ViewportPixelSize; }
		}
	}
}