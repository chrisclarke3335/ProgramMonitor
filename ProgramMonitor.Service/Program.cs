using System;
using System.ServiceProcess;

namespace ProgramMonitor.Service
{
	static class Program
	{
		private static Service1 sConsoleServiceObject;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				sConsoleServiceObject = new Service1();
				Console.ReadLine();
			}
			else
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] 
				{ 
					new Service1() 
				};
				ServiceBase.Run(ServicesToRun);
			}
		}

	}
}
