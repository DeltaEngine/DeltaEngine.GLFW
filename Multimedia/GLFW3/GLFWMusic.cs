using DeltaEngine.Core;
using System;
using System.IO;
using DeltaEngine.Extensions;
using DeltaEngine.Multimedia.GLFW.Helpers;
using System.Diagnostics;
using DeltaEngine.Multimedia.MusicStreams;

namespace DeltaEngine.Multimedia.GLFW
{
	public class GLFWMusic : Music
	{
		private readonly GLFWSoundDevice openAL;
		private readonly int channelHandle;
		private readonly int[] buffers;
		private readonly byte[] bufferData;
		private const int NumberOfBuffers = 2;
		private const int BufferSize = 1024 * 16;
		private BaseMusicStream musicStream;
		private AudioFormat format;
		private DateTime playStartTime;

		public override float DurationInSeconds
		{
			get
			{
				return musicStream.LengthInSeconds;
			}
		}

		public override float PositionInSeconds
		{
			get
			{
				var seconds = (float)DateTime.Now.Subtract(playStartTime).TotalSeconds;
				return MathExtensions.Round(seconds.Clamp(0f, DurationInSeconds), 2);
			}
		}

		protected GLFWMusic(string contentName, GLFWSoundDevice soundDevice, Settings settings)
			: base(contentName, soundDevice, settings)
		{
			openAL = soundDevice;
			channelHandle = openAL.CreateChannel();
			buffers = openAL.CreateBuffers(NumberOfBuffers);
			bufferData = new byte[BufferSize];
		}

		protected override void LoadData(Stream fileData)
		{
			try
			{
				var stream = new MemoryStream();
				fileData.CopyTo(stream);
				stream.Seek(0, SeekOrigin.Begin);
				musicStream = new MusicStreamFactory().Load(stream, "Content/" + Name);
				format = musicStream.Channels == 2 ? AudioFormat.Stereo16 : AudioFormat.Mono16;
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				if (Debugger.IsAttached)
					throw new MusicNotFoundOrAccessible(Name, ex);
			}
		}

		protected override void PlayNativeMusic(float volume)
		{
			musicStream.Rewind();
			for (int index = 0; index < NumberOfBuffers; index++)
				if (!Stream(buffers[index]))
					break;
			openAL.Play(channelHandle);
			openAL.SetVolume(channelHandle, volume);
			playStartTime = DateTime.Now;
		}

		protected override void StopNativeMusic()
		{
			openAL.Stop(channelHandle);
			EmptyBuffers();
		}

		protected override void DisposeData()
		{
			base.DisposeData();
			openAL.DeleteBuffers(buffers);
			openAL.DeleteChannel(channelHandle);
			musicStream = null;
		}

		public override bool IsPlaying()
		{
			return GetState() != ChannelState.Stopped;
		}

		public override void Run()
		{
			if (UpdateBuffersAndCheckFinished())
				HandleStreamFinished();
			else
				if (!IsPlaying())
					openAL.Play(channelHandle);
		}

		private void EmptyBuffers()
		{
			int queued = openAL.GetNumberOfBuffersQueued(channelHandle);
			while (queued-- > 0)
				openAL.UnqueueBufferFromChannel(channelHandle);
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

		private bool Stream(int buffer)
		{
			try
			{
				int bytesRead = musicStream.Read(bufferData, BufferSize);
				if (bytesRead == 0)
					return false;
				openAL.BufferData(buffer, format, bufferData, bytesRead, musicStream.Samplerate);
				openAL.QueueBufferInChannel(buffer, channelHandle);
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
	}
}