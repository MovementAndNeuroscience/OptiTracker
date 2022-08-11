using MDOL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using System.Windows.Forms.DataVisualization.Charting;
namespace Optitracker
{
    public partial class Treadmill : Form
    {
        public abstract class TreadmillPacket : IO.DataSaver.DataPacket
        {

            public class Startup : TreadmillPacket
            {
                protected override string PacketStr => "Treadmill;Startup";
                public Startup(long GlobalTrigger,string saveFile)
                {
                    AddData("GlobalTrigger", GlobalTrigger);
                    AddData("saveFile", saveFile);
                }
            }

            public class Calibration : TreadmillPacket
            {
                protected override string PacketStr => "Treadmill;Calibration";
                public Calibration(PhaseEstimator phaseEstimator)
                {
                    AddData("ID", phaseEstimator.ID);
                    AddData("standSeconds", phaseEstimator.Calibrated.standSeconds);
                    AddData("standHeight", phaseEstimator.Calibrated.standHeight);
                    AddData("swingSeconds", phaseEstimator.Calibrated.swingSeconds);
                    AddData("cycleSeconds", phaseEstimator.Calibrated.cycleSeconds);
                    AddData("beltSpeed", phaseEstimator.Calibrated.beltSpeed);
                }
            }
            public class FootStrike : TreadmillPacket
            {
                protected override string PacketStr => "Treadmill;FootStrike";
                public FootStrike(int ID, FormVisual.VisualMarker visualMarker)
                {
                    AddData("ID", ID);
                    AddData("Position", visualMarker);
                }
            }
            public class Frame : TreadmillPacket
            {
                protected override string PacketStr => "Treadmill;Frame";
                public Frame(FormVisual.VisualMarker[] LeftRight)
                {
                    AddData("LeftRight", LeftRight);
                }
            }
        }

        public class PhaseEstimator
        {
            public static double tauHeight = 0.02;
            public static bool FilterCycles = false;
            public int ID = -1;
            public class Phase
            {
                public double cycleSeconds = double.NaN;
                public double swingSeconds = double.NaN;
                public double standSeconds = double.NaN;
                public double standHeight = double.NaN;
                public double standMid = double.NaN;
                public double beltSpeed = double.NaN;
                public double stepX = double.NaN;
                public double stepY = double.NaN;
                public double stepLength
                {
                    get
                    {
                        return beltSpeed * standSeconds;
                    }
                }
                public Phase(DateTime[] Time, FormVisual.VisualMarker[] XYZs, int iChange,double cycleSeconds)
                {
                    standHeight = XYZs.Where((xyz, i) => i >= iChange).Select(xyz => xyz.Z).Median();
                    standMid = XYZs.Where((xyz, i) => i >= iChange).Select(xyz => xyz.X).Median();
                    stepX = XYZs[iChange].X;
                    stepY = XYZs[iChange].Y;

                    swingSeconds = (Time[iChange] - Time[0]).TotalSeconds;
                    standSeconds = (Time.Last() - Time[iChange]).TotalSeconds;
                    this.cycleSeconds = cycleSeconds;
                    List<double> vel = new List<double>();
                    for (int i = iChange; i < Time.Length - 1; i++)
                        if (XYZs[i].Z < standHeight + tauHeight && XYZs[i + 1].Z < standHeight + tauHeight)
                            vel.Add(Math.Sqrt((XYZs[i + 1].X - XYZs[i].X).Sqr() + (XYZs[i + 1].Y - XYZs[i].Y).Sqr() + (XYZs[i + 1].Z - XYZs[i].Z).Sqr()) / (Time[i + 1] - Time[i]).TotalSeconds);
                    beltSpeed = vel.Median();
                }
                public Phase() { }
                public static Phase FromJson(string json)
                {
                    if (json != "")
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<Phase>(json);
                    else
                        return new Phase();
                }
                public string ToJson()
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(this);
                }
                public static Phase Median(Phase[] Phases)
                {
                    return new Phase()
                    {
                        standSeconds = Phases.Select(phase => phase.standSeconds).Median(),
                        standHeight = Phases.Select(phase => phase.standHeight).Median(),
                        standMid = Phases.Select(phase => phase.standMid).Median(),
                        swingSeconds = Phases.Select(phase => phase.swingSeconds).Median(),
                        cycleSeconds = Phases.Select(phase => phase.cycleSeconds).Median(),
                        beltSpeed = Phases.Select(phase => phase.beltSpeed).Median(),
                        stepX = Phases.Select(phase => phase.stepX).Median(),
                        stepY = Phases.Select(phase => phase.stepY).Median(),
                    };
                }
            }
            readonly List<FormVisual.VisualMarker> XYZs = new List<FormVisual.VisualMarker>();
            readonly List<Phase> Phases = new List<Phase>();
            readonly List<DateTime> Time = new List<DateTime>();
            readonly List<bool> Swing = new List<bool>();
            double posPrev = double.NaN;
            int Phasetype = 0;

            public Phase Calibrated = new Phase();
            public event EventHandler<FormVisual.VisualMarker> StandStarted;
            public event EventHandler<FormVisual.VisualMarker> SwingStarted;
            
            bool Calibrating = false;
            public void StartCalibrating()
            {
                Phases.Clear();
                Time.Clear();
                Swing.Clear();
                XYZs.Clear();
                Calibrating = true;
            }
            public void StopCalibrating()
            {
                // Remove first phase, as this is most likely incomplete
                if (Phases.Count > 1)
                    Phases.RemoveAt(0);
                
                Calibrating = false;
                Calibrated = Phase.Median(Phases.ToArray());
            }

            DateTime lastSwing = DateTime.MinValue;
            bool SearchForTrueStand = false;
            Vector<Dimensions.III>[] medianHist = new Vector<Dimensions.III>[3]
            {
                new Vector<Dimensions.III>(0,0,0),
                new Vector<Dimensions.III>(0,0,0),
                new Vector<Dimensions.III>(0,0,0)
            };
            int kHist = 0;
            public int AnalyzePoint(FormVisual.VisualMarker visualMarker)
            {
                if (FilterCycles)
                {
                    medianHist[kHist++] = new Vector<Dimensions.III>(visualMarker.X, visualMarker.Y, visualMarker.Z);
                    if (kHist == medianHist.Length)
                        kHist = 0;
                    Vector<Dimensions.III> median = medianHist.Median();
                    visualMarker.X = median.X;
                    visualMarker.Y = median.Y;
                    visualMarker.Z = median.Z;
                }

                Phasetype = Math.Sign(Phasetype);
                if (Phasetype == 0) // Phasetype not detected
                {
                    if (!double.IsNaN(posPrev))
                        Phasetype = visualMarker.Y < posPrev ? -1 : 1; // Detect phasetype based on increasing or decreasing value
                }
                else
                {
                    // Detect change in phase
                    if (Phasetype * visualMarker.Y < Phasetype * posPrev)
                    {
                        Phasetype *= -2;
                        if (Phasetype > 0) // Swingstart
                        {
                            SwingStarted?.Invoke(this, visualMarker);
                            if (Calibrating)
                            {
                                int iChange = Swing.FindIndex((swing) => !swing);
                                if (iChange > 0)
                                {
                                    Phase phase = new Phase(Time.ToArray(), XYZs.ToArray(), iChange, (DateTime.Now-lastSwing).TotalSeconds);
                                    if (phase.standSeconds != 0)
                                        Phases.Add(phase);
                                }
                                Time.Clear();
                                Swing.Clear();
                                XYZs.Clear();
                                Time.Add(DateTime.Now);
                                XYZs.Add(visualMarker);
                                Swing.Add(Phasetype > 0);
                                lastSwing = DateTime.Now;
                            }
                        }
                        else
                            SearchForTrueStand = true;
                    }
                    if (Phasetype < 0 && SearchForTrueStand && visualMarker.Z < tauHeight + Calibrated.standHeight)
                    {
                        StandStarted?.Invoke(this, visualMarker);
                        SearchForTrueStand = false;
                    }
                    if (Calibrating)
                    {
                        Time.Add(DateTime.Now);
                        XYZs.Add(visualMarker);
                        Swing.Add(Phasetype > 0);

                        Calibrated = Phase.Median(Phases.ToArray());
                    }
                }

                posPrev = visualMarker.Y;
                return Phasetype;
            }
        }
        readonly Timer tmr = new Timer();
        readonly Series[] serieSignal = new Series[2];
        readonly Series[] serieEvents = new Series[2];
        readonly Series[] serieSignalHeight = new Series[2];
        readonly Series[] serieEventsHeight = new Series[2];
        readonly PhaseEstimator[] phaseEstimators = new PhaseEstimator[2] { new PhaseEstimator() { ID = 0 }, new PhaseEstimator() { ID = 1 } };

        bool Ignore = false;
        public Treadmill()
        {
            InitializeComponent();

            PopulatePanel<TargetOptions>(pnlTarget);

            new FormResizer(this, chtPhaseHeight);

            if (GUI.MoCap.moCapSystem != null)
                GUI.MoCap.moCapSystem.FrameReceived += MoCapSystem_FrameReceived;

            for (int i = 0; i < 2; i++)
            {
                Color color = i == 0 ? Color.Red : Color.Green;
                serieSignal[i] = new Series()
                {
                    ChartType = SeriesChartType.FastLine,
                    Color = color
                };
                serieEvents[i] = new Series()
                {
                    ChartType = SeriesChartType.Point,
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 10,
                    BorderColor = color,
                    Color = color
                };
                chtPhaseHeight.Series.Add(serieSignal[i]);
                chtPhaseHeight.Series.Add(serieEvents[i]);

                serieSignalHeight[i] = new Series()
                {
                    ChartType = SeriesChartType.FastLine,
                    Color = color
                };
                serieEventsHeight[i] = new Series()
                {
                    ChartType = SeriesChartType.Point,
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 10,
                    BorderColor = color,
                    Color = color
                };
                chtPhaseHeight.Series.Add(serieSignalHeight[i]);
                chtPhaseHeight.Series.Add(serieEventsHeight[i]);
            }

            tmr = new Timer()
            {
                Interval = 30
            };
            tmr.Tick += Tmr_Tick;
            tmr.Start();

            stats[0, 1] = "Left";
            stats[0, 2] = "Right";
            stats[1, 0] = "Belt Speed (km/hr)";
            stats[2, 0] = "Belt Height (m)";
            stats[3, 0] = "StepLength (m)";
        }

        private void MoCapSystem_FrameReceived(object sender, GUI.MoCap.MoCapSystem.Frame e)
        {
            LeftRight = ((GUI.MoCap.MoCapSystem)sender).StepPrepare(e);
            IO.DataSaver.AddPacket(new TreadmillPacket.Frame(LeftRight));
        }

        class Disturbance
        {
            double Probability = 0;
            double xOffset = 0;
            double yOffset = 0;
            double speedScale = 1;
            public override string ToString()
            {
                return (int)(Probability * 100) + " %" + (xOffset == 0 ? "" : " - X") + (yOffset == 0 ? "" : " - Y") + (speedScale == 1 ? "" : " - Speed");
            }
        }
        FormVisual.VisualMarker[] LeftRight = new FormVisual.VisualMarker[0];
        int kFrames = 0;
        int stepcount = 0;
        readonly string[,] stats = new string[7, 3];
        int nexti = -1;
        int nextStepFrequency = -1;
        private void Tmr_Tick(object sender, EventArgs e)
        {
            FormVisual.FORMVISUAL.DrawUser(LeftRight);
            for (int i = 0; i < LeftRight.Length; i++)
            {
                FormVisual.VisualMarker visualMarker = LeftRight[i];
                if (IsClosing)
                    return;

                int phasetype = phaseEstimators[i].AnalyzePoint(visualMarker);
                
                if (Math.Abs(phasetype) == 2)
                {
                    serieEvents[i].Points.AddXY(kFrames, visualMarker.Y);
                    DataPoint point = serieEvents[i].Points.Last();
                    point.Color = phasetype == 2 ? point.BorderColor : Color.Transparent;

                    serieEventsHeight[i].Points.AddXY(kFrames, visualMarker.Z);
                    point = serieEventsHeight[i].Points.Last();
                    point.Color = phasetype == 2 ? point.BorderColor : Color.Transparent;
                }
                serieSignal[i].Points.AddXY(kFrames, visualMarker.Y);
                serieSignalHeight[i].Points.AddXY(kFrames, visualMarker.Z);
                double min = kFrames - 5 * 1000 / tmr.Interval;
                chtPhaseHeight.ChartAreas[0].AxisX.Minimum = min;
                chtPhaseHeight.ChartAreas[0].AxisX.Maximum = kFrames;
                for (int j = 0; j < serieSignal[i].Points.Count; j++)
                    if (serieSignal[i].Points[j].XValue < min)
                        serieSignal[i].Points.RemoveAt(j--);
                    else
                        break;
                for (int j = 0; j < serieEvents[i].Points.Count; j++)
                    if (serieEvents[i].Points[j].XValue < min)
                        serieEvents[i].Points.RemoveAt(j--);
                    else
                        break;

                for (int j = 0; j < serieSignalHeight[i].Points.Count; j++)
                    if (serieSignalHeight[i].Points[j].XValue < min)
                        serieSignalHeight[i].Points.RemoveAt(j--);
                    else
                        break;
                for (int j = 0; j < serieEventsHeight[i].Points.Count; j++)
                    if (serieEventsHeight[i].Points[j].XValue < min)
                        serieEventsHeight[i].Points.RemoveAt(j--);
                    else
                        break;
                
                if (Calibrating)
                {
                    stats[1, i + 1] = (phaseEstimators[i].Calibrated.beltSpeed / 1000 * 3600).ToString("0.00");
                    stats[2, i + 1] = phaseEstimators[i].Calibrated.standHeight.ToString("0.00");
                    stats[3, i + 1] = phaseEstimators[i].Calibrated.stepLength.ToString("0.00");
                }
            }

            if (Calibrating)
                lblStats.Text = table2str(stats);
            kFrames++;
            FormVisual.FORMVISUAL.UpdateTargets();
        }
        static Random rnd = new Random((int)DateTime.Now.Ticks);

        string table2str(string[,] table)
        {
            int space = 2;
            string str = "";
            int maxWidth = 0;
            int m = table.GetLength(0);
            int n = table.GetLength(1);
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                {
                    if (table[i, j] == null)
                        table[i, j] = "";
                    if (table[i, j].Length > maxWidth)
                        maxWidth = table[i, j].Length;
                }
            maxWidth += 2;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j == 0)
                    {
                        str += table[i, j];
                        for (int k = table[i, j].Length; k < maxWidth + space; k++)
                            str += " ";
                    }
                    else
                    {
                        int halfspace = (maxWidth - table[i, j].Length) / 2;
                        for (int k = -1; k < halfspace; k++)
                            str += " ";
                        str += table[i, j];
                        for (int k = -1; k < halfspace; k++)
                            str += " ";
                    }
                }
                if (i != m - 1)
                    str += "\r\n";
            }
            return str;
        }

        bool IsClosing = false;
        private void Treadmill_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsClosing = true;
            IO.DataSaver.Close();
            FormVisual.FORMVISUAL.Clear();
            tmr.Stop();
        }

        bool Started = false;
        int Timeout = 0;
        private void cmdStart_Click(object sender, EventArgs e)
        {
            if (!Started && !chkStartTrigger.Checked && globalTrigger == 0)
            { 
                MessageBox.Show("No globaltrigger has been sent (send global trigger or enable 'Trigger on Start')");
                return;
            }
            sw.Reset();
            Started = !Started;
            cmdStart.BackColor = Started ? Color.Red : Color.Lime;
            cmdStart.Text = Started ? "Stop" : "Start";
            if (!Started)
            {
                FormVisual.FORMVISUAL.SetTime(0);
                cmdCalibrate.Enabled = true;
                tmrTimeout.Stop();
                IO.DataSaver.Close();
                FormVisual.FORMVISUAL.Clear();
                for(int i = 0; i < LeftRight.Length; i++)
                    phaseEstimators[i].StandStarted -= Treadmill_StandStarted;
            }
            else
            {
                sw.Restart();
                FormVisual.FORMVISUAL.SetTime(0);
                if (chkStartTrigger.Checked)
                    globalTrigger = DateTime.Now.Ticks;
                GUI.Trigger.SendTrigger();
                cmdCalibrate.Enabled = false;
                if (nudTimeout.Value != 0)
                {
                    Timeout = (int)nudTimeout.Value;
                    tmrTimeout.Start();
                }
                string filename = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                if (trbLeftRight.Value == 0)
                    filename += "_Left";
                else if (trbLeftRight.Value == 100)
                    filename += "_Right";
                filename += txtPostfix.Text == "" ? "" : "_" + txtPostfix.Text;
                if (!System.IO.Directory.Exists("Recordings"))
                    System.IO.Directory.CreateDirectory("Recordings");
                IO.DataSaver.NewFile("Recordings/" + filename + ".bin",true);
                IO.DataSaver.AddPacket(new FormVisual.VisualPacket.Startup(FormVisual.FORMVISUAL));
                if (GUI.MoCap.moCapSystem != null)
                    IO.DataSaver.AddPacket(new GUI.MoCap.MoCapPacket.Info(GUI.MoCap.moCapSystem));
                IO.DataSaver.AddPacket(new GUI.Tobii.TobiiPacket());

                IO.DataSaver.AddPacket(new TreadmillPacket.Startup(globalTrigger, saveFile()));
                FormVisual.FORMVISUAL.ClearScore();
                foreach (PhaseEstimator phaseEstimator in phaseEstimators)
                    IO.DataSaver.AddPacket(new TreadmillPacket.Calibration(phaseEstimator));
                NextI();

                if (chkGroupRandom.Checked)
                {
                    double sumProb = lstTargets.Items.Cast<TargetOptions>().Sum(target => target.Probability / 100.0);
                    double[] probs = lstTargets.Items.Cast<TargetOptions>().Select(target => target.Probability / 100.0 / sumProb).ToArray();
                    double minProb = probs.Min();
                    int n = (int)(2 / minProb);
                    RandomList = new int[n];
                    for (int i = 0, k = 0; i < probs.Length; i++)
                    {
                        for (int j = 0; j < probs[i] * n; j++)
                            RandomList[k++] = i;
                    }
                    RandomList = RandomList.OrderBy(dummy => rnd.NextDouble()).ToArray();
                    kRandom = 0;
                }
                for (int i = 0; i < LeftRight.Length; i++)
                    phaseEstimators[i].StandStarted += Treadmill_StandStarted;
            }
        }

        DateTime[] lastStep = new DateTime[2];
        int[] RandomList;
        int kRandom;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        private void Treadmill_StandStarted(object sender, FormVisual.VisualMarker visualMarker)
        {
            PhaseEstimator phaseEstimator = (PhaseEstimator)sender;
            IO.DataSaver.AddPacket(new TreadmillPacket.FootStrike(phaseEstimator.ID, visualMarker));
            if (sw.ElapsedMilliseconds >= phaseEstimator.Calibrated.cycleSeconds * 1000 && phaseEstimator.ID == nexti && ++stepcount >= nextStepFrequency)
            {
                sw.Restart();
                NextI();
                if (phaseEstimator.ID == nexti)
                    stepcount = 0;
                else
                    stepcount = -1;
                if (lstTargets.Items.Count > 0)
                {
                    int targetType = 0;
                    double x, y;
                    if (chkFixSpawn.Checked)
                    {
                        x = phaseEstimator.Calibrated.stepX;
                        y = phaseEstimator.Calibrated.stepY;
                    }
                    else
                    {
                        x = visualMarker.X;
                        y = visualMarker.Y;
                    }
                    double speed = phaseEstimator.Calibrated.beltSpeed;
                    if (chkGroupRandom.Checked)
                    {
                        targetType = RandomList[kRandom++];
                        if (kRandom == RandomList.Length)
                        {
                            RandomList = RandomList.OrderBy(dummy => rnd.NextDouble()).ToArray();
                            kRandom = 0;
                        }
                    }
                    else
                    {
                        double[] probs = lstTargets.Items.Cast<TargetOptions>().Select(target => target.Probability / 100.0).ToArray();
                        double sumProb = probs.Sum();
                        double val = rnd.NextDouble();
                        double cumsum = 0;
                        for (int i = 0; i < lstTargets.Items.Count; i++)
                        {
                            cumsum += probs[i] / sumProb;
                            if (cumsum > val)
                            {
                                targetType = i;
                                break;
                            }
                        }
                    }
                    ((TargetOptions)lstTargets.Items[targetType]).Create(x, y, speed, phaseEstimator, targetType);
                }
            }
            lastStep[phaseEstimator.ID] = DateTime.Now;
        }

        void NextI()
        {
            double split = trbLeftRight.Value / 100.0;
            nexti = rnd.NextDouble() >= split ? 0 : 1;

            nextStepFrequency = rnd.Next((int)nudFrequencyMin.Value, (int)nudFrequencyMax.Value + 1);
        }

        void CountDown(string Title,int Seconds,string Parameter = "")
        {
            ProgressForm progressForm = new ProgressForm(Title, Parameter)
            {
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None
            };
            Timer tmr = new Timer()
            {
                Interval = 1000
            };
            int tick = 0;
            tmr.Tick += (tmr_s, tmr_e) =>
            {
                tick++;
                progressForm.SetValue(0, tick * 100 / Seconds);
                if (tick == Seconds)
                {
                    tmr.Stop();
                    progressForm.Close();
                }
            };
            tmr.Start();
            progressForm.ShowDialog();
        }

        bool Calibrating = false;
        private void cmdCalibrate_Click(object sender, EventArgs e)
        {
            int delay = (int)nudCalibDelay.Value;
            if (!Calibrating && delay != 0)
            { 
                CountDown("", delay, "Prepare");
                System.Media.SystemSounds.Asterisk.Play();
            }
            Calibrating = !Calibrating;
            cmdCalibrate.BackColor = Calibrating ? Color.Red : Color.Lime;
            cmdCalibrate.Text = Calibrating ? "Stop" : "Calibrate";
            if (Calibrating)
                PhaseEstimator.tauHeight = (double)nudHeight.Value / 100;
            foreach (PhaseEstimator phaseEstimator in phaseEstimators)
                if (Calibrating)
                    phaseEstimator.StartCalibrating();
                else
                    phaseEstimator.StopCalibrating();
            if (!Calibrating)
            {
                GUI.MoCap.StopCalibration();
                SaveCalibration();
                double mid = (phaseEstimators[0].Calibrated.standMid + phaseEstimators[1].Calibrated.standMid) / 2;
                FormVisual.FORMVISUAL.SetMid(mid);
                FormVisual.FORMVISUAL.MidVisibility(true);
                cmdStart.Enabled = true;
            }
            else
            {
                GUI.MoCap.StartCalibration();
                cmdStart.Enabled = false;
                FormVisual.FORMVISUAL.MidVisibility(false);
                int time = (int)nudCalibTime.Value;
                if (time != 0)
                {
                    CountDown("",time,"Calibrating");
                    System.Media.SystemSounds.Hand.Play();
                    cmdCalibrate_Click(null, null);
                }
            }
        }

        private void Treadmill_Load(object sender, EventArgs e)
        {
            Bounds = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height);
            if (Settings.TreadmillFile == "" || !System.IO.File.Exists(Settings.TreadmillFile))
                newToolStripMenuItem_Click(null, null);
            else
                LoadFile();
            LoadCalibration();
        }
        void SaveCalibration()
        {
            Settings.TreadmillCalibrationLeft = phaseEstimators[0].Calibrated;
            Settings.TreadmillCalibrationRight = phaseEstimators[1].Calibrated;
            Settings.Save();
        }
        void LoadCalibration()
        {
            phaseEstimators[0].Calibrated = Settings.TreadmillCalibrationLeft;
            phaseEstimators[1].Calibrated = Settings.TreadmillCalibrationRight;
            double mid = (phaseEstimators[0].Calibrated.standMid + phaseEstimators[1].Calibrated.standMid) / 2;
            FormVisual.FORMVISUAL.SetMid(mid);
            FormVisual.FORMVISUAL.MidVisibility(true);

            for (int i = 0; i < 2; i++)
            {
                stats[1, i + 1] = (phaseEstimators[i].Calibrated.beltSpeed / 1000 * 3600).ToString("0.00");
                stats[2, i + 1] = phaseEstimators[i].Calibrated.standHeight.ToString("0.00");
                stats[3, i + 1] = phaseEstimators[i].Calibrated.stepLength.ToString("0.00");
            }
            lblStats.Text = table2str(stats);
        }

        private void chkShowMid_CheckedChanged(object sender, EventArgs e)
        {
            FormVisual.FORMVISUAL.MidVisibility(chkShowMid.Checked);
            SaveFile();
        }

        private void trbLeftRight_Scroll(object sender, EventArgs e)
        {
            lblLeft.Text = "Left: " + (100 - trbLeftRight.Value) + "%";
            lblRight.Text = "Right: " + trbLeftRight.Value + "%";
            SaveFile();
        }

        private void nudHeight_ValueChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                InitialDirectory = Application.StartupPath,
                Filter = "Treadmill Settings (*.tread)|*.tread",
                Title = "Choose file for saving Treadmill-settings"
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Settings.TreadmillFile = sfd.FileName;
                Text = Settings.TreadmillFile;
                lstTargets.Items.Clear();
                System.Reflection.FieldInfo[] fields = typeof(TargetOptions).GetFields();
                for (int i = 0; i < fields.Length; i++)
                    pnlTarget.Controls[i * 2 + 1].Enabled = false;
                Settings.Save();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                InitialDirectory = Application.StartupPath,
                Filter = "Treadmill Settings (*.tread)|*.tread",
                Title = "Choose file containg Treadmill-settings"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Settings.TreadmillFile = ofd.FileName;
                Settings.Save();
                LoadFile();
            }
        }
        void LoadFile()
        {
            Text = Settings.TreadmillFile;
            txtPostfix.Text = System.IO.Path.GetFileNameWithoutExtension(Text);
            IO.XML settings = IO.XML.Read(Settings.TreadmillFile);
            lstTargets.Items.Clear();
            System.Reflection.FieldInfo[] fields = typeof(TargetOptions).GetFields();
            for (int i = 0; i < fields.Length; i++)
                pnlTarget.Controls[i * 2 + 1].Enabled = false;
            Ignore = true;

            trbLeftRight.Value = settings.getInt("LeftRight", 50);
            lblLeft.Text = "Left: " + (100 - trbLeftRight.Value) + "%";
            lblRight.Text = "Right: " + trbLeftRight.Value + "%";

            nudFrequencyMin.Value = settings.getInt("FrequencyMin", 2);
            nudFrequencyMax.Value = settings.getInt("FrequencyMax", 2);
            chkFixSpawn.Checked = settings.getBool("FixSpawn", false);
            nudHeight.Value = (decimal)settings.getDouble("Height", 2);
            chkMedianFilter.Checked = settings.getBool("MedianFilter", true);
            nudTimeout.Value = (decimal)settings.getDouble("Timeout", 0);
            chkStartTrigger.Checked = settings.getBool("StartTrigger", false);
            chkGroupRandom.Checked = settings.getBool("GroupRandom", false);
            chkShowMid.Checked = settings.getBool("ShowMid", true);

            IO.XML targets = settings.getElement("Targets");
            int nTargets = targets.getInt("nTargets", 0);
            for (int i = 0; i < nTargets; i++)
            {
                string json = targets.getString(i + "Target", "");
                if (json != "")
                    lstTargets.Items.Add(TargetOptions.FromJson(json));
            }

            Ignore = false;
            if (lstTargets.Items.Count > 0)
                lstTargets.SelectedIndex = 0;
        }
        string saveFile()
        {
            IO.XML xml = new IO.XML("Treadmill",
                new IO.XML("Treadfile", Settings.TreadmillFile),
                new IO.XML("LeftRight", trbLeftRight.Value.ToString()),
                new IO.XML("FrequencyMin", ((double)nudFrequencyMin.Value).ToStringD()),
                new IO.XML("FrequencyMax", ((double)nudFrequencyMax.Value).ToStringD()),
                new IO.XML("FixSpawn", chkFixSpawn.Checked.ToString()),
                new IO.XML("Height", ((double)nudHeight.Value).ToStringD()),
                new IO.XML("MedianFilter", chkMedianFilter.Checked.ToString()),
                new IO.XML("ShowMid", chkShowMid.Checked.ToString()),
                new IO.XML("Timeout", ((double)nudTimeout.Value).ToStringD()),
                new IO.XML("StartTrigger", chkStartTrigger.Checked.ToString()),
                new IO.XML("GroupRandom", chkGroupRandom.Checked.ToString())
                );

            int i = 0;
            IO.XML targets = new IO.XML("Targets");
            targets.addElement(new IO.XML("nTargets", lstTargets.Items.Count.ToString()));
            foreach (TargetOptions target in lstTargets.Items)
                targets.addElement(new IO.XML(i++.ToString() + "Target",target.ToJson()));
            xml.addElement(targets);

            return xml.ToString();
        }
        void SaveFile()
        {
            if (Ignore)
                return;
            System.IO.File.WriteAllText(Settings.TreadmillFile, saveFile());
        }

        private void nudFrequencyMin_ValueChanged(object sender, EventArgs e)
        {
            SaveFile();
        }
        private void nudFrequencyMax_ValueChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void nudActivate_ValueChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void nudDeactivate_ValueChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void nudRadius_ValueChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void chkHitSound_CheckedChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void chkDelay_CheckedChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void dgvDisturbances_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                SaveFile();
        }

        private void dgvDisturbances_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            SaveFile();
        }

        private void tmrTimeout_Tick(object sender, EventArgs e)
        {
            if (--Timeout <= 0)
                cmdStart_Click(null, null);
            else
            {
                cmdStart.Text = "Stop (" + Timeout + ")";
                FormVisual.FORMVISUAL.SetTime(((int)nudTimeout.Value - Timeout)* 100 / (int)nudTimeout.Value);
            }
        }

        private void nudTimeout_ValueChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        class TargetOptions
        {
            public string Name = "NoName";
            public int Probability = 100;
            public int Radius = 10;
            public int OffsetX = 0;
            public int OffsetY = 0;
            public int PctOffsetX = 0;
            public int PctOffsetY = 0;
            public int OffsetRadiusX = 0;
            public int PctOffsetRadiusX = 0;
            public int OffsetRadiusY = 0;
            public int PctOffsetRadiusY = 0;
            public int SpeedScale = 100;
            public Color HitColor = Color.Green;
            public Color NoHitColor = Color.Red;

            public enum ActivationModes { AfterStance,AfterSwing}
            public ActivationModes ActivationMode = ActivationModes.AfterStance;
            public bool CorrectOffsetActivation = true;
            public int PctActivate = 100;
            public Color ActivationColor = Color.Red;
            public int ActivationOffsetX = 0;
            public int ActivationOffsetY = 0;
            public int PctActivationOffsetX = 0;
            public int PctActivationOffsetY = 0;

            public bool CountScore = true;
            public bool HitSound = true;
            public bool ShowHit = true;
            public bool Bullseye = true;
            public override string ToString()
            {
                return Name;
            }
            public static TargetOptions FromJson(string json)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<TargetOptions>(json);
            }
            public string ToJson()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }
            public void Create(double x,double y,double speed,PhaseEstimator phaseEstimator, int TargetType)
            {
                bool left = phaseEstimator.ID == 0;
                double stepLength = phaseEstimator.Calibrated.stepLength;
                double xOffset = (left ? -1 : 1f)* (OffsetX / 100.0+(PctOffsetX/100.0* stepLength));
                double yOffset = OffsetY / 100.0 + (PctOffsetY / 100.0 * stepLength);

                if(OffsetRadiusX != 0 || PctOffsetRadiusX != 0 || OffsetRadiusY != 0 || PctOffsetRadiusY != 0)
                {
                    double v = rnd.NextDouble() * 2 * Math.PI;
                    double r = rnd.NextDouble();
                    xOffset += r * Math.Cos(v) * (OffsetRadiusX / 100.0 + PctOffsetRadiusX / 100.0 * stepLength);
                    yOffset += r * Math.Sin(v) * (OffsetRadiusY / 100.0 + PctOffsetRadiusY / 100.0 * stepLength);
                }

                x += xOffset;
                y += yOffset;

                speed *= SpeedScale / 100.0;

                double xActivationOffset = (left ? -1 : 1f) * (ActivationOffsetX / 100.0 + (PctActivationOffsetX / 100.0 * stepLength));
                double yActivationOffset = ActivationOffsetY / 100.0 + (PctActivationOffsetY / 100.0 * stepLength);

                y += speed * phaseEstimator.Calibrated.cycleSeconds;
                FormVisual.Target target;
                if (Bullseye)
                    target = new FormVisual.Bullseye(new PointF((float)x, (float)y), new PointF(0, -(float)speed), Radius / 100.0, TargetType, left ? 0 : 1)
                    {
                        SP1 = HitSound ? new System.Media.SoundPlayer(Properties.Resources.hit) : null,
                        SP2 = HitSound ? new System.Media.SoundPlayer(Properties.Resources.hit2) : null,
                        SP3 = HitSound ? new System.Media.SoundPlayer(Properties.Resources.hit3) : null,
                    };
                else
                    target = new FormVisual.Box(new PointF((float)x, (float)y), new PointF(0, -(float)speed), Radius / 100.0, TargetType, left ? 0 : 1)
                    {
                        NoHitColor = NoHitColor,
                        HitColor = HitColor,
                        ActivationColor = ActivationColor,
                        SP = HitSound ? new System.Media.SoundPlayer(Properties.Resources.hit) : null,
                    };
                target.ActivationOffset = new PointF((float)xActivationOffset, (float)yActivationOffset);
                target.CountScore = CountScore;
                EventHandler<FormVisual.VisualMarker> shoot = null;
                shoot = (s, e) =>
                {
                    target.Shoot(e, ShowHit);
                    phaseEstimator.StandStarted -= shoot;
                };
                phaseEstimator.StandStarted += shoot;

                Timer tmrActivate = new Timer();
                int interval = 0;
                switch(ActivationMode)
                {
                    case ActivationModes.AfterStance:
                        interval = (int)(phaseEstimator.Calibrated.standSeconds * PctActivate / 100 * 1000);
                        break;
                    case ActivationModes.AfterSwing:
                        interval = (int)((phaseEstimator.Calibrated.standSeconds + phaseEstimator.Calibrated.swingSeconds * PctActivate / 100) * 1000);
                        break;
                }
                if (CorrectOffsetActivation)
                    interval += (int)(yOffset / (phaseEstimator.Calibrated.stepLength / phaseEstimator.Calibrated.swingSeconds) * 1000);
                if (interval <= 0)
                    tmrActivate.Interval = 1;
                else
                    tmrActivate.Interval = interval;
                tmrActivate.Tick += (tmr_s, tmr_e) =>
                {
                    target.Activate();
                    tmrActivate.Stop();
                    tmrActivate = null;
                };
                tmrActivate.Start();
            }
        }
        void Change(System.Reflection.FieldInfo fieldInfo, object value)
        {
            if (Ignore)
                return;
            int[] I = lstTargets.SelectedIndices.Cast<int>().ToArray();
            foreach (int i in I)
            {
                fieldInfo.SetValue(lstTargets.Items[i], value);
                lstTargets.Items[i] = lstTargets.Items[i];
            }
            for (int i = 0; i < lstTargets.Items.Count; i++)
                lstTargets.SetSelected(i, I.Contains(i));
            SaveFile();
        }
        void PopulatePanel<T>(Panel pnl)
        {
            pnl.Controls.Clear();

            int h = 20;
            int hSpace = 12;
            System.Reflection.FieldInfo[] fields = typeof(T).GetFields();
            TargetOptions targetOptions = new TargetOptions();
            for(int i = 0; i < fields.Length;i++)
            {
                pnl.Controls.Add(new Label()
                {
                    Size = new Size(pnl.Width / 3, h),
                    Location = new Point(12, (h + hSpace) * i + 12),
                    Text = fields[i].Name,
                });
                object value = fields[i].GetValue(targetOptions);
                Control control = null;
                if (value.GetType() == typeof(string))
                {
                    TextBox txt = new TextBox();
                    txt.TextChanged += (nud_s, nud_e) =>
                    {
                        Change((System.Reflection.FieldInfo)txt.Tag, txt.Text);
                    };
                    control = txt;
                }
                else if (value.GetType() == typeof(int))
                {
                    NumericUpDown nud = new NumericUpDown()
                    {
                        Minimum = -200,
                        Maximum = 200,
                        Value = (int)value,
                    };
                    nud.MouseWheel += (nud_s, nud_e) =>
                    {
                        ((HandledMouseEventArgs)nud_e).Handled = true;
                    };
                    nud.ValueChanged += (nud_s, nud_e) =>
                    {
                        Change((System.Reflection.FieldInfo)nud.Tag, (int)nud.Value);
                    };
                    control = nud;
                }
                else if (value.GetType() == typeof(bool))
                {
                    CheckBox chk = new CheckBox()
                    {
                        Checked = (bool)value,
                    };
                    chk.CheckedChanged += (chk_s, chk_e) =>
                    {
                        Change((System.Reflection.FieldInfo)chk.Tag, chk.Checked);
                    };
                    control = chk;
                }
                else if (value.GetType() == typeof(Color))
                {
                    ComboBox cmb = new ComboBox();
                    cmb.Items.Add(Color.Red);
                    cmb.Items.Add(Color.Green);
                    cmb.Items.Add(Color.Blue);
                    cmb.Items.Add(Color.Magenta);
                    cmb.Items.Add(Color.Cyan);
                    cmb.Items.Add(Color.Yellow);
                    cmb.Items.Add(Color.Transparent);
                    cmb.Items.Add(Color.Black);
                    cmb.SelectedItem = (Color)value;
                    cmb.SelectedIndexChanged += (chk_s, chk_e) =>
                    {
                        Change((System.Reflection.FieldInfo)cmb.Tag, cmb.SelectedItem);
                    };
                    cmb.MouseWheel += (chk_s, chk_e) =>
                    {
                        ((HandledMouseEventArgs)chk_e).Handled = true;
                    };
                    control = cmb;
                }
                else if(value.GetType() == typeof(TargetOptions.ActivationModes))
                {
                    ComboBox cmb = new ComboBox();
                    foreach(TargetOptions.ActivationModes mode in Enum.GetValues(typeof(TargetOptions.ActivationModes)))
                        cmb.Items.Add(mode);
                    cmb.SelectedItem = (TargetOptions.ActivationModes)value;
                    cmb.SelectedIndexChanged += (chk_s, chk_e) =>
                    {
                        Change((System.Reflection.FieldInfo)cmb.Tag, cmb.SelectedItem);
                    };
                    cmb.MouseWheel += (chk_s, chk_e) =>
                    {
                        ((HandledMouseEventArgs)chk_e).Handled = true;
                    };
                    control = cmb;
                }
                if(control != null)
                {
                    control.Tag = Tag = fields[i];
                    control.Size = new Size(pnl.Width / 3, h);
                    control.Location = new Point((int)(12 + pnl.Width / 3 * 1.5), (h + hSpace) * i + 12);
                    control.Enabled = false;
                    pnl.Controls.Add(control);
                }
            }
        }

        private void lstTargets_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Reflection.FieldInfo[] fields = typeof(TargetOptions).GetFields();
            if (lstTargets.SelectedIndices.Count != 0)
            {
                for (int i = 0; i < fields.Length; i++)
                    pnlTarget.Controls[i * 2 + 1].Enabled = true;
            }
            if (lstTargets.SelectedIndices.Count == 1)
            {
                Ignore = true;
                TargetOptions targetOptions = (TargetOptions)lstTargets.SelectedItem;
                for (int i = 0; i < fields.Length; i++)
                {
                    Control control = pnlTarget.Controls[i * 2 + 1];
                    if (control.GetType() == typeof(TextBox))
                        ((TextBox)control).Text = (string)fields[i].GetValue(targetOptions);
                    else if (control.GetType() == typeof(NumericUpDown))
                        ((NumericUpDown)control).Value = (int)fields[i].GetValue(targetOptions);
                    else if (control.GetType() == typeof(CheckBox))
                        ((CheckBox)control).Checked = (bool)fields[i].GetValue(targetOptions);
                    else if (control.GetType() == typeof(ComboBox))
                        ((ComboBox)control).SelectedItem = fields[i].GetValue(targetOptions);
                }
                Ignore = false;
            }
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            lstTargets.Items.Add(new TargetOptions());
            lstTargets.ClearSelected();
            lstTargets.SetSelected(lstTargets.Items.Count - 1, true);
            SaveFile();
        }

        List<string> copiedTargetOptions = new List<string>();
        private void lstTargets_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lstTargets.SelectedIndices.Count != 0)
            {
                int[] I = lstTargets.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToArray();
                foreach (int i in I)
                    lstTargets.Items.RemoveAt(i);
                SaveFile();
            }
            else if (e.Control && e.KeyCode == Keys.A) // Select all
                for (int i = 0; i < lstTargets.Items.Count; i++)
                    lstTargets.SetSelected(i, true);
            else if (e.Control && e.KeyCode == Keys.C) // Copy selected
            {
                copiedTargetOptions.Clear();
                foreach (TargetOptions targetOptions in lstTargets.SelectedItems)
                    copiedTargetOptions.Add(targetOptions.ToJson());
            }
            else if (e.Control && e.KeyCode == Keys.V) // Add to end of list
            {
                foreach (string str in copiedTargetOptions)
                    lstTargets.Items.Add(TargetOptions.FromJson(str));
                SaveFile();
            }
        }

        private void grpTargets_Resize(object sender, EventArgs e)
        {
            pnlTarget.Height = grpTargets.Height - 24;
            lstTargets.Height = grpTargets.Height - 63;
        }

        private void Treadmill_Resize(object sender, EventArgs e)
        {
            grpTargets.Height = Height/3;
            grpStart.Top = grpTargets.Bottom + 12;
        }

        long globalTrigger = 0;
        Timer tmrTrigger = null;
        private void cmdGlobalTrigger_Click(object sender, EventArgs e)
        {
            if(globalTrigger == 0 || MessageBox.Show("A global trigger has already been sent!\r\nDo you want to send a new trigger?", "Overwrite global trigger?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                globalTrigger = DateTime.Now.Ticks;
                if (tmrTrigger == null)
                {
                    tmrTrigger = new Timer();
                    tmrTrigger.Interval = 1000;
                    tmrTrigger.Tick += (s, tmr_e) =>
                      {
                          TimeSpan diff = (DateTime.Now - new DateTime(globalTrigger));
                          cmdGlobalTrigger.Text = "Globaltrigger\r\n" + diff.Hours.ToString("D2") + ":" + diff.Minutes.ToString("D2") + ":" + diff.Seconds.ToString("D2");
                      };
                    tmrTrigger.Start();
                }
                GUI.Trigger.SendTrigger();
            }
        }

        private void chkGroupRandom_CheckedChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void chkStartTrigger_CheckedChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void chkFixSpawn_CheckedChanged(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void chkMedianFilter_CheckedChanged(object sender, EventArgs e)
        {
            PhaseEstimator.FilterCycles = chkMedianFilter.Checked;
            SaveFile();
        }
    }
}