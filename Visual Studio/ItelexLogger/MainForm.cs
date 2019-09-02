using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ItelexLogger
{
	public enum CodeStandards { Ita2, UsTTy }

	public enum ConnectionType { Unknown, Itelex, Ascii }

	public enum Directions { Recv, Send }

	public partial class MainForm : Form
	{
		private string _serialBuffer;

		private string _currentLine;
		private int _currentXpos;

		private int _extension;

		private ConnectionType _connectionType = ConnectionType.Unknown;

		private LogView _logView;

		private object _commLogLock = new object();

		private ComPortManager _comPortManager;

		private CodeConversion.ShiftStates _shiftState;

		private bool _needNewline = false;

		public MainForm()
		{
			InitializeComponent();

			_comPortManager = new ComPortManager();
			_comPortManager.DataReceived += ComPortManager_DataReceived;

			OutputListView.View = View.Details;
			OutputListView.HideSelection = true;
			OutputListView.FullRowSelect = true;
			OutputListView.Sorting = SortOrder.None;
			OutputListView.Columns[0].Width = OutputListView.Width - 4;
			OutputListView.HeaderStyle = ColumnHeaderStyle.None;

			ComPortsCb.DataSource = _comPortManager.GetPorts();
			ComPortsCb.DisplayMember = "ComPort";

			//Init("COM6");
		}
		private void ConnectCb_Click(object sender, EventArgs e)
		{
			if (!ConnectCb.Checked)
			{
				if (_comPortManager.IsConnected)
				{
					Deinit();
				}
				ConnectCb.Text = "Connect";
				//ConnectCb.Checked = false;
			}
			else
			{
				if (!_comPortManager.IsConnected)
				{
					ComPortSelectionItem item = (ComPortSelectionItem)ComPortsCb.SelectedItem;
					ConnectCb.Checked = false;
					if (item != null)
					{
						if (_comPortManager.OpenComPort(item.ComPort))
						{
							ConnectCb.Text = "Disconnect";
							ConnectCb.Checked = true;
							Init();
						}
					}
				}
			}

		}

		public void Init()
		{
			_serialBuffer = "";
			_shiftState = CodeConversion.ShiftStates.Figs;
			OutputListView.Items.Clear();
			_currentLine = "";
			_currentXpos = 0;
			_extension = 0;
		}

		private void ComPortManager_DataReceived(string data)
		{
			_serialBuffer += data;

			while (true)
			{
				int pos = _serialBuffer.IndexOf('\x0A');
				if (pos == -1)
				{
					// line not complete
					return;
				}

				string line = _serialBuffer.Substring(0, pos + 1);
				if (pos < _serialBuffer.Length)
				{
					_serialBuffer = _serialBuffer.Substring(pos + 1);
				}
				else
				{
					_serialBuffer = "";
				}

				DebugLog(line);
				LogLister.Instance.Add(line);
				_logView?.Log(line);
				DoLine(line);
			}
		}

		private void Deinit()
		{
			_comPortManager.CloseComPort();
		}

		private void DoLine(string line)
		{
			if (line.Length<13)
			{
				// error
				return;
			}

			line = line.Trim(new char[] { '\r', '\n', ' ' });

			Debug.WriteLine(line);

			string timestamp = line.Substring(0, 11);
			line = line.Substring(13);

			string col1;
			line = GetNextColumn(line, out col1);
			if (line==null)
			{
				return;
			}

			if (CheckLeftString(line, "Server-Socket #"))
			{
				ParseIncomingConnection(line);
				return;
			}

			if (CheckLeftString(line, "Durchwahl-Anfrage"))
			{
				ParseDirectDial(line);
				return;
			}

			if (CheckLeftString(line, "Verbindungsaufbau zu"))
			{
				ParseOutgoingConnectionIp(line);
				return;
			}
			if (CheckLeftString(line, "Client-Socket #"))
			{
				ParseOutgoingConnection(line);
				WriteMessage("connect");
				_connectionType = ConnectionType.Unknown;
				return;
			}

			// wegen eines Fehlers bei der Log-Ausgabe bei ASCII-Empfang, kann es sein,
			// dass die Socket-Empfang-Meldung an einer vorhergehenden Zeile dranhängt.
			int idx = line.IndexOf("Socket Empfang:");
			if (idx!=-1)
			{
				line = line.Substring(idx + 15);
				ParseSocketSendRecv(line, Directions.Recv);
				return;
			}

			idx = line.IndexOf("Socket Sendung:");
			if (idx != -1)
			{
				line = line.Substring(idx + 15);
				ParseSocketSendRecv(line, Directions.Send);
				return;
			}
		}

		private bool CheckLeftString(string str, string subStr)
		{
			return str.Length >= subStr.Length && str.Substring(0, subStr.Length) == subStr;
		}

		private string GetNextColumn(string line, out string column)
		{
			column = null;
			if (string.IsNullOrWhiteSpace(line))
			{
				return "";
			}
			line = line.Trim();

			int pos = line.IndexOf(':');
			if (pos==-1)
			{
				return line;
			}

			column = line.Substring(0, pos + 1);
			if (pos < line.Length)
			{
				line = line.Substring(pos + 1);
			}
			else
			{
				line = "";
			}
			return line.Trim();
		}

		private void ParseIncomingConnection(string data)
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				return;
			}

			int pos1 = data.IndexOf("IP");
			int pos2 = data.IndexOf("/");
			if (pos1==-1 || pos2==-1 || pos2<pos1)
			{
				return;
			}
			string ip = data.Substring(pos1 + 2, pos2 - pos1 - 2).Trim();

			pos1 = data.IndexOf("MAC");
			pos2 = data.IndexOf("...");
			if (pos1 == -1 || pos2 == -1 || pos2 < pos1)
			{
				return;
			}
			string mac = data.Substring(pos1 + 3, pos2 - pos1 - 4).Trim();

			WriteMessage($"\r\n");
			WriteMessage($"incoming ip={ip} / mac={mac} ({DateTime.Now})");
		}

		private void ParseDirectDial(string data)
		{
			if (string.IsNullOrWhiteSpace(data) || data.Length<=18)
			{
				return;
			}

			string extStr = data.Substring(18).Trim();
			int ext;
			if (!int.TryParse(extStr, out ext))
			{
				return;
			}
			WriteMessage($"recv direct dial {ext}");
		}

		private void ParseOutgoingConnectionIp(string data)
		{
			//"Verbindungsaufbau zu IP 84.157.50.239 Port 8134"

			if (string.IsNullOrWhiteSpace(data))
			{
				return;
			}

			int pos1 = data.IndexOf("IP");
			int pos2 = data.IndexOf("Port");
			if (pos1 == -1 || pos2 == -1 || pos2 < pos1)
			{
				return;
			}
			string ip = data.Substring(pos1 + 2, pos2 - pos1 - 2).Trim();

			pos1 = data.IndexOf("Port");
			if (pos1 == -1)
			{
				return;
			}
			string port = data.Substring(pos1 + 4, data.Length - pos1 - 4).Trim();

			WriteMessage($"\r\n");
			WriteMessage($"outgoing ip={ip}:{port} ({DateTime.Now})");
		}

		private void ParseOutgoingConnection(string data)
		{
			//"Client-Socket #1 iTelex erfolgreich geoeffnet -> sende Durchwahl 12 und Version 1"
			if (string.IsNullOrWhiteSpace(data))
			{
				return;
			}

			int pos1 = data.IndexOf("Durchwahl");
			int pos2 = data.IndexOf("und");
			if (pos1 == -1 || pos2 == -1 || pos2 < pos1)
			{
				return;
			}
			string extension = data.Substring(pos1 + 9, pos2 - pos1 - 9).Trim();

			pos1 = data.IndexOf("Version");
			if (pos1 == -1)
			{
				return;
			}
			string version = data.Substring(pos1 + 7, data.Length - pos1 - 7).Trim();

			WriteMessage($"outgoing extension={extension} / version={version}");

		}

		private void ParseSocketSendRecv(string data, Directions dir)
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				return;
			}

			data = data.Trim();
			if (data[0]!='(')
			{
				return;
			}

			List<string> list = new List<string>();
			string item = "";
			bool quote = false;
			for (int i=0; i<data.Length; i++)
			{
				char chr = data[i];
				switch(chr)
				{
					case ' ':
						if (!quote)
						{
							if (!string.IsNullOrEmpty(item))
							{
								list.Add(item);
								item = "";
							}
						}
						else
						{
							item += chr;
						}
						break;
					case '\'':
						if (!quote)
						{
							item += "'";
							quote = true;
						}
						else
						{
							if (!string.IsNullOrEmpty(item))
							{
								list.Add(item);
								item = "";
							}
							quote = false;
						}
						break;
					default:
						item += chr;
						break;
				}
			}

			if (!string.IsNullOrEmpty(item))
			{
				list.Add(item);
			}

			byte[] buffer = new byte[256];
			int cnt = 0;
			for (int i = 1; i < list.Count; i++)
			{
				if (list[i]=="-->")
				{
					break;
				}
				if (list[i][0]=='\'')
				{
					for (int c=1; c<list[i].Length; c++)
					{
						buffer[cnt++] = (byte)list[i][c];
					}
					continue;
				}

				try
				{
					buffer[cnt++] = Convert.ToByte(list[i], 16);
				}
				catch(Exception ex)
				{
					buffer[cnt++] = 0;
				}
			}

			if (cnt<2)
			{
				return;
			}

			if (_connectionType==ConnectionType.Unknown)
			{
				if (buffer[0]==0x0D)
				{
					_connectionType = ConnectionType.Ascii;
					WriteMessage($"ASCII");
				}
				else
				{
					_connectionType = ConnectionType.Itelex;
					WriteMessage($"i-Telex");
				}
			}

			if (_connectionType==ConnectionType.Ascii)
			{
				string ascii = Encoding.UTF8.GetString(buffer, 0, cnt);
				ascii = ConvertAscii(ascii);
				WriteOutput(ascii);
				_needNewline = true;
				return;
			}

			ItelexPacket packet = new ItelexPacket(buffer);

			//int cmd = buffer[0];
			//int len = buffer[1];

			switch(packet.CommandType)
			{
				case ItelexCommands.Heartbeart:
					break;
				case ItelexCommands.BaudotData:  // i-telex data
					if (packet.Len==0)
					{
						return;
					}
					string ascii = CodeConversion.BaudotStringToAscii(packet.Data, ref _shiftState, CodeStandards.Ita2);
					ascii = ConvertAscii(ascii);
					WriteOutput(ascii);
					_needNewline = true;
					break;
				case ItelexCommands.End:  // verbindung beenden
					WriteMessage($"{dir} disconnect ({DateTime.Now})");
					break;
				case ItelexCommands.ProtocolVersion:
					if (dir == Directions.Recv)
					{
						if (packet.Data.Length > 1)
						{
							// get version string
							string versionStr = Encoding.ASCII.GetString(packet.Data, 1, packet.Data.Length - 1);
							versionStr = versionStr.TrimEnd('\x00'); // remove 00-byte suffix
							WriteMessage($"{dir} protocol vers {packet.Data[0]} '{versionStr}'");
						}
					}
					break;
				case ItelexCommands.DirectDial:
					if (dir == Directions.Recv)
					{
						_extension = packet.Data[0];
						WriteMessage($"{dir} direct dial {_extension}");
					}
					break;
				case ItelexCommands.Ack:
					break;
				default:
					Debug.WriteLine($"{dir} {packet.CommandType}");
					break;
			}
		}

		private string ConvertAscii(string ascii)
		{
			string conv = "";
			for (int i=0; i< ascii.Length; i++)
			{
				char chr = ascii[i];
				if (chr < 0x20 && chr != '\r' && chr != '\n')
				{
					conv += $"<{(int)chr:X02}>";
				}
				/*
				switch(chr)
				{
					case '\a':
						chr = '*';
						break;
					case '\u0005':
						chr = '#';
						break;
				}
				*/
				conv += chr;
			}
			return conv;
		}

		private void WriteMessage(string msg)
		{
			if (msg == "\r\n")
			{
				WriteOutput(msg);
			}
			else
			{
				if (_needNewline)
				{
					WriteOutput("\r\n");
				}
				WriteOutput($"- {msg}\r\n".ToUpper());
			}
			_needNewline = false;
		}

		private void WriteOutput(string str)
		{
			CommLog(str);

			if (OutputListView.Items.Count == 0)
			{
				Helper.ControlInvokeRequired(OutputListView, () =>
				{
					OutputListView.Items.Add(new ListViewItem(""));
				});
			}

			for (int i = 0; i < str.Length; i++)
			{
				char chr = str[i];
				switch (chr)
				{
					case '\r':
						_currentXpos = 0;
						break;
					case '\n':
						// newline
						Helper.ControlInvokeRequired(OutputListView, () =>
						{
							OutputListView.Items[OutputListView.Items.Count - 1] = new ListViewItem(_currentLine);
							OutputListView.Items.Add(new ListViewItem(""));
						});

						if (_currentXpos > 0)
						{
							_currentLine = new string(' ', _currentXpos);
						}
						else
						{
							_currentLine = "";
						}
						break;
					default:
						if (_currentXpos < _currentLine.Length)
						{
							char[] ch = _currentLine.ToCharArray();
							ch[_currentXpos] = chr;
							_currentLine = new string(ch);
						}
						else
						{
							_currentLine += chr;
						}
						_currentXpos++;
						break;
				}
			}

			Helper.ControlInvokeRequired(OutputListView, () =>
				{
					OutputListView.Items[OutputListView.Items.Count - 1] = new ListViewItem(_currentLine);
					OutputListView.EnsureVisible(OutputListView.Items.Count - 1);
				});
		}

		private void CommLog(string text)
		{
			lock (_commLogLock)
			{
				try
				{
					string fullName = Path.Combine(Constants.LOG_PATH, Constants.COMM_LOG);
					File.AppendAllText(fullName, text);
				}
				catch (Exception)
				{
				}
			}
		}

		private void DebugLog(string text)
		{
			lock (_commLogLock)
			{
				try
				{
					string fullName = Path.Combine(Constants.LOG_PATH, Constants.DEBUG_LOG);
					File.AppendAllText(fullName, text);
				}
				catch (Exception)
				{
				}
			}
		}

		private void MainForm_Click(object sender, EventArgs e)
		{
		}

		private void ShowLogCb_Click(object sender, EventArgs e)
		{
			if (_logView == null)
			{
				_logView = new LogView();
				_logView.Show();
				_logView.Closed += LogView_Closed;
			}
			else
			{
				_logView.Close();
				_logView = null;
			}
		}

		private void LogView_Closed()
		{
			_logView = null;
		}
	}
}
