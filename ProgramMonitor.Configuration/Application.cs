namespace ProgramMonitor.Configuration
{
	public class Application
	{
		public string DisplayName { get; set; }
		public string InstalledLocation { get; set; }
		public string Executable { get; set; }
		public bool AllUsers { get; set; }
		public string InstalledForUser { get; set; }
		public int TotalAllowedMinutes { get; set; }
		public bool AllowedIsDefaultValue { get; set; }

		public Application()
		{
		}

		/// <summary>
		/// This constructor represents an application installed for all users
		/// </summary>
		/// <param name="displayName">The display name of the application</param>
		/// <param name="installedLocation">The location where the application is installed</param>
		/// <param name="excutable">The executable that starts the application</param>
		public Application(string displayName, string installedLocation, string excutable)
		{
			this.DisplayName = displayName;
			this.InstalledLocation = installedLocation;
			this.Executable = excutable;
			this.AllUsers = true;
			this.InstalledForUser = null;
		}

		/// <summary>
		/// This constructor represents an application installed for a specific user
		/// </summary>
		/// <param name="displayName">The display name of the application</param>
		/// <param name="installedLocation">The location where the application is installed</param>
		/// <param name="excutable">The executable that starts the application</param>
		/// <param name="installedForUser">The user for whom the application is installed</param>
		public Application(string displayName, string installedLocation, string excutable, string installedForUser) :
			this(displayName, installedLocation, excutable)
		{
			this.AllUsers = false;
			this.InstalledForUser = installedForUser;
		}
	}
}
