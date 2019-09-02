using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItelexLogger
{
	class ComPortSelectionItem
	{
		public int ComNumber { get; set; }

		public string ComPort { get; set; }

		public string Description { get; set; }

		public ComPortSelectionItem(int comNumber, string comPort)
		{
			ComNumber = comNumber;
			ComPort = comPort;
			Description = comPort;
		}

		public override string ToString()
		{
			return $"{ComNumber} {ComPort}";
		}
	}
}
