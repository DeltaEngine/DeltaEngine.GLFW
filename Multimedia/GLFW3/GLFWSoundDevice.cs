using System;
using System.Runtime.InteropServices;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Multimedia.GLFW.Helpers;
using Pencil.Gaming.Audio;

namespace DeltaEngine.Multimedia.GLFW
{
	public sealed class GLFWSoundDevice : SoundDevice
	{
		public GLFWSoundDevice(Window window)
		{
			if (window == null)
				throw new ArgumentNullException("window");
		}

		public int CreateBuffer()
		{
			uint newBuffer;
			AL.GenBuffers(1, out newBuffer);
			return (int)newBuffer;
		}

		public int[] CreateBuffers(int numberOfBuffers)
		{
			uint[] newBuffers = new uint[numberOfBuffers];
			AL.GenBuffers(numberOfBuffers, newBuffers);
			int[] returnBuffers = new int[numberOfBuffers];
			for (int num = 0; num < numberOfBuffers; num++)
				returnBuffers[num] = (int)newBuffers[num];
			return returnBuffers;
		}

		public void DeleteBuffer(int bufferHandle)
		{
			uint handle = (uint)bufferHandle;
			AL.DeleteBuffers(1, ref handle);
		}

		public void DeleteBuffers(int[] bufferHandles)
		{
			uint[] buffers = new uint[bufferHandles.Length];
			for (int num = 0; num < bufferHandles.Length; num++)
				buffers[num] = (uint)bufferHandles[num];
			AL.DeleteBuffers(bufferHandles.Length, buffers);
		}

		public void BufferData(int bufferHandle, AudioFormat format, byte[] data, int length, int sampleRate)
		{
			GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			try
			{
				AL.BufferData((uint)bufferHandle, AudioFormatToALFormat(format), gcHandle.AddrOfPinnedObject(), length, sampleRate);
			}
			finally
			{
				gcHandle.Free();
			}
		}

		public int CreateChannel()
		{
			uint channel;
			AL.GenSources(1, out channel);
			return (int)channel;
		}

		public void DeleteChannel(int channelHandle)
		{
			uint handle = (uint)channelHandle;
			AL.DeleteSources(1, ref handle);
		}

		public void AttachBufferToChannel(int bufferHandle, int channelHandle)
		{
			AL.Source((uint)channelHandle, ALSourcei.Buffer, bufferHandle);
		}

		public void QueueBufferInChannel(int bufferHandle, int channelHandle)
		{
			AL.SourceQueueBuffers((uint)channelHandle, 1, new uint[] { (uint)bufferHandle });
		}

		public int UnqueueBufferFromChannel(int channelHandle)
		{
			uint[] bids = new uint[1];
			AL.SourceUnqueueBuffers((uint)channelHandle, 1, bids);
			return (int)bids[0];
		}

		public int GetNumberOfBuffersQueued(int channelHandle)
		{
			int numberOfBuffersQueued;
			AL.GetSource((uint)channelHandle, (ALSourcei)ALGetSourcei.BuffersQueued, out numberOfBuffersQueued);
			return numberOfBuffersQueued;
		}

		public int GetNumberOfBuffersProcessed(int channelHandle)
		{
			int numberOfBuffersProcessed;
			AL.GetSource((uint)channelHandle, (ALSourcei)ALGetSourcei.BuffersProcessed, out numberOfBuffersProcessed);
			return numberOfBuffersProcessed;
		}

		public ChannelState GetChannelState(int channelHandle)
		{
			int sourceState;
			AL.GetSource((uint)channelHandle, (ALSourcei)ALGetSourcei.SourceState, out sourceState);
			return ALSourceStateToChannelState((ALSourceState)sourceState);
		}

		public void SetVolume(int channelHandle, float volume)
		{
			AL.Source((uint)channelHandle, ALSourcef.Gain, volume);
		}

		public void SetPosition(int channelHandle, Vector3D position)
		{
			AL.Source((uint)channelHandle, ALSource3f.Position, position.X, position.Y, position.Z);
		}

		public void SetPitch(int channelHandle, float pitch)
		{
			AL.Source((uint)channelHandle, ALSourcef.Pitch, pitch);
		}

		public void Play(int channelHandle)
		{
			AL.SourcePlay((uint)channelHandle);
		}

		public void Stop(int channelHandle)
		{
			AL.SourceStop((uint)channelHandle);
		}

		public bool IsPlaying(int channelHandle)
		{
			int state;
			AL.GetSource((uint)channelHandle, (ALSourcei)ALGetSourcei.SourceState, out state);
			return state == (int)ALSourceState.Playing;
		}

		private static ALFormat AudioFormatToALFormat(AudioFormat audioFormat)
		{
			switch (audioFormat)
			{
				case AudioFormat.Mono8:
					return ALFormat.Mono8;
				case AudioFormat.Mono16:
					return ALFormat.Mono16;
				case AudioFormat.Stereo8:
					return ALFormat.Stereo8;
				default:
					return ALFormat.Stereo16;
			}
		}

		private static ChannelState ALSourceStateToChannelState(ALSourceState alSourceState)
		{
			switch (alSourceState)
			{
				case ALSourceState.Playing:
					return ChannelState.Playing;
				case ALSourceState.Paused:
					return ChannelState.Paused;
				default:
					return ChannelState.Stopped;
			}
		}
	}
}