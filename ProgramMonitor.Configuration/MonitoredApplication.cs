namespace ProgramMonitor.Configuration
{
	public class MonitoredApplication: Application
	{
		public int RemainingSecondsToday { get; set; }
		public string User { get; set; }

		public MonitoredApplication()
		{
		}

		public MonitoredApplication(string displayName, string installedLocation, string excutable) : 
			base(displayName, installedLocation, excutable)
		{
		}

		public MonitoredApplication(string displayName, string installedLocation, string excutable, string installedForUser) : 
			base(displayName, installedLocation, excutable, installedForUser)
		{
		}
	}
}
