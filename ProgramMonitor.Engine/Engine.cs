using System.Collections.Generic;
using ProgramMonitor.Configuration;

namespace ProgramMonitor.Engine
{
	public class Engine
	{
		private List<UserMonitor> mUserMonitors;
		private static readonly object MessageLoggedEvent = new object();

		public Engine()
		{
			this.mUserMonitors = new List<UserMonitor>(10);
		}

		public void Start()
		{
			string[] userAccounts = ConfigurationHelper.Instance.GetUserAccounts(true);

			foreach (string user in userAccounts)
			{

if (user != "cclarke")
    continue;
				this.mUserMonitors.Add(new UserMonitor(user));
			}
		}

		public event LogDelegate MessageLogged
		{
			add
			{
				Logger.MessageLogged += value;
			}
			remove
			{
				Logger.MessageLogged -= value;
			}
		}

		public string[] UsersMonitored
		{
			get
			{
				List<string> result = new List<string>();
				foreach(UserMonitor monitor in this.mUserMonitors)
				{
					result.Add(monitor.UserName);
				}
				return result.ToArray();
			}
		}
	}
}
