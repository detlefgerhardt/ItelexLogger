using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ItelexLogger
{
	public enum CodeStandards { Ita2, UsTTy }

	public enum ConnectionType { Unknown, Itelex, Ascii }

	public enum Directions { Recv, Send }

	public partial class MainForm : Form
	{
		public MainForm(int? port)
		{
			InitializeComponent();

			this.Text = Helper.GetVersion();
			InitTabs();

			if (port != null)
			{
				LoggerCtrl1.Connect(port.Value);
			}
		}

		public void InitTabs()
		{
			LoggerCtrl1.Init(1);
			LoggerCtrl1.Update += LoggerCtrl1_Update;
			SetTabText(1, null);

			LoggerCtrl2.Init(2);
			LoggerCtrl2.Update += LoggerCtrl2_Update;
			SetTabText(2, null);

			LoggerCtrl3.Init(3);
			LoggerCtrl3.Update += LoggerCtrl3_Update;
			SetTabText(3, null);
		}

		private void LoggerCtrl1_Update(string title)
		{
			SetTabText(1, title);
		}

		private void LoggerCtrl2_Update(string title)
		{
			SetTabText(2, title);
		}

		private void LoggerCtrl3_Update(string title)
		{
			SetTabText(3, title);
		}

		private void SetTabText(int index, string text)
		{
			if (index<1 || index>3)
			{
				return;
			}

			if (string.IsNullOrEmpty(text))
			{
				text = $"Log {index} (-)";
			}
			else
			{
				text = $"Log {index} ({text})";
			}
			TabCtrl.TabPages[index-1].Text = text;
			TabCtrl.Refresh();
		}

	}
}
