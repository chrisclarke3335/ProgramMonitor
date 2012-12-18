using System.Windows.Forms;

namespace ProgramMonitor.Tray
{
	public partial class Password : Form
	{
		public Password()
		{
			InitializeComponent();
		}

		public string PasswordText
		{
			get { return txtPassword.Text; }
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
