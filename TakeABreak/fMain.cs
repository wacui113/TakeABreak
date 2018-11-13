using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TakeABreak
{
    public partial class fMain : Form
    {
        public const int SECOND = 60;

        public int[] timerArr = new int[]
        {
            1, 2, 15, 20, 25, 30
        };

        private int _End = 0;
        private int minutes = 0, second = 0;
        private int TIMER = 0;

        public fMain()
        {
            InitializeComponent();
            timerMain.Interval = 30000;
            //StartWithOS();
        }

        #region Events
        private void fMain_Load(object sender, EventArgs e)
        {
            Minimize2Tray();
            LoadItem2_ToolStripMenu();
            timerMain.Tick += TimerMain_Tick;
            LoadComboBox();
        }

        private void fMain_Shown(object sender, EventArgs e)
        {
            Minimize2Tray();
        }

        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Minimize2Tray();
            e.Cancel = true;
        }

        private void TimerMain_Tick(object sender, EventArgs e)
        {
            double _now = (DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second);
            if (_now >= _End)
            {
                //timerMain.Stop();
                cbActive.Checked = timerMain.Enabled = false;
                MessageBox.Show("It is time to REST", "TAKE A BREAK", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RunSleepCommand();
            }
            if (second <= 0)
            { minutes--; second = SECOND; }
            else second -= timerMain.Interval/1000; //second -= 10;

            LabelText();
        }
        private void btnHide_Click(object sender, EventArgs e)
        {
            fMain_Shown(sender, e);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            quitToolStripMenuItem1_Click(sender, e);
        }
        private void cbTimerSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            TIMER = timerArr[cb.SelectedIndex];
        }

        private void cbActive_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            Handler(cb.Checked);
        }       

        private void openWindowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void activeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            switch (tsmi.Text)
            {
                case "Active":
                    cbActive.Checked = true;
                    break;
                case "Deactive":
                    cbActive.Checked = false;
                    break;
            }
        }

        private void quitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //this.Close();
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
                //timerMain.Start();

                _End = (_now.Hour * 3600 + _now.Minute * 60 + _now.Second) + TIMER * 60;
                lblTimer.Text = "Remaining: " + TIMER + "m";
                minutes = TIMER--;
                timerToolStripMenuItem.Enabled = false;
            }
            else
            {
                //timerMain.Stop();
                timerToolStripMenuItem.Enabled = true;
                second = SECOND;
                lblTimer.Text = "Remaining: ";
            }

            cmnsTray.Items[1].Text = !status ? "Ative" : "Deactive";
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
        static void StartWithOS()
        {
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
        }
        #endregion

        private void LabelText()
        {
            //string remain = "Remaining: " + minutes + "m ";
            //lblTimer.Text = remain + (second < 10 ? string.Format("0{0}s", second) : string.Format("{0}s", second));

            lblTimer.Text = "Remaining: " + minutes + "m  " + second + "s";
        }
        #endregion
    }
}
