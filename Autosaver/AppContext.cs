using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Autosaver
{
    class AppContext : ApplicationContext
    {
        //Component declarations
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private Form SettingsMenuForm;
        private ToolStripMenuItem SettingsMenuItem;
        private ToolStripMenuItem CloseMenuItem;
        
        private ManualResetEvent SaverReleaseEvent = new(false);
        
        const uint KEYEVENTF_KEYUP = 2;
        const int VK_CONTROL = 0x11;
        const int VC_S = 0x53;

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public AppContext()
        {
            Application.ApplicationExit += OnApplicationExit;
            InitializeComponent();
            TrayIcon.Visible = true;
        }

        private void InitializeComponent()
        {
            SettingsMenuForm = new SettingsForm();

            TrayIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/floppy.ico")
            };
            
            TrayIconContextMenu = new ContextMenuStrip();
            SettingsMenuItem = new ToolStripMenuItem();
            CloseMenuItem = new ToolStripMenuItem();
            TrayIconContextMenu.SuspendLayout();
            
            TrayIconContextMenu.Items.AddRange(new ToolStripItem[]
            {
                SettingsMenuItem,
                CloseMenuItem
            });
            TrayIconContextMenu.Name = "TrayIconContextMenu";
            TrayIconContextMenu.Size = new Size(153, 70);
            
            SettingsMenuItem.Name = "SettingsMenuItem";
            SettingsMenuItem.Size = new Size(152, 22);
            SettingsMenuItem.Text = "Settings";
            SettingsMenuItem.Click += SettingsMenuItem_Click;
            
            CloseMenuItem.Name = "CloseMenuItem";
            CloseMenuItem.Size = new Size(152, 22);
            CloseMenuItem.Text = "Exit";
            CloseMenuItem.Click += CloseMenuItem_Click;

            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
            new Thread(AutoSaver).Start();
        }

        private void AutoSaver()
        {
            while (true)
            {
                if (SaverReleaseEvent.WaitOne(1000 * 60 * 5))
                //if (SaverReleaseEvent.WaitOne(20000))
                    return;
                SendSave();
                ShowToast();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void SendSave()
        {
            keybd_event(VK_CONTROL,0,0,0);
            keybd_event(VC_S, 0, 0, 0 ); //Send the S key
            keybd_event(VC_S, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);// 'Left Control Up
        }

        private void ShowToast()
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode("Autosaver"));
            stringElements[1].AppendChild(toastXml.CreateTextNode("Just saved the file!"));

            var imagePath = "file:///" + Path.GetFullPath("Resources/floppy.ico");
            var imageElements = toastXml.GetElementsByTagName("image");

            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.GetDefault().CreateToastNotifier("AutoSaver").Show(toast);
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
        }

        private void SettingsMenuItem_Click(object sender, EventArgs e)
        {
            SettingsMenuForm.Show();
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to exit the program?",
                "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                SaverReleaseEvent.Set();
                Environment.Exit(0);
            }
        }
    }
}