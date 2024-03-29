﻿using System;
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
	public partial class LogViewForm : Form
	{
		private const string TAG = nameof(LogViewForm);

		public delegate void ClosedEventHandler();
		public event ClosedEventHandler Closed;

		private Point _position;
		private Point? _tempPosition;

		private LogLister _logLister;
		private Logging _logging;

		private string _title;

		public LogViewForm(Point position, string title, LogLister logLister, Logging logging)
		{
			_tempPosition = position;
			_title = title;
			_logLister = logLister;
			_logging = logging;

			InitializeComponent();

			this.Text = $"LogView {title}";

			LogListView.View = View.Details;
			LogListView.HideSelection = true;
			LogListView.FullRowSelect = true;
			LogListView.Sorting = SortOrder.None;
			LogListView.Columns[0].Width = LogListView.Width - 4;
			LogListView.HeaderStyle = ColumnHeaderStyle.None;

			//LogLister.Instance.Add("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");

			LogListView.Items.Clear();
			foreach (string line in _logLister.LogList)
			{
				LogListView.Items.Add(new ListViewItem(line));
			}
		}

		public void Log(string line)
		{
			try
			{
				Helper.ControlInvokeRequired(LogListView, () =>
				{
					LogListView.Items.Add(new ListViewItem(line));
					LogListView.EnsureVisible(LogListView.Items.Count - 1);
					LogListView.Refresh();
				});
			}
			catch(Exception ex)
			{
				_logging.Error(TAG, nameof(Log), line, ex);
			}
		}

		private void LogView_Load(object sender, EventArgs e)
		{
			return;

			if (_tempPosition != null)
			{
				SetPosition(_tempPosition.Value.X, _tempPosition.Value.Y);
				_tempPosition = null;
			}
			else
			{
				SetPosition(_position.X, _position.Y);
			}
			SetLogViewWidth();
		}

		private void LogView_FormClosed(object sender, FormClosedEventArgs e)
		{
			Closed?.Invoke();
		}

		private void CloseBtn_Click(object sender, EventArgs e)
		{
			Close();
		}

		public void SetPosition(int x, int y)
		{
			_position = new Point(x, y);
			SetBounds(x, y, Bounds.Width, Bounds.Height);
		}

		public void ChangePosition(int x, int y)
		{
			if (Bounds.X != -32000)
			{
				int dx = x - _position.X;
				int dy = y - _position.Y;
				SetPosition(Bounds.X + dx, Bounds.Y + dy);
			}
		}

		private void LogView_LocationChanged(object sender, EventArgs e)
		{
			_position = new Point(Bounds.X, Bounds.Y);
		}

		private void LogView_ResizeEnd(object sender, EventArgs e)
		{
			SetLogViewWidth();
		}

		private void SetLogViewWidth()
		{
			LogListView.Columns[0].Width = LogListView.ClientSize.Width;
		}
	}
}
