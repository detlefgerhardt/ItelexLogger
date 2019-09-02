using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItelexLogger
{
	class LogLister
	{
		public List<string> LogList;

		/// <summary>
		/// singleton pattern
		/// </summary>
		private static LogLister _instance;

		public static LogLister Instance => _instance ?? (_instance = new LogLister());

		private LogLister()
		{
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
	}
}
