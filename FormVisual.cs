using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Windows.Forms.DataVisualization.Charting;
using MDOL;
namespace Optitracker
{
    public partial class FormVisual : Form
    {
        public abstract class VisualPacket : MDOL.IO.DataSaver.DataPacket
        {
            public class Startup : VisualPacket
            {
                protected override string PacketStr => "Visual;Startup";
                public Startup(FormVisual formVisual)
                {
                    AddData("xRange", formVisual.xRange);
                    AddData("yRange", formVisual.yRange);
                    AddData("xRangeFull", formVisual.xRangeFull);
                    AddData("yRangeFull", formVisual.yRangeFull);
                    AddData("userMarkerSize", formVisual.serieUser.MarkerSize);
                    AddData("meter2unit", formVisual.meter2unit);
                }
            }
            public class TargetCreated : VisualPacket
            {
                protected override string PacketStr => "Visual;TargetCreated";
                public TargetCreated(int TargetType, int TargetID, int VisualMarkerID, PointF Start, double Radius, PointF Speed)
                {
                    AddData("TargetType", TargetType);
                    AddData("TargetID", TargetID);
                    AddData("VisualMarkerID", VisualMarkerID);
                    AddData("Position", Start);
                    AddData("Radius", Radius);
                    AddData("Speed", Speed);
                }
            }
            public class TargetActivated : VisualPacket
            {
                protected override string PacketStr => "Visual;TargetActivated";
                public TargetActivated(int TargetID, PointF Position)
                {
                    AddData("TargetID", TargetID);
                    AddData("Position", Position);
                }
            }
            public class TargetHit : VisualPacket
            {
                public int targetID = -1;
                public PointF targetPosition = PointF.Empty;
                protected override string PacketStr => "Visual;TargetHit";
                public TargetHit(int TargetID, PointF Position, VisualMarker visualMarker)
                {
                    AddData("TargetID", TargetID);
                    AddData("Position", Position);
                    AddData("VisualMarker", visualMarker);
                }
            }
            public class TargetDeactivated : VisualPacket
            {
                public int targetID = -1;
                public PointF targetPosition = PointF.Empty;
                protected override string PacketStr => "Visual;TargetDeactivated";
                public TargetDeactivated(int TargetID, PointF Position)
                {
                    AddData("TargetID", TargetID);
                    AddData("Position", Position);
                }
            }
        }


        public int Score = 0;
        public int MaxScore = 0;
        public double[] yRange = new double[2] { -1, 3 };
        double[] xRange = new double[2];

        double[] xRangeFull = new double[2];
        double[] yRangeFull = new double[2];

        public static FormVisual FORMVISUAL;
        public List<Target> Targets = new List<Target>();
        public Series serieMid = new Series()
        {
            ChartType = SeriesChartType.Line,
            Color = Color.Blue,
            CustomProperties = "IsXAxisQuantitative=True" // Enable multiple values for same x-value
        };
        public void Alert()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Timer tmrAlert = new Timer() { Interval = 50 };
            tmrAlert.Tick += (s, e) =>
              {
                  double sec = sw.ElapsedMilliseconds / 1000.0;
                  if (sec > 2)
                  {
                      chart.ChartAreas[0].BackColor = Color.White;
                      tmrAlert.Stop();
                  }
                  else
                  {
                      int value = (int)((1-Math.Abs(sec - 1)) * 128);
                      chart.ChartAreas[0].BackColor = Color.FromArgb(255, 255 - value, 255 - value);
                  }
              };
            tmrAlert.Start();
        }
        public void AddSeries(params Series[] series)
        {
            for (int i = series.Length - 1; i >= 0; i--)
                FORMVISUAL.chart.Series.Insert(0, series[i]);
        }
        public void RemoveSeries(params Series[] series)
        {
            foreach (Series serie in series)
                if(chart.Series.Contains(serie))
                    chart.Series.Remove(serie);
        }
        public Series serieUser = new Series()
        {
            ChartType = SeriesChartType.Point,
            MarkerStyle = MarkerStyle.Circle,
            MarkerSize = 9,
        };
        Screen screen;
        public FormVisual()
        {
            FORMVISUAL = this;
            InitializeComponent();

            Load += (s, e) =>
              {
                  if (Screen.AllScreens.Length == 1)
                  {
                      screen = Screen.PrimaryScreen;
                      Bounds = new Rectangle(screen.Bounds.Width / 2, 0, screen.Bounds.Width / 2, screen.Bounds.Height);
                  }
                  else
                  {
                      screen = Screen.AllScreens.First(secScreen => secScreen != Screen.PrimaryScreen);
                      Bounds = screen.Bounds;
                  }
              };

            chart.Series.Add(serieUser);
            chart.Series.Add(serieMid);

            chart.MouseClick += (s, e) =>
              {
                  if (CtrlClick != null && e.Button == MouseButtons.Left && System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
                  {
                      double x = chart.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                      double y = chart.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);
                      CtrlClick(this, new PointF((float)x, (float)y));
                  }
              };
            chart.MouseMove += (s, e) =>
            {
                if (CtrlMove != null && System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
                {
                    double x = chart.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                    double y = chart.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);
                    CtrlMove(this, new PointF((float)x, (float)y));
                }
            };
        }
        public event EventHandler<PointF> CtrlMove = null;
        public event EventHandler<PointF> CtrlClick = null;
        public void SetMid(double mid)
        {
            serieMid.Points.Clear();
            if (!double.IsNaN(mid))
            {
                serieMid.Points.AddXY(mid, yRange[0]);
                serieMid.Points.AddXY(mid, yRange[1]);
            }
        }
        public void MidVisibility(bool Visibility)
        {
            if (Visibility)
            {
                if (!chart.Series.Contains(serieMid))
                    chart.Series.Add(serieMid);
            }
            else if (chart.Series.Contains(serieMid))
                chart.Series.Remove(serieMid);
        }
        public class VisualMarker : Vector<Dimensions.II>, IO.DataSaver.byteAble
        {
            public Color Color;
            public double Z;
            public VisualMarker(double X, double Y, double Z, Color Color) : base(X, Y)
            {
                this.Z = Z;
                this.Color = Color;
            }
            public byte[] getBytes()
            {
                List<byte[]> bytes = new List<byte[]>();
                bytes.Add(BitConverter.GetBytes(X));
                bytes.Add(BitConverter.GetBytes(Y));
                bytes.Add(BitConverter.GetBytes(Z));
                return bytes.SelectMany(b => b).ToArray();
            }
        }
        public void DrawUser(params VisualMarker[] XY)
        {
            if (Visible)
            {
                serieUser.Points.Clear();
                foreach (VisualMarker xy in XY)
                    serieUser.Points.Add(new DataPoint(xy.X, xy.Y)
                    {
                        Color = xy.Color
                    });
            }
        }

        bool TrueClose = false;
        private void FormVisual_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!TrueClose)
            {
                Hide();
                e.Cancel = true;
            }
        }

        public double meter2unit = 1;
        private void FormVisual_Resize(object sender, EventArgs e)
        {
            chart.Size = new Size(Width, Height);
            chart.ChartAreas[0].AxisY.Minimum = yRange[0];
            chart.ChartAreas[0].AxisY.Maximum = yRange[1];
            double xWidth = (yRange[1] - yRange[0]) / Height * Width;
            xRange[0] = -xWidth / 2;
            xRange[1] = xWidth / 2;
            chart.ChartAreas[0].AxisX.Minimum = xRange[0];
            chart.ChartAreas[0].AxisX.Maximum = xRange[1];

            Timer tmr = new Timer();
            tmr.Interval = 100;
            tmr.Tick += (tmr_s, tmr_e) =>
              {
                  double x1 = chart.ChartAreas[0].AxisX.ValueToPixelPosition(0);
                  double x2 = chart.ChartAreas[0].AxisX.ValueToPixelPosition(1);

                  double y1 = chart.ChartAreas[0].AxisY.ValueToPixelPosition(0);
                  double y2 = chart.ChartAreas[0].AxisY.ValueToPixelPosition(1);
                  meter2unit = (Math.Abs(x1 - x2) + Math.Abs(y1 - y2)) / 2;
                  ZoomChanged?.Invoke(this, null);
                  tmr.Stop();
                  tmr = null;

                  Point TopLeft = chart.PointToScreen(chart.Location);
                  Point BottomRight = chart.PointToScreen(new Point(chart.Right, chart.Bottom));
                  Func<double, double> x2Real = (x) => { return (x - TopLeft.X) / (BottomRight.X - TopLeft.X) * (xRange[1] - xRange[0]) + xRange[0]; };
                  Func<double, double> y2Real = (y) => { return (-y + BottomRight.Y) / (BottomRight.Y - TopLeft.Y) * (yRange[1] - yRange[0]) + yRange[0]; };
                  if (screen != null)
                  {
                      xRangeFull[0] = x2Real(screen.Bounds.Left);
                      xRangeFull[1] = x2Real(screen.Bounds.Right);
                      yRangeFull[0] = y2Real(screen.Bounds.Bottom);
                      yRangeFull[1] = y2Real(screen.Bounds.Top);
                    }

                  Text = "x: [ " + xRange[0].ToString("0.00") + ";" + xRange[1].ToString("0.00") + "] " +
                "y:[" + yRange[0].ToString("0.00") + ";" + yRange[1].ToString("0.00") + "] " +
                "Markersize(CM): " + (serieUser.MarkerSize / meter2unit * 100 / 2);
              };
            tmr.Start();

            Text = "x: [ " + xRange[0].ToString("0.00") + ";" + xRange[1].ToString("0.00") + "] " +
                "y:[" + yRange[0].ToString("0.00") + ";" + yRange[1].ToString("0.00") + "] " +
                "Markersize(CM): " + (serieUser.MarkerSize / meter2unit * 100 / 2);

            pgbTime.Width = Width - 40;
        }
        public void SetTime(int PCT)
        {
            pgbTime.Value = PCT;
        }

        private void FormVisual_Load(object sender, EventArgs e)
        {
            FormVisual_Resize(null, null);
        }

        public void UpdateScore(bool PCT = false)
        {
            string score = Score.ToString();
            if (PCT)
                score = (MaxScore > 0 ? 100 * Score / MaxScore : 0) + " %";
            lblScore.Text = "Score: " + score;
        }

        public void ClearScore()
        {
            Score = 0;
            MaxScore = 0;

            UpdateScore();
        }

        public void ShowScore()
        {
            lblScore.Visible = true;
        }
        public void HideScore()
        {
            lblScore.Visible = false;
        }

        protected static int TARGET_ID = 0;
        public void Clear()
        {
            foreach (Target target in Targets)
                target.Destroy();
            Targets.Clear();
            TARGET_ID = 0;
        }
        public void UpdateTargets()
        {
            for (int i = 0; i < Targets.Count; i++)
                if (!Targets[i].Update(xRange, yRange))
                    Targets.RemoveAt(i--);
        }

        public abstract class Target
        {
            public bool CountScore = false;
            public PointF Position;
            protected double Radius;
            public PointF Speed;
            public PointF ActivationOffset;
            protected DateTime lastUpdate = DateTime.Now;
            protected bool Hit = false;
            protected bool Activated = false;
            public Color ActivationColor = Color.Empty;
            public Color NoHitColor = Color.Empty;
            public Color HitColor = Color.Empty;

            protected Series[] series;
            Series serieHit;

            bool HasBeenSeen = false;

            bool Destroyed = false;
            protected int TargetID;
            protected abstract void CreateSerie();
            protected virtual void activate()
            {

            }
            public bool Update(double[] xRange = null, double[] yRange = null)
            {
                if (!Speed.IsEmpty)
                {
                    float seconds = (float)(DateTime.Now - lastUpdate).TotalSeconds;
                    float xChange = Speed.X * seconds;
                    float yChange = Speed.Y * seconds;
                    Position = new PointF(Position.X + xChange, Position.Y + yChange);

                    foreach (Series serie in series)
                        for (int i = 0; i < serie.Points.Count; i++)
                        {
                            serie.Points[i].XValue += xChange;
                            serie.Points[i].YValues[0] += yChange;
                        }
                    if (serieHit != null)
                    {
                        serieHit.Points[0].XValue += xChange;
                        serieHit.Points[0].YValues[0] += yChange;
                    }
                    lastUpdate = DateTime.Now;

                    // If the target has been visibible to the user, delete it if the target has moved outside the visible region.
                    if (xRange != null)
                        if (HasBeenSeen)
                        {
                            if (Position.X < xRange[0] || Position.X > xRange[1] ||
                                Position.Y < yRange[0] || Position.Y > yRange[1])
                            {
                                Destroy();
                                return false;
                            }
                        }
                        // If the target has not been seen, check to see if the target has moved inside the visible region
                        else if (Position.X > xRange[0] && Position.X < xRange[1] &&
                            Position.Y > yRange[0] && Position.Y < yRange[1])
                            HasBeenSeen = true;
                }
                return true;
            }
            public void Activate()
            {
                if (Activated || Destroyed)
                    return;
                Activated = true;
                if (!ActivationOffset.IsEmpty)
                {
                    Position.X += ActivationOffset.X;
                    Position.Y += ActivationOffset.Y;
                    foreach (Series serie in series)
                        for (int i = 0; i < serie.Points.Count; i++)
                        {
                            serie.Points[i].XValue += ActivationOffset.X;
                            serie.Points[i].YValues[0] += ActivationOffset.Y;
                        }
                }
                MDOL.IO.DataSaver.AddPacket(new VisualPacket.TargetActivated(TargetID, Position));
                if (!ActivationColor.IsEmpty)
                    foreach (Series serie in series)
                        serie.BorderColor = ActivationColor;
                activate();
            }
            public void Shoot(VisualMarker visualMarker, bool ShowHit = false)
            {
                if (!Activated || Destroyed)
                    return;
                Update();
                if (ShowHit)
                {
                    serieHit = new Series()
                    {
                        ChartType = SeriesChartType.Point,
                        Color = Color.Black,
                        MarkerSize = 10,
                        MarkerStyle = MarkerStyle.Cross,
                    };
                    serieHit.Points.AddXY(visualMarker.X, visualMarker.Y);
                    FORMVISUAL.chart.Series.Add(serieHit);
                }
                shoot(visualMarker, ShowHit);
                FORMVISUAL.UpdateScore();
            }
            protected abstract void shoot(VisualMarker visualMarker, bool ShowHit = false);

            public void Destroy()
            {
                Destroyed = true;
                foreach (Series serie in series)
                    FORMVISUAL.chart.Series.Remove(serie);
                if (serieHit != null)
                    FORMVISUAL.chart.Series.Remove(serieHit);
            }
            public Target()
            {

            }
            public Target(PointF Start, PointF Speed, double Radius, int TargetType, int VisualMarkerID = -1)
            {
                Position = Start;
                this.Speed = Speed;
                this.Radius = Radius;
                TargetID = TARGET_ID++;

                MDOL.IO.DataSaver.AddPacket(new VisualPacket.TargetCreated(TargetType, TargetID, VisualMarkerID, Start, Radius, Speed));

                CreateSerie();
                FORMVISUAL.AddSeries(series);
                FORMVISUAL.Targets.Add(this);
            }
        }
        public class Box : Target
        {
            public System.Media.SoundPlayer SP = null;
            public Box(PointF Start, PointF Speed, double Radius, int TargetType, int VisualMarkerID = -1) : base(Start, Speed, Radius, TargetType, VisualMarkerID) { }

            protected override void CreateSerie()
            {
                series = new Series[1]{
                    new Series()
                    {
                        ChartType = SeriesChartType.Point,
                        MarkerStyle = MarkerStyle.Square,
                        Color = Color.Red,
                        BorderWidth = 2,
                        MarkerSize = (int)(Radius*2*FORMVISUAL.meter2unit),
                    }
                };
            }
            protected override void shoot(VisualMarker visualMarker, bool ShowHit = false)
            {
                if (Math.Max(Math.Abs(visualMarker.X - Position.X), Math.Abs(visualMarker.Y - Position.Y)) <= Radius)
                {
                    Hit = true;
                    MDOL.IO.DataSaver.AddPacket(new VisualPacket.TargetHit(TargetID, Position, visualMarker));
                    if (!HitColor.IsEmpty)
                        foreach (Series serie in series)
                            serie.Color = HitColor;
                    if (CountScore)
                        FORMVISUAL.Score++;
                    if (SP != null)
                        SP.Play();
                }
                else if (!NoHitColor.IsEmpty)
                    foreach (Series serie in series)
                        serie.Color = NoHitColor;
            }
        }
        public class Bullseye : Target
        {
            public System.Media.SoundPlayer SP1 = null;
            public System.Media.SoundPlayer SP2 = null;
            public System.Media.SoundPlayer SP3 = null;
            public Bullseye(PointF Start, PointF Speed, double Radius, int TargetType, int VisualMarkerID = -1) : base(Start, Speed, Radius, TargetType, VisualMarkerID)
            {
                ActivationColor = Color.Red;
            }

            protected override void CreateSerie()
            {
                Color color = Color.Transparent;
                series = new Series[]{
                    new Series()
                    {
                        ChartType = SeriesChartType.Point,
                        BorderColor = color,
                        Color = Color.White,
                        MarkerStyle = MarkerStyle.Circle,
                        BorderWidth = 2,
                        MarkerSize = (int)(Radius*2*FORMVISUAL.meter2unit),
                    },new Series()
                    {
                        ChartType = SeriesChartType.Point,
                        BorderColor = color,
                        Color = Color.White,
                        MarkerStyle = MarkerStyle.Circle,
                        BorderWidth = 2,
                        MarkerSize = (int)(Radius*2/3*2*FORMVISUAL.meter2unit),
                    },new Series()
                    {
                        ChartType = SeriesChartType.Point,
                        BorderColor = color,
                        Color = Color.White,
                        MarkerStyle = MarkerStyle.Circle,
                        BorderWidth = 2,
                        MarkerSize = (int)(Radius/5*2*FORMVISUAL.meter2unit),
                    }
                };
                foreach (Series serie in series)
                    serie.Points.AddXY(Position.X, Position.Y);
            }
            protected override void activate()
            {
                series[2].Color = Color.Black;
            }
            protected override void shoot(VisualMarker visualMarker, bool ShowHit = false)
            {
                double dist = Math.Sqrt((visualMarker.X - Position.X).Sqr() + (visualMarker.Y - Position.Y).Sqr());
                if (dist <= Radius)
                {
                    Hit = true;
                    MDOL.IO.DataSaver.AddPacket(new VisualPacket.TargetHit(TargetID, Position, visualMarker));
                    if (dist > Radius * 2 / 3)
                    {
                        series[0].Color = Color.Red;
                        if (CountScore)
                            FORMVISUAL.Score += 1;
                        SP1.Play();
                    }
                    else if (dist > Radius / 5)
                    {
                        series[1].Color = Color.Yellow;
                        if (CountScore)
                            FORMVISUAL.Score += 2;
                        SP2.Play();
                    }
                    else
                    {
                        series[2].Color = Color.Lime;
                        if (CountScore)
                            FORMVISUAL.Score += 3;
                        SP3.Play();
                    }
                }
            }
        }
        public event EventHandler ZoomChanged = null;
        private void FormVisual_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            double change = e.Control ? 0.1 : 0.5;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    yRange[0] += change;
                    yRange[1] += change;
                    break;
                case Keys.Down:
                    yRange[0] -= change;
                    yRange[1] -= change;
                    break;
                case Keys.Oemplus:
                    if (yRange[0] + change >= yRange[1] - change)
                        return;
                    yRange[0] += change;
                    yRange[1] -= change;
                    break;
                case Keys.OemMinus:
                    yRange[0] -= change;
                    yRange[1] += change;
                    break;
                case Keys.Left:
                    serieUser.MarkerSize = Math.Max(1, serieUser.MarkerSize - 1);
                    break;
                case Keys.Right:
                    serieUser.MarkerSize++;
                    break;
            }
            FormVisual_Resize(null, null);
        }
    }
}
