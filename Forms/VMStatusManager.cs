using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;

namespace HyperVMs
{
    public partial class VMStatusManager : Form
    {
        private dynamic settings;

        private readonly Image runningImage;
        private readonly Image stoppedImage;

        public VMStatusManager()
        {
            InitializeComponent();

            mainIcon.Icon = new Icon(GetType().Assembly.GetManifestResourceStream("HyperVMs.Resources.Icons.main.ico"));
            runningImage = Image.FromStream(GetType().Assembly.GetManifestResourceStream("HyperVMs.Resources.Icons.running.ico"));
            stoppedImage = Image.FromStream(GetType().Assembly.GetManifestResourceStream("HyperVMs.Resources.Icons.stopped.ico"));

            PopulateVMs();
            mainIcon.Visible = true;
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }

        private void OnMainContextMenuStripOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PopulateVMs();
        }

        private void PopulateVMs()
        {
            try
            {
                ParseSettings();

                mainContextMenuStrip.Items.Clear();

                using (var shell = PowerShell.Create())
                {
                    var results = shell.AddCommand("Get-VM").Invoke();

                    if (shell.HadErrors)
                    {
                        var errorMessage = "";
                        foreach (var item in shell.Streams.Error.ReadAll())
                        {
                            errorMessage += item.Exception.Message + Environment.NewLine;
                        }
                        MessageBox.Show(errorMessage, Properties.Resources.title_Main, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        var menuItemVMStatusContainer = new ToolStripMenuItem(Properties.Resources.menu_PowerStates);
                        var menuItemVMShellContainer = new ToolStripMenuItem(Properties.Resources.menu_OpenShell);

                        foreach (var result in results)
                        {
                            var vmName = result.Properties["Name"].Value.ToString();
                            var vmState = result.Properties["State"].Value.ToString();
                            var vmRunning = vmState == "Running";

                            var menuItemVMStatus = new ToolStripMenuItem(vmName, vmRunning ? runningImage : stoppedImage, ChangeVMPowerStatus)
                            {
                                Tag = result,
                                ToolTipText = vmRunning ? Properties.Resources.tooltip_StopVM : Properties.Resources.tooltip_StartVM,
                            };
                            menuItemVMStatusContainer.DropDownItems.Add(menuItemVMStatus);

                            var menuItemVMOpenShell = new ToolStripMenuItem(vmName, null, OpenVMShell)
                            {
                                Tag = result,
                                ToolTipText = Properties.Resources.tooltip_OpenShell,
                            };
                            menuItemVMShellContainer.DropDownItems.Add(menuItemVMOpenShell);
                        }

                        if (menuItemVMStatusContainer.DropDownItems.Count > 0)
                        {
                            mainContextMenuStrip.Items.Add(menuItemVMShellContainer);
                            mainContextMenuStrip.Items.Add(menuItemVMStatusContainer);
                            mainContextMenuStrip.Items.Add(new ToolStripSeparator());
                        }

                        mainContextMenuStrip.Items.Add(Properties.Resources.menu_OpenSettings, null, OpenSettings);
                        mainContextMenuStrip.Items.Add(new ToolStripSeparator());
                        mainContextMenuStrip.Items.Add(Properties.Resources.menu_Exit, null, ExitApplication);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Properties.Resources.title_Main, MessageBoxButtons.OK, MessageBoxIcon.Error);

                mainContextMenuStrip.Items.Clear();
                mainContextMenuStrip.Items.Add(Properties.Resources.menu_Exit, null, ExitApplication);
            }
        }

        private void ParseSettings()
        {
            settings = JsonConvert.DeserializeObject(File.ReadAllText(@"settings.json"));
            var autorun = settings["autorun"].Value as bool?;

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (autorun.GetValueOrDefault(false))
            {
                registryKey.SetValue("HyperVMs", $"\"{Application.ExecutablePath.ToString()}\"");
            }
            else
            {
                registryKey.DeleteValue("HyperVMs", false);
            }
        }

        private void ChangeVMPowerStatus(object sender, EventArgs e)
        {
            var vm = (sender as ToolStripMenuItem).Tag as PSObject;

            var vmName = vm.Properties["Name"].Value.ToString();
            var vmState = vm.Properties["State"].Value.ToString();
            var vmRunning = vmState == "Running";

            using (var shell = PowerShell.Create())
            {
                shell.AddCommand($"{(vmRunning ? "Stop" : "Start")}-VM").AddParameter("Name", vmName).Invoke();

                if (shell.HadErrors)
                {
                    var errorMessage = "";
                    foreach (var item in shell.Streams.Error.ReadAll())
                    {
                        errorMessage += item.Exception.Message + Environment.NewLine;
                    }
                    MessageBox.Show(errorMessage, Properties.Resources.title_VMStatus, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            mainIcon.ShowBalloonTip(0, Properties.Resources.title_VMStatus, $"{vmName} - {vm.Properties["State"].Value.ToString()}", ToolTipIcon.None);
        }

        private void OpenVMShell(object sender, EventArgs e)
        {
            try
            {
                var menuItem = sender as ToolStripMenuItem;

                var defaultShell = settings["defaultShell"].Value as string;
                var defaultShellCommand = settings["shells"].GetValue(defaultShell).GetValue("command").Value as string;
                var defaultShellCommandArgs = settings["shells"].GetValue(defaultShell).GetValue("arguments").Value as string;
                var sshCommand = settings["sshCommand"].Value as string;

                var vm = menuItem.Tag as PSObject;
                var vmName = vm.Properties["Name"].Value.ToString();
                var hostName = $"{vmName}.mshome.net";

                Ping pingSender = new Ping();

                if (Attempt(() =>
                {
                    PingReply reply = pingSender.Send(hostName, 100);
                    return reply.Status == IPStatus.Success;
                }))
                {
                    var userName = settings["hosts"].GetValue(vmName)?.GetValue("userName");

                    if (userName == null)
                    {
                        userName = InputDialog.ShowInputDialog();
                        var hosts = settings["hosts"] as JObject;

                        dynamic newHost = new JObject();
                        newHost.userName = userName;

                        hosts.Add(vmName, newHost);

                        File.WriteAllText(@"settings.json", JsonConvert.SerializeObject(settings, Formatting.Indented));
                    }

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = defaultShellCommand,
                        Arguments = $"{defaultShellCommandArgs} {sshCommand} {userName}@{hostName}"
                    };
                    Process.Start(startInfo);
                }
                else
                {
                    mainIcon.ShowBalloonTip(0, Properties.Resources.title_VMOpenShell, Properties.Resources.message_NoIP, ToolTipIcon.None);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.title_VMOpenShell, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"settings.json"
            };
            Process.Start(startInfo);
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private bool Attempt(Func<bool> action, int retryInterval = 1000, int maxAttemptCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    Thread.Sleep(retryInterval);

                    if (action())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
            else
            {
                return false;
            }
        }
    }
}
