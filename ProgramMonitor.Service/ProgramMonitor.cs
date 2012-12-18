using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ProgramMonitor.Remoting;

namespace ProgramMonitor.Service
{
	class ProgramMonitor: IProgramMonitor
	{
		#region Fields
		private DateTime mLastCheckTime;
		private List<String> mProcessesToWatch;
		private Hashtable mWatchedProcessesRunning;
		private int mQuotaSecondsUsedToday;
		private int mQuota = -1;
		private System.Configuration.Configuration mConfiguration;
		private RemotingServer mRemotingServer;
		private bool m30SecondWarningShown;
		#endregion

		#region Constructor
		public ProgramMonitor()
		{
			File.Delete(this.LogFileName);
			this.mLastCheckTime = DateTime.Now;
			this.mProcessesToWatch = new List<string>();
			this.mWatchedProcessesRunning = new Hashtable();
			this.mRemotingServer = new RemotingServer();
			this.mRemotingServer.Initialize(this);

			this.GetConfiguration();
		}
		#endregion

		#region Public methods
		public void ProcessTimerTick()
		{
			try
			{
				// see if we rolled over to a new day.  reset quota if so
				if (this.mLastCheckTime.Day != DateTime.Now.Day)
				{
					this.m30SecondWarningShown = false;
					this.UpdateTimeUsed(0);
					Log(string.Format("New day has started.  Resetting quota"));
				}

				this.mLastCheckTime = DateTime.Now;

				TimeSpan remaining;
				string timeRemaining = RemotingHelper.GetTimeDescription(this.TimeRemaining);

				// reset the entries in the watched list
				Hashtable copy = new Hashtable(this.mWatchedProcessesRunning.Count);
				foreach (string processName in this.mWatchedProcessesRunning.Keys)
					copy[processName] = false;

				this.mWatchedProcessesRunning = copy;

				// see if any of the processes in the watch list are running
				string processesKilled = "";
				List<String> processKillList = new List<string>();
				Process[] runningProcesses = Process.GetProcesses();
				foreach (Process process in runningProcesses)
				{
					string formattedName = FormatProcessName(process);

					// strip off the path
					string processName = Path.GetFileNameWithoutExtension(process.ProcessName);

					//Log(string.Format("DEBUG: Checking process {0} for inclusion in list", processName));

					bool isWatchedProcess = false;
					foreach (string p in this.mProcessesToWatch)
					{
						//Log(string.Format("DEBUG: Comparing {0} to {1}", p, processName));
						if (p.Equals(processName, StringComparison.InvariantCultureIgnoreCase))
						{
							isWatchedProcess = true;
							break;
						}
					}

					if (isWatchedProcess)
					{
						//Log(string.Format("DEBUG: Process {0} is under watch", processName));

						// a watched process is running
						if (this.mQuotaSecondsUsedToday >= this.mQuota)
						{
							// over quota.  kill it
							Log(string.Format("Quota has expired for the day.  Killing process {0}", processName));
							processesKilled += "\n  - " + process.MainWindowTitle;
							processKillList.Add(process.MainWindowTitle);
							process.Kill();

							this.mWatchedProcessesRunning.Remove(formattedName);
						}
						else
						{
							// under quota.  add to watch list
							if (this.mWatchedProcessesRunning.ContainsKey(formattedName))
								this.mWatchedProcessesRunning[formattedName] = true;
							else
							{
								Log(string.Format("A process subject to quota was started: {0}.  Quota remaining: {1}", processName,
												  timeRemaining));
								this.mWatchedProcessesRunning[formattedName] = true;
							}
						}
					}
				}

				if (processKillList.Count > 0)
				{
					try
					{
						this.mRemotingServer.NotifyTerminated(processKillList.ToArray());
					}
					catch (Exception ex)
					{
						Log("Exception while sending out terminated notice: " + ex.Message);
						Log(ex.StackTrace);
					}
				}

				if (this.mWatchedProcessesRunning.Keys.Count > 0 && TimeRemaining.TotalSeconds <= 30 && this.m30SecondWarningShown == false)
				{
					// if the monitor has just been started with < 30 seconds remaining then:
					//   if the time remaining is < 10 seconds, then don't both with the warning, the 'killed' balloon will suffice
					//   if the time remaining is 10-25 seconds, then show the exact amount of time left
					//   if the time remaining is 25-30 seconds, round up to 30, since there will probably be some error
					//     in the timer

					int warningTime = 30;
					if (TimeRemaining.TotalSeconds > 10)
					{
						if (TimeRemaining.TotalSeconds < 25)
							warningTime = (int)TimeRemaining.TotalSeconds;

						try
						{
							this.mRemotingServer.NotifyWarning();
						}
						catch (Exception ex)
						{
							Log("Exception while sending out 30 second warning: " + ex.Message);
							Log(ex.StackTrace);
						}
					}

					this.m30SecondWarningShown = true;
				}

				// show countdown to 0 as we get within 10 seconds
				if (TimeRemaining.TotalSeconds > 0 && TimeRemaining.TotalSeconds <= 10)
				{
					if (this.mWatchedProcessesRunning.Count > 0)
						Log(string.Format("Quota for the day expires in {0}.  {1} process(es) subject to quota are still running",
										  timeRemaining, this.mWatchedProcessesRunning.Count));
				}

				// see if any watched processes are no longer running
				copy = new Hashtable(this.mWatchedProcessesRunning.Count);
				foreach (string processName in this.mWatchedProcessesRunning.Keys)
				{
					if ((bool)this.mWatchedProcessesRunning[processName] == false)
					{
						string[] parts = processName.Split(new [] { ':' });
						Log(string.Format("A process subject to quota was stopped by the user: {0}.  Quota remaining: {1}",
										  parts[0], timeRemaining));
					}
					else
						copy[processName] = true;
				}
				this.mWatchedProcessesRunning = copy;

				// update total of quota used today
				if (this.mWatchedProcessesRunning.Count > 0 && this.mQuotaSecondsUsedToday < this.mQuota)
				{
					this.IncrementTimeUsed();
				}
			}
			catch (Exception ex)
			{
				Log("Exception: " + ex.Message);
				Log(ex.StackTrace);
			}
		}

		public void AddTime(int sessionId, int minutes)
		{
			TimeSpan prevTimeRemaining = this.TimeRemaining;
			int newTimeUsed = Math.Max(this.mQuotaSecondsUsedToday - minutes * 60, 0);
			this.mQuotaSecondsUsedToday = newTimeUsed;

			Log(string.Format("Session {0} increased time remaining from {1} to {2}",
				sessionId, RemotingHelper.GetTimeDescription(prevTimeRemaining),
				RemotingHelper.GetTimeDescription(this.TimeRemaining)));
		}

		public void RemoveTime(int sessionId, int minutes)
		{
			TimeSpan prevTimeRemaining = this.TimeRemaining;
			int newTimeUsed = Math.Min(this.mQuotaSecondsUsedToday + minutes * 60, this.mQuota);
			this.mQuotaSecondsUsedToday = newTimeUsed;

			Log(string.Format("Session {0} decreased time remaining from {1} to {2}",
				sessionId, RemotingHelper.GetTimeDescription(prevTimeRemaining),
				RemotingHelper.GetTimeDescription(this.TimeRemaining)));
		}

		public bool ValidatePassword(string password)
		{
			bool result = false;
			if (this.mConfiguration.AppSettings.Settings["password"] != null)
				result = this.mConfiguration.AppSettings.Settings["password"].Value == password;

			return result;
		}

		#endregion

		#region Private methods
		private string LogFileName
		{
			get
			{
				return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "log.txt");
			}
		}
		private void Log(string message)
		{
			Console.WriteLine(message);

			StreamWriter writer = new StreamWriter(this.LogFileName, true);
			writer.WriteLine(string.Format("[{0}] {1}", DateTime.Now, message));
			writer.Dispose();
		}

		private void GetConfiguration()
		{
			this.mConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			// read the quota
			if (this.mConfiguration.AppSettings.Settings["quotaInMinutes"] != null)
				this.mQuota = int.Parse(this.mConfiguration.AppSettings.Settings["quotaInMinutes"].Value) * 60;
			else
				this.mQuota = 30 * 60;		// default to 30 minutes

			// read the amount of time used and start with that value, if the last date on which it was
			// written is the same as today.  If it was written on a different day, then start with 0 used
			int cachedTimeUsed = 0;
			if (this.mConfiguration.AppSettings.Settings["cachedTimeUsed"] != null)
				cachedTimeUsed = int.Parse(this.mConfiguration.AppSettings.Settings["cachedTimeUsed"].Value);

			DateTime dateCached = DateTime.Now;
			if (this.mConfiguration.AppSettings.Settings["cachedTimeUsedDateWritten"] != null)
				dateCached = DateTime.Parse(this.mConfiguration.AppSettings.Settings["cachedTimeUsedDateWritten"].Value);

			DateTime now = DateTime.Now;

			if (dateCached.Date.Equals(now.Date) == false)
				cachedTimeUsed = 0;

			this.mQuotaSecondsUsedToday = cachedTimeUsed;

			Log(string.Format("Quota is {0} minute(s)", this.mQuota / 60));
			Log(string.Format("Time remaining on quota: {0}", RemotingHelper.GetTimeDescription(TimeRemaining)));

			// read in list of processes to monitor
			string list = null;
			if (this.mConfiguration.AppSettings.Settings["processes"] != null)
				list = this.mConfiguration.AppSettings.Settings["processes"].Value;

			if (String.IsNullOrEmpty(list))
			{
				mProcessesToWatch.Clear();
				return;
			}

			string[] processes = list.Split(new [] { ',' });
			foreach (string processName in processes)
			{
				// strip off the leading path
				string strippedProcessName = Path.GetFileNameWithoutExtension(processName);

				if (this.mProcessesToWatch.Contains(strippedProcessName) == false)
				{
					Log(string.Format("Adding process {0} to watch list", strippedProcessName));
					this.mProcessesToWatch.Add(strippedProcessName.ToLower());
				}
			}
		}

		private static string FormatProcessName(Process process)
		{
			return string.Format("{0}:{1}", Path.GetFileNameWithoutExtension(process.ProcessName).ToLower(), process.Id);
		}

		public TimeSpan TimeRemaining
		{
			get { return new TimeSpan(0, 0, this.mQuota - this.mQuotaSecondsUsedToday); }
		}

		private void IncrementTimeUsed()
		{
			UpdateTimeUsed(this.mQuotaSecondsUsedToday + 1);
		}

		private void UpdateTimeUsed(int value)
		{
			this.mQuotaSecondsUsedToday = value;

			if (this.mConfiguration.AppSettings.Settings["cachedTimeUsed"] == null)
				this.mConfiguration.AppSettings.Settings.Add("cachedTimeUsed", this.mQuotaSecondsUsedToday.ToString());
			else
				this.mConfiguration.AppSettings.Settings["cachedTimeUsed"].Value = this.mQuotaSecondsUsedToday.ToString();

			DateTime now = DateTime.Now;
			if (this.mConfiguration.AppSettings.Settings["cachedTimeUsedDateWritten"] == null)
				this.mConfiguration.AppSettings.Settings.Add("cachedTimeUsedDateWritten", now.ToString());
			else
				this.mConfiguration.AppSettings.Settings["cachedTimeUsedDateWritten"].Value = now.ToString();

			this.mConfiguration.Save();
		}
		#endregion
	}
}
