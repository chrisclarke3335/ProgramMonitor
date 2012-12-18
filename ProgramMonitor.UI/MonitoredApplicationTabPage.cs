namespace ProgramMonitor.UI
{
	class MonitoredApplicationTabPage: ApplicationTabPage
	{
		private MonitoredApplicationListView mListView;

		public MonitoredApplicationTabPage(int tabIndex):
			base(tabIndex)
		{
			this.mListView = new MonitoredApplicationListView();
			this.Controls.Add(this.mListView);
		}

		internal void Reload()
		{
			this.mListView.Reload();
		}

		public MonitoredApplicationTabPage(string name, int tabIndex)
			: base(name, tabIndex)
		{
			this.mListView = new MonitoredApplicationListView(name);
			this.Controls.Add(this.mListView);
		}

		#region Overrides of ApplicationTabPage
		protected override string DefaultName
		{
			get { return "(Default)"; }
		}
		#endregion
	}
}
