using System;
using System.Diagnostics;
using System.Windows.Forms;
using ProgramMonitor.Remoting;

namespace ProgramMonitor.Tray
{
	public partial class Form1 : Form
	{
		private RemotingClient mRemotingClient;
		private bool mClosing;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.ShowInTaskbar = false;
			this.WindowState = FormWindowState.Minimized;
			this.Hide();

			this.tmrEstablishConnection.Enabled = true;
		}

		private void OnTerminated(TerminatedEventArgs args)
		{
			if (this.mClosing == false)
			{
				string message = "";
				foreach (string program in args.Programs)
				{
					message += "\n  - " + program;
				}

				this.BeginInvoke(new MethodInvoker(delegate()
				{
					this.notifyIcon1.ShowBalloonTip(5000, "Processes Killed",
						"The following processes were terminated because the daily allotment of time has expired: \n" +
						message, ToolTipIcon.Info);
				}));
			}
		}

		public void OnWarning()
		{
			if (this.mClosing == false)
			{
				this.BeginInvoke(new MethodInvoker(delegate()
				{
					this.notifyIcon1.ShowBalloonTip(
						5000,
						string.Format("{0} Second Warning", 30),
						string.Format("All monitored programs will be terminated in {0} seconds", 30),
						ToolTipIcon.Warning);
				}));
			}
		}

		private void tmrEstablishConnection_Tick(object sender, EventArgs e)
		{
			try
			{
				if (this.mClosing == false)
				{
					if (this.mRemotingClient == null)
					{
						this.mRemotingClient = new RemotingClient();
						this.mRemotingClient.Initialize();
					}

					this.mRemotingClient.GetRemoteObject();

					if (this.mRemotingClient.RemoteObject.IsRunning)
					{
						this.mRemotingClient.Warning += new WarningDelegate(OnWarning);
						this.mRemotingClient.Terminated += new TerminatedDelegate(OnTerminated);

						this.tmrEstablishConnection.Enabled = false;
						this.tmrUpdateText.Enabled = true;
					}
				}
			}
			catch (Exception ex)
			{
				this.notifyIcon1.Text = "Unable to connect to Program Monitor service";
			}
		}

		private void tmrUpdateText_Tick(object sender, EventArgs e)
		{
			try
			{
				if (this.mClosing == false && this.mRemotingClient != null)
				{
					this.notifyIcon1.Text = "Time remaining for day: " + 
						RemotingHelper.GetTimeDescription(this.mRemotingClient.RemoteObject.TimeRemaining);
				}
			}
			catch (Exception)
			{
				Reconnect();
			}
		}

		private void Reconnect()
		{
			this.notifyIcon1.Text = "Unable to connect to Program Monitor service";
			this.tmrUpdateText.Enabled = false;
			this.tmrEstablishConnection.Enabled = true;
		}

		private void SetTimeToAdd(int minutes)
		{
			if (this.mRemotingClient != null)
			{
				try
				{
					this.mRemotingClient.RemoteObject.AddTime(
						Process.GetCurrentProcess().SessionId, minutes);
				}
				catch (Exception)
				{
					Reconnect();
				}
			}
		}

		private void SetTimeToRemove(int minutes)
		{
			if (this.mRemotingClient != null)
			{
				try
				{
					this.mRemotingClient.RemoteObject.RemoveTime(
						Process.GetCurrentProcess().SessionId, minutes);
				}
				catch (Exception)
				{
					Reconnect();
				}
			}
		}

		private void MinuteMenuItemClick(object sender, EventArgs e)
		{
			if (sender is ToolStripMenuItem)
			{
				Password passwordForm = new Password();
				if (passwordForm.ShowDialog() == DialogResult.OK)
				{
					try
					{
						if (this.mRemotingClient.RemoteObject.ValidatePassword(passwordForm.PasswordText))
						{
							ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
							int timeToAdd = int.Parse(menuItem.Tag.ToString());
							SetTimeToAdd(timeToAdd);
						}
						else
						{
							MessageBox.Show("Incorrect password", "Bad Password", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
						}
					}
					catch (Exception)
					{
						MessageBox.Show("Lost connection to Program Monitor service", "Lost Connection", 
							MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
						Reconnect();
					}
				}
			}
		}

		private void ExitMenuItemClick(object sender, EventArgs e)
		{
			this.Close();
		}

		private void RemoveTimeClick(object sender, EventArgs e)
		{
			RemoveTime removeTimeForm = new RemoveTime();
			if (removeTimeForm.ShowDialog() == DialogResult.OK)
			{
				try
				{
					SetTimeToRemove(removeTimeForm.TimeToRemove);
				}
				catch (Exception)
				{
					MessageBox.Show("Lost connection to Program Monitor service", "Lost Connection",
						MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
					Reconnect();
				}
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.mClosing = true;
			this.tmrEstablishConnection.Enabled = false;
			this.tmrUpdateText.Enabled = false;

			if (this.mRemotingClient != null)
			{
				try
				{
					this.mRemotingClient.UnsubscribeEvents();
				}
				catch (Exception)
				{
					// ignore exception; we're stopping anyway
				}
			}
		}
	}
}
