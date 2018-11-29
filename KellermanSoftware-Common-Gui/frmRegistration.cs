using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace KellermanSoftware.Common.Gui
{
    /// <summary>
    /// Registration form
    /// </summary>
    public partial class frmRegistration : Form
    {
        ILicenseInterface _licensingLibrary;

        public frmRegistration(ILicenseInterface licensingLibrary)
        {
            InitializeComponent();
            _licensingLibrary = licensingLibrary;
        }

        private void txtUserName_Validating(object sender, CancelEventArgs e)
        {
            GuiUtility.epRequired(epValidation, txtUserName, lblUserName.Text);
        }

        private void txtLicenseKey_Validating(object sender, CancelEventArgs e)
        {
            GuiUtility.epRequired(epValidation, txtLicenseKey, lblLicenseKey.Text);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _licensingLibrary.UserName = txtUserName.Text;
            _licensingLibrary.License = txtLicenseKey.Text;

            _licensingLibrary.CheckLicense();

            if (_licensingLibrary.LicensedUser == false)
                epValidation.SetError(txtLicenseKey, _licensingLibrary.AdditionalInfo);
            else
                Close();
        }
    }
}