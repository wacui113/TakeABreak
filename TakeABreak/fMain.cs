using System;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TakeABreak
{
    public partial class fMain : Form
    {
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbfont, uint cbfont, IntPtr pdv, [In] ref uint pcFonts);

        FontFamily ff;
        Font font;

        private void loadFont()
        {
            byte[] fontArray = TakeABreak.Properties.Resources.digital_7__mono_italic_;
            int dataLength = TakeABreak.Properties.Resources.digital_7__mono_italic_.Length;

            IntPtr ptrData = Marshal.AllocCoTaskMem(dataLength);

            Marshal.Copy(fontArray, 0, ptrData, dataLength);

            uint cFonts = 0;

            AddFontMemResourceEx(ptrData, (uint)fontArray.Length, IntPtr.Zero, ref cFonts);

            PrivateFontCollection pfc = new PrivateFontCollection();

            pfc.AddMemoryFont(ptrData, dataLength);

            Marshal.FreeCoTaskMem(ptrData);

            ff = pfc.Families[0];
            font = new Font(ff, 15f, FontStyle.Italic);
        }

        private void AllocFont(Font f, Control c, float size)
        {
            FontStyle fontStyle = FontStyle.Italic;

            c.Font = new Font(ff, size, fontStyle);
        }

        public const byte SECOND = 59;
        private static string AppName = "TakeABreak";
        private byte[] timerArr = new byte[]
        {
            /*1, 2,*/ 15, 20, 25, 30
        };

        private System.Drawing.Image img = System.Drawing.Image.FromFile("tick.png");
        
        private int selected = 0;
        private byte minutes = 0;
        private int second = 0;

        private KeyboardHook hook = new KeyboardHook();

        public fMain()
        {
            InitializeComponent();

            tsbActive.ToggleChanged += TsbActive_ToggleChanged;
            timerMain.Tick += TimerMain_Tick;

            timerMain.Interval = 1000;
            StartWithOS(false);

            // register the event that is fired after the key press
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);

            // register the Control + Alt + K combination as hot key
            hook.RegisterHotKey(TakeABreak.ModifierKeys.Control | TakeABreak.ModifierKeys.Alt, Keys.K);

            
        }


        #region Events
        private void fMain_Load(object sender, EventArgs e)
        {
            LoadItem2_ToolStripMenu();
            
            LoadComboBox();

            loadFont();
            AllocFont(font, this.lblTimer, 68);

            Minimize2Tray();
        }

        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Minimize2Tray();
            e.Cancel = true;
        }
        
        private void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            tsbActive.Toggled = !tsbActive.Toggled;
        }

        private void TsbActive_ToggleChanged(object sender, EventArgs e)
        {
            Handler(tsbActive.Toggled);
            pnlComboBox.Visible = !tsbActive.Toggled;
        }

        private void TimerMain_Tick(object sender, EventArgs e)
        {
            if(minutes < 0)
            { 
                if(second <= 0)
                {
                    tsbActive.Toggled = timerMain.Enabled = false;
                    StopTimer();
                    MessageBox.Show("It is time to REST", "TAKE A BREAK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    RunSleepCommand();
                }
            }
            if (second <= 0)
            { second = SECOND; minutes--; }
            else { second -= timerMain.Interval / 1000;  }
            
            LabelText();
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            Minimize2Tray();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            quitToolStripMenuItem1_Click(sender, e);
        }

        private void cbTimerSelect_SelectedIndexChanged(object sender, EventArgs e)
        {  
            lblTimer.Text = (timerArr[cbTimerSelect.SelectedIndex] - 1).ToString("00") + ":59";
            timerToolStripMenuItem.DropDownItems[selected].Image = null;
            timerToolStripMenuItem.DropDownItems[cbTimerSelect.SelectedIndex].Image = img;
            selected = cbTimerSelect.SelectedIndex;
        }      

        private void openWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.ShowInTaskbar = true;
            this.Opacity = 100;

            this.ShowIcon = true;
        }

        private void activeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tsbActive.Toggled = !tsbActive.Toggled;
        }

        private void quitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void cmnsTrayItemSelect(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;

            timerToolStripMenuItem.DropDownItems[selected].Image = null;
            cbTimerSelect.SelectedIndex =  Array.IndexOf(timerArr, Convert.ToByte(tsmi.Tag));
        }
        #endregion


        #region Method

        #region Registry that open with window
        static void StartWithOS(bool enable)
        {
            /*RegistryKey regkey = Registry.CurrentUser.CreateSubKey("Software\\RestApplication");
            RegistryKey regstart = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            string keyvalue = "1";
            try
            {
                regkey.SetValue("Index", keyvalue);
                regstart.SetValue("RestApplication", System.Reflection.Assembly.GetExecutingAssembly().Location);
                regkey.Close();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }*/
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            Microsoft.Win32.RegistryKey startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey);

            if (enable)
            {
                if (startupKey.GetValue(AppName) == null)
                {
                    startupKey.Close();
                    startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
                    // Add startup reg key
                    startupKey.SetValue(AppName, Application.ExecutablePath.ToString());
                    startupKey.Close();
                }
            }
            else
            {
                // remove startup
                startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
                startupKey.DeleteValue(AppName, false);
                startupKey.Close();
            }

        }
        #endregion

        private void LoadComboBox()
        {
            string str = "";
            foreach (int item in timerArr)
            {
                str = item + " minutes";
                cbTimerSelect.Items.Add(str);
            }

            selected = cbTimerSelect.SelectedIndex = 0;
            timerToolStripMenuItem.DropDownItems[selected].Image = img;
        }

        private void LoadItem2_ToolStripMenu()
        {
            string str = "";
            for(int i = 0; i < timerArr.Length; i++)
            {
                str = timerArr[i] + " minutes";
                timerToolStripMenuItem.DropDownItems.Add(str, null, new EventHandler(cmnsTrayItemSelect));
                timerToolStripMenuItem.DropDownItems[i].Tag = timerArr[i].ToString();
            }
        }

        private void Handler(bool status)
        {
            timerMain.Enabled = status;
            if (status)
            {
                StartTimer();
            }
            else
            {
                StopTimer();
            }

            cmnsTray.Items[1].Text = !status ? "Ative" : "Deactive";
        }

        private void StartTimer()
        {
            minutes = (byte)(timerArr[cbTimerSelect.SelectedIndex] - 1);
            second = SECOND;
            timerToolStripMenuItem.Enabled = false;
        }
        private void StopTimer()
        {
            timerToolStripMenuItem.Enabled = true;
            second = SECOND;
            lblTimer.Text = (timerArr[cbTimerSelect.SelectedIndex] - 1).ToString("00") + ":59";
        }

        private void Minimize2Tray()
        {
            niTray.Visible = true;
            niTray.ShowBalloonTip(500, "Message", "Application Take A Rest was be minimize to here!",
                ToolTipIcon.Info);
            this.Hide();
        }

        // Sleep Command
        private void RunSleepCommand()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C rundll32.exe powrprof.dll,SetSuspendState 0,1,0";
            process.StartInfo = startInfo;
            process.Start();
        }       

        private void LabelText()
        {
            lblTimer.Text = minutes.ToString("00") + ":" + second.ToString("00");
        }
        #endregion
    }
}
