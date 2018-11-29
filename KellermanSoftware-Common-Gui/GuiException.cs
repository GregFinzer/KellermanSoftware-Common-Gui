#region Using Statements

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

#endregion

namespace KellermanSoftware.Common.Gui
{
    /// <summary>
    /// Encapsulate exceptions in Winform applications
    /// </summary>
    public static class GuiException
    {
        #region Class Variables

        private static string _previousError = string.Empty;
        private static string _email = "noreply@kellermansoftware.com";
        private const string ScreenshotKey = "*KJ*&&^^%jkRgO0$!~[{8742nx|?JIL";

        #endregion

        #region Properties

        /// <summary>
        /// How often to delete temporary encrypted screen shots
        /// </summary>
        public static int TempFilePurgeMinutes { get; set; } = 10;

        /// <summary>
        /// The maximum number of errors before exiting the application
        /// </summary>
        public static int MaxErrorCount { get; set; } = 5;

        /// <summary>
        /// The support email to send errors to
        /// </summary>
        public static string SupportEmail { get; set; } = "support@kellermansoftware.com";

        /// <summary>
        /// Get/Set the general application name
        /// </summary>
        public static string ProgramName { get; set; } = "Unknown";

        /// <summary>
        /// The number of errors that have occurred in this session
        /// </summary>
        public static int ErrorCount { get; private set; }

        /// <summary>
        /// The name of the SMTP server used to send email
        /// </summary>
        public static string SmtpServer { get; set; } = string.Empty;

        /// <summary>
        /// The login name for the SMTP Server
        /// </summary>
        public static string Login { get; set; } = string.Empty;

        /// <summary>
        /// The password for the SMTP Server
        /// </summary>
        public static string Password { get; set; } = string.Empty;

        /// <summary>
        /// If true, a message box will be used instead of an Error Dialog Form
        /// </summary>
        public static bool MessageBoxErrorHandler { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load the email settings from the site config
        /// </summary>
        public static void LoadEmailSettings()
        {
            try
            {
                //Default to blank
                SmtpServer = string.Empty;
                Login = string.Empty;
                Password = string.Empty;
                _email = "noreply@kellermansoftware.com";
            }
            catch
            {
                //We don't care if it bombs, we will use mapi or mailto if there is no SMTP Server defined
            }
        }

        /// <summary>
        /// Display and log an exception using the Error Dialog
        /// </summary>
        /// <param name="ex">The exception to display</param>
        /// <returns>True if successfully logged and displayed</returns>
        public static bool HandleException(Exception ex)
        {
            return HandleException(ex, "n/a");
        }


        [Obsolete("Use HandleException(ex,additionalInfo)")]
        public static bool HandleException(Exception ex, string className, string procedureName)
        {
            return HandleException(ex, className, procedureName, "n/a");
        }

        [Obsolete("Use HandleException(ex,additionalInfo)")]
        public static bool HandleException(Exception ex, string className, string procedureName, string additionalInfo)
        {
            return HandleException(ex, additionalInfo);
        }

        /// <summary>
        /// Display an Error using the Error Form
        /// </summary>
        /// <param name="ex">Exception that occured</param>
        /// <param name="additionalInfo">Additional Information About The Error</param>
        /// <returns>True if successfully logged and displayed</returns>		
        public static bool HandleException(Exception ex, string additionalInfo)
        {
            string message = string.Empty;
            ErrorInformation errorInfo = new ErrorInformation();
            string currentError = string.Empty;
            bool success = true;

            try
            {
                //Exit when we are past the maximum number of errors
                if (ErrorCount >= MaxErrorCount)
                {
                    message = ProgramName + " has experienced a number of errors.\nPlease restart " + ProgramName + ".";
                    GuiUtility.MessageBoxCritical(message, "Restart " + ProgramName);
                    Application.Exit();
                }

                //Ignore duplicate errors
                currentError = ex.Message + ex.StackTrace;

                if (currentError == _previousError)
                {
                    success = false;
                    return success; //We don't care if the same error happens over and over again
                }

                ErrorCount++;
                _previousError = currentError;


                CallingInfo callingInfo = ReflectionUtil.GetCallingInfo();

                //Fill the error information object
                errorInfo.ProgramName = ProgramName;
                errorInfo.SupportEmail = SupportEmail;
                errorInfo.SmtpServer = SmtpServer;
                errorInfo.Login = Login;
                errorInfo.Password = Password;
                errorInfo.GUIException = ex;
                errorInfo.AssemblyName = callingInfo.AssemblyName;
                errorInfo.Version = callingInfo.AssemblyVersion;
                errorInfo.ClassName = callingInfo.ClassName;
                errorInfo.ProcedureName = callingInfo.MethodName;


                errorInfo.AdditionalInfo = additionalInfo;

                //Log the event log
                success &= SimpleLog.LogToEventLog(errorInfo.GetEventLogMessage(), EventLogEntryType.Error, ProgramName);

                //Get a screen shot
                errorInfo.ScreenShot = GuiUtility.ScreenShot();

                //Save the event log to a text file and return the path
                errorInfo.EventLogText = ReadFromEventLog();

                //Show the giant error handling dialog
                if (MessageBoxErrorHandler== false)
                {
                    ErrorDialog frmErrorDialog = new ErrorDialog(errorInfo);
                    frmErrorDialog.ShowDialog();
                }
                else //Generic message box
                {
                    GuiUtility.MessageBoxExclamation(errorInfo.GetMessageBoxMessage(), "Error");
                }

                return success;
            }
            catch (Exception exHandler) //Really bad, there is an error in the error handler
            {
                success = false;

                //Super simple error message so nothing else goes wrong
                message = "Error in Error Handler\n";
                message += exHandler.Message + "\n";
                message += exHandler.TargetSite + "\n";
                message += exHandler.StackTrace;

                GuiUtility.MessageBoxCritical(message, "Error in Error Handler");
                return success;
            }
        }

        
        #endregion

        #region Private Methods


        
        /// <summary>
        /// Read from the event log and save to a file
        /// </summary>
        /// <returns></returns>
        private static string ReadFromEventLog()
        {
            const string defaultEventLog = "Application";
            EventLog elogger = new EventLog();
            StreamWriter swOutput=null;
            string filePath = string.Empty;
            string tempPath = string.Empty;

            try
            {

                //Read from the program event log if it exists
                if (EventLog.SourceExists(ProgramName))
                {
                    elogger.Source = ProgramName;
                }
                else
                {
                    elogger.Source = defaultEventLog;
                }

                filePath = Path.GetTempFileName();
                tempPath = FileUtil.ExtractPath(filePath);
                filePath = Path.Combine(tempPath,Path.GetFileNameWithoutExtension(filePath) + ".txt");

                swOutput = new StreamWriter(filePath, true, Encoding.ASCII);

                foreach (EventLogEntry entry in elogger.Entries)
                {
                    swOutput.WriteLine(entry.Message.Replace("\n","\r\n"));
                    swOutput.WriteLine(StringUtil.CharString(80, "-"));
                }

                swOutput.Close();
            }
            catch
            {
                if (swOutput != null)
                {
                    swOutput.Close();
                }

                return string.Empty;
            }

            return filePath;
        }



        #endregion

    }
}
