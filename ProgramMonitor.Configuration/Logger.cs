using System;
using System.IO;

namespace ProgramMonitor.Configuration
{
	public delegate void LogDelegate(string message);

	public static class Logger
	{
		private const string FILE_NAME = "ProgramMonitorLog.txt";
		private static object sSyncRoot = new object();
		private static bool sInitialized = false;

		public static LogDelegate MessageLogged;

		public static void Log(string message)
		{
			try
			{
				lock (sSyncRoot)
				{
					if (sInitialized == false)
					{
						File.Delete(FILE_NAME);
						sInitialized = true;
					}

					Console.WriteLine(message);

					if (MessageLogged != null)
						MessageLogged(message);

					using (StreamWriter writer = new StreamWriter(FILE_NAME, true))
					{
						writer.WriteLine(string.Format("[{0}] {1}", DateTime.Now, message));
					}
				}
			}
			catch
			{
				// leave logger errors alone
			}
		}

		public static void LogException(Exception ex)
		{
			Exception e = ex;
			int depth = 1;
			lock(sSyncRoot)
			{
				while (e != null)
				{
					Log("***********************************************************");
					Log(string.Format("Exception {0}: {1}", depth, e.Message));
					Log(ex.GetType().ToString());
					Log("");
					Log(e.StackTrace);
					Log("");

					e = e.InnerException;
					depth++;
				}
			}
		}
	}
}
