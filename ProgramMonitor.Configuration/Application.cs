using System;

namespace ProgramMonitor.Configuration
{
	/// <summary>
	/// An installed application
	/// </summary>
	public class Application
	{
		/// <summary>
		/// The friendly name.  Example: Internet Explorer
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// The directory where the application is installed
		/// </summary>
		public string InstalledLocation { get; set; }

		/// <summary>
		/// The executable that is associated with the application.  For Internet Explorer, iexplore.exe
		/// </summary>
		public string Executable { get; set; }

		/// <summary>
		/// Is the appliation installed for all users
		/// </summary>
		public bool AllUsers { get; set; }

		/// <summary>
		/// If the application is not installed for all users, which user is it installed for
		/// </summary>
		public string InstalledForUser { get; set; }

		/// <summary>
		/// The total number of minutes allowed per day for this application
		/// </summary>
		public int TotalAllowedMinutes { get; set; }

		/// <summary>
		/// Is the allowed minutes the default or is it overridden for this application
		/// </summary>
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


		public override bool Equals(object obj)
		{
			if (obj is Application)
			{
				return string.Compare((obj as Application).Executable, Executable, StringComparison.InvariantCultureIgnoreCase) == 0;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
