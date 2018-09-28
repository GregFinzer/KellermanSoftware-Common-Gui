using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KellermanSoftware.Common.Gui
{
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
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _licensingLibrary.UserName = txtUserName.Text;
            _licensingLibrary.License = txtLicenseKey.Text;

            _licensingLibrary.CheckLicense();

            if (_licensingLibrary.LicensedUser == false)
                epValidation.SetError(txtLicenseKey, _licensingLibrary.AdditionalInfo);
            else
                this.Close();
        }
    }
}