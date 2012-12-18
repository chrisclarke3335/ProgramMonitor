using System;

namespace ProgramMonitor.Remoting
{
	public interface IProgramMonitor
	{
		TimeSpan TimeRemaining { get; }
		void AddTime(int sessionId, int minutes);
		void RemoveTime(int sessionId, int minutes);
		bool ValidatePassword(string password);
	}
}
