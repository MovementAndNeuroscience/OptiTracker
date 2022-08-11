using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MDOL;
namespace Optitracker.GUI
{
    public partial class MoCap : UserControl, tabInterface
    {
        int SelectedMarker = -1;
        public static MoCapSystem moCapSystem = null;
        MoCapSystem.Frame.Marker[] MoCapMarkers = new MoCapSystem.Frame.Marker[0];
        public static readonly FormVisual frmVisual = new FormVisual();
        public static string[] KnownMarkers = null;
        Scene.Primitive.PointCloud pclMarkers;
        Scene.Primitive.Line pclSkeleton;

        enum MoCapSystems { TreadmillSimulator, Qualisys, Motive, Simulator, MouseSimulator }

        public MoCap()
        {
            InitializeComponent();

            FormResizer.FillParent(this, grpMocapSystem);
            FormResizer.FillParent(grpMocapSystem, scene);
            
            scene.TransformMode = Scene.SceneControl.TransformModes.OrigoFixed;

            cmbMode.Items.Add(MoCapSystem.BodyModel.Modes.LeftRight);
            cmbMode.Items.Add(MoCapSystem.BodyModel.Modes.Relative);
            cmbMode.Items.Add(MoCapSystem.BodyModel.Modes.Absolute);
            cmbMode.SelectedItem = Settings.BodyModel.BodyMode;
            cmbMode.SelectedIndexChanged += (s, e) =>
            {
                if (cmbMode.SelectedItem != null)
                {
                    MoCapSystem.BodyModel.Modes mode = (MoCapSystem.BodyModel.Modes)cmbMode.SelectedItem;
                    if (mode == MoCapSystem.BodyModel.Modes.Absolute && !Settings.BodyModel.Calibrated)
                    { 
                        cmbMode.SelectedItem = MoCapSystem.BodyModel.Modes.Relative;
                        MessageBox.Show("You need to calibrate with Left/Right or Relative before you can use Absolute");
                    }
                    Settings.BodyModel.BodyMode = mode;
                    Settings.Save();
                }
            };

            chkTrack.Checked = Settings.BodyModel.TrackOnError;
            chkTrack.CheckedChanged += (s, e) =>
            {
                Settings.BodyModel.TrackOnError = chkTrack.Checked;
                Settings.Save();
            };

            nudIgnoreZ.Value = Settings.BodyModel.IgnoreZ;
            nudIgnoreZ.ValueChanged += (s, e) =>
            {
                Settings.BodyModel.IgnoreZ = (int)nudIgnoreZ.Value;
                Settings.Save();
            };

            if (Settings.MainCamera == null)
            {
                Settings.MainCamera = scene.GetMainCamera();
                Settings.Save();
            }
            else
                scene.SetMainCamera(Settings.MainCamera);
            scene.MainCameraChanged += (s, e) =>
            {
                Settings.MainCamera = e;
                Settings.Save();
            };

            pclMarkers = new Scene.Primitive.PointCloud(0)
            {
                PointSize = 10
            };
            pclSkeleton = new Scene.Primitive.Line(new double[3], 255, 0, 0);
            scene.AddSceneObject(pclMarkers);
            scene.AddSceneObject(pclSkeleton);

            lstKnownMarkers.SelectedIndexChanged += (s, e) =>
            {
                if (SelectedMarker == lstKnownMarkers.SelectedIndex)
                    lstKnownMarkers.SelectedIndex = -1;
                SelectedMarker = lstKnownMarkers.SelectedIndex;
            };
            cmdCalibrateBody.Click += (s, e) =>
              {
                  if (moCapSystem != null)
                  {
                      if (!Calibrating)
                          StartCalibration();
                      else
                          StopCalibration();
                  }
                  else
                      MessageBox.Show("No system connected");
              };
            cmdConnectSystem.Click += (s, e) =>
              {
                  string[] strButtons = Enum.GetNames(typeof(MoCapSystems));
                  Button[] buttons = new Button[strButtons.Length];
                  Form form = new Form()
                  {
                      Width = 120,
                      Height = strButtons.Length * (30 + 3) + 20 + 40,
                      StartPosition = FormStartPosition.CenterParent
                  };

                  for (int i = 0; i < strButtons.Length; i++)
                  {
                      int id = i;
                      Button button = new Button()
                      {
                          Bounds = new Rectangle(10, (30 + 3) * i + 10, 100, 30),
                          Text = strButtons[i],
                          Enabled = false
                      };
                      button.Click += (button_s, button_e) =>
                      {
                          SetMoCapSystem((MoCapSystems)id, button.Tag);
                          form.Close();
                      };
                      form.Controls.Add(button);
                      buttons[i] = button;
                  }
                  BackgroundWorker BG = new BackgroundWorker();
                  BG.WorkerReportsProgress = true;
                  BG.WorkerSupportsCancellation = true;
                  BG.DoWork += (BG_s, BG_e) =>
                  {
                      while (!BG.CancellationPending)
                      {
                          BG.ReportProgress((int)MoCapSystems.TreadmillSimulator);
                          BG.ReportProgress((int)MoCapSystems.MouseSimulator);
                          BG.ReportProgress((int)MoCapSystems.Motive);

                        // Search for MikkelSimulatorFiles
                        if (System.IO.Directory.Exists(Application.StartupPath + "/Simulators") && System.IO.Directory.GetFiles(Application.StartupPath + "/Simulators/", "*.bin").Length > 0)
                              BG.ReportProgress((int)MoCapSystems.Simulator);

                        // Search for Qualisys
                        System.Threading.Thread.Sleep(500);
                          string[] ips = MoCapSystem.Qualisys.FindServers();
                          if (ips.Length != 0)
                              BG.ReportProgress((int)MoCapSystems.Qualisys, ips[0]);
                          else
                              BG.ReportProgress((int)MoCapSystems.Qualisys);
                      }
                  };
                  BG.ProgressChanged += (BG_s, BG_e) =>
                  {
                      buttons[BG_e.ProgressPercentage].Enabled = true;
                      if (BG_e.ProgressPercentage == (int)MoCapSystems.Qualisys)
                      {
                          if (BG_e.UserState == null)
                              buttons[BG_e.ProgressPercentage].Text += "_Manual";
                          buttons[BG_e.ProgressPercentage].Tag = BG_e.UserState;
                      }
                  };
                  BG.RunWorkerAsync();
                  form.ShowDialog();
                  BG.CancelAsync();
              };
        }

        void SetMoCapSystem(MoCapSystems MoCapSystem, object Tag)
        {
            if (moCapSystem != null)
            {
                moCapSystem.FrameReceived -= MoCapSystem_FrameReceived;
                moCapSystem.Disconnect();
            }
            switch (MoCapSystem)
            {
                case MoCapSystems.TreadmillSimulator:
                    moCapSystem = new MoCapSystem.TreadmillSimulator();
                    break;
                case MoCapSystems.Qualisys:
                    if (Tag != null)
                        moCapSystem = new MoCapSystem.Qualisys((string)Tag);
                    else
                    {
                        string ip = MDOL.InputForm.InputText("No server was found - input ip manually");
                        if (ip == null)
                            return;
                        moCapSystem = new MoCapSystem.Qualisys(ip);
                    }
                    break;
                case MoCapSystems.Motive:
                    moCapSystem = new MoCapSystem.Motive();
                    break;
                case MoCapSystems.Simulator:
                    string[] binFiles = System.IO.Directory.GetFiles(Application.StartupPath + "/Simulators/", "*.bin");
                    if (binFiles.Length == 1)
                        moCapSystem = new MoCapSystem.Simulator(binFiles[0]);
                    else
                    {
                        Button[] buttons = new Button[binFiles.Length];
                        Form form = new Form()
                        {
                            Width = 120,
                            Height = binFiles.Length * (30 + 3) + 20 + 40,
                            StartPosition = FormStartPosition.CenterParent
                        };

                        for (int i = 0; i < binFiles.Length; i++)
                        {
                            int id = i;
                            Button button = new Button()
                            {
                                Bounds = new Rectangle(10, (30 + 3) * i + 10, 100, 30),
                                Text = System.IO.Path.GetFileNameWithoutExtension(binFiles[i]),
                            };
                            button.Click += (button_s, button_e) =>
                            {
                                moCapSystem = new MoCapSystem.Simulator(binFiles[id]);
                                form.Close();
                            };
                            form.Controls.Add(button);
                            buttons[i] = button;
                        }
                        form.ShowDialog();
                    }
                    break;
                case MoCapSystems.MouseSimulator:
                    moCapSystem = new MoCapSystem.MouseSimulator();
                    break;
            }
            if (moCapSystem != null)
            {
                moCapSystem.populateSettings(pnlSettings);
                cmdConnectSystem.ForeColor = Color.Green;
                cmdConnectSystem.Text = "Connected to " + MoCapSystem.ToString();
                KnownMarkers = moCapSystem.GetMarkers();
                lstKnownMarkers.Items.Clear();
                lstKnownMarkers.Items.AddRange(KnownMarkers);
                moCapSystem.FrameReceived += MoCapSystem_FrameReceived;
            }
        }
        static bool Calibrating = false;
        public static void StartCalibration()
        {
            Calibrating = true;
            moCapSystem.StartCalibration();
        }
        public static void StopCalibration()
        {
            Calibrating = false;
            moCapSystem.StopCalibration();
        }

        public void onLoad()
        {
        }
        public void Destroy()
        {
            moCapSystem?.Disconnect();
        }
        public string[] IsReady()
        {
            if (moCapSystem == null)
                return new string[] {"No system conneced"};
            return new string[0];
        }
        bool nMarkerUsed = false;
        public void UpdateGUI()
        {
            cmdCalibrateBody.BackColor = Calibrating ? Color.Red : Color.Green;
            lblCalibration.Text = Settings.BodyModel.ToString();
            if (moCapSystem != null)
            {
                MoCapSystem.Frame.Marker[] MoCapMarkers = (MoCapSystem.Frame.Marker[])this.MoCapMarkers.Clone();

                lstNMarkers.Items.Clear();
                nMarkerUsed = true;
                KeyValuePair<int, int>[] nMarkerScores = this.nMarkerScores.Select(nMarkerScore => nMarkerScore).OrderBy(nMarkerScore => nMarkerScore.Key).ToArray();
                nMarkerUsed = false;
                foreach (KeyValuePair<int, int> nMarkerScore in nMarkerScores)
                    lstNMarkers.Items.Add(nMarkerScore.Key + ": " + (100*nMarkerScore.Value/nMarkerHist.Length) + " %");
                lstNMarkers.BackColor = Helper.weigth2Color(1 - Extension.Sqr(Extension.Max(nMarkerScores.Select(nMarkerScore => (double)nMarkerScore.Value/ nMarkerHist.Length).ToArray())));
                pclMarkers.Update(MoCapMarkers.SelectMany(marker => new double[] { marker.X, marker.Y, marker.Z }).ToArray(), MoCapMarkers.SelectMany(marker => (int)marker.Joint != SelectedMarker ? new byte[] { 255, 255, 255 } : new byte[] { 255, 0, 128 }).ToArray());
                int[] skeleton = moCapSystem.GetSkeleton();
                List<double> skeletonVertices = new List<double>();
                for (int i = 0; i < skeleton.Length; i += 2)
                {
                    int i1 = skeleton[i];
                    int i2 = skeleton[i + 1];
                    MoCapSystem.Frame.Marker v1 = null, v2 = null;
                    for (int j = 0; j < MoCapMarkers.Length; j++)
                    {
                        if ((int)MoCapMarkers[j].Joint == i1)
                            v1 = MoCapMarkers[j];
                        else if ((int)MoCapMarkers[j].Joint == i2)
                            v2 = MoCapMarkers[j];
                    }
                    if (v1 != null && v2 != null)
                    {
                        skeletonVertices.Add(v1.X);
                        skeletonVertices.Add(v1.Y);
                        skeletonVertices.Add(v1.Z);
                        skeletonVertices.Add(v2.X);
                        skeletonVertices.Add(v2.Y);
                        skeletonVertices.Add(v2.Z);
                    }
                }
                pclSkeleton.UpdateVertices(skeletonVertices.ToArray());
                scene.DrawNow();
            }
        }

        Dictionary<int, int> nMarkerScores = new Dictionary<int, int>();
        int[] nMarkerHist = new int[500].Select(nMarker => -1).ToArray();
        int kMarkerHist = 0;
        private void MoCapSystem_FrameReceived(object sender, MoCapSystem.Frame e)
        {
            if (nMarkerUsed)
                return;
            int nMarkers = nMarkerHist[kMarkerHist];
            if (nMarkerScores.ContainsKey(nMarkers))
            {
                if (--nMarkerScores[nMarkers] <= 0)
                    nMarkerScores.Remove(nMarkers);
            }
            nMarkers = e.Markers.Length;
            nMarkerHist[kMarkerHist++] = nMarkers;
            if (kMarkerHist == nMarkerHist.Length)
                kMarkerHist = 0;
            if (nMarkerScores.ContainsKey(nMarkers))
                nMarkerScores[nMarkers]++;
            else
                nMarkerScores.Add(nMarkers, 1);

            if (e.Markers.Length == 0)
                return;
            MDOL.IO.DataSaver.AddPacket(new MoCapPacket.Frame(e));

            MoCapMarkers = (MoCapSystem.Frame.Marker[])e.Markers.Clone();
        }

        public abstract class MoCapSystem
        {
            public class BodyModel
            {
                public enum Modes { LeftRight, Relative, Absolute }
                public Modes BodyMode = Modes.LeftRight;
                public bool TrackOnError = false;
                public int IgnoreZ = 0;
                public enum Joints { Unknown = -1, Back, HipL, HipR, KneeL, KneeR, ToeL, ToeR, MalL, MalR, HeelL, HeelR, ThighL, ThighR };

                public int nMarkers = 11;
                List<double>[,] Calibrations = null;

                private bool calibrated = false;
                public bool Calibrated { get { return calibrated; } }
                private double MalHeel, MalToe, HeelToe;
                private double MalKnee, KneeHip;

                private double stdMalHeel, stdMalToe, stdHeelToe;
                private double stdMalKnee, stdKneeHip;
                public double FootError(KeyValuePair<double, int>[] A, KeyValuePair<double, int>[] B)
                {
                    return Math.Abs(A[0].Key - HeelToe) + Math.Abs(A[1].Key - MalToe) + Math.Abs(A[2].Key - MalHeel) +
                                Math.Abs(B[0].Key - HeelToe) + Math.Abs(B[1].Key - MalToe) + Math.Abs(B[2].Key - MalHeel);
                }

                public static BodyModel FromJson(string json)
                {
                    if (json != "")
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<BodyModel>(json);
                    else
                        return new BodyModel();
                }
                public string ToJson()
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(this);
                }

                public override string ToString()
                {
                    string[,] table = new string[6, 3];
                    table[0, 0] = "";
                    table[0, 1] = "CM";
                    table[0, 2] = "Std";
                    table[1, 0] = "MalToe";
                    table[1, 1] = MalToe.ToStringD(2);
                    table[1, 2] = "(" + stdMalToe.ToStringD(2) + ")";
                    table[2, 0] = "MalHeel";
                    table[2, 1] = MalHeel.ToStringD(2);
                    table[2, 2] = "(" + stdMalHeel.ToStringD(2) + ")";
                    table[3, 0] = "HeelToe";
                    table[3, 1] = HeelToe.ToStringD(2);
                    table[3, 2] = "(" + stdHeelToe.ToStringD(2) + ")";
                    table[4, 0] = "MalKnee";
                    table[4, 1] = MalKnee.ToStringD(2);
                    table[4, 2] = "(" + stdMalKnee.ToStringD(2) + ")";
                    table[5, 0] = "KneeHip";
                    table[5, 1] = KneeHip.ToStringD(2);
                    table[5, 2] = "(" + stdKneeHip.ToStringD(2) + ")";
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

                public void Start(MoCapSystem moCapSystem)
                {
                    int nJoints = 0;
                    foreach (Joints joint in Enum.GetValues(typeof(Joints)))
                        if (joint != Joints.Unknown)
                            nJoints++;
                    Calibrations = new List<double>[nJoints, nJoints];
                    for (int i = 0; i < nJoints; i++)
                        for (int j = 0; j < nJoints; j++)
                            Calibrations[i, j] = new List<double>();
                    moCapSystem.FrameReceived += MoCapSystem_FrameReceived;
                }

                private void MoCapSystem_FrameReceived(object sender, Frame e)
                {
                    foreach (Joints joint1 in Enum.GetValues(typeof(Joints)))
                        foreach (Joints joint2 in Enum.GetValues(typeof(Joints)))
                            if (joint1 != joint2 && joint1 != Joints.Unknown && joint2 != Joints.Unknown)
                            {
                                Frame.Marker marker1 = e.GetMarker((int)joint1);
                                Frame.Marker marker2 = e.GetMarker((int)joint2);
                                if (marker1 != null && marker2 != null)
                                {
                                    double d = marker1.Dist(marker2);
                                    Calibrations[(int)joint1, (int)joint2].Add(d);
                                    Calibrations[(int)joint2, (int)joint1].Add(d);
                                }
                            }
                }

                public void Stop(MoCapSystem moCapSystem)
                {
                    moCapSystem.FrameReceived -= MoCapSystem_FrameReceived;

                    stdMalHeel = Calibrations[(int)Joints.MalL, (int)Joints.HeelL].Concat(Calibrations[(int)Joints.MalR, (int)Joints.HeelR]).Std(out MalHeel);
                    stdMalToe = Calibrations[(int)Joints.MalL, (int)Joints.ToeL].Concat(Calibrations[(int)Joints.MalR, (int)Joints.ToeR]).Std(out MalToe);
                    stdHeelToe = Calibrations[(int)Joints.HeelL, (int)Joints.ToeL].Concat(Calibrations[(int)Joints.HeelR, (int)Joints.ToeR]).Std(out HeelToe);

                    stdMalKnee = Calibrations[(int)Joints.MalL, (int)Joints.KneeL].Concat(Calibrations[(int)Joints.MalR, (int)Joints.KneeR]).Std(out MalKnee);
                    stdKneeHip = Calibrations[(int)Joints.KneeL, (int)Joints.HipL].Concat(Calibrations[(int)Joints.KneeR, (int)Joints.HipR]).Std(out KneeHip);
                    calibrated = true;
                    Settings.Save();
                }
            }


            public class Frame
            {
                public class Marker : MDOL.IO.DataSaver.byteAble
                {
                    public BodyModel.Joints Joint = BodyModel.Joints.Unknown;
                    // float, s.t. ID and X,Y,Z use 4 bytes - faster import in Matlab
                    public float X;
                    public float Y;
                    public float Z;
                    public Marker(BodyModel.Joints Joint, float X, float Y, float Z)
                    {
                        this.Joint = Joint;
                        this.X = X;
                        this.Y = Y;
                        this.Z = Z;
                    }
                    public Marker(float X, float Y, float Z)
                    {
                        this.X = X;
                        this.Y = Y;
                        this.Z = Z;
                    }
                    public double DistSqr(Marker marker)
                    {
                        return (X - marker.X).Sqr() + (Y - marker.Y).Sqr() + (Z - marker.Z).Sqr();
                    }
                    public double Dist(Marker marker)
                    {
                        return Math.Sqrt(DistSqr(marker));
                    }
                    public static Marker operator -(Marker A, Marker B)
                    {
                        return new Marker(A.X - B.X, A.Y - B.Y, A.Z - B.Z);
                    }
                    public static Marker operator +(Marker A, Marker B)
                    {
                        return new Marker(A.X + B.X, A.Y + B.Y, A.Z + B.Z);
                    }
                    public Vector<Dimensions.III> ToVector()
                    {
                        return new Vector<Dimensions.III>(X, Y, Z);
                    }
                    public double Dot(Marker B)
                    {
                        return X * B.X + Y * B.Y + Z * B.Z;
                    }
                    public byte[] getBytes()
                    {
                        List<byte[]> bytes = new List<byte[]>();
                        bytes.Add(BitConverter.GetBytes((int)Joint));
                        bytes.Add(BitConverter.GetBytes(X));
                        bytes.Add(BitConverter.GetBytes(Y));
                        bytes.Add(BitConverter.GetBytes(Z));
                        return bytes.SelectMany(b => b).ToArray();
                    }
                }
                int[] Joints2I = new int[14].Select(i => -1).ToArray();

                public Marker[] Markers;
                public Marker[] ZMarkers;
                public Marker[] rawMarkers;
                public long Timestamp;

                static Marker[] falseMarkers = new Marker[0];
                static Marker[] falseMarkers2 = new Marker[0];
                static double nExpectedMarkers = 0;
                static long prevTimestamp = -1;
                public Frame(long Timestamp, Marker[] Markers)
                {
                    this.Timestamp = Timestamp;
                    rawMarkers = Markers;
                    if (Settings.BodyModel.IgnoreZ != 0)
                    {
                        ZMarkers = Markers.Where(marker => marker.Z >= (Settings.BodyModel.IgnoreZ / 100.0)).ToArray();
                        this.Markers = Markers.Where(marker => marker.Z < (Settings.BodyModel.IgnoreZ / 100.0)).ToArray();
                    }
                    else
                    {
                        this.Markers = Markers;
                        ZMarkers = new Marker[0];
                    }

                    if (Settings.BodyModel.TrackOnError)
                    {
                        if (Markers.Length >= Math.Ceiling(nExpectedMarkers))
                            nExpectedMarkers = Markers.Length;
                        else
                        {
                            KeyValuePair<int, double>[] minDist = new KeyValuePair<int, double>[falseMarkers.Length];
                            for (int i = 0; i < falseMarkers.Length; i++)
                            {
                                Marker marker1 = falseMarkers[i];
                                double dMin = double.MaxValue;
                                foreach (Marker marker2 in Markers)
                                {
                                    double d = marker1.DistSqr(marker2);
                                    if (d < dMin)
                                        dMin = d;
                                }
                                minDist[i] = new KeyValuePair<int, double>(i, dMin);
                            }
                            minDist = minDist.OrderByDescending(m => m.Value).ToArray();
                            Markers = Markers.Concat(minDist.Take((int)Math.Ceiling(nExpectedMarkers) - Markers.Length).Select(m =>
                            {
                                Marker match = falseMarkers[m.Key];
                                if (falseMarkers2.Length == falseMarkers.Length)
                                {
                                    int iMin = -1;
                                    double dMin = double.MaxValue;
                                    for (int i = 0; i < falseMarkers2.Length; i++)
                                    {
                                        double d = match.DistSqr(falseMarkers2[i]);
                                        if (d < dMin)
                                        {
                                            iMin = i;
                                            dMin = d;
                                        }
                                    }
                                    match = match + (match - falseMarkers2[iMin]);
                                }
                                return match;
                            })).ToArray();
                            if(Timestamp != prevTimestamp)
                                nExpectedMarkers -= 0.05;
                        }

                        this.Markers = Markers;
                        if (Timestamp != prevTimestamp)
                        {
                            falseMarkers2 = falseMarkers;
                            falseMarkers = this.Markers;
                        }
                        prevTimestamp = Timestamp;
                    }
                }
                public void IdentifyMarker(int i, BodyModel.Joints joint)
                {
                    Markers[i].Joint = joint;
                    Joints2I[(int)joint] = i;
                }

                public Marker GetMarker(BodyModel.Joints joint) { return GetMarker((int)joint); }
                public Marker GetMarker(int joint)
                {
                    if (Joints2I[joint] != -1)
                        return Markers[Joints2I[joint]];
                    else
                        return null;
                }

                class Feet
                {
                    KeyValuePair<double, int>[] A = new KeyValuePair<double, int>[3];
                    KeyValuePair<double, int>[] B = new KeyValuePair<double, int>[3];
                    public double Error;
                    public int[] Ia;
                    public int[] Ib;
                    public Feet(int[] I, double[,] Dist, bool useMetric = false)
                    {
                        // Create array of marker-marker distance, as well as opposite/unused marker-id
                        A[0] = new KeyValuePair<double, int>(Dist[I[0], I[1]], 2);
                        A[1] = new KeyValuePair<double, int>(Dist[I[1], I[2]], 0);
                        A[2] = new KeyValuePair<double, int>(Dist[I[2], I[0]], 1);
                        B[0] = new KeyValuePair<double, int>(Dist[I[3], I[4]], 5);
                        B[1] = new KeyValuePair<double, int>(Dist[I[4], I[5]], 3);
                        B[2] = new KeyValuePair<double, int>(Dist[I[5], I[3]], 4);

                        // Order in descending order - biggest distance should be Heel-Toe and smallest distance should be Mal-Toe
                        A = A.OrderByDescending(a => a.Key).ToArray();
                        B = B.OrderByDescending(b => b.Key).ToArray();
                        Ia = A.Select(a => I[a.Value]).ToArray();
                        Ib = B.Select(b => I[b.Value]).ToArray();

                        if (useMetric)
                            Error = Settings.BodyModel.FootError(A, B);
                        else
                            Error = Math.Abs(A[0].Key - B[0].Key) + Math.Abs(A[1].Key - B[1].Key) + Math.Abs(A[2].Key - B[2].Key);
                    }

                    public void IdentifyLeftRight(Frame frame)
                    {
                        // Identify Left/Right foot, based on the crossproduct - the leg should point upwards
                        Marker aMalToe = frame.Markers[Ia[2]] - frame.Markers[Ia[0]];
                        Marker aMalHeel = frame.Markers[Ia[1]] - frame.Markers[Ia[0]];
                        Marker bMalToe = frame.Markers[Ib[2]] - frame.Markers[Ib[0]];
                        Marker bMalHeel = frame.Markers[Ib[1]] - frame.Markers[Ib[0]];

                        double LR = (aMalToe.X * aMalHeel.Y - aMalToe.Y * aMalHeel.X) + (bMalHeel.X * bMalToe.Y - bMalHeel.Y * bMalToe.X);
                        double RL = (aMalHeel.X * aMalToe.Y - aMalHeel.Y * aMalToe.X) + (bMalToe.X * bMalHeel.Y - bMalToe.Y * bMalHeel.X);
                        if (LR > RL) // First is right foot
                        {
                            frame.IdentifyMarker(Ia[0], BodyModel.Joints.MalR);
                            frame.IdentifyMarker(Ia[1], BodyModel.Joints.HeelR);
                            frame.IdentifyMarker(Ia[2], BodyModel.Joints.ToeR);
                            frame.IdentifyMarker(Ib[0], BodyModel.Joints.MalL);
                            frame.IdentifyMarker(Ib[1], BodyModel.Joints.HeelL);
                            frame.IdentifyMarker(Ib[2], BodyModel.Joints.ToeL);
                        }
                        else
                        {
                            frame.IdentifyMarker(Ia[0], BodyModel.Joints.MalL);
                            frame.IdentifyMarker(Ia[1], BodyModel.Joints.HeelL);
                            frame.IdentifyMarker(Ia[2], BodyModel.Joints.ToeL);
                            frame.IdentifyMarker(Ib[0], BodyModel.Joints.MalR);
                            frame.IdentifyMarker(Ib[1], BodyModel.Joints.HeelR);
                            frame.IdentifyMarker(Ib[2], BodyModel.Joints.ToeR);
                        }
                    }
                }

                double[,] Dist = new double[6, 6];
                int[][] FeetGroups = {
                    new int[]{0, 1, 2, 3, 4, 5},
                    new int[]{0, 1, 3, 2, 4, 5},
                    new int[]{0, 1, 4, 2, 3, 5},
                    new int[]{0, 1, 5, 2, 3, 4},
                    new int[]{0, 2, 3, 1, 4, 5},
                    new int[]{0, 2, 4, 1, 3, 5},
                    new int[]{0, 2, 5, 1, 3, 4},
                    new int[]{0, 3, 4, 1, 2, 5},
                    new int[]{0, 3, 5, 1, 2, 4},
                    new int[]{0, 4, 5, 1, 2, 3}
                };
                public double FeetError
                {
                    get
                    {
                        Marker[] MalHeelToe = new Marker[6];
                        MalHeelToe[0] = GetMarker(BodyModel.Joints.MalL);
                        MalHeelToe[1] = GetMarker(BodyModel.Joints.MalR);
                        MalHeelToe[2] = GetMarker(BodyModel.Joints.HeelL);
                        MalHeelToe[3] = GetMarker(BodyModel.Joints.HeelR);
                        MalHeelToe[4] = GetMarker(BodyModel.Joints.ToeL);
                        MalHeelToe[5] = GetMarker(BodyModel.Joints.ToeR);
                        if (MalHeelToe.All(marker => marker != null))
                        {
                            double MalHeelL = MalHeelToe[0].Dist(MalHeelToe[2]);
                            double MalHeelR = MalHeelToe[1].Dist(MalHeelToe[3]);
                            double MalToeL = MalHeelToe[0].Dist(MalHeelToe[4]);
                            double MalToeR = MalHeelToe[1].Dist(MalHeelToe[5]);
                            double HeelToeL = MalHeelToe[2].Dist(MalHeelToe[4]);
                            double HeelToeR = MalHeelToe[3].Dist(MalHeelToe[5]);
                            if (MalHeelL < MalToeL && MalHeelR < MalToeR)
                                return Math.Abs(MalHeelL - MalHeelR) +
                                    Math.Abs(MalToeL - MalToeR) +
                                    Math.Abs(HeelToeL - HeelToeR);
                            else
                                return double.PositiveInfinity;
                        }
                        else
                            return double.PositiveInfinity;
                    }
                }

                public void Identify()
                {
                    Markers = Markers.OrderBy(marker => marker.Z).ToArray();
                    switch (Markers.Length)
                    {
                        case 1:
                            IdentifyMarker(0, BodyModel.Joints.ToeL);
                            break;
                        case 2:
                            // Sort from left to right
                            Markers = Markers.OrderBy(marker => marker.X).ToArray();
                            IdentifyMarker(0, BodyModel.Joints.ToeL);
                            IdentifyMarker(1, BodyModel.Joints.ToeR);
                            break;
                        default:
                            if (Markers.Length >= 6)
                            {
                                int[] Feet = new int[] { 0, 1, 2, 3, 4, 5 };
                                int[] LFoot, RFoot;
                                switch (Settings.BodyModel.BodyMode)
                                {
                                    case BodyModel.Modes.LeftRight:
                                        // Feet
                                        // Sort from left to right
                                        Feet = Feet.OrderBy(i => Markers[i].X).ToArray();
                                        // Split into left/right and order from back to front
                                        LFoot = Feet.Take(3).OrderBy(i => Markers[i].Y).ToArray();
                                        Feet = Feet.Reverse().ToArray();
                                        RFoot = Feet.Take(3).OrderBy(i => Markers[i].Y).ToArray();
                                        // Identify
                                        IdentifyMarker(LFoot[0], BodyModel.Joints.HeelL);
                                        IdentifyMarker(LFoot[1], BodyModel.Joints.MalL);
                                        IdentifyMarker(LFoot[2], BodyModel.Joints.ToeL);
                                        IdentifyMarker(RFoot[0], BodyModel.Joints.HeelR);
                                        IdentifyMarker(RFoot[1], BodyModel.Joints.MalR);
                                        IdentifyMarker(RFoot[2], BodyModel.Joints.ToeR);
                                        break;
                                    case BodyModel.Modes.Relative:
                                    case BodyModel.Modes.Absolute:
                                        // Feet
                                        for (int i = 0; i < 6; i++)
                                        {
                                            Marker iMarker = Markers[i];
                                            for (int j = i + 1; j < 6; j++)
                                                Dist[i, j] = Dist[j, i] = iMarker.Dist(Markers[j]);
                                        }
                                        Feet[] Feets = FeetGroups.Select(feetgroup => new Feet(feetgroup, Dist, Settings.BodyModel.BodyMode == BodyModel.Modes.Absolute)).OrderBy(feet => feet.Error).ToArray();
                                        Feets[0].IdentifyLeftRight(this);
                                        break;
                                }
                            }
                            if (Markers.Length >= 11)
                            {
                                int[] Knees = new int[2] { 6, 7 };
                                int[] Hips = new int[2] { Markers.Length - 2, Markers.Length - 3 };
                                IdentifyMarker(Markers.Length - 1, BodyModel.Joints.Back);
                                switch (Settings.BodyModel.BodyMode)
                                {
                                    case BodyModel.Modes.LeftRight:
                                        // Knees
                                        // Sort from left to right
                                        Knees = Knees.OrderBy(i => Markers[i].X).ToArray();
                                        IdentifyMarker(Knees[0], BodyModel.Joints.KneeL);
                                        IdentifyMarker(Knees[1], BodyModel.Joints.KneeR);

                                        Hips = Hips.OrderBy(i => Markers[i].X).ToArray();
                                        IdentifyMarker(Hips[0], BodyModel.Joints.HipL);
                                        IdentifyMarker(Hips[1], BodyModel.Joints.HipR);
                                        break;
                                    case BodyModel.Modes.Relative:
                                    case BodyModel.Modes.Absolute:
                                        Marker back = Markers.Last();
                                        // Hips
                                        Marker back0 = Markers[Hips[0]] - back;
                                        Marker back1 = Markers[Hips[1]] - back;
                                        if (back0.X * back1.Y - back0.Y * back1.X > 0) // First is right hip
                                        {
                                            IdentifyMarker(Hips[0], BodyModel.Joints.HipR);
                                            IdentifyMarker(Hips[1], BodyModel.Joints.HipL);
                                        }
                                        else
                                        {
                                            IdentifyMarker(Hips[0], BodyModel.Joints.HipL);
                                            IdentifyMarker(Hips[1], BodyModel.Joints.HipR);
                                        }
                                        // Knees
                                        Marker HipL = GetMarker(BodyModel.Joints.HipL);
                                        Marker Right = GetMarker(BodyModel.Joints.HipR) - HipL;
                                        Knees = Knees.OrderBy(i => (Markers[i] - HipL).Dot(Right)).ToArray();
                                        IdentifyMarker(Knees[0], BodyModel.Joints.KneeL);
                                        IdentifyMarker(Knees[1], BodyModel.Joints.KneeR);
                                        break;
                                }
                            }
                            break;
                    }
                }
                Marker[] identifiedPrev = new Marker[0];
                JointHistory[] JointHistories = new JointHistory[Enum.GetValues(typeof(BodyModel.Joints)).Length-1];
                class JointHistory
                {
                    const int HIST = 5;
                    int kHist = 0;
                    int[] Type = new int[HIST];
                    float[][] Position = new float[HIST][];
                    public void Add(float[] Position,int Type)
                    {
                        this.Position[kHist] = Position;
                        this.Type[kHist++] = Type;
                        if (kHist == HIST)
                            kHist = 0;
                    }
                }
            }


            ~MoCapSystem()
            {
                Disconnect();
            }

            public abstract event EventHandler<Frame> FrameReceived;
            public virtual string[] GetMarkers()
            {
                return Enum.GetNames(typeof(BodyModel.Joints));
            }
            public virtual int[] GetSkeleton()
            {
                if (Settings.BodyModel.nMarkers == 13)
                    return new int[]{
                        (int)BodyModel.Joints.ToeL,(int)BodyModel.Joints.MalL,
                        (int)BodyModel.Joints.HeelL,(int)BodyModel.Joints.MalL,
                        (int)BodyModel.Joints.MalL,(int)BodyModel.Joints.KneeL,
                        (int)BodyModel.Joints.KneeL,(int)BodyModel.Joints.ThighL,
                        (int)BodyModel.Joints.ThighL,(int)BodyModel.Joints.HipL,
                        (int)BodyModel.Joints.HipL,(int)BodyModel.Joints.Back,
                        (int)BodyModel.Joints.Back,(int)BodyModel.Joints.HipR,
                        (int)BodyModel.Joints.HipR,(int)BodyModel.Joints.ThighR,
                        (int)BodyModel.Joints.ThighR,(int)BodyModel.Joints.KneeR,
                        (int)BodyModel.Joints.KneeR,(int)BodyModel.Joints.MalR,
                        (int)BodyModel.Joints.MalR,(int)BodyModel.Joints.HeelR,
                        (int)BodyModel.Joints.MalR,(int)BodyModel.Joints.ToeR
                    };
                else
                    return new int[]{
                        (int)BodyModel.Joints.ToeL,(int)BodyModel.Joints.MalL,
                        (int)BodyModel.Joints.HeelL,(int)BodyModel.Joints.MalL,
                        (int)BodyModel.Joints.MalL,(int)BodyModel.Joints.KneeL,
                        (int)BodyModel.Joints.KneeL,(int)BodyModel.Joints.HipL,
                        (int)BodyModel.Joints.HipL,(int)BodyModel.Joints.Back,
                        (int)BodyModel.Joints.Back,(int)BodyModel.Joints.HipR,
                        (int)BodyModel.Joints.HipR,(int)BodyModel.Joints.KneeR,
                        (int)BodyModel.Joints.KneeR,(int)BodyModel.Joints.MalR,
                        (int)BodyModel.Joints.MalR,(int)BodyModel.Joints.HeelR,
                        (int)BodyModel.Joints.MalR,(int)BodyModel.Joints.ToeR
                    };
            }
            public void StartCalibration()
            {
                Settings.BodyModel.Start(this);
            }

            public void StopCalibration()
            {
                Settings.BodyModel.Stop(this);
                Settings.Save();
            }
            public abstract void Disconnect();
            public void populateSettings(Panel pnlSettings)
            {
                pnlSettings.Controls.Clear();
                List<KeyValuePair<string, Control>> settings = getSettings();
                if (settings.Count > 0)
                {
                    int h = 20;
                    int space = 12;
                    for (int i = 0; i < settings.Count; i++)
                    {
                        pnlSettings.Controls.Add(settings[i].Value);
                        settings[i].Value.Top = (h + space) * i + 12;
                        settings[i].Value.Height = h;
                        if (settings[i].Key == "")
                        {
                            settings[i].Value.Left = 12;
                            settings[i].Value.Width = pnlSettings.Width - 12 * 2;
                        }
                        else
                        {
                            int width = (pnlSettings.Width - 12 * 3) / 2;
                            pnlSettings.Controls.Add(new Label()
                            {
                                Size = new Size(width, h),
                                Location = new Point(12, (h + space) * i + 12),
                                Text = settings[i].Key
                            });
                            settings[i].Value.Left = 12 + width + 12;
                            settings[i].Value.Width = width;
                        }
                    }
                }
            }
            public virtual List<KeyValuePair<string, Control>> getSettings() { return new List<KeyValuePair<string, Control>>(); }
            protected virtual double[] stepPrepare(Frame frame)
            {
                Frame.Marker ToeL = frame.GetMarker((int)BodyModel.Joints.ToeL);
                Frame.Marker ToeR = frame.GetMarker((int)BodyModel.Joints.ToeR);
                double[] LeftRight = new double[6];
                if (frame.Markers.Length == 1)
                    ToeR = ToeL;
                if (ToeL != null)
                {
                    LeftRight[0] = ToeL.X;
                    LeftRight[1] = ToeL.Y;
                    LeftRight[2] = ToeL.Z;
                }
                if (ToeR != null)
                {
                    LeftRight[3] = ToeR.X;
                    LeftRight[4] = ToeR.Y;
                    LeftRight[5] = ToeR.Z;
                }
                return LeftRight;
            }
            public FormVisual.VisualMarker[] StepPrepare(Frame frame)
            {
                double[] LeftRight = stepPrepare(frame);
                return new FormVisual.VisualMarker[2] {
                    new FormVisual.VisualMarker(LeftRight[0],LeftRight[1],LeftRight[2],Color.Red),
                    new FormVisual.VisualMarker(LeftRight[3],LeftRight[4],LeftRight[5],Color.Green)};
            }

            public class Qualisys : MoCapSystem
            {
                static QTMRealTimeSDK.RTProtocol rtProtocol = new QTMRealTimeSDK.RTProtocol();

                public static string[] FindServers()
                {
                    string[] IPs = new string[0];
                    if (rtProtocol.DiscoverRTServers(4545))
                        IPs = rtProtocol.DiscoveryResponses.Select(response => response.IpAddress).ToArray();
                    return IPs;
                }
                public override event EventHandler<Frame> FrameReceived;

                BackgroundWorker BG = null;
                System.IO.StreamWriter swLog = null;
                Timer tmrLog = null;

                public Qualisys(string IP)
                {
                    if (Settings.Debug)
                    {
                        if (!System.IO.Directory.Exists("QualisysLog"))
                            System.IO.Directory.CreateDirectory("QualisysLog");
                        swLog = new System.IO.StreamWriter("QualisysLog/" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
                        tmrLog = new Timer()
                        {
                            Interval = 1000,
                        };
                        tmrLog.Tick += (s, e) =>
                        {
                            swLog.WriteLine(DateTime.Now.ToString("HHmmss") + ";" +
                                (rtProtocol.IsConnected() ? "Connected" : "Not Connected") + ";" + rtProtocol.GetErrorString() + ";" +
                                (lastPacket == null ? "NullPacket" : lastPacket.TimeStamp.ToString() + ";" + "Error:" + lastPacket.GetErrorString() + ";" + lastPacket.PacketType)
                                );
                        };
                        tmrLog.Start();
                    }

                    CheckConnection(IP, true);

                    BG = new BackgroundWorker()
                    {
                        WorkerSupportsCancellation = true
                    };
                    BG.DoWork += (s, e) =>
                    {
                        while (!BG.CancellationPending)
                        {
                            if (CheckConnection(IP))
                                GetData();
                        }
                    };
                    BG.RunWorkerAsync();
                }

                ~Qualisys()
                {
                    Disconnect();
                }

                bool CheckConnection(string IP, bool ShowMessage = false)
                {
                    // Check if connection to QTM is possible
                    if (rtProtocol.IsConnected())
                        return true;

                    if (!rtProtocol.Connect(IP))
                    {
                        if (ShowMessage)
                            MessageBox.Show("Could not connect to Qualisys on IP:" + IP);
                        return false;
                    }

                    // Check for available 3DOF data in the stream
                    if (rtProtocol.Settings3D == null)
                    {
                        if (!rtProtocol.Get3dSettings())
                        {
                            if (ShowMessage)
                                MessageBox.Show("Could not receive 3D-settings from Qualisys");
                            return false;
                        }
                        rtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.Component3dNoLabels);
                    }
                    return true;
                }

                public override void Disconnect()
                {
                    if (tmrLog != null)
                    {
                        tmrLog.Stop();
                        if (swLog != null)
                        {
                            swLog.Close();
                            swLog = null;
                        }
                    }
                    if (BG != null)
                        BG.CancelAsync();
                    if (rtProtocol.IsConnected())
                    {
                        rtProtocol.StreamFramesStop();
                        rtProtocol.Disconnect();
                    }
                }

                QTMRealTimeSDK.Data.RTPacket lastPacket = null;
                void GetData()
                {
                    rtProtocol.ReceiveRTPacket(out QTMRealTimeSDK.Data.PacketType packetType, false, 5000);
                    QTMRealTimeSDK.Data.RTPacket packet = rtProtocol.GetRTPacket();
                    lastPacket = packet;
                    switch (packetType)
                    {
                        case QTMRealTimeSDK.Data.PacketType.PacketData:
                            List<QTMRealTimeSDK.Data.Q3D> markerdata = packet.Get3DMarkerNoLabelsData();
                            Frame.Marker[] markers = markerdata.Select(marker => new Frame.Marker(-marker.Position.Y / 1000, marker.Position.X / 1000, marker.Position.Z / 1000)).ToArray();
                            Frame frame = new Frame(packet.TimeStamp, markers);
                            frame.Identify();
                            FrameReceived?.Invoke(this, frame);
                            break;
                        case QTMRealTimeSDK.Data.PacketType.PacketEvent:
                            var qtmEvent = packet.GetEvent();
                            Console.WriteLine("{0}", qtmEvent);
                            break;
                    }
                }
            }

            public class Motive : MoCapSystem
            {
                NatNetML.NatNetClientML NatNet = null;
                NatNetML.NatNetClientML.ConnectParams connectParams;
                public override event EventHandler<Frame> FrameReceived;

                ~Motive()
                {
                    Disconnect();
                }
                public Motive(string LocalIP = "127.0.0.1", string ServerIP = "127.0.0.1")
                {
                    NatNet = new NatNetML.NatNetClientML();
                    connectParams = new NatNetML.NatNetClientML.ConnectParams()
                    {
                        ConnectionType = NatNetML.ConnectionType.Multicast,
                        LocalAddress = LocalIP,
                        ServerAddress = ServerIP
                    };
                    NatNet.OnFrameReady += (e, s) =>
                    {
                        Frame.Marker[] markers = new Frame.Marker[e.nMarkers + e.nOtherMarkers];
                        int k = 0;
                        for (int i = 0; i < e.nMarkers; i++, k++)
                            markers[k] = new Frame.Marker(e.LabeledMarkers[i].x, -e.LabeledMarkers[i].z, e.LabeledMarkers[i].y);
                        for (int i = 0; i < e.nOtherMarkers; i++, k++)
                            markers[k] = new Frame.Marker(e.OtherMarkers[i].x, -e.OtherMarkers[i].z, e.OtherMarkers[i].y);
                        Frame frame = new Frame((long)(e.fTimestamp * 1000), markers);
                        frame.Identify();
                        FrameReceived?.Invoke(this, frame);
                    };
                    NatNet.Connect(connectParams);
                }

                public override void Disconnect()
                {
                    if (NatNet != null)
                    {
                        NatNet.Disconnect();
                        NatNet = null;
                    }
                }
                public override int[] GetSkeleton()
                {
                    return new int[]{(int)BodyModel.Joints.ToeL,(int)BodyModel.Joints.MalL,
                          (int)BodyModel.Joints.HeelL,(int)BodyModel.Joints.MalL,
                          (int)BodyModel.Joints.MalL,(int)BodyModel.Joints.KneeL,
                          (int)BodyModel.Joints.KneeL,(int)BodyModel.Joints.ThighL,
                          (int)BodyModel.Joints.ThighL,(int)BodyModel.Joints.HipL,
                          (int)BodyModel.Joints.HipL,(int)BodyModel.Joints.Back,
                          (int)BodyModel.Joints.Back,(int)BodyModel.Joints.HipR,
                          (int)BodyModel.Joints.HipR,(int)BodyModel.Joints.ThighR,
                          (int)BodyModel.Joints.ThighR,(int)BodyModel.Joints.KneeR,
                          (int)BodyModel.Joints.KneeR,(int)BodyModel.Joints.MalR,
                          (int)BodyModel.Joints.MalR,(int)BodyModel.Joints.HeelR,
                          (int)BodyModel.Joints.MalR,(int)BodyModel.Joints.ToeR};
                }
            }

            public class TreadmillSimulator : MoCapSystem
            {
                public override event EventHandler<Frame> FrameReceived;
                Timer tmr = new Timer();
                public TreadmillSimulator()
                {
                    tmr.Interval = 20;
                    DateTime start = DateTime.Now;
                    tmr.Tick += (s, e) =>
                    {
                        double seconds = (DateTime.Now - start).TotalSeconds;
                        float heightL = (float)Math.Sin(seconds * Math.PI * Settings.TreadmillSimulatorSpeed + Math.PI) * 0.2f;
                        heightL = heightL < 0 ? 0 : heightL;
                        float heightR = (float)Math.Sin(seconds * Math.PI * Settings.TreadmillSimulatorSpeed) * 0.2f;
                        heightR = heightR < 0 ? 0 : heightR;
                        Frame frame = new Frame(DateTime.Now.Ticks, new Frame.Marker[] {
                              new Frame.Marker(BodyModel.Joints.ToeL,-0.25f,(float)Math.Cos(seconds * Math.PI*Settings.TreadmillSimulatorSpeed)/2,heightL),
                              new Frame.Marker(BodyModel.Joints.ToeR, 0.25f,(float)Math.Cos(seconds*Math.PI*Settings.TreadmillSimulatorSpeed+Math.PI)/2, heightR) });
                        frame.IdentifyMarker(0, BodyModel.Joints.ToeL);
                        frame.IdentifyMarker(1, BodyModel.Joints.ToeR);
                        FrameReceived?.Invoke(this, frame);
                    };
                    tmr.Start();
                }

                public override List<KeyValuePair<string, Control>> getSettings()
                {
                    NumericUpDown nudSpeed = new NumericUpDown()
                    {
                        Minimum = 0,
                        Maximum = 100,
                        DecimalPlaces = 2,
                        Increment = (decimal)0.1,
                        Value = (decimal)Settings.TreadmillSimulatorSpeed
                    };
                    nudSpeed.ValueChanged += (s, e) =>
                    {
                        Settings.TreadmillSimulatorSpeed = (double)nudSpeed.Value;
                        Settings.Save();
                    };
                    return new List<KeyValuePair<string, Control>>()
                    {
                        new KeyValuePair<string, Control>("Speed(m/s)", nudSpeed)
                    };
                }

                ~TreadmillSimulator()
                {
                    Disconnect();
                }
                public override void Disconnect()
                {
                    tmr.Stop();
                }
            }
            public class Simulator : MoCapSystem
            {
                class frame
                {
                    int n;
                    int[] ID;
                    float[] XYZ;
                    public frame(System.IO.BinaryReader BR)
                    {
                        n = BR.ReadInt32();
                        ID = new int[n];
                        XYZ = new float[3 * n];
                        for (int i = 0; i < n; i++)
                            ID[i] = BR.ReadInt32();
                        for (int i = 0; i < n; i++)
                        {
                            XYZ[i * 3] = BR.ReadSingle();
                            XYZ[i * 3 + 1] = BR.ReadSingle();
                            XYZ[i * 3 + 2] = BR.ReadSingle();
                        }
                    }
                    public Frame.Marker[] getMarkers()
                    {
                        return ID.Select((id, i) => new Frame.Marker((BodyModel.Joints)id, XYZ[i * 3], XYZ[i * 3 + 1], XYZ[i * 3 + 2])).ToArray();
                    }
                }
                bool Freeze = false;
                double iFracPrev = 0;
                public override event EventHandler<Frame> FrameReceived;
                Timer tmr = new Timer();
                List<frame> Frames = new List<frame>();
                Random rnd = new Random(DateTime.Now.Millisecond);
                public Simulator(string binFile)
                {
                    using (System.IO.BinaryReader BR = new System.IO.BinaryReader(System.IO.File.OpenRead(binFile)))
                    {
                        while (BR.BaseStream.Position < BR.BaseStream.Length)
                            Frames.Add(new frame(BR));
                    }
                    int nFrames = Frames.Count;
                    tmr.Interval = 20;
                    DateTime lastUpdate = DateTime.Now;
                    tmr.Tick += (s, e) =>
                    {
                        double seconds = (DateTime.Now - lastUpdate).TotalSeconds;
                        lastUpdate = DateTime.Now;
                        double iFrac = iFracPrev + Settings.SimulatorFrequency * seconds * 120;
                        if (Freeze)
                            iFrac = iFracPrev;
                        else
                            iFracPrev = iFrac;

                        int frameID = (int)iFrac % Frames.Count;
                        Frame.Marker[] markers = Frames[frameID].getMarkers();
                        if (Settings.SimulatorNoise > 0)
                        {
                            int nRemove = (int)Math.Ceiling(Settings.SimulatorNoise);
                            double prob = Settings.SimulatorNoise - Math.Floor(Settings.SimulatorNoise);
                            if (rnd.NextDouble() < prob)
                                markers = markers.OrderBy(marker => marker.Z).Where((marker, i) => i < markers.Length - nRemove).ToArray();
                        }
                        if (Settings.SimulatorRotator != 0)
                        {
                            Rotator += Settings.SimulatorRotator;
                            double cos = Math.Cos(Rotator);
                            double sin = Math.Sin(Rotator);
                            for (int i = 0; i < markers.Length; i++)
                            {
                                double x = markers[i].X;
                                double y = markers[i].Y;
                                markers[i].X = (float)(cos * x - sin * y);
                                markers[i].Y = (float)(sin * x + cos * y);
                            }
                        }
                        Frame frame = new Frame(frameID, markers);
                        frame.Identify();
                        FrameReceived?.Invoke(this, frame);
                    };
                    tmr.Start();
                }

                double Rotator = 0;
                public override List<KeyValuePair<string, Control>> getSettings()
                {
                    NumericUpDown nudFrequency = new NumericUpDown()
                    {
                        Minimum = 0,
                        Maximum = 100,
                        DecimalPlaces = 2,
                        Increment = (decimal)0.1,
                        Value = (decimal)Settings.SimulatorFrequency
                    };
                    nudFrequency.ValueChanged += (s, e) =>
                    {
                        Settings.SimulatorFrequency = (double)nudFrequency.Value;
                        Settings.Save();
                    };
                    NumericUpDown nudNoise = new NumericUpDown()
                    {
                        Minimum = 0,
                        Maximum = 100,
                        DecimalPlaces = 2,
                        Increment = (decimal)0.01,
                        Value = (decimal)Settings.SimulatorNoise
                    };
                    nudNoise.ValueChanged += (s, e) =>
                    {
                        Settings.SimulatorNoise = (double)nudNoise.Value;
                        Settings.Save();
                    };
                    NumericUpDown nudRotator = new NumericUpDown()
                    {
                        Minimum = -1,
                        Maximum = 1,
                        DecimalPlaces = 2,
                        Increment = (decimal)0.01,
                        Value = (decimal)Settings.SimulatorRotator
                    };
                    nudRotator.ValueChanged += (s, e) =>
                    {
                        Settings.SimulatorRotator = (double)nudRotator.Value;
                        if (Settings.SimulatorRotator == 0)
                            Rotator = 0;
                        Settings.Save();
                    };
                    Button cmdBackward = new Button()
                    {
                        Text = "Backward"
                    };
                    Timer tmrHold = new Timer();
                    int step = 0;
                    tmrHold.Interval = 20;
                    tmrHold.Tick += (s, e) =>
                    {
                        iFracPrev += step;
                    };
                    tmrHold.Start();
                    cmdBackward.MouseDown += (s, e) =>
                    {
                        Freeze = true;
                        step = -1;
                    };
                    cmdBackward.Click += (s, e) =>
                    {
                        Freeze = true;
                        iFracPrev--;
                    };
                    cmdBackward.MouseUp += (s, e) =>
                    {
                        step = 0;
                    };
                    Button cmdUnfreeze = new Button()
                    {
                        Text = "Unfreeze"
                    };
                    cmdUnfreeze.Click += (s, e) =>
                    {
                        Freeze = false;
                    };
                    Button cmdForward = new Button()
                    {
                        Text = "Forward"
                    };
                    cmdForward.Click += (s, e) =>
                    {
                        Freeze = true;
                        iFracPrev++;
                    };
                    cmdForward.MouseDown += (s, e) =>
                    {
                        Freeze = true;
                        step = 1;
                    };
                    cmdForward.MouseUp += (s, e) =>
                    {
                        step = 0;
                    };

                    return new List<KeyValuePair<string, Control>>()
                    {
                        new KeyValuePair<string, Control>("Cycles per Second", nudFrequency),
                        new KeyValuePair<string, Control>("Noise ", nudNoise),
                        new KeyValuePair<string, Control>("Rotate", nudRotator),
                        new KeyValuePair<string, Control>("", cmdBackward),
                        new KeyValuePair<string, Control>("", cmdUnfreeze),
                        new KeyValuePair<string, Control>("", cmdForward),
                    };
                }

                ~Simulator()
                {
                    Disconnect();
                }
                public override void Disconnect()
                {
                    tmr.Stop();
                }
            }
            public class MouseSimulator : MoCapSystem
            {
                public override event EventHandler<Frame> FrameReceived;
                public MouseSimulator()
                {
                    FormVisual.FORMVISUAL.CtrlMove += FORMVISUAL_CtrlMove;
                }

                private void FORMVISUAL_CtrlMove(object sender, PointF e)
                {
                    Frame.Marker toeL = new Frame.Marker(BodyModel.Joints.ToeL, e.X, e.Y, 0);
                    Frame.Marker toeR = new Frame.Marker(BodyModel.Joints.ToeR, e.X, e.Y, 0);
                    Frame frame = new Frame(DateTime.Now.Ticks,
                        new Frame.Marker[2] {
                            toeL,
                            toeR
                        });
                    frame.IdentifyMarker(0, BodyModel.Joints.ToeL);
                    frame.IdentifyMarker(1, BodyModel.Joints.ToeR);
                    FrameReceived?.Invoke(this, frame);
                }

                ~MouseSimulator()
                {
                    Disconnect();
                }
                public override void Disconnect()
                {
                    FormVisual.FORMVISUAL.CtrlMove -= FORMVISUAL_CtrlMove;
                }
            }
        }
        public abstract class MoCapPacket : MDOL.IO.DataSaver.DataPacket
        {
            public class Info : MoCapPacket
            {
                protected override string PacketStr => "MoCap;Info";
                public Info(MoCapSystem moCapSystem)
                {
                    AddData("moCapSystem", moCapSystem.ToString());
                }
            }
            public class Frame : MoCapPacket
            {
                protected override string PacketStr => "MoCap;Frame";
                public Frame(MoCapSystem.Frame frame)
                {
                    AddData("Timestamp", frame.Timestamp);
                    AddData("Markers", frame.Markers);
                    AddData("RawMarkers", frame.rawMarkers);
                    AddData("ZMarkers", frame.ZMarkers);
                }
            }
            public class Trigger : MoCapPacket
            {
                protected override string PacketStr => "MoCap;Trigger";
            }
        }
    }
}
