namespace ItelexLogger
{
	partial class LoggerControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OutputListView = new System.Windows.Forms.ListView();
			this.Line = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ConnectCb = new System.Windows.Forms.CheckBox();
			this.ComPortsCb = new System.Windows.Forms.ComboBox();
			this.ShowLogCb = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// OutputListView
			// 
			this.OutputListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Line});
			this.OutputListView.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OutputListView.Location = new System.Drawing.Point(0, 30);
			this.OutputListView.Name = "OutputListView";
			this.OutputListView.Size = new System.Drawing.Size(558, 412);
			this.OutputListView.TabIndex = 1;
			this.OutputListView.UseCompatibleStateImageBehavior = false;
			// 
			// Line
			// 
			this.Line.Width = -1;
			// 
			// ConnectCb
			// 
			this.ConnectCb.Appearance = System.Windows.Forms.Appearance.Button;
			this.ConnectCb.AutoSize = true;
			this.ConnectCb.Location = new System.Drawing.Point(130, 1);
			this.ConnectCb.Name = "ConnectCb";
			this.ConnectCb.Size = new System.Drawing.Size(57, 23);
			this.ConnectCb.TabIndex = 5;
			this.ConnectCb.Text = "Connect";
			this.ConnectCb.UseVisualStyleBackColor = true;
			this.ConnectCb.Click += new System.EventHandler(this.ConnectCb_Click);
			// 
			// ComPortsCb
			// 
			this.ComPortsCb.FormattingEnabled = true;
			this.ComPortsCb.Location = new System.Drawing.Point(0, 3);
			this.ComPortsCb.Name = "ComPortsCb";
			this.ComPortsCb.Size = new System.Drawing.Size(121, 21);
			this.ComPortsCb.TabIndex = 4;
			this.ComPortsCb.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ComPortsCb_MouseClick);
			// 
			// ShowLogCb
			// 
			this.ShowLogCb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ShowLogCb.Appearance = System.Windows.Forms.Appearance.Button;
			this.ShowLogCb.AutoSize = true;
			this.ShowLogCb.Location = new System.Drawing.Point(193, 1);
			this.ShowLogCb.Name = "ShowLogCb";
			this.ShowLogCb.Size = new System.Drawing.Size(61, 23);
			this.ShowLogCb.TabIndex = 6;
			this.ShowLogCb.Text = "Show log";
			this.ShowLogCb.UseVisualStyleBackColor = true;
			this.ShowLogCb.Click += new System.EventHandler(this.ShowLogCb_Click);
			// 
			// LoggerControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ShowLogCb);
			this.Controls.Add(this.ConnectCb);
			this.Controls.Add(this.ComPortsCb);
			this.Controls.Add(this.OutputListView);
			this.Name = "LoggerControl";
			this.Size = new System.Drawing.Size(558, 442);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView OutputListView;
		private System.Windows.Forms.ColumnHeader Line;
		private System.Windows.Forms.CheckBox ConnectCb;
		private System.Windows.Forms.ComboBox ComPortsCb;
		private System.Windows.Forms.CheckBox ShowLogCb;
	}
}
