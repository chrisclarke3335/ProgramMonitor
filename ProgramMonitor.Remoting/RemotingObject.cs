using System;

namespace ProgramMonitor.Remoting
{
	public class RemotingObject: MarshalByRefObject, IProgramMonitorRemoting
	{
		private IProgramMonitor mMonitor;

		public event WarningDelegate Warning;
		public event TerminatedDelegate Terminated;
		
		public TimeSpan TimeRemaining
		{
			get { return mMonitor.TimeRemaining; }
		}

		public void RemoveTime(int sessionId, int minutes)
		{
			mMonitor.RemoveTime(sessionId, minutes);
		}

		public bool ValidatePassword(string password)
		{
			return mMonitor.ValidatePassword(password);
		}

		public bool IsRunning
		{
			get { return true; }
		}

		public void AddTime(int sessionId, int minutes)
		{
			mMonitor.AddTime(sessionId, minutes);
		}

		public RemotingObject(IProgramMonitor monitor)
		{
			mMonitor = monitor;
		}

		public void NotifyWarning()
		{
			if (Warning != null)
				Warning();
		}

		public void NotifyTerminated(string[] programs)
		{
			if (Terminated != null)
				Terminated(new TerminatedEventArgs(programs));
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}
	}
}
