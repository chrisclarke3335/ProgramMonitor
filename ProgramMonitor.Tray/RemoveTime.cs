using System;
using System.Windows.Forms;

namespace ProgramMonitor.Tray
{
	public partial class RemoveTime : Form
	{
		public RemoveTime()
		{
			InitializeComponent();
		}

		public int TimeToRemove
		{
			get { return (int)this.udTimeToRemove.Value; }
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
