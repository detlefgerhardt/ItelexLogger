namespace ItelexLogger
{
	partial class LogView
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogView));
			this.LogListView = new System.Windows.Forms.ListView();
			this.Line = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.CloseBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// LogListView
			// 
			this.LogListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LogListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Line});
			this.LogListView.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LogListView.Location = new System.Drawing.Point(12, 12);
			this.LogListView.Name = "LogListView";
			this.LogListView.Size = new System.Drawing.Size(560, 369);
			this.LogListView.TabIndex = 1;
			this.LogListView.UseCompatibleStateImageBehavior = false;
			// 
			// Line
			// 
			this.Line.Width = 560;
			// 
			// CloseBtn
			// 
			this.CloseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseBtn.Location = new System.Drawing.Point(497, 387);
			this.CloseBtn.Name = "CloseBtn";
			this.CloseBtn.Size = new System.Drawing.Size(75, 23);
			this.CloseBtn.TabIndex = 2;
			this.CloseBtn.Text = "Close";
			this.CloseBtn.UseVisualStyleBackColor = true;
			this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
			// 
			// LogView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 422);
			this.Controls.Add(this.CloseBtn);
			this.Controls.Add(this.LogListView);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "LogView";
			this.Text = "i-TelexLogger Log";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LogView_FormClosed);
			this.Load += new System.EventHandler(this.LogView_Load);
			this.ResizeEnd += new System.EventHandler(this.LogView_ResizeEnd);
			this.LocationChanged += new System.EventHandler(this.LogView_LocationChanged);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView LogListView;
		private System.Windows.Forms.ColumnHeader Line;
		private System.Windows.Forms.Button CloseBtn;
	}
}