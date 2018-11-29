#region Using Statements

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

#endregion

namespace KellermanSoftware.Common.Gui
{
    /// <summary>
    /// Holds error information and combines error information for specific destinations
    /// </summary>
    public class ErrorInformation
    {
        #region Class Variables

        private const string MessageBoxHeader = "An error has occurred\n";
        private const string EmailHeader = "Technical Support,\n\nPlease see the error below.\n\n";
        private string _notepadHeader = string.Empty;
        private string _mailtoHeader = string.Empty;
        private const string CannotRetrieve = "Cannot retrieve";

        /// <summary>
        /// Public constructor
        /// </summary>
        public ErrorInformation()
        {
            Steps = string.Empty;
            EventLogText = string.Empty;
            ScreenShot = string.Empty;
            AdditionalInfo = string.Empty;
            ClassName = string.Empty;
            Version = string.Empty;
            SupportEmail = string.Empty;
            ProgramName = string.Empty;
            GUIException = null;
            Password = string.Empty;
            Login = string.Empty;
            Email = string.Empty;
            SmtpServer = string.Empty;
            AssemblyName = string.Empty;
            ProcedureName = string.Empty;
        }

       
        #endregion

        #region Properties

        /// <summary>
        /// The Email Server
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// The from email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The user name for logging into email
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// The password for logging into email
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Get the current culture
        /// </summary>
        public string Culture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.EnglishName;
            }
        }

        /// <summary>
        /// Get the identity of the current user
        /// </summary>
        public string UserName
        {
            get
            {
                try
                {
                    return SystemUtil.GetUserName();
                }
                catch
                {
                    return Environment.UserName;
                }
            }
        }

        /// <summary>
        /// Get the machine name
        /// </summary>
        public string ComputerName
        {
            get
            {
                return Environment.MachineName;
            }
        }

        /// <summary>
        /// Get the current version of the .NET Framework
        /// </summary>
        public string GetFrameworkVersion
        {
            get
            {
                return Environment.Version.ToString();
            }
        }

        /// <summary>
        /// The current GUI Exception
        /// </summary>
        public Exception GUIException { get; set; }

        /// <summary>
        /// The name of the program for the exception
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// The support email for the exception
        /// </summary>
        public string SupportEmail { get; set; }

        /// <summary>
        /// The version of the assembly that called the error handler
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The name of the assembly that called the error handler
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// The version of the assembly
        /// </summary>
        public string AssemblyVersion { get; set; }

        /// <summary>
        /// The class name of what caused the exception
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// The procedure name that caused the exception
        /// </summary>
        public string ProcedureName { get; set; }

        

        /// <summary>
        /// Additional Information to email to support
        /// </summary>
        public string AdditionalInfo { get; set; }

        /// <summary>
        /// Path of the screen shot 
        /// </summary>
        public string ScreenShot { get; set; }

        /// <summary>
        /// Path of the text of the event log
        /// </summary>
        public string EventLogText { get; set; }

        /// <summary>
        /// Steps to reproduce entered by the end user
        /// </summary>
        public string Steps { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Build an error message for manual email, event log, and simple message boxes
        /// </summary>
        /// <returns></returns>
        private string BuildErrorMessage()
        {
            StringBuilder sb = new StringBuilder(4096);

            sb.Append("Program Name:  ");
            sb.Append(ProgramName);
            sb.Append("\n");

            sb.Append("Assembly Name:  ");
            sb.Append(AssemblyName);
            sb.Append("\n");

            sb.Append("Class:  ");
            sb.Append(ClassName);
            sb.Append("\n");

            sb.Append("Procedure:  ");
            sb.Append(ProcedureName);
            sb.Append("\n");

            sb.Append("Version:  ");
            sb.Append(Version);
            sb.Append("\n");

            sb.Append("Error Message:  ");
            sb.Append(GUIException.Message);
            sb.Append("\n");

            sb.Append("Stack Trace:  ");
            sb.Append(GUIException.StackTrace);
            sb.Append("\n");

            sb.Append("Additional Info:  ");
            sb.Append(AdditionalInfo);
            sb.Append("\n");

            sb.Append("Steps:  ");
            sb.Append(Steps);
            sb.Append("\n");

            sb.Append("Source:  ");
            sb.Append(GUIException.Source);
            sb.Append("\n");

            sb.Append("Target:  ");
            sb.Append(GUIException.TargetSite);
            sb.Append("\n");

            sb.Append("Exception Type:  ");
            sb.Append(GUIException.GetType());
            sb.Append("\n");

            sb.Append("Error Date:  ");
            sb.Append(FormatUtil.FormatUSDateTime(DateTime.Now));
            sb.Append("  (");
            sb.Append(SystemUtil.GetCurrentTimeZone());
            sb.Append(")\n");

            return sb.ToString();
        }

        /// <summary>
        /// Get System Information for manual email, event log, and simple message boxes
        /// </summary>
        /// <returns></returns>
        private string GetSystemInformation()
        {
            StringBuilder sb = new StringBuilder(4096);

            sb.Append("User Name:  ");
            sb.Append(UserName);
            sb.Append("\n");

            sb.Append("PC Name:  ");
            sb.Append(Environment.MachineName);
            sb.Append("\n");

            sb.Append("Culture:  ");
            sb.Append(Culture);
            sb.Append("\n");

            sb.Append("OS:  ");
            sb.Append(SystemInfo.GetOSVersion());
            sb.Append("\n");

            sb.Append("CPU:  ");
            sb.Append(SystemInfo.GetCPUInfo());
            sb.Append("\n");

            sb.Append("RAM:  ");
            sb.Append(SystemInfo.GetTotalRAM());
            sb.Append("\n");

            sb.Append("HD:  ");
            sb.Append(SystemInfo.GetFreeSpace(Path.GetPathRoot(Environment.CurrentDirectory)));
            sb.Append("\n");

            sb.Append("Framework:  ");
            sb.Append(Environment.Version);
            sb.Append("\n");

            sb.Append("System Directory:  ");
            sb.Append(Environment.SystemDirectory);
            sb.Append("\n");

            sb.Append("Current Directory:  ");
            sb.Append(FileUtil.GetCurrentDirectory());
            sb.Append("\n");

            return sb.ToString();
        }

        /// <summary>
        /// Get an XML Message to send in the email
        /// </summary>
        /// <returns></returns>
        public string GetXMLMessage()
        {
            //Write XML file to a memory stream
            MemoryStream memStream = new MemoryStream();
            XmlTextWriter textWriter = new XmlTextWriter(memStream, Encoding.ASCII);
            textWriter.Formatting = Formatting.Indented;
                      
            // Opens the document 
            textWriter.WriteStartDocument();

            // Write first element
            textWriter.WriteStartElement("Error");

            //Error XML Version
            textWriter.WriteStartElement("ErrorXMLVersion", "");
            textWriter.WriteString("1.0");
            textWriter.WriteEndElement();

            //Error Information
            textWriter.WriteStartElement("ProgramName", "");
            textWriter.WriteString(ProgramName);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("AssemblyName", "");
            textWriter.WriteString(AssemblyName);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("Class", "");
            textWriter.WriteString(ClassName);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("Procedure", "");
            textWriter.WriteString(ProcedureName);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("Version", "");
            textWriter.WriteString(Version);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("ErrorMessage", "");
            textWriter.WriteString(GUIException.Message);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("StackTrace", "");
            textWriter.WriteString(GUIException.StackTrace);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("AdditionalInfo", "");
            textWriter.WriteString(AdditionalInfo);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("Source", "");
            textWriter.WriteString(GUIException.Source);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("Target", "");
            textWriter.WriteString(GUIException.TargetSite.ToString());
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("ExceptionType", "");
            textWriter.WriteString(GUIException.GetType().ToString());
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("ErrorDate", "");
            textWriter.WriteString(FormatUtil.FormatUSDateTime(DateTime.Now) + "  (" + SystemUtil.GetCurrentTimeZone() + ")");
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("Steps", "");
            textWriter.WriteString(Steps);
            textWriter.WriteEndElement();

            //System Information
            textWriter.WriteStartElement("UserName", "");
            textWriter.WriteString(UserName);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("PCName", "");
            textWriter.WriteString(Environment.MachineName);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("Culture", "");
            textWriter.WriteString(Culture);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("OS", "");
            textWriter.WriteString(SystemInfo.GetOSVersion());
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("CPU", "");
            textWriter.WriteString(SystemInfo.GetCPUInfo());
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("RAM", "");
            textWriter.WriteString(SystemInfo.GetTotalRAM());
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("HD", "");
            textWriter.WriteString(SystemInfo.GetFreeSpace(Path.GetPathRoot(Environment.CurrentDirectory)));
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("Framework", "");
            textWriter.WriteString(Environment.Version.ToString());
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("SystemDirectory", "");
            textWriter.WriteString(Environment.SystemDirectory);
            textWriter.WriteEndElement();

            textWriter.WriteStartElement("CurrentDirectory", "");
            textWriter.WriteString(FileUtil.GetCurrentDirectory());
            textWriter.WriteEndElement();

            //Write Final element
            textWriter.WriteEndElement();

            // Ends the document.
            textWriter.WriteEndDocument();

            // close writer
            textWriter.Close();

            return Encoding.ASCII.GetString(memStream.ToArray());
        }


        /// <summary>
        /// Get only the error information for the message box
        /// </summary>
        /// <returns></returns>
        public string GetMessageBoxMessage()
        {
            StringBuilder sb = new StringBuilder(4096);

            sb.Append(MessageBoxHeader);
            sb.Append(BuildErrorMessage());
            return sb.ToString();
        }

        /// <summary>
        /// Get email message to be sent manually when there is no internet connection
        /// </summary>
        /// <returns></returns>
        public string GetManualEmailMessage()
        {
            const int maxHTMLGetOperationCharacters = 2083;

            StringBuilder sbBody = new StringBuilder(8112);
            string message=string.Empty;

            BuildHeaders();

            sbBody.Append(EmailHeader);
            sbBody.Append(BuildErrorMessage());
            sbBody.Append(StringUtil.CharString(40, "-"));
            sbBody.Append(Environment.NewLine);
            sbBody.Append(GetSystemInformation());

            message = StringUtil.Left(_mailtoHeader + StringUtil.URLEscape(sbBody.ToString().Replace("\n", "\r\n")), maxHTMLGetOperationCharacters);
            return message;
        }

        /// <summary>
        /// Get a message formatted for the event log
        /// </summary>
        /// <returns></returns>
        public string GetEventLogMessage()
        {
            StringBuilder sb = new StringBuilder(8112);

            sb.Append(BuildErrorMessage());
            sb.Append(StringUtil.CharString(40, "-"));
            sb.Append("\n");
            sb.Append(GetSystemInformation());

            return sb.ToString();
        }

        /// <summary>
        /// Get a message formatted for notepad
        /// </summary>
        /// <returns></returns>
        public string GetNotepadMessage()
        {
            StringBuilder sb = new StringBuilder(8112);

            BuildHeaders();

            sb.Append(_notepadHeader);
            sb.Append(BuildErrorMessage());
            sb.Append(StringUtil.CharString(40, "-"));
            sb.Append("\n");
            sb.Append(GetSystemInformation());


            return sb.ToString().Replace("\n","\r\n");
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// Build the headers for the message types
        /// </summary>
        private void BuildHeaders()
        {
            _notepadHeader = "Technical Support Email: " + SupportEmail + "\n\n" + EmailHeader;
            _mailtoHeader = "mailto:" + SupportEmail + "?Subject=" + ProgramName + "%20Error&body=";
        }

        #endregion
    }
}
