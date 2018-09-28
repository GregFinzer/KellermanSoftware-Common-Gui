
#region Includes
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Microsoft.Win32;
#endregion

namespace KellermanSoftware.Common.Gui
{
	/// <summary>
	/// Form to display errors and send to technical support
	/// </summary>
	public class ErrorDialog : System.Windows.Forms.Form
	{
		#region Class Variables		
        ErrorInformation myErrorInformation = null;

#if DEBUG
		private bool mbShowDetails=true;
#else
		private bool mbShowDetails=false;
#endif

		#endregion

		#region Windows Form Controls
		private System.Windows.Forms.TextBox txtCulture;
        private System.Windows.Forms.Label lblCulture;
        private System.Windows.Forms.Button btnSendError;
        private System.Windows.Forms.Button btnDontSend;

		private System.Windows.Forms.Label lblApplicationName;
		private System.Windows.Forms.Label lblErrorType;
		private System.Windows.Forms.Label lblProcedureName;
		private System.Windows.Forms.Label lblClassName;
		private System.Windows.Forms.TextBox txtApplicationName;
		private System.Windows.Forms.TextBox txtProcedureName;
        private System.Windows.Forms.TextBox txtClassName;
        private IContainer components;
        private System.Windows.Forms.TextBox txtAdditionalInfo;
        private System.Windows.Forms.ErrorProvider epValidation;
		private System.Windows.Forms.Label lblHeader;
		private System.Windows.Forms.PictureBox picYield;
		private System.Windows.Forms.Label lblStackTrace;
        private System.Windows.Forms.GroupBox grpErrorDetails;
		private System.Windows.Forms.Label lblAdditional;
		private System.Windows.Forms.Label lblSorry;
		private System.Windows.Forms.Label lblSteps;
		private System.Windows.Forms.TextBox txtErrorMessage;
        private System.Windows.Forms.Button btnShowDetails;
        private TableLayoutPanel pnlMain;
        private Panel pnlTitle;
        private Button btnNotepad;
        private TableLayoutPanel pnlButtons;
        private Panel pnlSendDontSend;
        private Panel pnlCopyToNotepad;
        private Panel pnlErrorDetails;
        private TableLayoutPanel pnlTableDetails;
        private Panel pnlSteps;
        private TextBox txtSteps;
		private System.Windows.Forms.TextBox txtStack;
		#endregion

		#region Constructor/Destructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="e">The exception to display</param>
		/// <param name="sApplicationName">The name of the application</param>
		/// <param name="sClassName">The current executing class</param>
		/// <param name="sProcedureName">The name of the current procedure</param>
		/// <param name="dblVersion">The version number of the application</param>
		/// <param name="sAdditionalInfo">Any additional information about the error</param>
		/// <param name="sScreenShot">Path to a screen shot of what happened during the error</param>
        public ErrorDialog(ErrorInformation errorInfo)						
		{
			//
			// Required for Windows Form Designer support
			//

			InitializeComponent();

            try
            {
                GuiException.MessageBoxErrorHandler = true;

                myErrorInformation = errorInfo;
                //string mailLink = string.Empty;
                //string systemInfo = string.Empty;
                //string linkError = string.Empty;
                //string filePath = string.Empty;

                //No wait cursor
                GuiUtility.Hourglass(false);

                //Get the constructor variables
                txtApplicationName.Text = myErrorInformation.ProgramName;
                txtCulture.Text = myErrorInformation.Culture;
                txtClassName.Text = myErrorInformation.ClassName;
                txtProcedureName.Text = myErrorInformation.ProcedureName;
                txtErrorMessage.Text = myErrorInformation.GUIException.Message;
                txtStack.Text = myErrorInformation.GUIException.StackTrace;
                txtAdditionalInfo.Text = myErrorInformation.AdditionalInfo;
            }
            catch (Exception ex)
            {
                GuiException.HandleException(ex);
            }
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

		#region Private Methods

		/// <summary>
		/// Verifies the control has all the required Information
		/// </summary>
		/// <returns>True if validation passes</returns>
		private bool ValidateForm()
		{
            try
            {
                string errorMessage = string.Empty;
                this.ValidateChildren();

                errorMessage = epValidation.GetError(txtSteps);

                //If there was a validation issue, set the focus to the control and cancel the next button
                if (errorMessage.Length > 0)
                {
                    txtSteps.Focus();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                GuiException.HandleException(ex);
                return false;
            }

		}

		/// <summary>
		/// Try the mailto link 
		/// </summary>
		/// <param name="sLinkData"></param>
		/// <returns></returns>
		private bool TryLink(string linkData)
		{
			try
			{
                System.Diagnostics.Process.Start(linkData);
				return true;
			}
			catch
			{
                if (linkData.Length > 10)
                    return TryLink(StringUtil.Left(linkData, linkData.Length - 10));
				else
					return false;
			}
		}

        /// <summary>
        /// Send a message using mapi dll built into windows
        /// </summary>
        /// <returns></returns>
        private bool SendMapiMessage()
        {
            try
            {
                string subject = "Error in " + myErrorInformation.ProgramName;

                myErrorInformation.Steps = txtSteps.Text;
                
                Win32Mapi.Mapi myMapi = new Win32Mapi.Mapi();
                myMapi.AddRecip("", myErrorInformation.SupportEmail,false);
                myMapi.Attach(myErrorInformation.ScreenShot);
                myMapi.Attach(myErrorInformation.EventLogText);
                myMapi.Send(subject, myErrorInformation.GetXMLMessage());

                return true;
            }
            catch (Exception ex)
            {
                //Smtp server send failed, send with a mailto link
                GuiException.HandleException(ex);
                return false;
            }
        }


		/// <summary>
		/// Email the error to technical support
		/// </summary>
		/// <returns></returns>
		private bool SendError()
		{            
            string body;
            string subject= "Error in " + myErrorInformation.ProgramName;
            string attachments = string.Empty;
            EmailEngine email = new EmailEngine();
            

            try
            {
                this.ValidateChildren();

                if (epValidation.GetError(txtSteps).Length == 0)
                {
                    //Send using mailto link if there is no smtp server specified
                    if (myErrorInformation.SmtpServer == string.Empty)
                    {
                        TryLink(myErrorInformation.GetManualEmailMessage());

                        //if (SendMapiMessage() == false)
                        //{
                        //    TryLink(myErrorInformation.GetManualEmailMessage());
                        //}

                        return true;
                    }
                    else
                    {
                        //Send using smtp server
                        email.ServerName= myErrorInformation.SmtpServer; //"prod-mail.nc.customercenter.net";
                        myErrorInformation.Steps = txtSteps.Text;
                        body = myErrorInformation.GetXMLMessage();
                        attachments += myErrorInformation.ScreenShot + ",";
                        attachments += myErrorInformation.EventLogText + ",";

                        email.SendMail(myErrorInformation.Email, myErrorInformation.SupportEmail, subject, body, attachments);
                        return true;
                    }
                }
                else
                {
                    MessageBox.Show(this, epValidation.GetError(txtSteps), "Errors on Screen", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }
            catch (Exception ex)
            {
                //Smtp server send failed, send with a mailto link
                GuiException.HandleException(ex);
                TryLink(myErrorInformation.GetManualEmailMessage());
                return false;
            }
		}

        /// <summary>
        /// Show or hide the error details section
        /// </summary>
        /// <param name="bShow"></param>
        private void ShowDetails(bool bShow)
        {
            try
            {
                if (bShow)
                {
                    this.Height = 648;
                    grpErrorDetails.Visible = true;
                    this.CenterToScreen();
                    btnShowDetails.Text = "Hide Details <<";
                }
                else
                {
                    grpErrorDetails.Visible = false;
                    this.Height = 295;
                    this.CenterToScreen();
                    btnShowDetails.Text = "Show Details >>";
                }

                mbShowDetails = !bShow;
            }
            catch (Exception ex)
            {
                GuiException.HandleException(ex);
            }
        }

		#endregion

		#region Events

		/// <summary>
		/// Ensure the user entered someting intelligible
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void txtSteps_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string steps= txtSteps.Text.ToLower();

            if (steps.Trim().Length == 0)
			{
				epValidation.SetError(txtSteps,"Please fill in the Steps to reproduce.");
			}
            else if (steps == "don't know"
                || steps == "i don't know"
                || steps == "i dont know"
                || steps == "i have no idea"
                || steps == "i have no idea what happened"
                || StringUtil.NumberOfWords(steps, " ") <= 3)
			{				
				epValidation.SetError(txtSteps,"Please be more descriptive so this defect can be corrected.");
			}
            else if (Validation.WordPattern(steps, " ", 3) 
                || steps.IndexOf("asdf") >= 0
                || steps.IndexOf("jkl") >=0 )
			{
				epValidation.SetError(txtSteps,"Please type something in that is intelligible.");
			}
			else
			{
				epValidation.SetError(txtSteps,"");
			}
		}
		
		/// <summary>
		/// Send the error to technical support
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSendError_Click(object sender, System.EventArgs e)
		{
            myErrorInformation.Steps = txtSteps.Text;

            if (GuiUtility.CheckInternetConnection(false) == false)
            {
                if (SendMapiMessage() == false)
                {
                    TryLink(myErrorInformation.GetManualEmailMessage());
                }

                return;
            }

            if (SendError())
            {
                GuiException.MessageBoxErrorHandler = false;
                this.Close();
            }
		}

		/// <summary>
		/// Close the form and don't send the error to technical support
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDontSend_Click(object sender, System.EventArgs e)
		{
            try
            {
                if (System.IO.File.Exists(myErrorInformation.ScreenShot))
                {
                    System.IO.File.Delete(myErrorInformation.ScreenShot);
                }

                if (System.IO.File.Exists(myErrorInformation.EventLogText))
                {
                    System.IO.File.Delete(myErrorInformation.EventLogText);
                }

                GuiException.MessageBoxErrorHandler = false;
                this.Close();
            }
            catch (Exception ex)
            {
                GuiException.HandleException(ex);
                this.Close();
            }
        }

		/// <summary>
		/// Another error was thrown while displaying the error, display a notepad error message
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void frmError_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
            GuiException.MessageBoxErrorHandler = false;
		}

        /// <summary>
        /// Show or Hide the Error Details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowDetails_Click(object sender, System.EventArgs e)
        {
            ShowDetails(mbShowDetails);
        }

        /// <summary>
        /// Form Load Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmError_Load(object sender, System.EventArgs e)
        {
            ShowDetails(mbShowDetails);
        }

        /// <summary>
        /// Copy the error to notepad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNotepad_Click(object sender, EventArgs e)
        {
            try
            {
                GuiUtility.NotepadMessage(myErrorInformation.GetNotepadMessage());
            }
            catch (Exception ex)
            {
                GuiException.HandleException(ex);
            }
        }

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorDialog));
            this.lblHeader = new System.Windows.Forms.Label();
            this.picYield = new System.Windows.Forms.PictureBox();
            this.lblApplicationName = new System.Windows.Forms.Label();
            this.lblErrorType = new System.Windows.Forms.Label();
            this.lblProcedureName = new System.Windows.Forms.Label();
            this.lblClassName = new System.Windows.Forms.Label();
            this.lblStackTrace = new System.Windows.Forms.Label();
            this.txtApplicationName = new System.Windows.Forms.TextBox();
            this.txtErrorMessage = new System.Windows.Forms.TextBox();
            this.txtProcedureName = new System.Windows.Forms.TextBox();
            this.txtStack = new System.Windows.Forms.TextBox();
            this.txtClassName = new System.Windows.Forms.TextBox();
            this.grpErrorDetails = new System.Windows.Forms.GroupBox();
            this.pnlTableDetails = new System.Windows.Forms.TableLayoutPanel();
            this.txtAdditionalInfo = new System.Windows.Forms.TextBox();
            this.lblAdditional = new System.Windows.Forms.Label();
            this.txtCulture = new System.Windows.Forms.TextBox();
            this.lblCulture = new System.Windows.Forms.Label();
            this.lblSorry = new System.Windows.Forms.Label();
            this.lblSteps = new System.Windows.Forms.Label();
            this.epValidation = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnSendError = new System.Windows.Forms.Button();
            this.btnDontSend = new System.Windows.Forms.Button();
            this.btnShowDetails = new System.Windows.Forms.Button();
            this.pnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.pnlButtons = new System.Windows.Forms.TableLayoutPanel();
            this.pnlSendDontSend = new System.Windows.Forms.Panel();
            this.pnlCopyToNotepad = new System.Windows.Forms.Panel();
            this.btnNotepad = new System.Windows.Forms.Button();
            this.pnlErrorDetails = new System.Windows.Forms.Panel();
            this.pnlSteps = new System.Windows.Forms.Panel();
            this.txtSteps = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picYield)).BeginInit();
            this.grpErrorDetails.SuspendLayout();
            this.pnlTableDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epValidation)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.pnlTitle.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.pnlSendDontSend.SuspendLayout();
            this.pnlCopyToNotepad.SuspendLayout();
            this.pnlErrorDetails.SuspendLayout();
            this.pnlSteps.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblHeader
            // 
            this.lblHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(54, 8);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(328, 31);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "An Error Has Occurred";
            // 
            // picYield
            // 
            this.picYield.Image = ((System.Drawing.Image)(resources.GetObject("picYield.Image")));
            this.picYield.Location = new System.Drawing.Point(8, 6);
            this.picYield.Name = "picYield";
            this.picYield.Size = new System.Drawing.Size(40, 40);
            this.picYield.TabIndex = 1;
            this.picYield.TabStop = false;
            // 
            // lblApplicationName
            // 
            this.lblApplicationName.Location = new System.Drawing.Point(3, 0);
            this.lblApplicationName.Name = "lblApplicationName";
            this.lblApplicationName.Size = new System.Drawing.Size(94, 23);
            this.lblApplicationName.TabIndex = 2;
            this.lblApplicationName.Text = "Application Name:";
            this.lblApplicationName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblErrorType
            // 
            this.lblErrorType.Location = new System.Drawing.Point(3, 100);
            this.lblErrorType.Name = "lblErrorType";
            this.lblErrorType.Size = new System.Drawing.Size(94, 23);
            this.lblErrorType.TabIndex = 3;
            this.lblErrorType.Text = "Error:";
            this.lblErrorType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblProcedureName
            // 
            this.lblProcedureName.Location = new System.Drawing.Point(3, 75);
            this.lblProcedureName.Name = "lblProcedureName";
            this.lblProcedureName.Size = new System.Drawing.Size(94, 23);
            this.lblProcedureName.TabIndex = 4;
            this.lblProcedureName.Text = "Procedure Name:";
            this.lblProcedureName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblClassName
            // 
            this.lblClassName.Location = new System.Drawing.Point(3, 50);
            this.lblClassName.Name = "lblClassName";
            this.lblClassName.Size = new System.Drawing.Size(94, 23);
            this.lblClassName.TabIndex = 5;
            this.lblClassName.Text = "Class Name:";
            this.lblClassName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStackTrace
            // 
            this.lblStackTrace.Location = new System.Drawing.Point(3, 163);
            this.lblStackTrace.Name = "lblStackTrace";
            this.lblStackTrace.Size = new System.Drawing.Size(94, 23);
            this.lblStackTrace.TabIndex = 6;
            this.lblStackTrace.Text = "Stack Trace:";
            this.lblStackTrace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtApplicationName
            // 
            this.txtApplicationName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtApplicationName.Location = new System.Drawing.Point(103, 3);
            this.txtApplicationName.Name = "txtApplicationName";
            this.txtApplicationName.ReadOnly = true;
            this.txtApplicationName.Size = new System.Drawing.Size(391, 20);
            this.txtApplicationName.TabIndex = 7;
            // 
            // txtErrorMessage
            // 
            this.txtErrorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtErrorMessage.Location = new System.Drawing.Point(103, 103);
            this.txtErrorMessage.Multiline = true;
            this.txtErrorMessage.Name = "txtErrorMessage";
            this.txtErrorMessage.ReadOnly = true;
            this.txtErrorMessage.Size = new System.Drawing.Size(391, 57);
            this.txtErrorMessage.TabIndex = 8;
            // 
            // txtProcedureName
            // 
            this.txtProcedureName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtProcedureName.Location = new System.Drawing.Point(103, 78);
            this.txtProcedureName.Name = "txtProcedureName";
            this.txtProcedureName.ReadOnly = true;
            this.txtProcedureName.Size = new System.Drawing.Size(391, 20);
            this.txtProcedureName.TabIndex = 9;
            // 
            // txtStack
            // 
            this.txtStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStack.Location = new System.Drawing.Point(103, 166);
            this.txtStack.Multiline = true;
            this.txtStack.Name = "txtStack";
            this.txtStack.ReadOnly = true;
            this.txtStack.Size = new System.Drawing.Size(391, 57);
            this.txtStack.TabIndex = 10;
            // 
            // txtClassName
            // 
            this.txtClassName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtClassName.Location = new System.Drawing.Point(103, 53);
            this.txtClassName.Name = "txtClassName";
            this.txtClassName.ReadOnly = true;
            this.txtClassName.Size = new System.Drawing.Size(391, 20);
            this.txtClassName.TabIndex = 11;
            // 
            // grpErrorDetails
            // 
            this.grpErrorDetails.Controls.Add(this.pnlTableDetails);
            this.grpErrorDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpErrorDetails.Location = new System.Drawing.Point(5, 5);
            this.grpErrorDetails.Name = "grpErrorDetails";
            this.grpErrorDetails.Padding = new System.Windows.Forms.Padding(5);
            this.grpErrorDetails.Size = new System.Drawing.Size(507, 314);
            this.grpErrorDetails.TabIndex = 14;
            this.grpErrorDetails.TabStop = false;
            this.grpErrorDetails.Text = "Details";
            // 
            // pnlTableDetails
            // 
            this.pnlTableDetails.ColumnCount = 2;
            this.pnlTableDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.pnlTableDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlTableDetails.Controls.Add(this.lblApplicationName, 0, 0);
            this.pnlTableDetails.Controls.Add(this.txtAdditionalInfo, 1, 6);
            this.pnlTableDetails.Controls.Add(this.lblAdditional, 0, 6);
            this.pnlTableDetails.Controls.Add(this.txtCulture, 1, 1);
            this.pnlTableDetails.Controls.Add(this.txtStack, 1, 5);
            this.pnlTableDetails.Controls.Add(this.txtErrorMessage, 1, 4);
            this.pnlTableDetails.Controls.Add(this.lblStackTrace, 0, 5);
            this.pnlTableDetails.Controls.Add(this.lblErrorType, 0, 4);
            this.pnlTableDetails.Controls.Add(this.lblCulture, 0, 1);
            this.pnlTableDetails.Controls.Add(this.txtApplicationName, 1, 0);
            this.pnlTableDetails.Controls.Add(this.lblClassName, 0, 2);
            this.pnlTableDetails.Controls.Add(this.txtProcedureName, 1, 3);
            this.pnlTableDetails.Controls.Add(this.txtClassName, 1, 2);
            this.pnlTableDetails.Controls.Add(this.lblProcedureName, 0, 3);
            this.pnlTableDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTableDetails.Location = new System.Drawing.Point(5, 18);
            this.pnlTableDetails.Name = "pnlTableDetails";
            this.pnlTableDetails.RowCount = 7;
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlTableDetails.Size = new System.Drawing.Size(497, 291);
            this.pnlTableDetails.TabIndex = 18;
            // 
            // txtAdditionalInfo
            // 
            this.txtAdditionalInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAdditionalInfo.Location = new System.Drawing.Point(103, 229);
            this.txtAdditionalInfo.Multiline = true;
            this.txtAdditionalInfo.Name = "txtAdditionalInfo";
            this.txtAdditionalInfo.ReadOnly = true;
            this.txtAdditionalInfo.Size = new System.Drawing.Size(391, 59);
            this.txtAdditionalInfo.TabIndex = 12;
            // 
            // lblAdditional
            // 
            this.lblAdditional.Location = new System.Drawing.Point(3, 226);
            this.lblAdditional.Name = "lblAdditional";
            this.lblAdditional.Size = new System.Drawing.Size(94, 23);
            this.lblAdditional.TabIndex = 13;
            this.lblAdditional.Text = "Additional Info:";
            this.lblAdditional.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCulture
            // 
            this.txtCulture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCulture.Location = new System.Drawing.Point(103, 28);
            this.txtCulture.Name = "txtCulture";
            this.txtCulture.ReadOnly = true;
            this.txtCulture.Size = new System.Drawing.Size(391, 20);
            this.txtCulture.TabIndex = 16;
            // 
            // lblCulture
            // 
            this.lblCulture.Location = new System.Drawing.Point(3, 25);
            this.lblCulture.Name = "lblCulture";
            this.lblCulture.Size = new System.Drawing.Size(94, 23);
            this.lblCulture.TabIndex = 17;
            this.lblCulture.Text = "Culture:";
            this.lblCulture.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSorry
            // 
            this.lblSorry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSorry.Location = new System.Drawing.Point(3, 45);
            this.lblSorry.Name = "lblSorry";
            this.lblSorry.Padding = new System.Windows.Forms.Padding(5);
            this.lblSorry.Size = new System.Drawing.Size(517, 60);
            this.lblSorry.TabIndex = 16;
            this.lblSorry.Text = resources.GetString("lblSorry.Text");
            // 
            // lblSteps
            // 
            this.lblSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSteps.Location = new System.Drawing.Point(3, 105);
            this.lblSteps.Name = "lblSteps";
            this.lblSteps.Padding = new System.Windows.Forms.Padding(5);
            this.lblSteps.Size = new System.Drawing.Size(517, 26);
            this.lblSteps.TabIndex = 17;
            this.lblSteps.Text = "Please enter the steps to reproduce this error:";
            // 
            // epValidation
            // 
            this.epValidation.ContainerControl = this;
            this.epValidation.DataMember = "";
            // 
            // btnSendError
            // 
            this.btnSendError.Location = new System.Drawing.Point(3, 5);
            this.btnSendError.Name = "btnSendError";
            this.btnSendError.Size = new System.Drawing.Size(72, 23);
            this.btnSendError.TabIndex = 19;
            this.btnSendError.Text = "Send Error";
            this.btnSendError.Click += new System.EventHandler(this.btnSendError_Click);
            // 
            // btnDontSend
            // 
            this.btnDontSend.Location = new System.Drawing.Point(81, 5);
            this.btnDontSend.Name = "btnDontSend";
            this.btnDontSend.Size = new System.Drawing.Size(72, 23);
            this.btnDontSend.TabIndex = 20;
            this.btnDontSend.Text = "Don\'t Send";
            this.btnDontSend.Click += new System.EventHandler(this.btnDontSend_Click);
            // 
            // btnShowDetails
            // 
            this.btnShowDetails.Location = new System.Drawing.Point(114, 5);
            this.btnShowDetails.Name = "btnShowDetails";
            this.btnShowDetails.Size = new System.Drawing.Size(104, 23);
            this.btnShowDetails.TabIndex = 21;
            this.btnShowDetails.Text = "Show Details >>";
            this.btnShowDetails.Click += new System.EventHandler(this.btnShowDetails_Click);
            // 
            // pnlMain
            // 
            this.pnlMain.ColumnCount = 1;
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlMain.Controls.Add(this.lblSorry, 0, 1);
            this.pnlMain.Controls.Add(this.pnlTitle, 0, 0);
            this.pnlMain.Controls.Add(this.lblSteps, 0, 2);
            this.pnlMain.Controls.Add(this.pnlButtons, 0, 4);
            this.pnlMain.Controls.Add(this.pnlErrorDetails, 0, 5);
            this.pnlMain.Controls.Add(this.pnlSteps, 0, 3);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.RowCount = 6;
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlMain.Size = new System.Drawing.Size(523, 566);
            this.pnlMain.TabIndex = 22;
            // 
            // pnlTitle
            // 
            this.pnlTitle.Controls.Add(this.picYield);
            this.pnlTitle.Controls.Add(this.lblHeader);
            this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTitle.Location = new System.Drawing.Point(3, 3);
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(517, 39);
            this.pnlTitle.TabIndex = 23;
            // 
            // pnlButtons
            // 
            this.pnlButtons.ColumnCount = 2;
            this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlButtons.Controls.Add(this.pnlSendDontSend, 1, 0);
            this.pnlButtons.Controls.Add(this.pnlCopyToNotepad, 0, 0);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlButtons.Location = new System.Drawing.Point(3, 189);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.RowCount = 1;
            this.pnlButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.pnlButtons.Size = new System.Drawing.Size(517, 44);
            this.pnlButtons.TabIndex = 24;
            // 
            // pnlSendDontSend
            // 
            this.pnlSendDontSend.Controls.Add(this.btnDontSend);
            this.pnlSendDontSend.Controls.Add(this.btnSendError);
            this.pnlSendDontSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlSendDontSend.Location = new System.Drawing.Point(356, 3);
            this.pnlSendDontSend.Name = "pnlSendDontSend";
            this.pnlSendDontSend.Size = new System.Drawing.Size(158, 44);
            this.pnlSendDontSend.TabIndex = 24;
            // 
            // pnlCopyToNotepad
            // 
            this.pnlCopyToNotepad.Controls.Add(this.btnNotepad);
            this.pnlCopyToNotepad.Controls.Add(this.btnShowDetails);
            this.pnlCopyToNotepad.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlCopyToNotepad.Location = new System.Drawing.Point(3, 3);
            this.pnlCopyToNotepad.Name = "pnlCopyToNotepad";
            this.pnlCopyToNotepad.Size = new System.Drawing.Size(225, 44);
            this.pnlCopyToNotepad.TabIndex = 25;
            // 
            // btnNotepad
            // 
            this.btnNotepad.Location = new System.Drawing.Point(3, 5);
            this.btnNotepad.Name = "btnNotepad";
            this.btnNotepad.Size = new System.Drawing.Size(105, 23);
            this.btnNotepad.TabIndex = 23;
            this.btnNotepad.Text = "Copy to Notepad";
            this.btnNotepad.UseVisualStyleBackColor = true;
            this.btnNotepad.Click += new System.EventHandler(this.btnNotepad_Click);
            // 
            // pnlErrorDetails
            // 
            this.pnlErrorDetails.Controls.Add(this.grpErrorDetails);
            this.pnlErrorDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlErrorDetails.Location = new System.Drawing.Point(3, 239);
            this.pnlErrorDetails.Name = "pnlErrorDetails";
            this.pnlErrorDetails.Padding = new System.Windows.Forms.Padding(5);
            this.pnlErrorDetails.Size = new System.Drawing.Size(517, 324);
            this.pnlErrorDetails.TabIndex = 25;
            // 
            // pnlSteps
            // 
            this.pnlSteps.Controls.Add(this.txtSteps);
            this.pnlSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSteps.Location = new System.Drawing.Point(3, 134);
            this.pnlSteps.Name = "pnlSteps";
            this.pnlSteps.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.pnlSteps.Size = new System.Drawing.Size(517, 49);
            this.pnlSteps.TabIndex = 26;
            // 
            // txtSteps
            // 
            this.txtSteps.AcceptsReturn = true;
            this.txtSteps.AcceptsTab = true;
            this.txtSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSteps.Location = new System.Drawing.Point(0, 0);
            this.txtSteps.Multiline = true;
            this.txtSteps.Name = "txtSteps";
            this.txtSteps.Size = new System.Drawing.Size(497, 49);
            this.txtSteps.TabIndex = 0;
            this.txtSteps.Validating += new System.ComponentModel.CancelEventHandler(this.txtSteps_Validating);
            // 
            // ErrorDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(523, 566);
            this.Controls.Add(this.pnlMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "ErrorDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Error";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmError_Closing);
            this.Load += new System.EventHandler(this.frmError_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picYield)).EndInit();
            this.grpErrorDetails.ResumeLayout(false);
            this.pnlTableDetails.ResumeLayout(false);
            this.pnlTableDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epValidation)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlTitle.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.pnlSendDontSend.ResumeLayout(false);
            this.pnlCopyToNotepad.ResumeLayout(false);
            this.pnlErrorDetails.ResumeLayout(false);
            this.pnlSteps.ResumeLayout(false);
            this.pnlSteps.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

	}
}
