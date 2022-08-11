using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using MDOL;
using System.ComponentModel;


namespace Optitracker
{
    public partial class MoCapForm : Form
    {
        Timer tmrGUIUpdater = new Timer();

        GUI.Trigger trigger;
        GUI.MoCap mocap;
        GUI.Tobii tobii;

        GUI.tabInterface[] GUIs;

        public MoCapForm()
        {
            InitializeComponent();

            Settings.Load();

            FormResizer.FillParent(this, tabControl);

            GUIs = new GUI.tabInterface[] {
                mocap = new GUI.MoCap(),
                trigger = new GUI.Trigger(),
                tobii = new GUI.Tobii()
            };
            foreach(UserControl gui in GUIs)
            {
                tabControl.TabPages.Add(gui.ToString().Replace("Optitracker.GUI.",""));
                tabControl.TabPages[tabControl.TabCount - 1].Controls.Add(gui);
                FormResizer.FillParent(tabControl, gui);
            }

            Debug.Start();

            new FormVisual().Show();
            tmrGUIUpdater.Interval = 30;
            tmrGUIUpdater.Tick += (s, e) =>
            {
                foreach (TabPage tabPage in tabControl.TabPages)
                {
                    string[] isReady = ((GUI.tabInterface)tabPage.Controls[0]).IsReady();
                    if (tabPage.Tag == null || isReady.Length != ((string[])tabPage.Tag).Length)
                    {
                        tabPage.Tag = isReady;
                        tabControl.Invalidate();
                    }
                }
                if (ContainsFocus)
                    ((GUI.tabInterface)tabControl.SelectedTab.Controls[0]).UpdateGUI();
            };
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += (s, e) =>
              {
                  Brush brush;
                  if (GUIs[e.Index].IsReady().Length == 0)
                      brush = Brushes.Green;
                  else
                      brush = Brushes.Red;
                  e.Graphics.FillRectangle(brush, e.Bounds);
                  e.Bounds.Inflate(-2, -2);
                  e.Graphics.DrawString(tabControl.TabPages[e.Index].Text, Font, Brushes.Black, e.Bounds);
              };
            tmrGUIUpdater.Start();
        }

        bool IsReady()
        {
            List<string> MSG = new List<string>();
            foreach (GUI.tabInterface gui in GUIs)
            {
                string[] msg = gui.IsReady();
                if (msg.Length != 0)
                    MSG.Add(gui.ToString().Replace("MoCapSequencer.GUI.", "") + "\r\n" + string.Join("\r\n", msg));
            }
            if (MSG.Count == 0 || MessageBox.Show(string.Join("\r\n\r\n", MSG) + "\r\n\r\nContinue anyway?", "One or more problems detected!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                return true;
            return false;
        }
        private void mnuTreadmill_Click(object sender, EventArgs e)
        {
            if (IsReady())
                new Treadmill().Show();
        }

        private void randomWalkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsReady())
                new RandomWalkStep().Show();
        }

        private void MoCapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (GUI.tabInterface gui in GUIs)
                gui.Destroy();
            Debug.Stop();
        }

        private void formVisualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormVisual().Show();
        }
    }
}