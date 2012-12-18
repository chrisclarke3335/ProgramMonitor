using System;

namespace ProgramMonitor.Remoting
{
	public interface IProgramMonitorRemoting
	{
		event WarningDelegate Warning;
		event TerminatedDelegate Terminated;

		TimeSpan TimeRemaining { get; }
		void AddTime(int sessionId, int minutes);
		void RemoveTime(int sessionId, int minutes);
		bool ValidatePassword(string password);
		bool IsRunning { get; }
	}

	public delegate void WarningDelegate();

	public delegate void TerminatedDelegate(TerminatedEventArgs args);

	[Serializable]
	public class TerminatedEventArgs
	{
		private string[] mPrograms;

		public TerminatedEventArgs(string[] programs)
		{
			this.mPrograms = programs;
		}

		public string[] Programs
		{
			get { return this.mPrograms; }
		}
	}
}
