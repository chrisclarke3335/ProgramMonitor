namespace ProgramMonitor.Tray
{
	partial class Form1
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addTimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mnu1Minute = new System.Windows.Forms.ToolStripMenuItem();
			this.mnu5Minutes = new System.Windows.Forms.ToolStripMenuItem();
			this.mnu30Minutes = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuRemoveTime = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.tmrEstablishConnection = new System.Windows.Forms.Timer(this.components);
			this.tmrUpdateText = new System.Windows.Forms.Timer(this.components);
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = "Program Monitor";
			this.notifyIcon1.Visible = true;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTimeToolStripMenuItem,
            this.mnuRemoveTime,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(170, 82);
			// 
			// addTimeToolStripMenuItem
			// 
			this.addTimeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnu1Minute,
            this.mnu5Minutes,
            this.mnu30Minutes});
			this.addTimeToolStripMenuItem.Name = "addTimeToolStripMenuItem";
			this.addTimeToolStripMenuItem.Size = new System.Drawing.Size(169, 24);
			this.addTimeToolStripMenuItem.Text = "Add Time";
			// 
			// mnu1Minute
			// 
			this.mnu1Minute.Name = "mnu1Minute";
			this.mnu1Minute.Size = new System.Drawing.Size(150, 24);
			this.mnu1Minute.Tag = "1";
			this.mnu1Minute.Text = "1 minute";
			this.mnu1Minute.Click += new System.EventHandler(this.MinuteMenuItemClick);
			// 
			// mnu5Minutes
			// 
			this.mnu5Minutes.Name = "mnu5Minutes";
			this.mnu5Minutes.Size = new System.Drawing.Size(150, 24);
			this.mnu5Minutes.Tag = "5";
			this.mnu5Minutes.Text = "5 minutes";
			this.mnu5Minutes.Click += new System.EventHandler(this.MinuteMenuItemClick);
			// 
			// mnu30Minutes
			// 
			this.mnu30Minutes.Name = "mnu30Minutes";
			this.mnu30Minutes.Size = new System.Drawing.Size(150, 24);
			this.mnu30Minutes.Tag = "30";
			this.mnu30Minutes.Text = "30 minutes";
			this.mnu30Minutes.Click += new System.EventHandler(this.MinuteMenuItemClick);
			// 
			// mnuRemoveTime
			// 
			this.mnuRemoveTime.Name = "mnuRemoveTime";
			this.mnuRemoveTime.Size = new System.Drawing.Size(169, 24);
			this.mnuRemoveTime.Text = "Remove Time";
			this.mnuRemoveTime.Click += new System.EventHandler(this.RemoveTimeClick);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(166, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(169, 24);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitMenuItemClick);
			// 
			// tmrEstablishConnection
			// 
			this.tmrEstablishConnection.Interval = 1000;
			this.tmrEstablishConnection.Tick += new System.EventHandler(this.tmrEstablishConnection_Tick);
			// 
			// tmrUpdateText
			// 
			this.tmrUpdateText.Interval = 3000;
			this.tmrUpdateText.Tick += new System.EventHandler(this.tmrUpdateText_Tick);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(331, 122);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.Timer tmrEstablishConnection;
		private System.Windows.Forms.Timer tmrUpdateText;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem addTimeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnu1Minute;
		private System.Windows.Forms.ToolStripMenuItem mnu5Minutes;
		private System.Windows.Forms.ToolStripMenuItem mnu30Minutes;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mnuRemoveTime;
	}
}

