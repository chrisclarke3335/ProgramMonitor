namespace ProgramMonitor.Configuration
{
	/// <summary>
	/// An application that is monitored for a particular user when that user starts it.  The remaining
	/// time available for the day is tracked
	/// </summary>
	public class MonitoredApplication: Application
	{
		/// <summary>
		/// The amount of time available for this application today
		/// </summary>
		public int RemainingSecondsToday { get; set; }

		/// <summary>
		/// The user for which this application is monitored
		/// </summary>
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
