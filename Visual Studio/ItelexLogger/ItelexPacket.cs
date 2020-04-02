using System;

namespace ItelexLogger
{
	public enum ItelexCommands
	{
		Heartbeart = 0,
		DirectDial = 1,
		BaudotData = 2,
		End = 3,
		Reject = 4,
		Ack = 6,
		ProtocolVersion = 7,
		SelfTest = 8,
		RemoteConfig = 9
	}

	class ItelexPacket
	{
		public int Command { get; set; }

		public ItelexCommands CommandType => (ItelexCommands)Command;

		public byte[] Data { get; set; }

		public int Len => Data == null ? 0 : Data.Length;

		public ItelexPacket() { }

		public ItelexPacket(byte[] buffer, int index)
		{
			Command = buffer[index];
			int len = buffer[index + 1];
			if (len > 0)
			{
				Data = new byte[len];
				Buffer.BlockCopy(buffer, index + 2, Data, 0, len);
			}
			else
			{
				Data = null;
			}
		}

		public ItelexPacket(byte[] buffer): this(buffer, 0)
		{
		}

		/*
		public void Dump(string pre)
		{
			Debug.Write($"{pre}: cmd={CommandType} [{Len}]");
			for (int i = 0; i < Len; i++)
			{
				Debug.Write($" {Data[i]:X2}");
			}
			Debug.WriteLine("");
		}
		*/

		public string GetDebugData()
		{
			string debStr = "";
			for (int i = 0; i < Len; i++)
			{
				debStr += $" {Data[i]:X2}";
			}
			return debStr.Trim();
		}

		public string GetDebugPacket()
		{
			return $"{Command:X02} {Len:X02} " + GetDebugData();
		}

		public override string ToString()
		{
			return $"{CommandType} {Len}: {GetDebugData()}";
		}

	}
}
