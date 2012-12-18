using System.Windows.Forms;

namespace ProgramMonitor.UI
{
	internal abstract class ApplicationTabPage : TabPage
	{
		protected bool mIsDefault;

		public ApplicationTabPage(int tabIndex)
		{
			this.Location = new System.Drawing.Point(4, 25);
			this.Padding = new Padding(3);
			this.TabIndex = tabIndex;
			this.Name = DefaultName;
			this.Text = DefaultName;
			this.Size = new System.Drawing.Size(578, 344);
			this.UseVisualStyleBackColor = true;

			this.mIsDefault = true;
		}

		public ApplicationTabPage(string name, int tabIndex)
		{
			this.Location = new System.Drawing.Point(4, 25);
			this.Padding = new Padding(3);
			this.TabIndex = tabIndex;
			this.Name = name;
			this.Text = name;
			this.Size = new System.Drawing.Size(578, 344);
			this.UseVisualStyleBackColor = true;

			this.mIsDefault = false;
		}

		protected abstract string DefaultName { get; }
	}
}
