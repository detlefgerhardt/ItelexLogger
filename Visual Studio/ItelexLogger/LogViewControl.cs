using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace ItelexLogger
{
	public partial class LoggerControl : UserControl
	{
		private const string TAG = nameof(LoggerControl);

		private Logging _logging;

		//private int _comPort;

		private string _serialBuffer;

		private LogLister _logLister;

		private string _currentLine;
		private int _currentXpos;

		private int _extension;

		private ConnectionType _connectionType = ConnectionType.Unknown;

		private LogViewForm _logView;

		private ComPortManager _comPortManager;

		private CodeConversion.ShiftStates _shiftState;

		private bool _needNewline = false;

		private bool _initSend = false;

		private int _index;

		public delegate void UpdateEventHandler(string title);
		public event UpdateEventHandler Update;

		public LoggerControl()
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

			this.Text = Helper.GetVersion();

			ComPortsCb.DataSource = _comPortManager.GetPorts();
			ComPortsCb.DisplayMember = "ComPort";

			SetShowLogViewStatus();
		}

		public void Init(int index)
		{
			_index = index;
			_logging = new Logging(Constants.LOG_PATH, $"{Constants.DEBUG_LOG}{_index}.log");
			InitLog();
		}

		public bool Connect(int port)
		{
			List<ComPortSelectionItem> newComPortList = _comPortManager.GetPorts();

			ComPortSelectionItem comPort = (from c in newComPortList where c.ComNumber == port select c).FirstOrDefault();
			if (comPort==null)
			{
				return false;
			}

			bool result = ConnectToComPort(comPort);
			//_comPort = port;
			SetShowLogViewStatus();
			return result;
		}

		/*
		private void Test()
		{
			string fullName = Path.Combine(Constants.LOG_PATH, "test.log");
			string[] lines = File.ReadAllLines(fullName);

			Init();
			foreach(string line in lines)
			{
				DoLine(line);
			}
		}
		*/

		private void ComPortsCb_MouseClick(object sender, MouseEventArgs e)
		{
			// save selection
			ComPortSelectionItem saveItem = (ComPortSelectionItem)ComPortsCb.SelectedItem;

			// refresh com port list
			List<ComPortSelectionItem> newComPortList = _comPortManager.GetPorts();
			ComPortsCb.DataSource = newComPortList;

			// restore selection
			if (saveItem != null)
			{
				foreach (ComPortSelectionItem selItem in newComPortList)
				{
					if (selItem.ComNumber == saveItem.ComNumber)
					{
						ComPortsCb.SelectedItem = saveItem;
						ComPortsCb.Text = saveItem.ComPort;
						break;
					}
				}
			}
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
				//_logLister.DebugLog($"Disconnected from COM port", true);
				_logging.Debug(TAG, nameof(ConnectCb_Click), $"Disconnected from COM port {_comPortManager?.ComPortName}");
				Update?.Invoke(null);
				LogViewClose();
			}
			else
			{
				if (!_comPortManager.IsConnected)
				{
					ComPortSelectionItem item = (ComPortSelectionItem)ComPortsCb.SelectedItem;
					ConnectToComPort(item);
				}
			}
			SetShowLogViewStatus();
		}

		private bool ConnectToComPort(ComPortSelectionItem item)
		{
			//InitLog();

			if (item != null)
			{
				if (_comPortManager.OpenComPort(item.ComPort))
				{
					ConnectCb.Text = "Disconnect";
					ConnectCb.Checked = true;
					Init();
					//_logLister.DebugLog($"Connected to COM port {item.ComPort}", true);
					_logging.Debug(TAG, nameof(ConnectToComPort), $"Connected to COM port {item.ComPort}");
					Update?.Invoke(item.ComPort);
					return true;
				}
			}

			ConnectCb.Checked = false;
			return false;
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

				//_logLister.DebugLog(line);
				_logging.Debug(TAG, nameof(ComPortManager_DataReceived), line);
				_logLister.Add(line);
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
			_logging.Debug(TAG, nameof(DoLine), line);

			if (line.Length < 13) return; // error

			line = line.Trim(new char[] { '\r', '\n', ' ' });

			Debug.WriteLine(line);

			//string timestamp = line.Substring(0, 11);
			if (line.Length > 13) line = line.Substring(13);

			line = GetNextColumn(line, out string col1);
			if (line == null) return;

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
			if (CheckLeftString(line, "Socket wird aktiv geschlossen"))
			{
				WriteMessage("disconnect");
				_connectionType = ConnectionType.Unknown;
				return;
			}

			// wegen eines Fehlers bei der Log-Ausgabe bei ASCII-Empfang, kann es sein,
			// dass die Socket-Empfang-Meldung an einer vorhergehenden Zeile dranhängt.
			try
			{
				int idx = line.IndexOf("Socket Empfang:");
				if (idx != -1)
				{
					_logging.Debug(TAG, nameof(DoLine), $"socket empfang, idx={idx}");
					line = line.Substring(idx + 15);
					ParseSocketSendRecv(line, Directions.Recv);
					return;
				}

				idx = line.IndexOf("Socket Sendung:");
				if (idx != -1)
				{
					_logging.Debug(TAG, nameof(DoLine), $"socket sendung, idx={idx}");
					line = line.Substring(idx + 15);
					ParseSocketSendRecv(line, Directions.Send);
					return;
				}
			}
			catch(Exception ex)
			{
				_logging.Error(TAG, nameof(DoLine), $"error", ex);
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
			if (pos == -1)
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
			if (pos1 == -1 || pos2 == -1 || pos2 < pos1)
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
			if (string.IsNullOrWhiteSpace(data) || data.Length <= 18)
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
			_logging.Debug(TAG, nameof(ParseSocketSendRecv), $"data={data}, dir={dir}, connectionType={_connectionType}");

			if (string.IsNullOrWhiteSpace(data)) return;

			data = data.Trim();
			if (data[0] != '(') return;

			List<string> list = new List<string>();
			string item = "";
			bool quote = false;
			for (int i = 0; i < data.Length; i++)
			{
				char chr = data[i];
				switch (chr)
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

			if (!string.IsNullOrEmpty(item)) list.Add(item);

			byte[] buffer = new byte[512];
			int bufferCnt = 0;
			for (int i = 1; i < list.Count; i++)
			{
				if (list[i] == "-->") break;

				if (list[i][0] == '\'')
				{
					for (int c = 1; c < list[i].Length; c++)
					{
						buffer[bufferCnt++] = (byte)list[i][c];
					}
					continue;
				}

				try
				{
					buffer[bufferCnt++] = Convert.ToByte(list[i], 16);
				}
				catch (Exception ex)
				{
					buffer[bufferCnt++] = 0;
				}
			}

			if (bufferCnt < 2) return;

			//if (_connectionType == ConnectionType.Unknown || _connectionType == ConnectionType.Ascii)
			if (_connectionType == ConnectionType.Unknown)
			{
				if (buffer[0] == 0x0D)
				{
					_connectionType = ConnectionType.Ascii;
					WriteMessage($"ASCII");
				}
				else
				{
					_connectionType = ConnectionType.Itelex;
					WriteMessage($"i-Telex");
				}
				_logging.Debug(TAG, nameof(ParseSocketSendRecv), $"buffer[0]==0x0D, connectionType={_connectionType}");
			}

			if (_connectionType == ConnectionType.Ascii)
			{
				string ascii = Encoding.UTF8.GetString(buffer, 0, bufferCnt);
				ascii = ConvertAscii(ascii);
				WriteOutput(ascii);
				_needNewline = true;
				return;
			}

			int bufferPos = 0;
			while (bufferPos < bufferCnt)
			{
				ItelexPacket packet = new ItelexPacket(buffer, bufferPos);
				bufferPos += packet.Len + 2;

				_logging.Debug(TAG, nameof(ParseSocketSendRecv), $"packet={packet}");

				switch (packet.CommandType)
				{
					case ItelexCommands.Heartbeart:
						break;
					case ItelexCommands.BaudotData:  // i-telex data
						if (packet.Len == 0)
						{
							return;
						}
						string ascii = CodeConversion.BaudotStringToAscii(packet.Data, ref _shiftState, CodeStandards.Ita2);
						//_logLister.DebugLog($"{dir} \"{ConvertAsciiLog(ascii)}\"", true);
						_logging.Debug(TAG, nameof(ParseSocketSendRecv), $"{dir} \"{ConvertAsciiLog(ascii)}\"");
						WriteOutput(ConvertAscii(ascii));
						_needNewline = true;
						break;
					case ItelexCommands.End:  // verbindung beenden
						WriteMessage($"{dir} disconnect ({DateTime.Now})");
						_connectionType = ConnectionType.Unknown;
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
		}

		private string ConvertAscii(string ascii)
		{
			string conv = "";
			for (int i = 0; i < ascii.Length; i++)
			{
				char chr = ascii[i];
				if ((chr < 0x20 || chr > 126) && chr != '\r' && chr != '\n')
				{
					conv += $"<{(int)chr:X02}>";
					continue;
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

		private string ConvertAsciiLog(string ascii)
		{
			string conv = "";
			for (int i = 0; i < ascii.Length; i++)
			{
				char chr = ascii[i];
				if (chr < 0x20 || chr > 0x126)
				{
					conv += $"<{(int)chr:X02}>";
					continue;
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
			_logLister.CommLog(str);

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

		private void ShowLogCb_Click(object sender, EventArgs e)
		{
			if (_logView == null)
			{
				_logView = new LogViewForm(new Point(this.Bounds.X + this.Width, this.Bounds.Y), _comPortManager.ComPortName, _logLister, _logging);
				_logView.Show();
				_logView.Closed += LogView_Closed;
			}
			else
			{
				LogViewClose();
			}
		}

		private void LogView_Closed()
		{
			_logView = null;
		}

		private void LogViewClose()
		{
			if (_logView != null)
			{
				_logView.Closed -= LogView_Closed;
				_logView.Close();
				_logView = null;
				//_logLister.DebugLog($"Close com port", true);
				_logging.Debug(TAG, nameof(LogViewClose), $"Close com port {_comPortManager?.ComPortName}");
			}
		}

		private void SetShowLogViewStatus()
		{
			if (_comPortManager.IsConnected)
			{
				ShowLogCb.Enabled = true;
			}
			else
			{
				ShowLogCb.Checked = false;
				ShowLogCb.Enabled = false;
			}
		}

		private void MainForm_LocationChanged(object sender, EventArgs e)
		{
			_logView?.ChangePosition(Bounds.X + this.Width, Bounds.Y);
		}

		private void InitLog()
		{
			if (_initSend)
			{
				return;
			}

			_logLister = new LogLister(_index, _logging);
			//_logLister.DebugLog($"{Helper.GetVersion()}\r\n", true);
			_logging.Debug(TAG, nameof(LogViewClose), $"{Helper.GetVersion()}\r\n");
			_logLister.CommLog($"{Helper.GetVersion()}\r\n");
			_initSend = true;
		}
	}
}
