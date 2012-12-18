using System;

namespace ProgramMonitor.Remoting
{
	public class RemotingHelper
	{
		public static string GetTimeDescription(TimeSpan time)
		{
			string timeRemaining;
			if (time.Minutes == 0)
				timeRemaining = string.Format("{0} second(s)", time.Seconds);
			else
				timeRemaining = string.Format("{0} minute(s)", time.Minutes);

			return timeRemaining;
		}
	}
}
