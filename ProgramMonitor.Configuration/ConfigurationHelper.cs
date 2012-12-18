using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Shell32;

namespace ProgramMonitor.Configuration
{
	public class ConfigurationHelper
	{
		#region Registry keys and values
		private const string INSTALLED_LOCATION_VALUE = "InstalledLocation";
		private const string DISPLAY_NAME_VALUE = "DisplayName";
		private const string INSTALLED_FOR_USER_VALUE = "InstalledForUser";
		private const string TIME_REMAINING_VALUE = "RemainingSecondsToday";
		private const string TIME_ALLOWED_VALUE = "DailyMinutesAllowed";
		private const string DONT_MONITOR_VALUE = "DontMonitor";
		private const string DEFAULT_ROOT = @"Monitored Applications\Default";
		private const string USER_ROOT = @"Monitored Applications\{0}";
		#endregion

		#region Fields
		private RegistryKey mProgramMonitorRoot;
		private Shell32.Shell mShell;
		private string[] mUserAccounts;

		private static ConfigurationHelper sInstance;
		#endregion

		#region Singleton accessor
		private ConfigurationHelper()
		{
			this.mProgramMonitorRoot = Registry.LocalMachine.CreateSubKey(@"software\Program Monitor");
			this.mShell = new Shell();
		}

		public static ConfigurationHelper Instance
		{
			get
			{
				if (sInstance == null)
				{
					sInstance = new ConfigurationHelper();
				}
				return sInstance;
			}
		}
		#endregion

		#region User accounts
		public string[] GetUserAccounts(bool forceReload)
		{
			if (this.mUserAccounts == null || forceReload)
			{
				this.mUserAccounts = ProcessHelper.GetUserAccounts();
			}
			return this.mUserAccounts;
		}
		#endregion

		#region Monitored applications - stored in Registry
		/*
			There are 2 levels of configuration stored in the registry:  Default and user.  The default level contains
			definitions for applications to monitor that can apply to all users, if a user doesn't override the default
			settings.
			A default application to monitor entry contains only the total allowed time per day.
			A user application to monitor entry contains the time remaining for that user, or the DontMonitor flag, which
			indicates that the default shouldn't apply.  A user section can also contain an overriden total time
			allowed per day, if it's different from the default time allowed.
		*/

		#region Public
		/// <summary>
		/// Get the default allowed total time for any application.  This is just the number to use to start the
		/// configuration for any new application monitoring.  
		/// </summary>
		/// <returns></returns>
		public int GetDefaultDailyMinutesAllowed()
		{
			int result = 30;
			RegistryKey defaultValues = this.mProgramMonitorRoot.CreateSubKey(@"Default");

			string s = defaultValues.GetValue("DailyMinutesAllowed") as string;
			if (s != null)
				int.TryParse(s, out result);

			return result;
		}

		public bool DoesApplicationHaveDefaultMonitoring(string exeName)
		{
			RegistryKey defaultAppMonitor = this.mProgramMonitorRoot.CreateSubKey(DEFAULT_ROOT);
			return defaultAppMonitor.OpenSubKey(exeName) != null;
		}

		public void AddOrModifyDefaultMonitoredApplication(string exeName, int totalAllowedMinutes)
		{
			RegistryKey defaultAppMonitor = this.mProgramMonitorRoot.CreateSubKey(DEFAULT_ROOT);
			
			if (defaultAppMonitor != null)
			{
				RegistryKey app = defaultAppMonitor.CreateSubKey(exeName);
				this.UpdateTimeAllowed(app, totalAllowedMinutes);
			}
		}

		public void AddOrModifyUserMonitoredApplication(string userName, 
			Application application,
			int totalAllowedMinutes, 
			int remainingSecondsToday)
		{
			RegistryKey userAppMonitor = this.mProgramMonitorRoot.CreateSubKey(string.Format(USER_ROOT, userName));

			if (userAppMonitor != null)
			{
				RegistryKey app = userAppMonitor.CreateSubKey(application.Executable);
				
				app.SetValue(DISPLAY_NAME_VALUE, application.DisplayName);
				app.SetValue(INSTALLED_LOCATION_VALUE, application.InstalledLocation);

				if (application.AllUsers)
					app.SetValue(INSTALLED_FOR_USER_VALUE, "(All Users)");
				else
					app.SetValue(INSTALLED_FOR_USER_VALUE, application.InstalledForUser);

				this.UpdateTimeAllowed(app, totalAllowedMinutes);
				this.UpdateTimeRemaining(app, remainingSecondsToday);
			}
		}

		public void UpdateRemainingTime(string userName, string exeName, int remainingSecondsToday)
		{
			RegistryKey userAppMonitor = this.mProgramMonitorRoot.CreateSubKey(string.Format(USER_ROOT, userName));

			if (userAppMonitor != null)
			{
				RegistryKey app = userAppMonitor.CreateSubKey(exeName);
				this.UpdateTimeRemaining(app, remainingSecondsToday);
			}
		}

		/// <summary>
		/// If an app is set to be monitored in Default, and a particular user should *not* get this monitoring, then
		/// use this method to setup a DontMonitor entry for that app
		/// </summary>
		/// <param name="userName">The user</param>
		/// <param name="exeName">The exe name</param>
		/// <param name="value">True to turn DontMonitor on, false to turn it off</param>
		public void SetDontMonitorForUser(string userName, string exeName, bool value)
		{
			RegistryKey userAppMonitor = this.mProgramMonitorRoot.CreateSubKey(string.Format(USER_ROOT, userName));
			userAppMonitor.SetValue(DONT_MONITOR_VALUE, value ? 1 : 0);
		}

		public void RemoveUserMonitoredApplication(string userName, string exeName)
		{
			RegistryKey userAppMonitor = this.mProgramMonitorRoot.OpenSubKey(string.Format(USER_ROOT, userName));
			if (userAppMonitor != null)
			{
				userAppMonitor.DeleteSubKeyTree(exeName);
			}
		}

		public void RemoveDefaultMonitoredApplication(string exeName)
		{
			RegistryKey defaultAppMonitor = this.mProgramMonitorRoot.OpenSubKey(DEFAULT_ROOT);
			if (defaultAppMonitor != null)
			{
				defaultAppMonitor.DeleteSubKeyTree(exeName);
			}
		}

		public MonitoredApplication[] GetDefaultMonitoredApplications()
		{
			List<MonitoredApplication> result = new List<MonitoredApplication>(20);

			RegistryKey defaultAppMonitor = this.mProgramMonitorRoot.CreateSubKey(DEFAULT_ROOT);
			string[] defaultAppMonitors = defaultAppMonitor.GetSubKeyNames();

			foreach(string application in defaultAppMonitors)
			{
				int dailyMinutes = GetDefaultDailyMinutesAllowed();
				RegistryKey app = defaultAppMonitor.OpenSubKey(application);
				if (app != null)
				{
					bool defaultUsed;
					dailyMinutes = this.GetTimeAllowed(app, dailyMinutes, out defaultUsed);

					string displayName = this.GetDisplayName(app);
					string installedLocation = this.GetInstalledLocation(app);
					string installedForUser = this.GetInstalledForUser(app);

					MonitoredApplication appMonitor = new MonitoredApplication(application, displayName, installedLocation, installedForUser);
					appMonitor.TotalAllowedMinutes = dailyMinutes;
					appMonitor.AllowedIsDefaultValue = true;

					result.Add(appMonitor);
				}
			}

			return result.ToArray();
		}

		public MonitoredApplication[] GetMonitoredApplicationsForUser(string userName)
		{
			// first get all the default monitored applications, and seed the results with that list
			MonitoredApplication[] defaultAppMonitors = this.GetDefaultMonitoredApplications();
			List<MonitoredApplication> result = new List<MonitoredApplication>(defaultAppMonitors);

			// build list of all executables being monitored for this user
			List<string> monitoredAppsForUser = new List<string>(20);

			// create the user root or reopen it
			RegistryKey userRootKey = this.mProgramMonitorRoot.CreateSubKey(string.Format(USER_ROOT, userName));

			List<Application> toRemove = new List<Application>();
			foreach (MonitoredApplication defaultAppMonitor in result)
			{
				RegistryKey userAppMonitor = userRootKey.OpenSubKey(defaultAppMonitor.Executable);
				if (userAppMonitor != null)
				{
					// user has a specific override for this default.  see if it's to set DontMonitor
					if (this.GetIsDontMonitor(userAppMonitor))
						toRemove.Add(defaultAppMonitor);
					else
					{
						// there's an entry for this application under the user root, and it's not set to DontMonitor
						
						// get the allowed time, using the default time as the default value
						int dailyMinutes = defaultAppMonitor.TotalAllowedMinutes;
						bool defaultUsed;
						dailyMinutes = this.GetTimeAllowed(userAppMonitor, dailyMinutes, out defaultUsed);

						// get time remaining
						int timeRemaining = this.GetTimeRemaining(userAppMonitor, dailyMinutes*60);

						defaultAppMonitor.TotalAllowedMinutes = dailyMinutes;
						defaultAppMonitor.AllowedIsDefaultValue = defaultUsed;
						defaultAppMonitor.RemainingSecondsToday = timeRemaining;
						defaultAppMonitor.User = userName;

						monitoredAppsForUser.Add(defaultAppMonitor.Executable);
					}
				}
				else
				{
					monitoredAppsForUser.Add(defaultAppMonitor.Executable);
				}
			}

			foreach(string executable in userRootKey.GetSubKeyNames())
			{
				if (monitoredAppsForUser.Contains(executable) == false)
				{
					RegistryKey userAppKey = userRootKey.OpenSubKey(executable);
					if (this.GetIsDontMonitor(userAppKey) == false)
					{
						bool defaultUsed;

						// it's an entry for the user, and it's not a "don't monitor" entry.  Add it to the results
						MonitoredApplication userApp = new MonitoredApplication(
							executable,
							this.GetDisplayName(userAppKey),
							this.GetInstalledLocation(userAppKey),
							this.GetInstalledForUser(userAppKey));

						userApp.TotalAllowedMinutes = this.GetTimeAllowed(userAppKey, this.GetDefaultDailyMinutesAllowed(),
						                                                  out defaultUsed);
						userApp.RemainingSecondsToday = this.GetTimeRemaining(userAppKey, userApp.TotalAllowedMinutes * 60);
						userApp.AllowedIsDefaultValue = false;
						userApp.User = userName;

						result.Add(userApp);
					}
				}
			}
			
			// remove the ones that we marked as not applicable for this user
			foreach (MonitoredApplication applicationMonitor in toRemove)
				result.Remove(applicationMonitor);
			
			return result.ToArray();
		}
		#endregion

		#region Private
		private int GetTimeRemaining(RegistryKey key, int defaultValue)
		{
			int result = defaultValue;
			object val = key.GetValue(TIME_REMAINING_VALUE);
			if (val != null)
				result = (int) val;

			return result;
		}

		private int GetTimeAllowed(RegistryKey key, int defaultValue, out bool defaultUsed)
		{
			int result = defaultValue;
			defaultUsed = false;

			object val = key.GetValue(TIME_ALLOWED_VALUE);
			if (val != null)
				result = (int)val;
			else
				defaultUsed = true;
			
			return result;
		}

		private string GetDisplayName(RegistryKey key)
		{
			string result = "(none)";
			string s = key.GetValue(DISPLAY_NAME_VALUE) as string;
			if (string.IsNullOrEmpty(s) == false)
				result = s;

			return result;
		}

		private string GetInstalledLocation(RegistryKey key)
		{
			string result = "(unknown)";
			string s = key.GetValue(INSTALLED_LOCATION_VALUE) as string;
			if (string.IsNullOrEmpty(s) == false)
				result = s;

			return result;
		}

		private string GetInstalledForUser(RegistryKey key)
		{
			string result = "(unknown)";
			string s = key.GetValue(INSTALLED_FOR_USER_VALUE) as string;
			if (string.IsNullOrEmpty(s) == false)
				result = s;

			return result;
		}
		
		private bool GetIsDontMonitor(RegistryKey key)
		{
			object o = key.GetValue(DONT_MONITOR_VALUE);
			return (o != null && (int) o == 1);
		}

		private void UpdateTimeRemaining(RegistryKey key, int newValue)
		{
			key.SetValue(TIME_REMAINING_VALUE, newValue);
		}

		private void UpdateTimeAllowed(RegistryKey key, int newValue)
		{
			key.SetValue(TIME_ALLOWED_VALUE, newValue);
		}
		#endregion

		#endregion

		#region Installed application methods
		public Application[] GetInstalledApplications(string[] users)
		{
			List<Application> result = new List<Application>(50);
			
			// first add All Users installed programs
			string allUsersPath = Path.Combine(Environment.GetEnvironmentVariable("ALLUSERSPROFILE"), @"Microsoft\Windows\Start Menu\Programs");

			AddApplicationsFromShortcuts(allUsersPath, result);

			foreach (string installedProgram in Directory.GetDirectories(allUsersPath))
			{
				AddApplicationsFromShortcuts(installedProgram, result);
			}

			// now add for each user
			foreach(string user in users)
			{
				string userPath = string.Format(@"{0}\Users\{1}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs",
				                                Environment.GetEnvironmentVariable("SystemDrive"), user);
				if (Directory.Exists(userPath))
				{
					AddApplicationsFromShortcuts(userPath, user, result);

					foreach (string installedProgram in Directory.GetDirectories(userPath))
					{
						AddApplicationsFromShortcuts(installedProgram, user, result);
					}
				}
			}
			
			return result.ToArray();
		}

		private void AddApplicationsFromShortcuts(string folder, List<Application> list)
		{
			AddApplicationsFromShortcutsInternal(folder, null, true, list);
		}

		private void AddApplicationsFromShortcuts(string folder, string user, List<Application> list)
		{
			AddApplicationsFromShortcutsInternal(folder, user, false, list);
		}

		private void AddApplicationsFromShortcutsInternal(string folder, string user, bool allUsers, List<Application> list)
		{
			foreach (string shortcut in Directory.GetFiles(folder))
			{
				// iterating over the shortcuts to installed applications
				string fullShortcutPath = System.IO.Path.GetFullPath(shortcut);
				var dir = this.mShell.NameSpace(System.IO.Path.GetDirectoryName(fullShortcutPath));
				var itm = dir.Items().Item(System.IO.Path.GetFileName(fullShortcutPath));
				try
				{
					var lnk = (Shell32.ShellLinkObject) itm.GetLink;

					if (string.IsNullOrEmpty(lnk.Path) == false)
					{
						string extension = Path.GetExtension(Path.GetFileName(lnk.Path));
						if (string.Compare(extension, ".exe", StringComparison.InvariantCultureIgnoreCase) != 0 &&
							string.Compare(extension, ".bat", StringComparison.InvariantCultureIgnoreCase) != 0 &&
							string.Compare(extension, ".cmd", StringComparison.InvariantCultureIgnoreCase) != 0)
						{
							continue;
						}
		
						if (allUsers)
						{
							list.Add(new Application(
										Path.GetFileNameWithoutExtension(fullShortcutPath),
										Path.GetDirectoryName(lnk.Path),
										Path.GetFileNameWithoutExtension(lnk.Path)));
						}
						else
						{
							list.Add(new Application(
										Path.GetFileNameWithoutExtension(fullShortcutPath),
										Path.GetDirectoryName(lnk.Path),
										Path.GetFileNameWithoutExtension(lnk.Path),
										user));
						}
					}
				}
				catch (NotImplementedException)
				{
				}
			}
		}
		#endregion

		#region Not user
		private Application[] GetInstalledApplicationsRegistryVersion()
		{
			// the installed applications are here:
			// HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall
			// this contains the display name and maybe the install location.  To get the exe associated with the application,
			// cross reference with the subkeys under here:
			// HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths
			List<Application> result = new List<Application>(50);
			RegistryKey uninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
			if (uninstallKey != null)
			{
				RegistryKey appPathKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths");
				if (appPathKey != null)
				{
					// compile a list of installed applications and their install locations
					Dictionary<string, string> uninstallLocationToDisplayName = new Dictionary<string, string>(50);
					foreach(string installedApp in uninstallKey.GetSubKeyNames())
					{
						string displayName = uninstallKey.OpenSubKey(installedApp).GetValue("DisplayName", string.Empty).ToString();
						if (string.IsNullOrEmpty(displayName) == false)
						{
							string installLocation = uninstallKey.OpenSubKey(installedApp).GetValue("InstallLocation", string.Empty).ToString();
							if (string.IsNullOrEmpty(installLocation) == false)
							{
								if (uninstallLocationToDisplayName.ContainsKey(installLocation) == false)
									uninstallLocationToDisplayName.Add(installLocation, displayName);
							}
						}
					}

					// now iterate over the keys in App Paths, looking for matches on (app)->Path and installLocation
					foreach(string app in appPathKey.GetSubKeyNames())
					{
						string path = appPathKey.OpenSubKey(app).GetValue("Path", string.Empty).ToString();

						// the exe is the (default) value.  Retrive with value name == null
						string exe = appPathKey.OpenSubKey(app).GetValue(null, string.Empty).ToString();

						if (string.IsNullOrEmpty(path) == false && string.IsNullOrEmpty(exe) == false)
						{
							// see if the path matches an entry in the dictionary
							if (uninstallLocationToDisplayName.ContainsKey(path))
							{
								string fixedExe = Path.GetFileNameWithoutExtension(exe);
								result.Add(new Application(uninstallLocationToDisplayName[path], path, fixedExe));
							}
						}
					}
				}
				else
				{
					Logger.Log(@"GetInstalledApplications: Unable to open HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths");
				}
			}
			else
			{
				Logger.Log(@"GetInstalledApplications: Unable to open HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
			}
			return result.ToArray();
		}
		#endregion
	}
}
