using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ProgramMonitor.Configuration;

namespace ProgramMonitor.Engine
{
	public class UserMonitor
	{
		private Thread mMonitorThread;
		private string mUserName;
		private Dictionary<string, ApplicationMonitor> mMonitorList;
		private ManualResetEvent mStopMonitoring;

		public UserMonitor(string userName)
		{
			this.mUserName = userName;

			this.mMonitorList = new Dictionary<string, ApplicationMonitor>(10);
			this.mStopMonitoring = new ManualResetEvent(false);
			this.mMonitorThread = new Thread(new ThreadStart(MonitorThreadProc));
			this.mMonitorThread.IsBackground = true;
			this.mMonitorThread.Start();
		}

		public void Stop()
		{
			Logger.Log(string.Format("UserMonitor ({0}): Requesting monitor thread stop", this.mUserName));
			this.mStopMonitoring.Set();
		}

		public string UserName
		{
			get { return this.mUserName; }
		}

		private void MonitorThreadProc()
		{
			Logger.Log(string.Format("UserMonitor ({0}): Thread starting for user {1}", this.mUserName, this.mUserName));
			bool keepMonitoring = true;

			while(keepMonitoring)
			{
				// read my configuration
				MonitoredApplication[] list = ConfigurationHelper.Instance.GetMonitoredApplicationsForUser(this.mUserName);

				// add applications we're newly monitoring
				foreach (MonitoredApplication newApp in list)
				{
					if (this.mMonitorList.ContainsKey(newApp.Executable) == false)
					{
						Logger.Log(string.Format("UserMonitor ({0}): Creating new application monitor for application {1}", this.mUserName, newApp.Executable));
						ApplicationMonitor monitor = new ApplicationMonitor(newApp);
						monitor.Initialize(this);
						this.mMonitorList.Add(monitor.Executable.ToLower(), monitor);
					}
				}

				// remove applications we're no longer monitoring
				foreach(ApplicationMonitor app in this.mMonitorList.Values)
				{
					bool found = false;
					foreach(ApplicationMonitor newApp in list)
					{
						if (newApp.Equals(app))
						{
							found = true;
							break;
						}
					}

					if (found == false)
					{
						Logger.Log(string.Format("UserMonitor ({0}): Removing application monitor for application {1}", this.mUserName, app.Executable));
						this.mMonitorList.Remove(app.Executable);
					}
				}

				try
				{
					if (this.mMonitorList.Count > 0)
					{
						// lists are up to date.  now get list of processes that belong to us
						Process[] myRunningProcesses = ProcessHelper.GetProcessesForUser(this.mUserName);
						foreach (Process p in myRunningProcesses)
						{
							if (this.mMonitorList.ContainsKey(p.ProcessName.ToLower()))
								this.mMonitorList[p.ProcessName.ToLower()].AddProcess(p);
						}
					}
				}
				catch (Exception ex)
				{
					Logger.LogException(ex);
				}

				// notify each application monitor to update itself
				foreach(ApplicationMonitor applicationMonitor in this.mMonitorList.Values)
				{
					applicationMonitor.Tick();
				}

				if (this.mStopMonitoring.WaitOne(5000))
				{
					Logger.Log(string.Format("UserMonitor ({0}): Stopping monitor thread", this.mUserName));
					keepMonitoring = false;
				}
			}
		}
	}
}
