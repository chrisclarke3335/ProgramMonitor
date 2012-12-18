using System.Collections.Generic;
using System.Windows.Forms;
using Application=ProgramMonitor.Configuration.Application;

namespace ProgramMonitor.UI
{
	class InstalledApplicationListView: ApplicationListView
	{
		public InstalledApplicationListView(string userName): base(userName)
		{
		}

		public InstalledApplicationListView()
		{
		}

		protected override void InitializeComponent()
		{
			base.InitializeComponent();

			// checkboxes
			this.CheckBoxes = true;
		}

		public void AddInstalledApplication(Application installedApp, bool isMonitored)
		{
			ListViewItem lvi = new ListViewItem(new[] { installedApp.DisplayName, installedApp.InstalledLocation, installedApp.Executable });
			lvi.Tag = installedApp;
			lvi.Checked = isMonitored;

			this.Items.Add(lvi);
		}

		public Application[] GetSelectedApplications()
		{
			List<Application> result = new List<Application>(); 
			foreach(ListViewItem lvi in this.Items)
				if (lvi.Checked)
					result.Add(lvi.Tag as Application);

			return result.ToArray();
		}
	}
}
