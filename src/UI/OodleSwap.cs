﻿using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Pro_Swapper.API;
using Bunifu.Framework.UI;
using System.Threading.Tasks;
namespace Pro_Swapper
{
    public partial class OodleSwap : Form
    {
        private api.Item ThisItem;

        public OodleSwap(api.Item item)
        {
            InitializeComponent();
            this.Icon = Main.appIcon;
            RPC.SetState(item.SwapsFrom + " To " + item.SwapsTo, true);
            ThisItem = item;
            string swaptext = ThisItem.SwapsFrom + " --> " + ThisItem.SwapsTo;
            Text = swaptext;
            label1.Text = swaptext;
            image.Image = global.ItemIcon(ThisItem.FromImage);
            swapsfrom.Image = global.ItemIcon(ThisItem.ToImage);
            Region = Region.FromHrgn(Main.CreateRoundRectRgn(0, 0, Width, Height, 30, 30));
            Icon = Main.appIcon;
            BackColor = global.MainMenu;
            logbox.BackColor = global.MainMenu;

            ConvertB.ForeColor = global.TextColor;
            ConvertB.BackColor = global.Button;
            ConvertB.Activecolor = global.Button;
            ConvertB.Normalcolor = global.Button;


            RevertB.ForeColor = global.TextColor;
            RevertB.BackColor = global.Button;
            RevertB.Activecolor = global.Button;
            RevertB.Normalcolor = global.Button;

            logbox.ForeColor = global.TextColor;
            if (global.CurrentConfig.swaplogs.Contains(ThisItem.SwapsFrom + " To " + ThisItem.SwapsTo + ","))
            {
                label3.ForeColor = Color.Lime;
                label3.Text = "ON";
            }
            else
            {
                label3.ForeColor = Color.Red;
                label3.Text = "OFF";
                if (ThisItem.Note != null) MessageBox.Show("Warning for " + ThisItem.SwapsTo + ": " + ThisItem.Note, ThisItem.SwapsFrom + " - " + ThisItem.SwapsTo, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void Log(string text)=> logbox.Invoke(new Action(() => { logbox.Text += $"{text}{Environment.NewLine}"; logbox.ScrollToCaret(); }));
        private async void ButtonbgWorker(bool Converting)
        {
            try
            {
                ConvertB.Invoke(new Action(() => { ConvertB.Enabled = false; }));
                RevertB.Invoke(new Action(() => { RevertB.Enabled = false; }));
                label3.Invoke(new Action(() => { label3.Text = "Loading..."; label3.ForeColor = Color.White; }));
               
                Stopwatch s = Stopwatch.StartNew();
                await Swap.SwapItem(ThisItem, Converting);
                s.Stop();
                ConvertB.Invoke(new Action(() => { ConvertB.Enabled = true; }));
                RevertB.Invoke(new Action(() => { RevertB.Enabled = true; }));
                logbox.Invoke(new Action(() => { logbox.Clear(); }));
                string swaplogs = global.CurrentConfig.swaplogs;
                if (Converting)
                {
                    Log($"[+] Converted item in {s.Elapsed.Milliseconds}ms");
                    label3.Invoke(new Action(() => { label3.Text = "ON"; label3.ForeColor = Color.Lime; }));
                    global.CurrentConfig.swaplogs += ThisItem.SwapsFrom + " To " + ThisItem.SwapsTo + ",";
                }
                else
                {
                    Log($"[-] Reverted item in {s.Elapsed.Milliseconds}ms");
                    label3.Invoke(new Action(() => { label3.Text = "OFF"; label3.ForeColor = Color.Red; }));
                    global.CurrentConfig.swaplogs = swaplogs.Replace(ThisItem.SwapsFrom + " To " + ThisItem.SwapsTo + ",", "");
                }
                global.SaveConfig();
            }
            catch (Exception ex)
            {
                Log($"Restart the swapper or refer to this error: {ex.Message} | {ex.StackTrace}");
            }
        }

        private void SwapButton_Click(object sender, EventArgs e)
        {
            string path = global.CurrentConfig.Paks + @"\pakchunk0-WindowsClient.sig";
            if (!File.Exists(path))
            {
                MessageBox.Show("Select your paks folder in Settings", "Pro Swapper");
                return;
            }
            if (!EpicGamesLauncher.CloseFNPrompt())
                return;

            foreach (api.Asset asset in ThisItem.Asset)
            {
                //Check if replace is longer
                for (int i = 0; i < asset.Search.Length; i++)
                {
                    if (asset.Search[i].Length < asset.Replace[i].Length)
                    {
                        string error = "The replace length is longer than the search, pleaes make sure the search is greater than or equal to the replace length";
                        MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log(error);
                        return;
                    }
                }
            }
            logbox.Clear();
            Log("Loading...");
            bool isconverting = ((BunifuFlatButton)(sender)).Text == "Convert";
            Task.Run(() => ButtonbgWorker(isconverting));
        }
        private void ExitButton_Click(object sender, EventArgs e) => Close();
        private void button2_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;
        private void swap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                global.FormMove(Handle);
        }
    }
}