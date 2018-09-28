#region Using Statements
using System;
using System.Diagnostics;
using System.Windows.Forms;
using KellermanSoftware.Common;
#endregion

namespace KellermanSoftware.Common.Gui
{
    public static class GuiException
    {
        #region Class Variables
        private static bool messageBoxErrorHandler = false;
        private static int errorCount = 0;
        private static int maxErrorCount=5;
        private static int tempFilePurgeMinutes = 10;
        private static string programName = "Unknown";
        private static string previousError = string.Empty;
        private static string supportEmail = "support@kellermansoftware.com";
        private static string smtpServer = string.Empty;
        private static string email = "noreply@kellermansoftware.com";    
        private static string login = string.Empty;
        private static string password = string.Empty;
        private const string screenshotKey = "*KJ*&&^^%jkRgO0$!~[{8742nx|?JIL";

        #endregion

        #region Properties

        /// <summary>
        /// How often to delete temporary encrypted screen shots
        /// </summary>
        public static int TempFilePurgeMinutes
        {
            get
            {
                return tempFilePurgeMinutes;
            }

            set
            {
                tempFilePurgeMinutes = value;
            }
        }

        /// <summary>
        /// The maximum number of errors before exiting the application
        /// </summary>
        public static int MaxErrorCount
        {
            get
            {
                return maxErrorCount;
            }

            set
            {
                maxErrorCount = value;
            }
        }

        /// <summary>
        /// The support email to send errors to
        /// </summary>
        public static string SupportEmail
        {
            get
            {
                return supportEmail;
            }

            set
            {
                supportEmail = value;
            }
        }

        /// <summary>
        /// Get/Set the general application name
        /// </summary>
        public static string ProgramName
        {
            get
            {
                return programName;
            }

            set
            {
                programName = value;
            }
        }

        /// <summary>
        /// The number of errors that have occurred in this session
        /// </summary>
        public static int ErrorCount
        {
            get
            {
                return errorCount;
            }
        }

        /// <summary>
        /// The name of the SMTP server used to send email
        /// </summary>
        public static string SmtpServer
        {
            get
            {
                return smtpServer;
            }
            set
            {
                smtpServer = value;
            }
        }

        /// <summary>
        /// The from email address of the sender
        /// </summary>
        public static string Email
        {
            get
            {
                return email;
            }
            set
            {
                email = value;
            }
        }

        /// <summary>
        /// The login name for the SMTP Server
        /// </summary>
        public static string Login
        {
            get
            {
                return login;
            }
            set
            {
                login = value;
            }
        }

        /// <summary>
        /// The password for the SMTP Server
        /// </summary>
        public static string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        /// <summary>
        /// If true, a message box will be used instead of an Error Dialog Form
        /// </summary>
        public static bool MessageBoxErrorHandler
        {
            get
            {
                return messageBoxErrorHandler;
            }

            set
            {
                messageBoxErrorHandler = value;
            }
        }



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
                smtpServer = string.Empty;
                login = string.Empty;
                password = string.Empty;
                email = "noreply@kellermansoftware.com";
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
                if (errorCount >= maxErrorCount)
                {
                    message = programName + " has experienced a number of errors.\nPlease restart " + programName + ".";
                    GuiUtility.MessageBoxCritical(message, "Restart " + programName);
                    Application.Exit();
                }

                //Ignore duplicate errors
                currentError = ex.Message + ex.StackTrace;

                if (currentError == previousError)
                {
                    success = false;
                    return success; //We don't care if the same error happens over and over again
                }
                else
                {
                    errorCount++;
                    previousError = currentError;
                }


                CallingInfo callingInfo = ReflectionUtil.GetCallingInfo();

                //Fill the error information object
                errorInfo.ProgramName = programName;
                errorInfo.SupportEmail = supportEmail;
                errorInfo.SmtpServer = smtpServer;
                errorInfo.Login = login;
                errorInfo.Password = password;
                errorInfo.GUIException = ex;
                errorInfo.AssemblyName = callingInfo.AssemblyName;
                errorInfo.Version = callingInfo.AssemblyVersion;
                errorInfo.ClassName = callingInfo.ClassName;
                errorInfo.ProcedureName = callingInfo.MethodName;


                errorInfo.AdditionalInfo = additionalInfo;

                //Log the event log
                success &= SimpleLog.LogToEventLog(errorInfo.GetEventLogMessage(), System.Diagnostics.EventLogEntryType.Error, programName);

                //Get an encrypted, compressed screen shot
                errorInfo.ScreenShot = GetEncryptedCompressedScreenShot();

                //Save the event log to a text file and return the path
                errorInfo.EventLogText = ReadFromEventLog();

                //Show the giant error handling dialog
                if (messageBoxErrorHandler== false)
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
            System.IO.StreamWriter swOutput=null;
            string filePath = string.Empty;
            string tempPath = string.Empty;

            try
            {

                //Read from the program event log if it exists
                if (EventLog.SourceExists(programName))
                {
                    elogger.Source = programName;
                }
                else
                {
                    elogger.Source = defaultEventLog;
                }

                filePath = System.IO.Path.GetTempFileName();
                tempPath = FileUtil.ExtractPath(filePath);
                filePath = System.IO.Path.Combine(tempPath,System.IO.Path.GetFileNameWithoutExtension(filePath) + ".txt");

                swOutput = new System.IO.StreamWriter(filePath, true, System.Text.Encoding.ASCII);

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

        /// <summary>
        /// Capture a screen shot then encrypt and compress
        /// </summary>
        /// <returns></returns>
        private static string GetEncryptedCompressedScreenShot()
        {
            string screenShot= string.Empty;
            string compressedFile = string.Empty;
            string encryptedFile = string.Empty;
            string tempPath = string.Empty;

            //Encryption myEncryption = new Encryption(Encryption.SymmProvEnum.Rijndael);

            try
            {
                //return Utility.ScreenShot();

                return GuiUtility.ScreenShot();
                ////Get the screen shot
                //screenShot = GuiUtility.ScreenShot();

                ////Encrypt the file with Rijndael
                //encryptedFile = System.IO.Path.GetTempFileName();

                //if (System.IO.File.Exists(encryptedFile))
                //{
                //    System.IO.File.Delete(encryptedFile);
                //}

                //tempPath = u.ExtractPath(encryptedFile);
                //encryptedFile = System.IO.Path.Combine(tempPath,System.IO.Path.GetFileNameWithoutExtension(encryptedFile) + ".jpg");

                //myEncryption.EncryptFile(screenShot, encryptedFile, screenshotKey);

                ////GZip compression makes the file larger
                //////Compress the file with GZip
                ////compressedFile = System.IO.Path.GetTempFileName();

                ////if (System.IO.File.Exists(compressedFile))
                ////{
                ////    System.IO.File.Delete(compressedFile);
                ////}

                ////Utility.GZipCompressFile(encryptedFile, compressedFile);

                ////Cleanup unencrypted temp files
                //System.IO.File.Delete(screenShot);                

                //return encryptedFile;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

    }
}
