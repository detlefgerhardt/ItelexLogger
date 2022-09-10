namespace ItelexLogger
{
	static class Constants
	{
		public const string PROGRAM_NAME = "ItelexLogger";
#if debug
		public const string LOG_PATH = @"c:\Itelex\ItelexLogger";
#else
		public const string LOG_PATH = "";
#endif
		public const string DEBUG_LOG = "itelexlogger_debug";
		public const string COMM_LOG = "itelexlogger_comm";
	}
}
