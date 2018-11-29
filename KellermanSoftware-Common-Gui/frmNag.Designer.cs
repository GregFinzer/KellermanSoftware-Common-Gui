using System.ComponentModel;
using System.Windows.Forms;

namespace KellermanSoftware.Common.Gui
{
    partial class frmNag
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTrialMessage = new System.Windows.Forms.Label();
            this.btnTry = new System.Windows.Forms.Button();
            this.btnBuyNow = new System.Windows.Forms.Button();
            this.btnRegistration = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTrialMessage
            // 
            this.lblTrialMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTrialMessage.Location = new System.Drawing.Point(10, 10);
            this.lblTrialMessage.Name = "lblTrialMessage";
            this.lblTrialMessage.Size = new System.Drawing.Size(271, 90);
            this.lblTrialMessage.TabIndex = 0;
            this.lblTrialMessage.Text = "lblTrialMessage";
            // 
            // btnTry
            // 
            this.btnTry.Location = new System.Drawing.Point(204, 118);
            this.btnTry.Name = "btnTry";
            this.btnTry.Size = new System.Drawing.Size(75, 23);
            this.btnTry.TabIndex = 2;
            this.btnTry.Text = "&Try";
            this.btnTry.UseVisualStyleBackColor = true;
            this.btnTry.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnBuyNow
            // 
            this.btnBuyNow.Location = new System.Drawing.Point(16, 118);
            this.btnBuyNow.Name = "btnBuyNow";
            this.btnBuyNow.Size = new System.Drawing.Size(75, 23);
            this.btnBuyNow.TabIndex = 3;
            this.btnBuyNow.Text = "&Buy Now!";
            this.btnBuyNow.UseVisualStyleBackColor = true;
            this.btnBuyNow.Click += new System.EventHandler(this.btnBuyNow_Click);
            // 
            // btnRegistration
            // 
            this.btnRegistration.Location = new System.Drawing.Point(112, 118);
            this.btnRegistration.Name = "btnRegistration";
            this.btnRegistration.Size = new System.Drawing.Size(75, 23);
            this.btnRegistration.TabIndex = 4;
            this.btnRegistration.Text = "&Enter Code";
            this.btnRegistration.UseVisualStyleBackColor = true;
            this.btnRegistration.Click += new System.EventHandler(this.btnRegistration_Click);
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.lblTrialMessage);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(5);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(10);
            this.panelTop.Size = new System.Drawing.Size(291, 110);
            this.panelTop.TabIndex = 5;
            // 
            // frmNag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 154);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.btnRegistration);
            this.Controls.Add(this.btnBuyNow);
            this.Controls.Add(this.btnTry);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmNag";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Trial Message";
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Label lblTrialMessage;
        private Button btnTry;
        private Button btnBuyNow;
        private Button btnRegistration;
        private Panel panelTop;
    }
}