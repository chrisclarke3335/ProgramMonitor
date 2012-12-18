using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ProgramMonitor.Configuration;
using Application=ProgramMonitor.Configuration.Application;

namespace ProgramMonitor.UI
{
	class MonitoredApplicationListView: ApplicationListView
	{
		private ContextMenu mContextMenu;

		private ColumnHeader mColAllowedMinutes;
		private ColumnHeader mColRemainingMinutes;
		private ColumnHeader mInstalledForUser;

		public MonitoredApplicationListView(string userName): base(userName)
		{
		}

		public MonitoredApplicationListView()
		{
		}

		internal void Reload()
		{
			this.SuspendLayout();
			this.Items.Clear();

			try
			{
				MonitoredApplication[] monitoredApps = ConfigurationHelper.Instance.GetMonitoredApplicationsForUser(this.mUserName);

				foreach (MonitoredApplication monitor in monitoredApps)
				{
					ListViewItem lvi = new ListViewItem(new []
					{
						monitor.DisplayName,
						monitor.InstalledLocation,
						monitor.Executable,
						new TimeSpan(0,0,monitor.TotalAllowedMinutes,0).ToString(),
						new TimeSpan(0,0,0,monitor.RemainingSecondsToday).ToString(),
						monitor.InstalledForUser
					});

					this.Items.Add(lvi);
				}
			}
			finally
			{
				this.ResumeLayout(true);
			}
		}

		protected override void AddColumns(List<ColumnHeader> columns)
		{
			this.mColAllowedMinutes = new ColumnHeader();
			this.mColAllowedMinutes.Text = "Allowed Time";
			this.mColAllowedMinutes.Width = 50;

			this.mColRemainingMinutes = new ColumnHeader();
			this.mColRemainingMinutes.Text = "Remaining Time";
			this.mColRemainingMinutes.Width = 70;

			this.mInstalledForUser = new ColumnHeader();
			this.mInstalledForUser.Text = "Installed For User";
			this.mInstalledForUser.Width = 100;

			columns.Add(this.mColAllowedMinutes);
			columns.Add(this.mColRemainingMinutes);
			columns.Add(this.mInstalledForUser);
		}
		protected override void InitializeComponent()
		{
			base.InitializeComponent();

			// context menu
			this.mContextMenu = new ContextMenu();
			this.ContextMenu = this.mContextMenu;

			MenuItem item = new MenuItem("&Add");
			item.Click += AddMonitor;
			this.mContextMenu.MenuItems.Add(item);

			item = new MenuItem("&Remove");
			item.Click += RemoveMonitor;
			this.mContextMenu.MenuItems.Add(item);
		}

		private void AddMonitor(object sender, System.EventArgs e)
		{
			DialogResult result;
			InstalledApplicationsForm installedApplicationsForm;

			if (this.mIsDefault)
			{
				installedApplicationsForm = new InstalledApplicationsForm(null);
				result = installedApplicationsForm.ShowDialog();
			}
			else
			{
				installedApplicationsForm = new InstalledApplicationsForm(this.mUserName);
				result = installedApplicationsForm.ShowDialog();
			}

			if (result == DialogResult.OK)
			{
				installedApplicationsForm.Commit();
				this.Reload();
			}

		}

		private void RemoveMonitor(object sender, System.EventArgs e)
		{

		}
	}
}
