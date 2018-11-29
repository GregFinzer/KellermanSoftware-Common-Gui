#region Includes

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

#endregion

namespace KellermanSoftware.Common.Gui
{
	/// <summary>
	/// About Screen
	/// </summary>
	public class frmAbout :  Form
	{
		#region WinForm Controls
		private Label lblProgramName;
		private PictureBox pictureBox1;
		private Label lblVersion;
		private Label lblCopyright;
        private Button btnOK;
		private LinkLabel linkHomePage;
		private LinkLabel linkSupport;
		private GroupBox grpSales;
        private LinkLabel linkSales;
        private GroupBox grpSupport;
		private Label lblSerial;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;

		#endregion

		#region Constructor/Destructor

		/// <summary>
		/// Constructor
		/// </summary>
		public frmAbout(string programName, 
			string version, 
			string companyName, 
			string url, 
			string salesEmail,
			string supportEmail,
			string licenseStatus)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//Form Caption
			Text= "About " + programName + " by " + companyName;

			//Title
			lblProgramName.Text = programName;

			//Version
			lblVersion.Text= version;

			//Copyright
			string sCopyright="Copyright " + DateTime.Now.Year + " " + companyName + ".  ";
			sCopyright+= "All rights reserved.";
			lblCopyright.Text= sCopyright;

            lblSerial.Text = licenseStatus;


			//Sales
			linkSales.Text= salesEmail;
			linkSales.Links.Add(0,linkSales.Text.Length,"mailto:" + salesEmail);

			//Support
			linkSupport.Text= supportEmail;
			linkSupport.Links.Add(0,linkSupport.Text.Length,"mailto:" + supportEmail);
		
			linkHomePage.Links.Add(0,linkHomePage.Text.Length,url);
		
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#endregion

		#region Events
		private void linkHomePage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(e.Link.LinkData.ToString());
		}

		private void linkEMail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(e.Link.LinkData.ToString());
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void linkSales_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(e.Link.LinkData.ToString());		
		}

		private void linkFeedback_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start(e.Link.LinkData.ToString());	
		}

		#endregion


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
            this.lblProgramName = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.linkHomePage = new System.Windows.Forms.LinkLabel();
            this.linkSupport = new System.Windows.Forms.LinkLabel();
            this.btnOK = new System.Windows.Forms.Button();
            this.grpSales = new System.Windows.Forms.GroupBox();
            this.linkSales = new System.Windows.Forms.LinkLabel();
            this.grpSupport = new System.Windows.Forms.GroupBox();
            this.lblSerial = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.grpSales.SuspendLayout();
            this.grpSupport.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblProgramName
            // 
            this.lblProgramName.BackColor = System.Drawing.Color.Transparent;
            this.lblProgramName.Font = new System.Drawing.Font("Times New Roman", 28F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgramName.Location = new System.Drawing.Point(58, 18);
            this.lblProgramName.Name = "lblProgramName";
            this.lblProgramName.Size = new System.Drawing.Size(537, 65);
            this.lblProgramName.TabIndex = 0;
            this.lblProgramName.Text = "lblProgramName";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(19, 32);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(39, 46);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // lblVersion
            // 
            this.lblVersion.BackColor = System.Drawing.Color.Transparent;
            this.lblVersion.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(67, 92);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(528, 27);
            this.lblVersion.TabIndex = 2;
            this.lblVersion.Text = "lblVersion";
            // 
            // lblCopyright
            // 
            this.lblCopyright.BackColor = System.Drawing.Color.Transparent;
            this.lblCopyright.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCopyright.Location = new System.Drawing.Point(67, 120);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(528, 28);
            this.lblCopyright.TabIndex = 3;
            this.lblCopyright.Text = "lblCopyright";
            // 
            // linkHomePage
            // 
            this.linkHomePage.BackColor = System.Drawing.Color.Transparent;
            this.linkHomePage.Location = new System.Drawing.Point(86, 331);
            this.linkHomePage.Name = "linkHomePage";
            this.linkHomePage.Size = new System.Drawing.Size(413, 27);
            this.linkHomePage.TabIndex = 4;
            this.linkHomePage.TabStop = true;
            this.linkHomePage.Text = "Website";
            this.linkHomePage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHomePage_LinkClicked);
            // 
            // linkSupport
            // 
            this.linkSupport.Location = new System.Drawing.Point(19, 28);
            this.linkSupport.Name = "linkSupport";
            this.linkSupport.Size = new System.Drawing.Size(499, 26);
            this.linkSupport.TabIndex = 5;
            this.linkSupport.TabStop = true;
            this.linkSupport.Text = "linkSupport";
            this.linkSupport.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkEMail_LinkClicked);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(506, 361);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(87, 28);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "&OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // grpSales
            // 
            this.grpSales.BackColor = System.Drawing.Color.Transparent;
            this.grpSales.Controls.Add(this.linkSales);
            this.grpSales.Location = new System.Drawing.Point(67, 185);
            this.grpSales.Name = "grpSales";
            this.grpSales.Size = new System.Drawing.Size(528, 65);
            this.grpSales.TabIndex = 7;
            this.grpSales.TabStop = false;
            this.grpSales.Text = "Sales";
            // 
            // linkSales
            // 
            this.linkSales.Location = new System.Drawing.Point(19, 28);
            this.linkSales.Name = "linkSales";
            this.linkSales.Size = new System.Drawing.Size(499, 26);
            this.linkSales.TabIndex = 8;
            this.linkSales.TabStop = true;
            this.linkSales.Text = "linkSales";
            this.linkSales.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkSales_LinkClicked);
            // 
            // grpSupport
            // 
            this.grpSupport.BackColor = System.Drawing.Color.Transparent;
            this.grpSupport.Controls.Add(this.linkSupport);
            this.grpSupport.Location = new System.Drawing.Point(67, 257);
            this.grpSupport.Name = "grpSupport";
            this.grpSupport.Size = new System.Drawing.Size(528, 59);
            this.grpSupport.TabIndex = 8;
            this.grpSupport.TabStop = false;
            this.grpSupport.Text = "Support";
            // 
            // lblSerial
            // 
            this.lblSerial.BackColor = System.Drawing.Color.Transparent;
            this.lblSerial.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSerial.Location = new System.Drawing.Point(67, 148);
            this.lblSerial.Name = "lblSerial";
            this.lblSerial.Size = new System.Drawing.Size(528, 26);
            this.lblSerial.TabIndex = 10;
            this.lblSerial.Text = "lblSerial";
            // 
            // frmAbout
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(645, 423);
            this.Controls.Add(this.lblSerial);
            this.Controls.Add(this.grpSupport);
            this.Controls.Add(this.grpSales);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.linkHomePage);
            this.Controls.Add(this.lblCopyright);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblProgramName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAbout";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.grpSales.ResumeLayout(false);
            this.grpSupport.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion


	}
}
