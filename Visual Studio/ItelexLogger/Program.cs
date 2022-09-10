using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ItelexLogger
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			//Logging.Instance.Init(Constants.LOG_PATH, Constants.DEBUG_LOG);
			//Logging.Instance.Info($"---------- {} {} ----------");

			int? port = null;
			if (args.Length > 0)
			{
				port = ParsePortNumber(args[0]);
				if (port == null)
				{
					Console.WriteLine("parameter error: ItelexLogger -cn (n = port number)");
					return;
				}
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm(port));
		}

		private static int? ParsePortNumber(string portStr)
		{
			if (portStr.Length < 2 || portStr.Substring(0, 2).ToLower() != "-c")
			{
				return null;
			}

			portStr = portStr.Substring(2);

			if (int.TryParse(portStr, out int port))
			{
				return port;
			}
			else
			{
				return null;
			}
		}
	}
}
