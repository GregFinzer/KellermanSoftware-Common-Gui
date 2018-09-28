using System;
using System.Windows.Forms;

namespace KellermanSoftware.Common.Gui
{
    public partial class frmNag : Form
    {
        private string _buyNowLink;
        ILicenseInterface _licensingLibrary;

        public frmNag(ILicenseInterface licensingLibrary, string buyNowLink)
        {
            InitializeComponent();
            _buyNowLink = buyNowLink;
            _licensingLibrary = licensingLibrary;
            lblTrialMessage.Text = _licensingLibrary.AdditionalInfo;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnBuyNow_Click(object sender, EventArgs e)
        {
            ProcessUtil.Shell(_buyNowLink, string.Empty, System.Diagnostics.ProcessWindowStyle.Maximized, false);
        }

        private void btnRegistration_Click(object sender, EventArgs e)
        {
            frmRegistration reg = new frmRegistration(_licensingLibrary);
            reg.ShowDialog(this);

            if (_licensingLibrary.CheckLicense())
            {
                Close();
            }
        }
    }
}