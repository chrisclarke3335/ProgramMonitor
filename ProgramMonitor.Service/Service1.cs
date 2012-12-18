using System.ServiceProcess;
using ProgramMonitor.Engine;

namespace ProgramMonitor.Service
{
	public partial class Service1 : ServiceBase
	{
		//private Timer mTimer;
		//private ProgramMonitor mMonitor;
		private Engine.Engine mEngine;

		public Service1()
		{
			this.mEngine = new Engine.Engine();
			this.mEngine.Start();

			//mTimer = new Timer(OnTimer);
			//mMonitor = new ProgramMonitor();

			//mTimer.Change(1000, 1000);
			InitializeComponent();
		}

		//void OnTimer(object o)
		//{
		//    mMonitor.ProcessTimerTick();
		//}

		protected override void OnStart(string[] args)
		{
		}

		protected override void OnStop()
		{
		}
	}
}
