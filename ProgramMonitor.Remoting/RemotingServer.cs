using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace ProgramMonitor.Remoting
{
	public class RemotingServer
	{
		private IpcChannel mChannel;
		private RemotingObject mRemotingObject;

		public void Initialize(IProgramMonitor monitor)
		{
			BinaryServerFormatterSinkProvider serverProv = new BinaryServerFormatterSinkProvider();
			serverProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

			BinaryClientFormatterSinkProvider clientProv = new BinaryClientFormatterSinkProvider();

			IDictionary props = new Hashtable();
			props["portName"] = "ProgramMonitor.RemotingServer";
			props["authorizedGroup"] = "Everyone";

			this.mChannel = new IpcChannel(props, clientProv, serverProv);

			ChannelServices.RegisterChannel(this.mChannel, true);
			this.mRemotingObject = new RemotingObject(monitor);

			RemotingServices.Marshal(this.mRemotingObject, "ProgramMonitorRemoting");
		}

		public void NotifyWarning()
		{
			this.mRemotingObject.NotifyWarning();
		}

		public void NotifyTerminated(string[] programs)
		{
			this.mRemotingObject.NotifyTerminated(programs);
		}
	}
}
