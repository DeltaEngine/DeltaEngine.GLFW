﻿using System;
using DeltaEngine.Core;

namespace DeltaEngine.Networking.Mocks
{
	/// <summary>
	/// Mock client used for unit testing networking when using a real client and server would
	/// be much too slow.
	/// </summary>
	public class MockClient : Client
	{
		public MockClient(MockServer server)
		{
			this.server = server;
		}

		protected MockServer server;

		public void Connect(string serverAddress, int serverPort)
		{
			IsConnected = true;
			TargetAddress = serverAddress + ":" + serverPort;
			server.ClientConnectedToServer(this);
			if (Connected != null)
				Connected();
		}

		public bool IsConnected { get; private set; }
		public string TargetAddress { get; private set; }
		public event Action Connected;

		public void Send(object message)
		{
			LastSentMessage = message;
			if (DataSent != null)
				DataSent(message);
			if (IsConnected)
				server.ReceiveMessage(this, BinaryDataExtensions.ToByteArrayWithLengthHeader(message));
		}

		public object LastSentMessage { get; private set; }
		public event Action<object> DataSent;

		internal void Receive(object message)
		{
			LastReceivedMessage = message;
			if (DataReceived != null)
				DataReceived(message);
		}

		public object LastReceivedMessage { get; private set; }
		public event Action<object> DataReceived;

		public void Dispose()
		{
			if (IsConnected)
				server.ClientDisconnectedFromServer();
			if (Disconnected != null)
				Disconnected();
			server = null;
		}

		public event Action Disconnected;
	}
}
