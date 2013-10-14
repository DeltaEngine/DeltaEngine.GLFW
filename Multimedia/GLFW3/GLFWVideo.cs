using DeltaEngine.Content;
using DeltaEngine.Core;
using DeltaEngine.Entities;
using DeltaEngine.Rendering2D;
using DeltaEngine.ScreenSpaces;
using Image = DeltaEngine.Content.Image;
using Size = DeltaEngine.Datatypes.Size;
using System;
using System.Diagnostics;
using System.IO;
using DeltaEngine.Extensions;
using DeltaEngine.Multimedia.GLFW.Helpers;
using DeltaEngine.Multimedia.VideoStreams;

namespace DeltaEngine.Multimedia.GLFW
{
	public class GLFWVideo : Video
	{
		private Image image;
		private readonly GLFWSoundDevice openAL;
		private int channelHandle;
		private int[] buffers;
		private const int NumberOfBuffers = 4;
		private BaseVideoStream video;
		private AudioFormat format;
		private Sprite surface;
		private float elapsedSeconds;

		public override float DurationInSeconds
		{
			get
			{
				return video.LengthInSeconds;
			}
		}

		public override float PositionInSeconds
		{
			get
			{
				return MathExtensions.Round(elapsedSeconds.Clamp(0f, DurationInSeconds), 2);
			}
		}

		protected GLFWVideo(string contentName, GLFWSoundDevice soundDevice)
			: base(contentName, soundDevice)
		{
			channelHandle = openAL.CreateChannel();
			buffers = openAL.CreateBuffers(NumberOfBuffers);
			openAL = soundDevice;
		}

		protected override void PlayNativeVideo(float volume)
		{
			video.Rewind();
			for (int index = 0; index < NumberOfBuffers; index++)
				if (!Stream(buffers[index]))
					break;
			video.Play();
			openAL.Play(channelHandle);
			openAL.SetVolume(channelHandle, volume);
			elapsedSeconds = 0f;
			if (image == null)
				image = ContentLoader.Create<Image>(new ImageCreationData(new Size(video.Width, video.Height)) { BlendMode = BlendMode.Opaque });
			surface = new Sprite(new Material(ContentLoader.Load<Shader>(Shader.Position2DUV), image), ScreenSpace.Current.Viewport);
		}

		private void UpdateVideoTexture()
		{
			byte[] rgbaColors = video.ReadImageRgbaColors(Time.Delta);
			if (rgbaColors != null)
				image.Fill(rgbaColors);
			else
				Stop();
		}

		protected override void LoadData(Stream fileData)
		{
			try
			{
				video = new VideoStreamFactory().Load(fileData, "Content/" + Name);
				format = video.Channels == 2 ? AudioFormat.Stereo16 : AudioFormat.Mono16;
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				if (Debugger.IsAttached)
					throw new VideoNotFoundOrAccessible(Name, ex);
			}
		}

		protected override void DisposeData()
		{
			base.DisposeData();
			openAL.DeleteBuffers(buffers);
			openAL.DeleteChannel(channelHandle);
			video.Dispose();
			video = null;
		}

		private bool Stream(int buffer)
		{
			try
			{
				byte[] bufferData = new byte[4096];
				video.ReadMusicBytes(bufferData, bufferData.Length);
				openAL.BufferData(buffer, format, bufferData, bufferData.Length, video.Samplerate);
				openAL.QueueBufferInChannel(buffer, channelHandle);
			}
			catch
			{
				return false;
			}
			return true;
		}

		protected override void StopNativeVideo()
		{
			if (surface != null)
				surface.IsActive = false;
			elapsedSeconds = 0;
			surface = null;
			openAL.Stop(channelHandle);
			EmptyBuffers();
			video.Stop();
		}

		private void EmptyBuffers()
		{
			int queued = openAL.GetNumberOfBuffersQueued(channelHandle);
			while (queued-- > 0)
				openAL.UnqueueBufferFromChannel(channelHandle);
		}

		public override bool IsPlaying()
		{
			return GetState() != ChannelState.Stopped;
		}

		public override void Update()
		{
			if (GetState() == ChannelState.Paused)
				return;
			elapsedSeconds += Time.Delta;
			bool isFinished = UpdateBuffersAndCheckFinished();
			if (isFinished)
			{
				Stop();
				return;
			}
			UpdateVideoTexture();
			if (GetState() != ChannelState.Playing)
				openAL.Play(channelHandle);
		}

		private ChannelState GetState()
		{
			return openAL.GetChannelState(channelHandle);
		}

		private bool UpdateBuffersAndCheckFinished()
		{
			int processed = openAL.GetNumberOfBuffersProcessed(channelHandle);
			while (processed-- > 0)
			{
				int buffer = openAL.UnqueueBufferFromChannel(channelHandle);
				if (!Stream(buffer))
					return true;
			}
			return false;
		}
	}
}