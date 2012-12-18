using System.Collections.Generic;
using System.Windows.Forms;

namespace ProgramMonitor.UI
{
	/// <summary>
	/// This is a ListView that is dedicated to managing lists of applications
	/// </summary>
	internal abstract class ApplicationListView: ListView
	{
		private ColumnHeader mColApplicationName;
		private ColumnHeader mColLocation;
		private ColumnHeader mColExecutable;

		protected bool mIsDefault;
		protected string mUserName;
	
		public ApplicationListView()
		{
			InitializeComponent();
		}

		public ApplicationListView(string userName)
		{
			InitializeComponent(userName);
		}

		protected virtual void AddColumns(List<ColumnHeader> columns)
		{
		}

		private void InitializeComponent(string userName)
		{
			InitializeComponent();
			this.mIsDefault = false;
			this.mUserName = userName;
		}

		protected virtual void InitializeComponent()
		{
			List<ColumnHeader> columns = new List<ColumnHeader>();

			// setup default column headers
			this.mColApplicationName = new ColumnHeader();
			this.mColApplicationName.Text = "Application Name";
			this.mColApplicationName.Width = 200;

			this.mColLocation = new ColumnHeader();
			this.mColLocation.Text = "Location";
			this.mColLocation.Width = 240;

			this.mColExecutable = new ColumnHeader();
			this.mColExecutable.Text = "Executable";
			this.mColExecutable.Width = 100;

			columns.Add(this.mColApplicationName);
			columns.Add(this.mColLocation);
			columns.Add(this.mColExecutable);

			this.AddColumns(columns);

			this.Columns.AddRange(columns.ToArray());

			// setup rest of control
			this.Dock = DockStyle.Fill;
			this.FullRowSelect = true;
			this.Location = new System.Drawing.Point(3, 3);
			this.Name = "lvApplications";
			this.Size = new System.Drawing.Size(572, 338);
			this.TabIndex = 0;
			this.UseCompatibleStateImageBehavior = false;
			this.View = View.Details;

			this.mIsDefault = true;
		}
	}
}
