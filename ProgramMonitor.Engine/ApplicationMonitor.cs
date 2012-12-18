using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProgramMonitor.Configuration;

namespace ProgramMonitor.Engine
{
	/// <summary>
	/// A monitor for a user's 
	/// </summary>
	public class ApplicationMonitor: MonitoredApplication
	{
		public ApplicationMonitor(MonitoredApplication monitoredApplication)
		{
			this.DisplayName = monitoredApplication.DisplayName;
			this.InstalledLocation = monitoredApplication.InstalledLocation;
			this.Executable = monitoredApplication.Executable;
			this.AllUsers = monitoredApplication.AllUsers;
			this.InstalledForUser = monitoredApplication.InstalledForUser;
			this.TotalAllowedMinutes = monitoredApplication.TotalAllowedMinutes;
			this.AllowedIsDefaultValue = monitoredApplication.AllowedIsDefaultValue;
			this.RemainingSecondsToday = monitoredApplication.RemainingSecondsToday;
			this.User = monitoredApplication.User;

			LastTick = DateTime.MinValue;
			this.mWatchedProcesses = new Dictionary<int, Process>(10);
		}

		/// <summary>
		/// The UserMonitor that owns this monitor.  A UserMonitor watches all monitored applications for
		/// a particular user
		/// </summary>
		private UserMonitor mOwner;

		/// <summary>
		/// The last time the UserMonitor updated this monitor
		/// </summary>
		private DateTime LastTick { get; set; }

		/// <summary>
		/// The processes associated with this application
		/// </summary>
		private Dictionary<int, Process> mWatchedProcesses;

		public void Initialize(UserMonitor owner)
		{
			this.mOwner = owner;
		}

		/// <summary>
		/// Add a process to the list of processes being watched for this application
		/// </summary>
		/// <param name="process">The Process object to add to the watch list</param>
		public void AddProcess(Process process)
		{
			lock(this.mWatchedProcesses)
			{
				if (this.mWatchedProcesses.ContainsKey(process.Id) == false)
				{
					Logger.Log(string.Format("ApplicationMonitor ({0}): Adding process {1} ({2}) to watch list.  Remaining time for the day is {3}",
						this.mOwner.UserName, process.Id, process.ProcessName, new TimeSpan(0, 0, 0, this.RemainingSecondsToday).ToString()));
					this.mWatchedProcesses.Add(process.Id, process);
					process.EnableRaisingEvents = true;
					process.Exited += OnProcessExited;

					if (this.mWatchedProcesses.Count == 1)
						LastTick = DateTime.Now;
				}
			}
		}

		/// <summary>
		/// Called by the UserMonitor when a new cycle starts.  Cycles occur about every 5 seconds
		/// </summary>
		public void Tick()
		{
			if (LastTick != DateTime.MinValue)
			{
				// if it's now a different day of the week, then we rolled over midnight.  Reset the remaining time to the total
				// allowed time
				if (DateTime.Now.DayOfWeek != this.LastTick.DayOfWeek)
				{
					Logger.Log(string.Format("ApplicationMonitor ({0}): Crossed over to a new day.  Resetting remaining time to {1} minutes",
						this.mOwner.UserName, this.TotalAllowedMinutes));
					this.RemainingSecondsToday = (int)(new TimeSpan(0, 0, this.TotalAllowedMinutes, 0).TotalSeconds);
				}

				lock(this.mWatchedProcesses)
				{
					if (this.mWatchedProcesses.Count > 0)
					{
						// if there are any processes running for this application, then figure out how long it's been 
						// since the last tick update and subtract that from the total remaining
						TimeSpan diff = DateTime.Now - LastTick;
						this.RemainingSecondsToday -= (int)diff.TotalSeconds;

						if (this.RemainingSecondsToday <= 0)
							this.RemainingSecondsToday = 0;

						// persist with the new time remaining
						ConfigurationHelper.Instance.UpdateRemainingTime(this.mOwner.UserName, this.Executable, this.RemainingSecondsToday);

						// if there is no time left, then kill all processes connected to this application
						if (this.RemainingSecondsToday == 0)
						{
							foreach (Process process in this.mWatchedProcesses.Values)
							{
								Logger.Log(string.Format("ApplicationMonitor ({0}): No time remaining today for application {1}.  Killing process {2}",
									this.mOwner.UserName, this.Executable, process.Id));

								try
								{
									// unsubscribe from Exited, so we don't try to process the exit event
									process.Exited -= OnProcessExited;
									process.Kill();
								}
								catch (InvalidOperationException)
								{
									Logger.Log(string.Format("ApplicationMonitor ({0}): InvalidOperationException while killing {1} ({2}). For iexplore version 8, this is probably due to child exit after parent termination",
										this.mOwner.UserName, process.Id, process.ProcessName));
								}
								catch (Exception ex)
								{
									Logger.LogException(ex);
								}
							}

							this.mWatchedProcesses.Clear();
						}
					}
				}
			}

			LastTick = DateTime.Now;
		}

		/// <summary>
		/// Callback for process exiting.  Processes in the list are removed when they exit
		/// </summary>
		/// <param name="sender">The associated process</param>
		/// <param name="e">The event information</param>
		void OnProcessExited(object sender, System.EventArgs e)
		{
			if (sender is Process)
			{
				// take it out of the list
				Process process = (sender as Process);
				Logger.Log(string.Format("ApplicationMonitor ({0}): Watched process {1} ({2}) has exited.  Time remaining: {3}", 
					this.mOwner.UserName, process.Id, process.ProcessName, new TimeSpan(0, 0, 0, this.RemainingSecondsToday)));
				
				lock(this.mWatchedProcesses)
				{
					this.mWatchedProcesses.Remove(process.Id);
				}
			}
		}
	}
}
