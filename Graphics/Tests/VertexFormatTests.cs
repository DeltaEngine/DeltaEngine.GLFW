using DeltaEngine.Datatypes;
using DeltaEngine.Graphics.Vertices;
using NUnit.Framework;

namespace DeltaEngine.Graphics.Tests
{
	public class VertexFormatTests
	{
		[Test]
		public void VertexSizeInBytes()
		{
			Assert.AreEqual(VertexPosition2DUV.SizeInBytes, 16);
			Assert.AreEqual(VertexPosition2DColor.SizeInBytes, 12);
			Assert.AreEqual(VertexPosition2DColorUV.SizeInBytes, 20);
			Assert.AreEqual(VertexPosition3DUV.SizeInBytes, 20);
			Assert.AreEqual(VertexPosition3DColor.SizeInBytes, 16);
			Assert.AreEqual(VertexPosition3DColorUV.SizeInBytes, 24);
		}

		[Test]
		public void VertexPositionColorTextured2D()
		{
			var vertex = new VertexPosition3DColorUV(Point.Zero, Color.Red, Point.One);
			Assert.AreEqual(vertex.Position, Vector.Zero);
			Assert.AreEqual(vertex.Color, Color.Red);
			Assert.AreEqual(vertex.UV, Point.One);
		}

		[Test]
		public void VertexPositionColorTextured3D()
		{
			var vertex = new VertexPosition3DColorUV(Vector.UnitX, Color.Red, Point.One);
			Assert.AreEqual(vertex.Position, Vector.UnitX);
			Assert.AreEqual(vertex.Color, Color.Red);
			Assert.AreEqual(vertex.UV, Point.One);
		}

		[Test]
		public void VertexElementPosition3D()
		{
			var element = new VertexElement(VertexElementType.Position3D);
			Assert.AreEqual(VertexElementType.Position3D, element.ElementType);
			Assert.AreEqual(3, element.ComponentCount);
			Assert.AreEqual(12, element.Size);
		}

		[Test]
		public void VertexElementTextureUV()
		{
			var element = new VertexElement(VertexElementType.TextureUV);
			Assert.AreEqual(VertexElementType.TextureUV, element.ElementType);
			Assert.AreEqual(2, element.ComponentCount);
			Assert.AreEqual(8, element.Size);
		}

		[Test]
		public void VertexElementColor()
		{
			var element = new VertexElement(VertexElementType.Color);
			Assert.AreEqual(VertexElementType.Color, element.ElementType);
			Assert.AreEqual(4, element.ComponentCount);
			Assert.AreEqual(4, element.Size);
		}

		[Test]
		public void VertexFormatPosition3DTextureUVColor()
		{
			var elements = new[] {
				new VertexElement(VertexElementType.Position3D),
				new VertexElement(VertexElementType.TextureUV),
				new VertexElement(VertexElementType.Color)};
			var format = new VertexFormat(elements);
			Assert.AreEqual(24, format.Stride);
			Assert.AreEqual(0, elements[0].Offset);
			Assert.AreEqual(12, elements[1].Offset);
			Assert.AreEqual(20, elements[2].Offset);
		}

		[Test]
		public void VertexFormatGetVertexElement()
		{
			var elements = new[] {
				new VertexElement(VertexElementType.Position3D),
				new VertexElement(VertexElementType.TextureUV) };
			var format = new VertexFormat(elements);
			Assert.IsNull(format.GetElementFromType(VertexElementType.Color));
			Assert.IsNotNull(format.GetElementFromType(VertexElementType.TextureUV));
		}

		[Test]
		public void AreEqual()
		{
			var elements = new[] {
				new VertexElement(VertexElementType.Position3D),
				new VertexElement(VertexElementType.TextureUV) };
			var format = new VertexFormat(elements);
			Assert.IsTrue(VertexFormat.Position3DUv.Equals(format));
			Assert.AreEqual(VertexFormat.Position3DUv, format);
			Assert.IsTrue(VertexFormat.Position3DUv == format);
			Assert.IsTrue(VertexFormat.Position2DUv.Equals(VertexFormat.Position2DUv));
			Assert.IsFalse(VertexFormat.Position2DUv == VertexFormat.Position2DColor);
			Assert.AreEqual(VertexFormat.Position2DUv, VertexFormat.Position2DUv);
			Assert.AreNotEqual(VertexFormat.Position2DUv, VertexFormat.Position2DColor);
		}
	}
}
