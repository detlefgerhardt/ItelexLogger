using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItelexLogger
{
	public class LogLister
	{
		public List<string> LogList;

		private int _index { get; set; }

		private Logging _logging;

		private object _commLogLock = new object();
		private object _debugLogLock = new object();

		public LogLister(int index, Logging logging)
		{
			_index = index;
			_logging = logging;
			LogList = new List<string>();
		}

		public void Add(string line)
		{
			LogList.Add(line);
		}

		public void Clear()
		{
			LogList.Clear();
		}

		public void CommLog(string text)
		{
			lock (_commLogLock)
			{
				try
				{
					string fullName = Path.Combine(Helper.GetLogPath(), $"{Constants.COMM_LOG}{_index}.log");
					File.AppendAllText(fullName, text);
				}
				catch (Exception)
				{
				}
			}
		}

		public void DebugLog1(string text, bool withTime = false)
		{
			return;

			if (text == null)
			{
				return;
			}

			text = text.Trim(new char[] { '\r', '\n' });

			if (withTime)
			{
				text = DateTime.Now.ToString("hh:mm:ss") + "   : " + text;
			}
			lock (_debugLogLock)
			{
				try
				{
					string fullName = Path.Combine(Helper.GetLogPath(), $"{Constants.DEBUG_LOG}{_index}.log");
					File.AppendAllText(fullName, $"{DateTime.Now:dd.MM.yyyy} {text}\r\n");
				}
				catch (Exception)
				{
				}
			}
		}
	}
}
