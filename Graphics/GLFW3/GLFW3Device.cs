using System;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using Pencil.Gaming;
using Pencil.Gaming.Graphics;
using DeltaEngine.Graphics.Vertices;

namespace DeltaEngine.Graphics.GLFW3
{
	public sealed class GLFW3Device : Device
	{
		private GlfwWindowPtr nativeWindow;
		private BlendMode currentBlendMode = BlendMode.Opaque;
		private bool cullBackFaces;
		public const int InvalidHandle = -1;

		public override bool CullBackFaces
		{
			get
			{
				return cullBackFaces;
			}
			set
			{
				if (cullBackFaces == value)
					return;
				cullBackFaces = value;
				if (cullBackFaces)
				{
					GL.Enable(EnableCap.CullFace);
					GL.FrontFace(FrontFaceDirection.Cw);
					GL.CullFace(CullFaceMode.Back);
				}
				else
					GL.Disable(EnableCap.CullFace);
			}
		}

		public GLFW3Device(Window window)
			: base(window)
		{
			nativeWindow = (GlfwWindowPtr)window.Handle;
			CheckOpenGLVersion();
		}

		private static void CheckOpenGLVersion()
		{
			var version = GL.GetString(StringName.Version);
			var extensions = GL.GetString(StringName.Extensions);
			var majorVersion = int.Parse(version[0] + "");
			if (majorVersion < 3 || string.IsNullOrEmpty(extensions))
				throw new GLFW3Device.OpenGLVersionDoesNotSupportShaders();
		}

		public override void Clear()
		{
			var color = window.BackgroundColor;
			if (color.A == 0)
				return;
			GL.ClearColor(color.RedValue, color.GreenValue, color.BlueValue, color.AlphaValue);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public override void Present()
		{
			Glfw.SwapBuffers(nativeWindow);
		}

		public override void Dispose()
		{
		}

		protected override void OnFullscreenChanged(Size displaySize, bool wantFullscreen)
		{
			nativeWindow = (GlfwWindowPtr)window.Handle;
			base.OnFullscreenChanged(displaySize, wantFullscreen);
		}

		public override void SetBlendMode(BlendMode blendMode)
		{
			if (currentBlendMode == blendMode)
				return;
			currentBlendMode = blendMode;
			switch (blendMode)
			{
				case BlendMode.Opaque:
					GL.Disable(EnableCap.Blend);
					break;
				case BlendMode.Normal:
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					GL.BlendEquation(BlendEquationMode.FuncAdd);
					break;
				case BlendMode.AlphaTest:
					GL.Disable(EnableCap.Blend);
					GL.Enable(EnableCap.AlphaTest);
					break;
				case BlendMode.Additive:
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
					GL.BlendEquation(BlendEquationMode.FuncAdd);
					break;
				case BlendMode.Subtractive:
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
					GL.BlendEquation(BlendEquationMode.FuncReverseSubtract);
					break;
				case BlendMode.LightEffect:
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.One);
					GL.BlendEquation(BlendEquationMode.FuncAdd);
					break;
			}
		}

		public override void SetViewport(Size viewportSize)
		{
			GL.Viewport(0, 0, (int)viewportSize.Width, (int)viewportSize.Height);
			SetModelViewProjectionMatrixFor2D();
		}

		public int CreateVertexBuffer(int sizeInBytes, GLFW3BufferMode mode)
		{
			int bufferHandle;
			GL.GenBuffers(1, out bufferHandle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandle);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)sizeInBytes, IntPtr.Zero, GetBufferMode(mode));
			return bufferHandle;
		}

		private static BufferUsageHint GetBufferMode(GLFW3BufferMode mode)
		{
			return mode == GLFW3BufferMode.Static ? BufferUsageHint.StaticDraw : mode == GLFW3BufferMode.Dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StreamDraw;
		}

		public void BindVertexBuffer(int bufferHandle)
		{
			GL.BindBuffer(BufferTarget.ArrayBuffer, bufferHandle);
		}

		public void LoadVertexData<T>(int offset, T[] vertices, int vertexDataSizeInBytes) where T : struct
		{
			GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)offset, (IntPtr)vertexDataSizeInBytes, vertices);
		}

		public int CreateIndexBuffer(int sizeInBytes, GLFW3BufferMode mode)
		{
			int bufferHandle;
			GL.GenBuffers(1, out bufferHandle);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferHandle);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)sizeInBytes, IntPtr.Zero, GetBufferMode(mode));
			return bufferHandle;
		}

		public void BindIndexBuffer(int bufferHandle)
		{
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferHandle);
		}

		public void LoadIndices(int offset, short[] indices, int indexDataSizeInBytes)
		{
			GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offset, (IntPtr)indexDataSizeInBytes, indices);
		}

		public void DeleteBuffer(int bufferHandle)
		{
			GL.DeleteBuffers(1, ref bufferHandle);
		}

		public override void EnableDepthTest()
		{
			GL.Enable(EnableCap.DepthTest);
		}

		public override void DisableDepthTest()
		{
			GL.Disable(EnableCap.DepthTest);
		}

		public void EnableTexturing()
		{
		}

		public void DisableTexturing()
		{
		}

		public int GenerateTexture()
		{
			int glHandle;
			GL.GenTextures(1, out glHandle);
			return glHandle;
		}

		public void BindTexture(int glHandle, int samplerIndex = 0)
		{
			GL.ActiveTexture(GetSamplerFrom(samplerIndex));
			GL.BindTexture(TextureTarget.Texture2D, glHandle);
		}

		private static TextureUnit GetSamplerFrom(int samplerIndex)
		{
			if (samplerIndex == 0)
				return TextureUnit.Texture0;
			if (samplerIndex == 1)
				return TextureUnit.Texture1;
			throw new GLFW3Device.UnsupportedTextureUnit();
		}

		public void LoadTexture(Size size, IntPtr data, bool hasAlpha)
		{
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)size.Width, (int)size.Height, 0, hasAlpha ? PixelFormat.Bgra : PixelFormat.Bgr, PixelType.UnsignedByte, data);
		}

		public void LoadTexture(Size size, byte[] data, bool hasAlpha)
		{
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)size.Width, (int)size.Height, 0, hasAlpha ? PixelFormat.Bgra : PixelFormat.Bgr, PixelType.UnsignedByte, data);
		}

		public void DeleteTexture(int glHandle)
		{
			GL.DeleteTextures(1, ref glHandle);
		}

		public void SetTextureSamplerState(bool disableLinearFiltering = false, bool allowTiling = false)
		{
			var minFilter = disableLinearFiltering ? TextureMinFilter.Nearest : TextureMinFilter.Linear;
			var magFilter = disableLinearFiltering ? TextureMagFilter.Nearest : TextureMagFilter.Linear;
			var clampMode = allowTiling ? TextureWrapMode.Repeat : TextureWrapMode.ClampToEdge;
			SetSamplerState((int)minFilter, (int)magFilter, (int)clampMode);
		}

		private static void SetSamplerState(int textureMinFilter, int textureMagFilter, int clampMode)
		{
			SetTexture2DParameter(TextureParameterName.TextureMinFilter, textureMinFilter);
			SetTexture2DParameter(TextureParameterName.TextureMagFilter, textureMagFilter);
			SetTexture2DParameter(TextureParameterName.TextureWrapS, clampMode);
			SetTexture2DParameter(TextureParameterName.TextureWrapT, clampMode);
		}

		private static void SetTexture2DParameter(TextureParameterName name, int value)
		{
			GL.TexParameter(TextureTarget.Texture2D, name, value);
		}

		public int CreateShaderProgram(string vertexShaderCode, string fragmentShaderCode)
		{
			int programHandle = GL.CreateProgram();
			int vertexShader = CreateShader(ShaderType.VertexShader, vertexShaderCode);
			int fragmentShader = CreateShader(ShaderType.FragmentShader, fragmentShaderCode);
			GL.AttachShader(programHandle, vertexShader);
			GL.AttachShader(programHandle, fragmentShader);
			GL.LinkProgram(programHandle);
			return programHandle;
		}

		private static int CreateShader(ShaderType shaderType, string code)
		{
			int shader = GL.CreateShader(shaderType);
			int codeLength = code.Length;
			GL.ShaderSource(shader, 1, new [] { code }, ref codeLength);
			GL.CompileShader(shader);
			return shader;
		}

		public void UseShaderProgram(int programHandle)
		{
			GL.UseProgram(programHandle);
		}

		public void DeleteShaderProgram(int programHandle)
		{
			GL.DeleteProgram(programHandle);
		}

		public int GetShaderAttributeLocation(int programHandle, string attributeName)
		{
			return GL.GetAttribLocation(programHandle, attributeName);
		}

		public int GetShaderUniformLocation(int programHandle, string uniformName)
		{
			return GL.GetUniformLocation(programHandle, uniformName);
		}

		public void DefineVertexAttributeWithFloats(int attributeLocation, int numberOfFloatComponents, int vertexTotalSize, int attributeOffsetInVertex)
		{
			GL.EnableVertexAttribArray(attributeLocation);
			GL.VertexAttribPointer(attributeLocation, numberOfFloatComponents, VertexAttribPointerType.Float, false, vertexTotalSize, (IntPtr)attributeOffsetInVertex);
		}

		public void DefineVertexAttributeWithBytes(int attributeLocation, int numberOfByteComponents, int vertexTotalSize, int attributeOffsetInVertex)
		{
			GL.EnableVertexAttribArray(attributeLocation);
			GL.VertexAttribPointer(attributeLocation, numberOfByteComponents, VertexAttribPointerType.UnsignedByte, true, vertexTotalSize, (IntPtr)attributeOffsetInVertex);
		}

		public void SetUniformValue(int location, int value)
		{
			GL.Uniform1(location, value);
		}

		public void SetUniformValue(int location, Matrix matrix)
		{
			GL.UniformMatrix4(location, 1, false, matrix.GetValues);
		}

		public void SetUniformValue(int location, Vector3D vector)
		{
			GL.Uniform3(location, vector.X, vector.Y, vector.Z);
		}

		public void SetUniformValues(int location, Matrix[] matrices)
		{
			var values = new float[matrices.Length * 16];
			for (int matrixIndex = 0; matrixIndex < matrices.Length; ++matrixIndex)
				matrices[matrixIndex].GetValues.CopyTo(values, matrixIndex * 16);
			GL.UniformMatrix4(location, matrices.Length, false, values);
		}

		public void DrawTriangles(int indexOffsetInBytes, int numberOfIndicesToRender)
		{
			Shader.BindVertexDeclaration();
			GL.DrawElements(BeginMode.Triangles, numberOfIndicesToRender, DrawElementsType.UnsignedShort, (IntPtr)indexOffsetInBytes);
		}

		public void DrawLines(int vertexOffset, int verticesCount)
		{
			Shader.BindVertexDeclaration();
			GL.DrawArrays(BeginMode.Lines, vertexOffset, verticesCount);
		}

		public void ReadPixels(Rectangle frame, byte[] bufferToStoreData)
		{
			GL.ReadPixels((int)frame.Left, (int)frame.Top, (int)frame.Width, (int)frame.Height, PixelFormat.Rgb, PixelType.UnsignedByte, bufferToStoreData);
		}

		public override CircularBuffer CreateCircularBuffer(ShaderWithFormat shader, BlendMode blendMode, VerticesMode drawMode = VerticesMode.Triangles)
		{
			return new GLFW3CircularBuffer(this, shader, blendMode, drawMode);
		}

		private class OpenGLVersionDoesNotSupportShaders : Exception
		{
		}
		private class UnsupportedTextureUnit : Exception
		{
		}
	}
}