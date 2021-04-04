using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

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
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipText = "I just autosaved!",
                BalloonTipTitle = "Autosave",
                Text = "Autosaver",
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
                //if (SaverReleaseEvent.WaitOne(1000 * 60 * 5))
                if (SaverReleaseEvent.WaitOne(5000))
                    return;
            }
            // ReSharper disable once FunctionNeverReturns
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