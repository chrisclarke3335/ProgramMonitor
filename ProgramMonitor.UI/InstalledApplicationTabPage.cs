using ProgramMonitor.Configuration;

namespace ProgramMonitor.UI
{
	class InstalledApplicationTabPage: ApplicationTabPage
	{
		private InstalledApplicationListView mListView;

		public InstalledApplicationTabPage(int tabIndex)
			: base(tabIndex)
		{
			this.mListView = new InstalledApplicationListView();
			this.Controls.Add(this.mListView);
		}

		public InstalledApplicationTabPage(string name, int tabIndex): base(name, tabIndex)
		{
			this.mListView = new InstalledApplicationListView(name);
			this.Controls.Add(this.mListView);
		}

		public void AddInstalledApplication(Application installedApp, bool isMonitored)
		{
			this.mListView.AddInstalledApplication(installedApp, isMonitored);
		}

		public Application[] GetSelectedApplications()
		{
			return this.mListView.GetSelectedApplications();
		}

		#region Overrides of ApplicationTabPage
		protected override string DefaultName
		{
			get { return "All Users"; }
		}
		#endregion
	}
}
