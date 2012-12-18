using System.Collections.Generic;
using System.Windows.Forms;
using ProgramMonitor.Configuration;
using Application=ProgramMonitor.Configuration.Application;

namespace ProgramMonitor.UI
{
	public partial class InstalledApplicationsForm : Form
	{
		private int mTabIndex = 0;
		private InstalledApplicationTabPage mAllUsersTab;
		private List<InstalledApplicationTabPage> mCreatedTabs;
		private string mRunningForUser;

		public InstalledApplicationsForm()
		{
			Setup(null);
		}

		public InstalledApplicationsForm(string user)
		{
			Setup(user);
		}

		/// <summary>
		/// Commit monitored application updates
		/// </summary>
		public void Commit()
		{
			int defaultDailyMinutesAllowed = ConfigurationHelper.Instance.GetDefaultDailyMinutesAllowed();

			foreach(InstalledApplicationTabPage tab in this.mCreatedTabs)
			{
				foreach(Application app in tab.GetSelectedApplications())
				{
					if (this.mRunningForUser == null)
					{
						ConfigurationHelper.Instance.AddOrModifyDefaultMonitoredApplication(app.Executable, 
							defaultDailyMinutesAllowed);
					}
					else
					{
						ConfigurationHelper.Instance.AddOrModifyUserMonitoredApplication(this.mRunningForUser, 
							app,
							defaultDailyMinutesAllowed, 
							defaultDailyMinutesAllowed * 60);
					}
				}
			}
		}

		private void Setup(string user)
		{
			this.mRunningForUser = user;
			this.mCreatedTabs = new List<InstalledApplicationTabPage>();

			InitializeComponent();

			// get the apps being monitored for the specified user (or for Default if user==null)
			Application[] monitoredApps;
			if (user == null)
			{
				monitoredApps = ConfigurationHelper.Instance.GetDefaultMonitoredApplications();
			}
			else
			{
				monitoredApps = ConfigurationHelper.Instance.GetMonitoredApplicationsForUser(user);
			}

			// build a dictionary for them, for faster searching later on
			Dictionary<string, List<Application>> monitoredAppDictionary = new Dictionary<string, List<Application>>();
			foreach(Application app in monitoredApps)
			{
				if (monitoredAppDictionary.ContainsKey(app.Executable) == false)
				{
					monitoredAppDictionary.Add(app.Executable, new List<Application>());
				}

				monitoredAppDictionary[app.Executable].Add(app);
			}

			// add Default tab
			this.mAllUsersTab = new InstalledApplicationTabPage(this.mTabIndex++);
			this.tcUsers.Controls.Add(this.mAllUsersTab);
			this.mCreatedTabs.Add(this.mAllUsersTab);

			// get installed apps and build UI
			Application[] installedApps =
				ConfigurationHelper.Instance.GetInstalledApplications(ConfigurationHelper.Instance.GetUserAccounts(false));

			foreach (Application app in installedApps)
			{
				if (app.AllUsers)
				{
					// add to All Users tab
					this.mAllUsersTab.AddInstalledApplication(app, monitoredAppDictionary.ContainsKey(app.Executable));
				}
				else
				{
					// installed app for a particular user.  add to that user's tab
					TabPage tab = this.tcUsers.TabPages[app.InstalledForUser];
					
					// no tab was found.  If we are adding for Default, then insert a tab for this user
					if (tab == null)
					{
						if (user == null)
						{
							// add tab for any user when user not specified
							tab = AddTab(app.InstalledForUser);
						}
						else
						{
							if (user == app.InstalledForUser)
							{
								// add tab for specified user when the first installed app is hit for that user
								tab = AddTab(app.InstalledForUser);
							}
						}
					}

					if (tab != null)
					{
						((InstalledApplicationTabPage)tab).AddInstalledApplication(app, monitoredAppDictionary.ContainsKey(app.Executable));
					}
				}
			}
		}

		private InstalledApplicationTabPage AddTab(string name)
		{
			InstalledApplicationTabPage tab = new InstalledApplicationTabPage(name, this.mTabIndex++);
			this.tcUsers.Controls.Add(tab);
			this.mCreatedTabs.Add(tab);

			return tab;
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}
	}
}
