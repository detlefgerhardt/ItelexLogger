using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItelexLogger
{
	class ComPortManager
	{
		private SerialPort _serialPort = null;


		public delegate void DataReceivedEventHandler(string data);
		public event DataReceivedEventHandler DataReceived;

		public bool IsConnected => _serialPort != null;

		public List<ComPortSelectionItem> GetPorts()
		{
			string[] ports = SerialPort.GetPortNames();
			List<ComPortSelectionItem> list = new List<ComPortSelectionItem>();
			for (int i = 0; i < ports.Length; i++)
			{
				int nr;
				if (int.TryParse(ports[i].Substring(3), out nr))
				{
					list.Add(new ComPortSelectionItem(nr, ports[i]));
				}
			}

			return list.OrderBy(o => o.ComNumber).ToList();

			// this more sophisticated COM port detection needs a higher DOTNET version
#if false
			List<ComPortSelectionItem> list = new List<ComPortSelectionItem>();
			//ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_SerialPort");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid = \"{4d36e978-e325-11ce-bfc1-08002be10318}\"");
			foreach (ManagementObject queryObj in searcher.Get())
			{
				/*
				foreach (var property in queryObj.Properties)
				{
					foreach (QualifierData q in property.Qualifiers)
					{
							Debug.WriteLine(
								queryObj.GetPropertyQualifierValue(
								property.Name, q.Name));
							Debug.WriteLine(property.Value);
						}

					}
					Debug.WriteLine("");
				}
				*/

				string devId = (string)queryObj["DeviceID"];
				string name = (string)queryObj["Name"];
				int p1 = name.LastIndexOf("(");
				if (p1 == -1)
					continue;
				int p2 = name.LastIndexOf(")");
				if (p2 == -1 || p1 >= p2)
					continue;

				string comPort = name.Substring(p1 + 1, p2 - p1 - 1);
				if (comPort.Length <= 3 || comPort.Substring(0, 3).ToLower() != "com")
					continue;

				int comNumber = Convert.ToInt32(comPort.Substring(3));

				string desc = (string)queryObj["Description"];
				if (desc.IndexOf("Kommunikationsanschluss") != -1 || desc.IndexOf("Serial Port") != -1)
					desc = "";

				list.Add(new ComPortSelectionItem()
				{
					ComNumber = comNumber,
					ComPort = comPort,
					Description = $"{comPort}: {desc}"
				});
			}

			list = list.OrderBy(o => o.ComNumber).ToList();
			return list;

			/*
			Debug.WriteLine(queryObj["Caption"]);
			Debug.WriteLine(queryObj["Description"]);
			Debug.WriteLine(queryObj["DeviceID"]);
			Debug.WriteLine(queryObj["Name"]);
			Debug.WriteLine(queryObj["PNPDeviceID"]);
			*/


			// geht nicht: access denied
			/*
			ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSSerial_PortName");
			foreach (ManagementObject queryObj2 in searcher2.Get())
			{
				Debug.WriteLine(queryObj2["PortName"]);
				Debug.WriteLine(queryObj2["InstanceName"]);
			}
			*/
#endif

		}

		public bool OpenComPort(string comPortName)
		{
			SerialPortFixer.Execute(comPortName);
			_serialPort = new SerialPort(comPortName);
			_serialPort.BaudRate = 9600;
			_serialPort.DataBits = 8;
			_serialPort.Parity = Parity.None;
			_serialPort.StopBits = StopBits.One;
			_serialPort.Handshake = Handshake.None;
			_serialPort.DataReceived += SerialPort_DataReceived;
			//_serialPort.RtsEnable = false;
			//_serialPort.DtrEnable = true;
			//serialPort.Encoding = Encoding.ASCII;

			// Set the read/write timeouts
			_serialPort.ReadTimeout = 500;
			_serialPort.WriteTimeout = 500;

			_serialPort.Open();
			return true;
		}

		public void CloseComPort()
		{
			if (_serialPort!=null)
			{
				_serialPort.Close();
				_serialPort = null;
			}
		}

		private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			SerialPort serialPort = (SerialPort)sender;
			string data = serialPort.ReadExisting();
			DataReceived?.Invoke(data);
		}
	}
}