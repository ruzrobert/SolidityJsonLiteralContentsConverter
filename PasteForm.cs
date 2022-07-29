using System;
using System.Windows.Forms;

namespace SolidityStandardJsonLiteralContents
{
	public partial class PasteForm : Form
	{
		public string PastedText => pasteTextBox.Text;

		private PasteForm()
		{
			InitializeComponent();
		}

		public PasteForm(string labelText = "") : this()
		{
			infoLabel.Text = labelText;
			DialogResult = DialogResult.Cancel;
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void pasteTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.A)
			{
				if (sender != null)
				{
					((TextBox)sender).SelectAll();
				}
			}
		}
	}
}
