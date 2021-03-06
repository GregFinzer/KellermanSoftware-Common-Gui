using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace KellermanSoftware.Common.Gui
{
    /// <summary>
    /// Utility methods for WinForm applications
    /// </summary>
    public static class GuiUtility
    {
        /// <summary>
        /// Used for Screen shots
        /// </summary>
        enum AnchorPosition
        {
            Top,
            Center,
            Bottom,
            Left,
            Right
        }

        #region Screen Positioning

        /// <summary>
        /// Loads the form location from a pipe delimited string (see SaveFormLocationToString)
        /// </summary>
        /// <param name="thisWindowGeometry"></param>
        /// <param name="formIn"></param>
        public static void LoadFormLocation(string thisWindowGeometry, Form formIn)
        {
            //Defaults if nothing is saved
            if (string.IsNullOrEmpty(thisWindowGeometry))
            {
                formIn.Left = 100;
                formIn.Top = 100;
                formIn.Width = Screen.PrimaryScreen.WorkingArea.Width / 2;
                formIn.Height = Screen.PrimaryScreen.WorkingArea.Height / 2;
                formIn.StartPosition = FormStartPosition.Manual;
                formIn.WindowState = FormWindowState.Normal;
                return;
            }

            string[] numbers = thisWindowGeometry.Split('|');
            string windowString = numbers[4];
            if (windowString == "Normal")
            {
                Point windowPoint = new Point(int.Parse(numbers[0]),
                    int.Parse(numbers[1]));
                Size windowSize = new Size(int.Parse(numbers[2]),
                    int.Parse(numbers[3]));

                bool locOkay = IsBizarreLocation(windowPoint, windowSize);
                bool sizeOkay = IsBizarreSize(windowSize);

                if (locOkay && sizeOkay)
                {
                    formIn.Location = windowPoint;
                    formIn.Size = windowSize;
                    formIn.StartPosition = FormStartPosition.Manual;
                    formIn.WindowState = FormWindowState.Normal;
                }
                else if (sizeOkay)
                {
                    formIn.Size = windowSize;
                }
            }
            else if (windowString == "Maximized")
            {
                formIn.Location = new Point(100, 100);
                formIn.StartPosition = FormStartPosition.Manual;
                formIn.WindowState = FormWindowState.Maximized;
            }
        }

        private static bool IsBizarreLocation(Point loc, Size size)
        {
            bool locOkay;
            if (loc.X < 0 || loc.Y < 0)
            {
                locOkay = false;
            }
            else if (loc.X + size.Width > Screen.PrimaryScreen.WorkingArea.Width)
            {
                locOkay = false;
            }
            else if (loc.Y + size.Height > Screen.PrimaryScreen.WorkingArea.Height)
            {
                locOkay = false;
            }
            else
            {
                locOkay = true;
            }
            return locOkay;
        }

        private static bool IsBizarreSize(Size size)
        {
            return (size.Height <= Screen.PrimaryScreen.WorkingArea.Height &&
                size.Width <= Screen.PrimaryScreen.WorkingArea.Width);
        }

        /// <summary>
        /// Saves the current form location to a string (See LoadFormLocation)
        /// </summary>
        /// <param name="mainForm"></param>
        /// <returns></returns>
        public static string SaveFormLocationToString(Form mainForm)
        {
            return mainForm.Location.X + "|" +
                mainForm.Location.Y + "|" +
                mainForm.Size.Width + "|" +
                mainForm.Size.Height + "|" +
                mainForm.WindowState;
        }

        #endregion

        /// <summary>
        /// Show an input box where the user can type text
        /// </summary>
        /// <param name="prompt">The prompt for the user</param>
        /// <param name="title">The title for the form</param>
        /// <param name="defaultResponse">The default response if no response is entered</param>
        /// <returns></returns>
        public static string InputBox(string prompt, string title, string defaultResponse)
        {
            int widthCenter = Screen.PrimaryScreen.WorkingArea.Width/2;
            int heightCenter = Screen.PrimaryScreen.WorkingArea.Height/2;

            return Interaction.InputBox(prompt, title, defaultResponse, widthCenter, heightCenter);
        }

        /// <summary>
        /// Put a list of strings in the list box
        /// </summary>
        /// <param name="list"></param>
        /// <param name="box"></param>
        public static void PutListInListBox(List<string> list, ListBox box)
        {
            box.Items.AddRange(list.ToArray());
        }

        /// <summary>
        /// Get a list of strings from a list box
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public static List<String> GetListFromListBox(ListBox box)
        {
            List<String> list = new List<string>();

            foreach (object o in box.Items)
            {
                list.Add(o.ToString());
            }

            return list;
        }

        /// <summary>
        /// Returns true if we are running in a virtual machine
        /// </summary>
        /// <returns></returns>
        public static bool RunningInAVirtualMachine()
        {
            string[] processNames ={ "vmsrvc", "vmwareservice", "vmwaretray", "vmwareuser", "vpcmap", "vmusrvc" };
            Process[] procs;

            try
            {
                //get a list of all processes running on the local machine
                procs = Process.GetProcesses();

                //See if VMWare or Virtual PC is already running
                foreach (Process proc in procs)
                {
                    //refreshing ensures we have the most current info about
                    //this process
                    proc.Refresh();

                    foreach (string item in processNames)
                    {
                        //the name of the process. this is usually exename.exe minus
                        //the .exe part
                        if (proc.ProcessName.ToLower().IndexOf(item) >= 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Write a message to a file and display it
        /// </summary>
        /// <param name="displayText">The text to display</param>        
        /// <returns>The file path</returns>
        public static string NotepadMessage(string displayText)
        {
            string filePath = Path.GetTempFileName();

            StreamWriter swOutput = new StreamWriter(filePath, true, Encoding.ASCII);
            swOutput.WriteLine(displayText);
            swOutput.Close();

            ProcessUtil.Shell("notepad.exe", filePath, ProcessWindowStyle.Normal, false);

            return filePath;
        }

        /// <summary>
        /// Set an error if a field is not filled in
        /// </summary>
        /// <param name="ep">Error Provider Control</param>
        /// <param name="txt">Text Box Control</param>
        /// <param name="description">The description to display, ie 'First Name'</param>
        /// <returns>True if filled in</returns>
        public static bool epRequired(ErrorProvider ep,
            TextBox txt,
            string description)
        {
            bool valid = true;

            if (txt.Text.Trim().Length == 0)
            {
                ep.SetError(txt, "Please fill in the " + description);
                valid = false;
            }
            else
            {
                ep.SetError(txt, "");
            }

            return valid;
        }

        /// <summary>
        /// Verify the SMTP Server is valid for the passed text box and set the error message if not
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="txt"></param>
        /// <param name="description"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        public static bool epValidSmtpServer(ErrorProvider ep,
            TextBox txt,
            string description,
            bool required)
        {
            string errorMessage = "";
            bool valid = true;

            if (required)
            {
                valid = epRequired(ep, txt, description);
            }

            if (valid && txt.Text.Length > 0)
            {
                if (Validation.ValidSmtpServer(txt.Text) == false)
                {
                    errorMessage = "Please enter a valid " + description;
                }

                if (errorMessage.Length > 0)
                {
                    ep.SetError(txt, errorMessage);
                    valid = false;
                }
            }

            if (valid)
            {
                ep.SetError(txt, "");
            }

            return valid;
        }

        /// <summary>
        /// Verify the E-Mail address is valid for the passed text box.  Set the error message.
        /// </summary>
        /// <param name="ep">Error Provider Control</param>
        /// <param name="txt">The Text Box</param>
        /// <param name="description">The description of the text box, ie 'Home E-Mail'</param>
        /// <param name="required">Is the E-Mail Address Required?</param>
        /// <returns>True if valid</returns>
        public static bool epValidEmail(ErrorProvider ep,
            TextBox txt,
            string description,
            bool required)
        {
            string errorMessage = "";
            bool valid = true;

            if (required)
            {
                valid = epRequired(ep, txt, description);
            }

            if (valid && txt.Text.Length > 0)
            {
                errorMessage = Validation.ValidEmail(txt.Text);

                if (errorMessage.Length > 0)
                {
                    ep.SetError(txt, errorMessage);
                    valid = false;
                }
            }

            if (valid)
            {
                ep.SetError(txt, "");
            }

            return valid;
        }

        /// <summary>
        /// Returns true if we have a connection to the internet
        /// </summary>
        /// <returns></returns>
        public static bool CheckInternetConnection(bool bQuiet)
        {
            return CheckInternetConnection("", bQuiet);
        }

        /// <summary>
        /// Returns true if we have a connection to the internet
        /// </summary>
        /// <returns></returns>
        public static bool CheckInternetConnection(string site, bool quiet)
        {
            try
            {
                if (site.Length == 0)
                {
                    site = "http://www.google.com";
                }

                WebRequest myRequest = WebRequest.Create(site);
                WebResponse myResponse = myRequest.GetResponse();
                myResponse.Close();
                return true;
            }
            catch
            {
                if (quiet == false)
                {
                    if (MessageBoxYesNo("Do you wish to connect to the internet?", "Connect?") == DialogResult.Yes)
                    {
                        ProcessUtil.Shell("rundll32.exe", "shell32.dll,Control_RunDLL ncpa.cpl,,0", ProcessWindowStyle.Normal, false);
                    }
                }

                //Still return false
                return false;
            }
        }

        /// <summary>
        /// Turn on/off hourglass cursor
        /// </summary>
        /// <param name="enable"></param>
        public static void Hourglass(bool enable)
        {
            if (enable)
            {
                Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();

            }
            else
            {
                Cursor.Current = Cursors.Default;
                Application.DoEvents();
            }
        }

        /// <summary>
        /// Display a message box with an critical icon
        /// </summary>
        /// <param name="prompt">Prompt message</param>
        /// <param name="title">Title to appear in the form</param>
        public static void MessageBoxCritical(string prompt, string title)
        {
            Hourglass(false);
            MessageBox.Show(prompt, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Display a message box with an exclamation icon
        /// </summary>
        /// <param name="sPrompt">Prompt message</param>
        /// <param name="sTitle">Title to appear in the form</param>
        public static void MessageBoxExclamation(string sPrompt, string sTitle)
        {
            Hourglass(false);
            MessageBox.Show(sPrompt, sTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Display a message box with an warning icon
        /// </summary>
        /// <param name="sPrompt">Prompt message</param>
        /// <param name="sTitle">Title to appear in the form</param>
        public static void MessageBoxWarning(string sPrompt, string sTitle)
        {
            Hourglass(false);
            MessageBox.Show(sPrompt, sTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Copy the text of a file to the clipboard
        /// </summary>
        /// <param name="filePath"></param>
        public static void CopyFileToClipboard(string filePath)
        {
            string fileText = File.ReadAllText(filePath);
            Clipboard.SetText(fileText);
        }

        /// <summary>
        /// Takes a screen shot into a temp file and returns the full file path
        /// </summary>
        /// <returns></returns>
        public static string ScreenShot()
        {
            string file = string.Empty;
            string tempPath;
            string sourceTempFile;

            try
            {
                SendKeys.SendWait("{PRTSC 2}");

                IDataObject data = Clipboard.GetDataObject();

                if (data != null && data.GetDataPresent(typeof(Bitmap)))
                {
                    Image img = (Bitmap)data.GetData(typeof(Bitmap));

                    sourceTempFile = Path.GetTempFileName();
                    tempPath = FileUtil.ExtractPath(sourceTempFile);

                    File.Delete(sourceTempFile);

                    sourceTempFile = Path.Combine(tempPath, Path.GetFileNameWithoutExtension(sourceTempFile) + ".jpg");

                    img.Save(sourceTempFile, ImageFormat.Jpeg);

                    file = sourceTempFile;
                }
            }
            catch
            {
            }

            return file;
        }

        /// <summary>
        /// Display a Yes/No Dialog with a question mark icon
        /// </summary>
        /// <param name="prompt">Prompt message</param>
        /// <param name="title">Title to appear in the form</param>
        /// <returns>Dialog Result</returns>
        public static DialogResult MessageBoxYesNo(string prompt, string title)
        {
            DialogResult result;
            Hourglass(false);
            result = MessageBox.Show(prompt, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            Application.DoEvents();
            return result;
        }

        /// <summary>
        /// Display a Yes/No Dialog with a question mark icon
        /// </summary>
        /// <param name="prompt">Prompt message</param>
        /// <param name="title">Title to appear in the form</param>
        /// <returns>Dialog Result</returns>
        public static DialogResult MessageBoxYesNoCancel(string prompt, string title)
        {
            DialogResult result;
            Hourglass(false);
            result = MessageBox.Show(prompt, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            Application.DoEvents();
            return result;
        }

        /// <summary>
        /// Display an okay box with the informational icon
        /// </summary>
        /// <param name="sPrompt">Prompt message</param>
        /// <param name="sTitle">Title to appear in the form</param>
        public static void MessageBoxOk(string sPrompt, string sTitle)
        {
            Hourglass(false);
            MessageBox.Show(sPrompt, sTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Get the items out of a combo box into a string list
        /// </summary>
        /// <param name="cbo"></param>
        /// <returns></returns>
        public static List<string> ComboToList(ComboBox cbo)
        {
            List<string> list =new List<string>();
            foreach (var cboItem in cbo.Items)
            {
                list.Add(cboItem.ToString());
            }

            return list;
        }

        /// <summary>
        /// Put the items from a string list into a combo box
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="list"></param>
        public static void ListToCombo(ComboBox cbo, List<string> list)
        {
            cbo.Items.Clear();
            cbo.Items.AddRange(list.ToArray());
        }
    }
}
