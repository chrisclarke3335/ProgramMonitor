using System.Windows.Forms;
using ProgramMonitor.Configuration;

namespace ProgramMonitor.UI
{
	public partial class ApplicationMonitorConfiguration : UserControl
	{
		private int mTabIndex;
		private ApplicationTabPage mDefaultTab;

		public ApplicationMonitorConfiguration()
		{
			InitializeComponent();

			// add Default tab
			this.mDefaultTab = new MonitoredApplicationTabPage(this.mTabIndex++);
			this.tcApplicationConfiguration.Controls.Add(this.mDefaultTab);

			// add one for each defined user
			string[] userAccounts = ConfigurationHelper.Instance.GetUserAccounts(false);
			foreach (string user in userAccounts)
			{
				AddUserTab(user);
			}			
		}

		private void AddUserTab(string name)
		{
			ApplicationTabPage tab = new MonitoredApplicationTabPage(name, this.mTabIndex++);
			this.tcApplicationConfiguration.Controls.Add(tab);
		}

		private void OnSelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (this.tcApplicationConfiguration.SelectedTab != null)
			{
				((MonitoredApplicationTabPage)this.tcApplicationConfiguration.SelectedTab).Reload();
			}
		}
	}
}
