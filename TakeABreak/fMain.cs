using System;
using System.Windows.Forms;

namespace TakeABreak
{
    public partial class fMain : Form
    {
        public const int SECOND = 59;
        private static string AppName = "TakeABreak";
        public int[] timerArr = new int[]
        {
            /*1, 2, */15, 20, 25, 30
        };

        private int _End = 0;
        private int minutes = 0, second = 0;
        private int TIMER = 0;

        private KeyboardHook hook = new KeyboardHook();

        public fMain()
        {
            InitializeComponent();
            tsbActive.ToggleChanged += TsbActive_ToggleChanged;
            
            timerMain.Interval = 1000;
            StartWithOS(false);

            // register the event that is fired after the key press
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);

            // register the Control + Alt + K combination as hot key
            hook.RegisterHotKey(TakeABreak.ModifierKeys.Control | TakeABreak.ModifierKeys.Alt, Keys.K);

        }


        private void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            // Không set phủ định (!) vì chỉ để kích hoạt Event ToggleChanged
            tsbActive.Toggled = tsbActive.Toggled;
        }

        #region Events
        private void fMain_Load(object sender, EventArgs e)
        {
            LoadItem2_ToolStripMenu();
            timerMain.Tick += TimerMain_Tick;
            LoadComboBox();

            Minimize2Tray();
        }

        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Minimize2Tray();
            e.Cancel = true;
        }
        
        private void TsbActive_ToggleChanged(object sender, EventArgs e)
        {
            Handler(tsbActive.Toggled);
            pnlComboBox.Visible = !tsbActive.Toggled;
        }

        private void TimerMain_Tick(object sender, EventArgs e)
        {
            double _now = (DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second);
            if (_now >= _End)
            {
                tsbActive.Toggled = timerMain.Enabled = false;
                MessageBox.Show("It is time to REST", "TAKE A BREAK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                StopTimer();
                RunSleepCommand();
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
            ComboBox cb = sender as ComboBox;
            TIMER = timerArr[cb.SelectedIndex];
            lblTimer.Text = timerArr[cb.SelectedIndex] + ":00";
        }

        //private void cbActive_CheckedChanged(object sender, EventArgs e)
        //{
        //    CheckBox cb = sender as CheckBox;
        //    Handler(cb.Checked);
        //}       

        private void openWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            this.ShowInTaskbar = true;
            this.Opacity = 100;

            this.ShowIcon = true;
        }

        private void activeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            //switch (tsmi.Text)
            //{
            //    case "Active":
            //        tsbActive.Toggled = true;
            //        break;
            //    case "Deactive":
            //        tsbActive.Toggled = false;
            //        break;
            //}
            tsbActive.Toggled = !tsbActive.Toggled;
        }

        private void quitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void cmnsTrayItemSelect(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            int selectedValue = Convert.ToInt32(tsmi.Tag);

            cbTimerSelect.SelectedIndex =  Array.IndexOf(timerArr, selectedValue);
            TIMER = selectedValue;
        }
        #endregion


        #region Method
        private void LoadComboBox()
        {
            string str = "";
            foreach (int item in timerArr)
            {
                str = item + " minutes";
                cbTimerSelect.Items.Add(str);
            }

            cbTimerSelect.SelectedIndex = 0;
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
            DateTime _now = DateTime.Now;
            if (status)
            {
                StartTimer(_now);
            }
            else
            {
                StopTimer();
            }

            cmnsTray.Items[1].Text = !status ? "Ative" : "Deactive";
        }

        private void StartTimer(DateTime _now)
        {
            _End = (_now.Hour * 3600 + _now.Minute * 60 + _now.Second) + TIMER * 60;
            //lblTimer.Text = "Remaining: " + TIMER + "m";
            minutes = TIMER;
            timerToolStripMenuItem.Enabled = false;
        }

        private void StopTimer()
        {
            timerToolStripMenuItem.Enabled = true;
            second = SECOND;
            //lblTimer.Text = "Remaining: ";
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

        #region Registry that open with window
        static void StartWithOS(bool enable)
        {
            /*
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey("Software\\RestApplication");
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
            }
            */
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
        

        private void LabelText()
        {
            lblTimer.Text = /*"Remaining: " +*/ minutes.ToString("00") + ":" + second.ToString("00");
        }
        #endregion
    }
}
