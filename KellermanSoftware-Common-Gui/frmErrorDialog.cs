
#region Includes

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Win32Mapi;

#endregion

namespace KellermanSoftware.Common.Gui
{
	/// <summary>
	/// Form to display errors and send to technical support
	/// </summary>
	public class ErrorDialog : Form
	{
		#region Class Variables		
        ErrorInformation myErrorInformation;

#if DEBUG
		private bool showDetails=true;
#else
		private bool showDetails=false;
#endif

		#endregion

		#region Windows Form Controls
		private TextBox txtCulture;
        private Label lblCulture;
        private Button btnSendError;
        private Button btnDontSend;

		private Label lblApplicationName;
		private Label lblErrorType;
		private Label lblProcedureName;
		private Label lblClassName;
		private TextBox txtApplicationName;
		private TextBox txtProcedureName;
        private TextBox txtClassName;
        private IContainer components;
        private TextBox txtAdditionalInfo;
        private ErrorProvider epValidation;
		private Label lblHeader;
		private PictureBox picYield;
		private Label lblStackTrace;
        private GroupBox grpErrorDetails;
		private Label lblAdditional;
		private Label lblSorry;
		private Label lblSteps;
		private TextBox txtErrorMessage;
        private Button btnShowDetails;
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
		private TextBox txtStack;
		#endregion

		#region Constructor/Destructor

		/// <summary>
		/// Constructor
		/// </summary>
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
                ValidateChildren();

                errorMessage = epValidation.GetError(txtSteps);

                //If there was a validation issue, set the focus to the control and cancel the next button
                if (errorMessage.Length > 0)
                {
                    txtSteps.Focus();
                    return false;
                }

                return true;
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
                Process.Start(linkData);
				return true;
			}
			catch
			{
			    if (linkData.Length > 10)
                    return TryLink(StringUtil.Left(linkData, linkData.Length - 10));
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
                
                Mapi myMapi = new Mapi();
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
                ValidateChildren();

                if (epValidation.GetError(txtSteps).Length == 0)
                {
                    //Send using mailto link if there is no smtp server specified
                    if (myErrorInformation.SmtpServer == string.Empty)
                    {
                        TryLink(myErrorInformation.GetManualEmailMessage());

                        return true;
                    }

                    //Send using smtp server
                    email.ServerName= myErrorInformation.SmtpServer; 
                    myErrorInformation.Steps = txtSteps.Text;
                    body = myErrorInformation.GetXMLMessage();
                    attachments += myErrorInformation.ScreenShot + ",";
                    attachments += myErrorInformation.EventLogText + ",";

                    email.SendMail(myErrorInformation.Email, myErrorInformation.SupportEmail, subject, body, attachments);
                    return true;
                }

                MessageBox.Show(this, epValidation.GetError(txtSteps), "Errors on Screen", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
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
                    Height = 648;
                    grpErrorDetails.Visible = true;
                    CenterToScreen();
                    btnShowDetails.Text = "Hide Details <<";
                }
                else
                {
                    grpErrorDetails.Visible = false;
                    Height = 295;
                    CenterToScreen();
                    btnShowDetails.Text = "Show Details >>";
                }

                showDetails = !bShow;
            }
            catch (Exception ex)
            {
                GuiException.HandleException(ex);
            }
        }

		#endregion

		#region Events

		/// <summary>
		/// Ensure the user entered something intelligible
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void txtSteps_Validating(object sender, CancelEventArgs e)
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
		private void btnSendError_Click(object sender, EventArgs e)
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
                Close();
            }
		}

		/// <summary>
		/// Close the form and don't send the error to technical support
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnDontSend_Click(object sender, EventArgs e)
		{
            try
            {
                if (File.Exists(myErrorInformation.ScreenShot))
                {
                    File.Delete(myErrorInformation.ScreenShot);
                }

                if (File.Exists(myErrorInformation.EventLogText))
                {
                    File.Delete(myErrorInformation.EventLogText);
                }

                GuiException.MessageBoxErrorHandler = false;
                Close();
            }
            catch (Exception ex)
            {
                GuiException.HandleException(ex);
                Close();
            }
        }

		/// <summary>
		/// Another error was thrown while displaying the error, display a notepad error message
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void frmError_Closing(object sender, CancelEventArgs e)
		{
            GuiException.MessageBoxErrorHandler = false;
		}

        /// <summary>
        /// Show or Hide the Error Details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowDetails_Click(object sender, EventArgs e)
        {
            ShowDetails(showDetails);
        }

        /// <summary>
        /// Form Load Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmError_Load(object sender, EventArgs e)
        {
            ShowDetails(showDetails);
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
            this.lblHeader.Location = new System.Drawing.Point(65, 9);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(393, 36);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "An Error Has Occurred";
            // 
            // picYield
            // 
            this.picYield.Image = ((System.Drawing.Image)(resources.GetObject("picYield.Image")));
            this.picYield.Location = new System.Drawing.Point(10, 7);
            this.picYield.Name = "picYield";
            this.picYield.Size = new System.Drawing.Size(48, 46);
            this.picYield.TabIndex = 1;
            this.picYield.TabStop = false;
            // 
            // lblApplicationName
            // 
            this.lblApplicationName.Location = new System.Drawing.Point(3, 0);
            this.lblApplicationName.Name = "lblApplicationName";
            this.lblApplicationName.Size = new System.Drawing.Size(112, 27);
            this.lblApplicationName.TabIndex = 2;
            this.lblApplicationName.Text = "Application Name:";
            this.lblApplicationName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblErrorType
            // 
            this.lblErrorType.Location = new System.Drawing.Point(3, 116);
            this.lblErrorType.Name = "lblErrorType";
            this.lblErrorType.Size = new System.Drawing.Size(112, 27);
            this.lblErrorType.TabIndex = 3;
            this.lblErrorType.Text = "Error:";
            this.lblErrorType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblProcedureName
            // 
            this.lblProcedureName.Location = new System.Drawing.Point(3, 87);
            this.lblProcedureName.Name = "lblProcedureName";
            this.lblProcedureName.Size = new System.Drawing.Size(112, 26);
            this.lblProcedureName.TabIndex = 4;
            this.lblProcedureName.Text = "Procedure Name:";
            this.lblProcedureName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblClassName
            // 
            this.lblClassName.Location = new System.Drawing.Point(3, 58);
            this.lblClassName.Name = "lblClassName";
            this.lblClassName.Size = new System.Drawing.Size(112, 26);
            this.lblClassName.TabIndex = 5;
            this.lblClassName.Text = "Class Name:";
            this.lblClassName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblStackTrace
            // 
            this.lblStackTrace.Location = new System.Drawing.Point(3, 161);
            this.lblStackTrace.Name = "lblStackTrace";
            this.lblStackTrace.Size = new System.Drawing.Size(112, 27);
            this.lblStackTrace.TabIndex = 6;
            this.lblStackTrace.Text = "Stack Trace:";
            this.lblStackTrace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtApplicationName
            // 
            this.txtApplicationName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtApplicationName.Location = new System.Drawing.Point(123, 3);
            this.txtApplicationName.Name = "txtApplicationName";
            this.txtApplicationName.ReadOnly = true;
            this.txtApplicationName.Size = new System.Drawing.Size(371, 22);
            this.txtApplicationName.TabIndex = 7;
            // 
            // txtErrorMessage
            // 
            this.txtErrorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtErrorMessage.Location = new System.Drawing.Point(123, 119);
            this.txtErrorMessage.Multiline = true;
            this.txtErrorMessage.Name = "txtErrorMessage";
            this.txtErrorMessage.ReadOnly = true;
            this.txtErrorMessage.Size = new System.Drawing.Size(371, 39);
            this.txtErrorMessage.TabIndex = 8;
            // 
            // txtProcedureName
            // 
            this.txtProcedureName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtProcedureName.Location = new System.Drawing.Point(123, 90);
            this.txtProcedureName.Name = "txtProcedureName";
            this.txtProcedureName.ReadOnly = true;
            this.txtProcedureName.Size = new System.Drawing.Size(371, 22);
            this.txtProcedureName.TabIndex = 9;
            // 
            // txtStack
            // 
            this.txtStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStack.Location = new System.Drawing.Point(123, 164);
            this.txtStack.Multiline = true;
            this.txtStack.Name = "txtStack";
            this.txtStack.ReadOnly = true;
            this.txtStack.Size = new System.Drawing.Size(371, 39);
            this.txtStack.TabIndex = 10;
            // 
            // txtClassName
            // 
            this.txtClassName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtClassName.Location = new System.Drawing.Point(123, 61);
            this.txtClassName.Name = "txtClassName";
            this.txtClassName.ReadOnly = true;
            this.txtClassName.Size = new System.Drawing.Size(371, 22);
            this.txtClassName.TabIndex = 11;
            // 
            // grpErrorDetails
            // 
            this.grpErrorDetails.Controls.Add(this.pnlTableDetails);
            this.grpErrorDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpErrorDetails.Location = new System.Drawing.Point(5, 5);
            this.grpErrorDetails.Name = "grpErrorDetails";
            this.grpErrorDetails.Padding = new System.Windows.Forms.Padding(5);
            this.grpErrorDetails.Size = new System.Drawing.Size(507, 278);
            this.grpErrorDetails.TabIndex = 14;
            this.grpErrorDetails.TabStop = false;
            this.grpErrorDetails.Text = "Details";
            // 
            // pnlTableDetails
            // 
            this.pnlTableDetails.ColumnCount = 2;
            this.pnlTableDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
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
            this.pnlTableDetails.Location = new System.Drawing.Point(5, 20);
            this.pnlTableDetails.Name = "pnlTableDetails";
            this.pnlTableDetails.RowCount = 7;
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.pnlTableDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.pnlTableDetails.Size = new System.Drawing.Size(497, 253);
            this.pnlTableDetails.TabIndex = 18;
            // 
            // txtAdditionalInfo
            // 
            this.txtAdditionalInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAdditionalInfo.Location = new System.Drawing.Point(123, 209);
            this.txtAdditionalInfo.Multiline = true;
            this.txtAdditionalInfo.Name = "txtAdditionalInfo";
            this.txtAdditionalInfo.ReadOnly = true;
            this.txtAdditionalInfo.Size = new System.Drawing.Size(371, 41);
            this.txtAdditionalInfo.TabIndex = 12;
            // 
            // lblAdditional
            // 
            this.lblAdditional.Location = new System.Drawing.Point(3, 206);
            this.lblAdditional.Name = "lblAdditional";
            this.lblAdditional.Size = new System.Drawing.Size(112, 26);
            this.lblAdditional.TabIndex = 13;
            this.lblAdditional.Text = "Additional Info:";
            this.lblAdditional.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCulture
            // 
            this.txtCulture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCulture.Location = new System.Drawing.Point(123, 32);
            this.txtCulture.Name = "txtCulture";
            this.txtCulture.ReadOnly = true;
            this.txtCulture.Size = new System.Drawing.Size(371, 22);
            this.txtCulture.TabIndex = 16;
            // 
            // lblCulture
            // 
            this.lblCulture.Location = new System.Drawing.Point(3, 29);
            this.lblCulture.Name = "lblCulture";
            this.lblCulture.Size = new System.Drawing.Size(112, 26);
            this.lblCulture.TabIndex = 17;
            this.lblCulture.Text = "Culture:";
            this.lblCulture.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSorry
            // 
            this.lblSorry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSorry.Location = new System.Drawing.Point(3, 52);
            this.lblSorry.Name = "lblSorry";
            this.lblSorry.Padding = new System.Windows.Forms.Padding(5);
            this.lblSorry.Size = new System.Drawing.Size(517, 69);
            this.lblSorry.TabIndex = 16;
            this.lblSorry.Text = resources.GetString("lblSorry.Text");
            // 
            // lblSteps
            // 
            this.lblSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSteps.Location = new System.Drawing.Point(3, 121);
            this.lblSteps.Name = "lblSteps";
            this.lblSteps.Padding = new System.Windows.Forms.Padding(5);
            this.lblSteps.Size = new System.Drawing.Size(517, 30);
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
            this.btnSendError.Location = new System.Drawing.Point(4, 6);
            this.btnSendError.Name = "btnSendError";
            this.btnSendError.Size = new System.Drawing.Size(86, 26);
            this.btnSendError.TabIndex = 19;
            this.btnSendError.Text = "Send Error";
            this.btnSendError.Click += new System.EventHandler(this.btnSendError_Click);
            // 
            // btnDontSend
            // 
            this.btnDontSend.Location = new System.Drawing.Point(97, 6);
            this.btnDontSend.Name = "btnDontSend";
            this.btnDontSend.Size = new System.Drawing.Size(87, 26);
            this.btnDontSend.TabIndex = 20;
            this.btnDontSend.Text = "Don\'t Send";
            this.btnDontSend.Click += new System.EventHandler(this.btnDontSend_Click);
            // 
            // btnShowDetails
            // 
            this.btnShowDetails.Location = new System.Drawing.Point(137, 6);
            this.btnShowDetails.Name = "btnShowDetails";
            this.btnShowDetails.Size = new System.Drawing.Size(125, 26);
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
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 63F));
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
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
            this.pnlTitle.Size = new System.Drawing.Size(517, 46);
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
            this.pnlButtons.Location = new System.Drawing.Point(3, 217);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.RowCount = 1;
            this.pnlButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            this.pnlButtons.Size = new System.Drawing.Size(517, 52);
            this.pnlButtons.TabIndex = 24;
            // 
            // pnlSendDontSend
            // 
            this.pnlSendDontSend.Controls.Add(this.btnDontSend);
            this.pnlSendDontSend.Controls.Add(this.btnSendError);
            this.pnlSendDontSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlSendDontSend.Location = new System.Drawing.Point(324, 3);
            this.pnlSendDontSend.Name = "pnlSendDontSend";
            this.pnlSendDontSend.Size = new System.Drawing.Size(190, 52);
            this.pnlSendDontSend.TabIndex = 24;
            // 
            // pnlCopyToNotepad
            // 
            this.pnlCopyToNotepad.Controls.Add(this.btnNotepad);
            this.pnlCopyToNotepad.Controls.Add(this.btnShowDetails);
            this.pnlCopyToNotepad.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlCopyToNotepad.Location = new System.Drawing.Point(3, 3);
            this.pnlCopyToNotepad.Name = "pnlCopyToNotepad";
            this.pnlCopyToNotepad.Size = new System.Drawing.Size(252, 52);
            this.pnlCopyToNotepad.TabIndex = 25;
            // 
            // btnNotepad
            // 
            this.btnNotepad.Location = new System.Drawing.Point(4, 6);
            this.btnNotepad.Name = "btnNotepad";
            this.btnNotepad.Size = new System.Drawing.Size(126, 26);
            this.btnNotepad.TabIndex = 23;
            this.btnNotepad.Text = "Copy to Notepad";
            this.btnNotepad.UseVisualStyleBackColor = true;
            this.btnNotepad.Click += new System.EventHandler(this.btnNotepad_Click);
            // 
            // pnlErrorDetails
            // 
            this.pnlErrorDetails.Controls.Add(this.grpErrorDetails);
            this.pnlErrorDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlErrorDetails.Location = new System.Drawing.Point(3, 275);
            this.pnlErrorDetails.Name = "pnlErrorDetails";
            this.pnlErrorDetails.Padding = new System.Windows.Forms.Padding(5);
            this.pnlErrorDetails.Size = new System.Drawing.Size(517, 288);
            this.pnlErrorDetails.TabIndex = 25;
            // 
            // pnlSteps
            // 
            this.pnlSteps.Controls.Add(this.txtSteps);
            this.pnlSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSteps.Location = new System.Drawing.Point(3, 154);
            this.pnlSteps.Name = "pnlSteps";
            this.pnlSteps.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.pnlSteps.Size = new System.Drawing.Size(517, 57);
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
            this.txtSteps.Size = new System.Drawing.Size(497, 57);
            this.txtSteps.TabIndex = 0;
            this.txtSteps.Validating += new System.ComponentModel.CancelEventHandler(this.txtSteps_Validating);
            // 
            // ErrorDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
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
