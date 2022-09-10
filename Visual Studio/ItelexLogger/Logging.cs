using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ItelexLogger
{
	public enum LogTypes { None = 0, Fatal = 1, Error = 2, Warn = 3, Info = 4, Debug = 5 };

	[Serializable]
	public class Logging
	{
		public delegate void RecvLogEvent(object sender, LogArgs e);
		public event RecvLogEvent RecvLog;

		public LogTypes LogLevel { get; set; }

		public string LogfileFullname
		{
			get
			{
				if (string.IsNullOrEmpty(_logPath) || string.IsNullOrEmpty(_logName)) return "";
				return Path.Combine(_logPath, _logName);
			}
		}

		private object _lock = new object();

		private string _logPath;

		private string _logName;

		public string LogfilePath
		{
			get { return _logPath; }
			set { _logPath = value; }
		}

		//private static Logging instance;
		//public static Logging Instance
		//{
		//	get
		//	{
		//		if (instance == null)
		//		{
		//			instance = new Logging();
		//		}
		//		return instance;
		//	}
		//}

		public Logging(string logPath, string logName)
		{
			LogLevel = LogTypes.Debug;

			if (string.IsNullOrEmpty(logPath)) logPath = Application.StartupPath;

			_logPath = logPath;
			_logName = logName;
		}

		public void Init(string logPath, string logName)
		{
			_logPath = logPath;
			_logName = logName;
		}

		public void Debug(string section, string method, string text)
		{
			Log(LogTypes.Debug, section, method, text);
		}

		public void Info(string section, string method, string text)
		{
			Log(LogTypes.Info, section, method, text);
		}

		public void Warn(string section, string method, string text)
		{
			Log(LogTypes.Warn, section, method, text);
		}

		public void Error(string section, string method, string text)
		{
			Log(LogTypes.Error, section, method, text);
		}

		public void Error(string section, string method, string text, Exception ex = null)
		{
			if (ex != null)
			{
				text = $"{text} result={ex.HResult} {ex.Message}";
			}
			Log(LogTypes.Error, section, method, text);
		}

		public void Fatal(string section, string method, string text)
		{
			Log(LogTypes.Fatal, section, method, text);
		}

		public void Log(LogTypes logType, string section, string method, string text, bool show = true)
		{
			if (IsActiveLevel(logType))
			{
				AppendLog(logType, section, method, text);
				OnLog(new LogArgs(logType, section, method, text));
			}
		}

		public void OnLog(LogArgs e)
		{
			RecvLog?.Invoke(this, e);
		}

		private void AppendLog(LogTypes logType, string section, string method, string text)
		{
			lock(_lock)
			{
				int? id = Task.CurrentId;
				if (!id.HasValue)
				{
					System.Diagnostics.Debug.Write("");
				}
				string prefix = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} {logType.ToString().PadRight(5)} [{Task.CurrentId}] [{section}]";
				string logStr = $"{prefix} [{method}] {text}\r\n";
				try
				{
					File.AppendAllText(LogfileFullname, logStr);
				}
				catch
				{
					// try to log in program directory
					string newName = Path.Combine(Application.StartupPath, _logName);
					File.AppendAllText(newName, $"{prefix} [AppendLog] Error writing logfile to '{LogfileFullname}'\r\n");
					File.AppendAllText(newName, logStr);
				}
			}
		}

		private bool IsActiveLevel(LogTypes current)
		{
			return (int)current <= (int)LogLevel;
		}
	}

	public class LogArgs : EventArgs
	{
		public LogTypes LogType { get; set; }

		public string Section { get; set; }

		public string Method { get; set; }

		public string Message { get; set; }

		public LogArgs(LogTypes logType, string section, string method, string msg)
		{
			LogType = logType;
			Section = section;
			Method = method;
			Message = msg;
		}
	}
}
