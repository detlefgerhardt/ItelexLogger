namespace ItelexLogger
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.TabCtrl = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.LoggerCtrl1 = new ItelexLogger.LoggerControl();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.LoggerCtrl2 = new ItelexLogger.LoggerControl();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.LoggerCtrl3 = new ItelexLogger.LoggerControl();
			this.TabCtrl.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.SuspendLayout();
			// 
			// TabCtrl
			// 
			this.TabCtrl.Controls.Add(this.tabPage1);
			this.TabCtrl.Controls.Add(this.tabPage2);
			this.TabCtrl.Controls.Add(this.tabPage3);
			this.TabCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabCtrl.Location = new System.Drawing.Point(0, 0);
			this.TabCtrl.Name = "TabCtrl";
			this.TabCtrl.SelectedIndex = 0;
			this.TabCtrl.Size = new System.Drawing.Size(636, 424);
			this.TabCtrl.TabIndex = 5;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.LoggerCtrl1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(628, 398);
			this.tabPage1.TabIndex = 3;
			this.tabPage1.Text = "tabPage1";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// LoggerCtrl1
			// 
			this.LoggerCtrl1.Location = new System.Drawing.Point(3, 3);
			this.LoggerCtrl1.Name = "LoggerCtrl1";
			this.LoggerCtrl1.Size = new System.Drawing.Size(599, 375);
			this.LoggerCtrl1.TabIndex = 1;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.LoggerCtrl2);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(628, 398);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "tabPage2";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// LoggerCtrl2
			// 
			this.LoggerCtrl2.Location = new System.Drawing.Point(3, 3);
			this.LoggerCtrl2.Name = "LoggerCtrl2";
			this.LoggerCtrl2.Size = new System.Drawing.Size(599, 375);
			this.LoggerCtrl2.TabIndex = 0;
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.LoggerCtrl3);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage3.Size = new System.Drawing.Size(628, 398);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "tabPage3";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// LoggerCtrl3
			// 
			this.LoggerCtrl3.Location = new System.Drawing.Point(3, 3);
			this.LoggerCtrl3.Name = "LoggerCtrl3";
			this.LoggerCtrl3.Size = new System.Drawing.Size(599, 375);
			this.LoggerCtrl3.TabIndex = 1;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(636, 424);
			this.Controls.Add(this.TabCtrl);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.Text = "i-Telex Logger";
			this.TabCtrl.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.TabControl TabCtrl;
		private System.Windows.Forms.TabPage tabPage2;
		private LoggerControl LoggerCtrl2;
		private System.Windows.Forms.TabPage tabPage3;
		private LoggerControl LoggerCtrl3;
		private System.Windows.Forms.TabPage tabPage1;
		private LoggerControl LoggerCtrl1;
	}
}

