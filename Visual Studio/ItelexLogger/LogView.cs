using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ItelexLogger
{
	public partial class LogView : Form
	{
		public delegate void ClosedEventHandler();
		public event ClosedEventHandler Closed;

		public LogView()
		{
			InitializeComponent();

			LogListView.View = View.Details;
			LogListView.HideSelection = true;
			LogListView.FullRowSelect = true;
			LogListView.Sorting = SortOrder.None;
			LogListView.Columns[0].Width = LogListView.Width - 4;
			LogListView.HeaderStyle = ColumnHeaderStyle.None;

			LogListView.Items.Clear();
			foreach(string line in LogLister.Instance.LogList)
			{
				LogListView.Items.Add(new ListViewItem(line));
			}
		}

		public void Log(string line)
		{
			Helper.ControlInvokeRequired(LogListView, () =>
			{
				LogListView.Items.Add(new ListViewItem(line));
				LogListView.EnsureVisible(LogListView.Items.Count - 1);
				LogListView.Refresh();
			});
		}

		private void LogView_FormClosed(object sender, FormClosedEventArgs e)
		{
			Closed?.Invoke();
		}

		private void CloseBtn_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
