using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace ProgramMonitor.Remoting
{
	[Serializable]
	public class RemotingClient: MarshalByRefObject
	{
		private IpcChannel mChannel;
		private IProgramMonitorRemoting mRemoteObject;

		public WarningDelegate Warning;
		public TerminatedDelegate Terminated;

		public void Initialize()
		{
			if (this.mChannel == null)
			{
				BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
				serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

				BinaryClientFormatterSinkProvider clientProv = new BinaryClientFormatterSinkProvider();

				IDictionary props = new Hashtable();
				props["portName"] = "ProgramMonitor.RemotingClient_" + Process.GetCurrentProcess().Id;
				props["authorizedGroup"] = "Everyone";

				this.mChannel = new IpcChannel(props, clientProv, serverProv);

				ChannelServices.RegisterChannel(this.mChannel, true);
			}
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		public void GetRemoteObject()
		{
			this.mRemoteObject =
				(IProgramMonitorRemoting)Activator.GetObject(typeof(IProgramMonitorRemoting),
				"ipc://ProgramMonitor.RemotingServer/ProgramMonitorRemoting");

			this.mRemoteObject.Warning += OnWarning;
			this.mRemoteObject.Terminated += OnTerminated;
		}

		public void UnsubscribeEvents()
		{
			if (this.mRemoteObject != null)
			{
				this.mRemoteObject.Warning -= OnWarning;
				this.mRemoteObject.Terminated -= OnTerminated;
			}
		}

		public IProgramMonitorRemoting RemoteObject
		{
			get { return this.mRemoteObject; }
		}

		public void OnTerminated(TerminatedEventArgs args)
		{
			if (this.Terminated != null)
				this.Terminated(args);
		}

		public void OnWarning()
		{
			if (this.Warning != null)
				this.Warning();
		}
	}
}
