using System;
using System.Windows.Forms;
using ProgramMonitor.Configuration;
using ProgramMonitor.Engine;
using ProgramMonitor.UI;
using Application=ProgramMonitor.Configuration.Application;

namespace Test1
{
	public partial class Form1 : Form
	{
		private Engine mEngine;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.mEngine = new Engine();
			this.mEngine.MessageLogged += OnMessageLogged;
			this.mEngine.Start();

			this.label1.Text = "Users monitored: " + string.Join(", ", this.mEngine.UsersMonitored);

			ApplicationMonitorConfiguration config = new ApplicationMonitorConfiguration();

			this.tabPage1.Controls.Add(config);
			config.Dock = DockStyle.Fill;

			Application[] installedApps = ConfigurationHelper.Instance.GetInstalledApplications(this.mEngine.UsersMonitored);
			foreach (Application app in installedApps)
			{
				ListViewItem lvi = new ListViewItem(new string[] { 
					app.DisplayName, 
					app.InstalledLocation, 
					app.Executable, 
					app.AllUsers ? "(All Users)" : app.InstalledForUser });
				this.listView1.Items.Add(lvi);
			}
		}

		void OnMessageLogged(string message)
		{
			this.BeginInvoke(new MethodInvoker(delegate
           	{
           		this.textBox1.AppendText(string.Format("[{0}]: {1}\r\n", DateTime.Now, message));
           	}));
		}
	}
}
