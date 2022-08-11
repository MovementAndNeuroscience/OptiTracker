using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL.GL;
using System.ComponentModel;

namespace MDOL
{
    public class Scene
    {
        public static Vector3d ToVector3d(Vector<Dimensions.III> v)
        {
            return new Vector3d(v.X, v.Y, v.Z);
        }
        public static IO.XML Vector3d2XML(string Tag,Vector3d v)
        {
            return new IO.XML(Tag, new IO.XML("X", v.X.ToStringD()), new IO.XML("Y", v.Y.ToStringD()), new IO.XML("Z", v.Z.ToStringD()));
        }
        public static Vector3d XML2Vector3d(IO.XML xml)
        {
            return new Vector3d(xml.getDouble("X", 0), xml.getDouble("Y", 0), xml.getDouble("Z", 0));
        }
        class Camera
        {
            // Define position and basis
            public Vector3d Origo = new Vector3d(0, 0, 5);
            public Vector3d XAxis = new Vector3d(-1, 0, 0);
            public Vector3d YAxis = new Vector3d(0, 1, 0);
            public Vector3d ZAxis = new Vector3d(0, 0, -1);

            /// <summary>
            /// Translate camera using Vector3d
            /// </summary>
            /// <param name="translation"></param>
            public Vector3d Translate(Vector3d translation)
            {
                return Translate(translation.X,translation.Y ,translation.Z);
            }

            /// <summary>
            /// Translate camera using three scalars
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="z"></param>
            public Vector3d Translate(double x, double y, double z)
            {
                Vector3d diff = XAxis * x + YAxis * y + ZAxis * z;
                Origo = Origo + diff;
                return diff;
            }

            /// <summary>
            /// Rotate camera around current X-axis
            /// </summary>
            /// <param name="angle"></param>
            public void RotateX(double angle)
            {
                YAxis = Vector3d.Transform(YAxis, Matrix4d.Rotate(XAxis, angle));
                ZAxis = Vector3d.Transform(ZAxis, Matrix4d.Rotate(XAxis, angle));
            }
            /// <summary>
            /// Rotate camera around current Y-axis
            /// </summary>
            /// <param name="angle"></param>
            public void RotateY(double angle)
            {
                XAxis = Vector3d.Transform(XAxis, Matrix4d.Rotate(YAxis, angle));
                ZAxis = Vector3d.Transform(ZAxis, Matrix4d.Rotate(YAxis, angle));
            }
            /// <summary>
            /// Rotate camera around current Z-axis
            /// </summary>
            /// <param name="angle"></param>
            public void RotateZ(double angle)
            {
                XAxis = Vector3d.Transform(XAxis, Matrix4d.Rotate(ZAxis, angle));
                YAxis = Vector3d.Transform(YAxis, Matrix4d.Rotate(ZAxis, angle));
            }
            /// <summary>
            /// Rotate camera around user-defined axis
            /// </summary>
            /// <param name="axis"></param>
            /// <param name="angle"></param>
            public void Rotate(Vector3d axis, double angle)
            {
                XAxis = Vector3d.Transform(XAxis, Matrix4d.Rotate(axis, angle));
                YAxis = Vector3d.Transform(YAxis, Matrix4d.Rotate(axis, angle));
                ZAxis = Vector3d.Transform(ZAxis, Matrix4d.Rotate(axis, angle));
            }

            /// <summary>
            /// Rotate around X-axis using user-define origin
            /// </summary>
            /// <param name="angle"></param>
            /// <param name="point"></param>
            public void RotateAroundX(double angle, Vector3d point)
            {
                Vector3d relPosition = Origo - point;
                Origo = point + Vector3d.Transform(relPosition, Matrix4d.Rotate(XAxis, angle));
                YAxis = Vector3d.Transform(YAxis, Matrix4d.Rotate(XAxis, angle));
                ZAxis = Vector3d.Transform(ZAxis, Matrix4d.Rotate(XAxis, angle));
            }
            /// <summary>
            /// Rotate around Y-axis using user-define origin
            /// </summary>
            /// <param name="angle"></param>
            /// <param name="point"></param>
            public void RotateAroundY(double angle, Vector3d point)
            {
                Vector3d relPosition = Origo - point;
                Origo = point + Vector3d.Transform(relPosition, Matrix4d.Rotate(YAxis, angle));
                XAxis = Vector3d.Transform(XAxis, Matrix4d.Rotate(YAxis, angle));
                ZAxis = Vector3d.Transform(ZAxis, Matrix4d.Rotate(YAxis, angle));
            }

            // Reset camera position and orientation to default
            public void Reset()
            {
                Origo = new Vector3d(0, 0, 5);
                XAxis = new Vector3d(-1, 0, 0);
                YAxis = new Vector3d(0, 1, 0);
                ZAxis = new Vector3d(0, 0, -1);
            }
        }

        /// <summary>
        /// Overall class for defining primitives in the 3D space
        /// </summary>
        public class Primitive
        {
            // Boolean parameter setting the visibility of the primitive
            public bool Show = true;

            // Array holding the RGB values for vertices
            public byte[] Colors;

            // Array holding the 3D positions for the vertices
            public double[] Vertices;

            //Array holding the indices for the primitve
            public uint[] Indices;

            protected int nVertices = 0;
            protected int nColors = 0;
            protected PrimitiveType Mode;

            // Write the vertices to the current framebuffer
            public virtual void WriteVertices()
            {
                GL.VertexPointer(3, VertexPointerType.Double, 0, Vertices.Pointer());
            }

            // Write the colors to the current framebuffer
            public virtual void WriteColor()
            {
                GL.ColorPointer(3, ColorPointerType.UnsignedByte, 0, Colors.Pointer());
            }

            // Write the indices to the framebuffer
            public virtual void WriteElements()
            {
                GL.DrawElements(Mode, Indices.Length, DrawElementsType.UnsignedInt, Indices.Pointer());
            }

            public virtual void WriteAll()
            {
                WriteVertices();
                WriteColor();
                WriteElements();
            }

            // Draw the primitive in the current framebuffer, using both 
            public void Draw()
            {
                if (Show)
                    WriteAll();
            }

            // Return center position of the primitive, calculated as the mean vertex positions
            public virtual Vector<Dimensions.III> GetCenter()
            {
                double[] center = new double[3];
                for (int i = 0; i < nVertices; i++)
                {
                    center[0] += Vertices[i * 3];
                    center[1] += Vertices[i * 3 + 1];
                    center[2] += Vertices[i * 3 + 2];
                }
                return new Vector<Dimensions.III>(center[0] / nVertices, center[1] / nVertices, center[2] / nVertices);
            }

            // Set the center of the primtive
            public virtual void SetCenter(Vector<Dimensions.III> newCenter)
            {
                Vector<Dimensions.III> translation = newCenter - GetCenter();
                for (int i = 0; i < nVertices; i++)
                {
                    Vertices[i * 3] += translation[0];
                    Vertices[i * 3 + 1] += translation[1];
                    Vertices[i * 3 + 2] += translation[2];
                }
            }

            // Define global color of all vertices in the primitive
            public virtual void SetColor(byte r, byte g, byte b)
            {
                for (int i = 0; i < nColors; i++)
                {
                    Colors[i * 3] = r;
                    Colors[i * 3 + 1] = g;
                    Colors[i * 3 + 2] = b;
                }
            }

            // Scale the vertex positons by scalar value
            public virtual void Scale(double s)
            {
                for (int i = 0; i < nVertices; i++)
                {
                    Vertices[i * 3] *= s;
                    Vertices[i * 3 + 1] *= s;
                    Vertices[i * 3 + 2] *= s;
                }
            }

            // Translate all vertices in the primitive
            public virtual void Translate(double x, double y, double z)
            {
                for (int i = 0; i < nVertices; i++)
                {
                    Vertices[i * 3] += x;
                    Vertices[i * 3 + 1] += y;
                    Vertices[i * 3 + 2] += z;
                }
            }

            public class PointCloud : Primitive
            {
                public float PointSize = 1;
                public PointCloud(int n) : this(new double[n * 3], new byte[n * 3]) { }
                public PointCloud(double[] vertices, byte r, byte g, byte b)
                {
                    nVertices = vertices.Length / 3;
                    nColors = vertices.Length / 3;
                    Mode = PrimitiveType.Points;
                    byte[] colors = new byte[nColors * 3];
                    for (int i = 0; i < nVertices; i++)
                    {
                        colors[i * 3] = r;
                        colors[i * 3 + 1] = g;
                        colors[i * 3 + 2] = b;
                    }
                    Update(vertices, colors);
                }
                public PointCloud(double[] vertices, byte[] colors)
                {
                    nVertices = vertices.Length / 3;
                    nColors = colors.Length / 3;
                    Mode = PrimitiveType.Points;
                    Update(vertices, colors);
                }
                public void Update(double[] vertices, byte[] colors)
                {
                    UpdateVertices(vertices);
                    UpdateColors(colors);
                }
                public void UpdateVertices(double[] vertices)
                {
                    nVertices = vertices.Length / 3;
                    Vertices = new double[nVertices * 3];
                    Buffer.BlockCopy(vertices, 0, Vertices, 0, 8 * nVertices * 3);
                }
                public void UpdateColors(byte[] colors)
                {
                    nColors = colors.Length / 3;
                    Colors = new byte[nColors * 3];
                    Buffer.BlockCopy(colors, 0, Colors, 0, nColors * 3);
                }
                public override void WriteElements()
                {
                    GL.Enable(EnableCap.ProgramPointSize);
                    GL.PointSize(PointSize);
                    GL.DrawArrays(Mode, 0, nVertices);
                }
            }
            public class Line : Primitive
            {
                public Line(double[] vertices, byte r, byte g, byte b)
                {
                    nVertices = vertices.Length / 3;
                    nColors = vertices.Length / 3;
                    Mode = PrimitiveType.Lines;
                    byte[] colors = new byte[nColors * 3];
                    for (int i = 0; i < nVertices; i++)
                    {
                        colors[i * 3] = r;
                        colors[i * 3 + 1] = g;
                        colors[i * 3 + 2] = b;
                    }
                    Update(vertices, colors);
                }
                public void Update(double[] vertices, byte[] colors)
                {
                    UpdateVertices(vertices);
                    UpdateColors(colors);
                }
                public void UpdateVertices(double[] vertices)
                {
                    nVertices = vertices.Length / 3;
                    Vertices = new double[nVertices * 3];
                    Buffer.BlockCopy(vertices, 0, Vertices, 0, 8 * nVertices * 3);
                    if(nColors!= nVertices)
                    {
                        byte[] colors = Colors;
                        Colors = new byte[nVertices * 3];
                        if (nColors > 0)
                            for (int i = 0; i < nVertices; i++)
                            {
                                int id = i % nColors;
                                Colors[i * 3] = colors[id * 3];
                                Colors[i * 3 + 1] = colors[id * 3 + 1];
                                Colors[i * 3 + 2] = colors[id * 3 + 2];
                            }
                        nColors = nVertices;
                    }
                }
                public void UpdateColors(byte[] colors)
                {
                    nColors = colors.Length / 3;
                    Colors = new byte[nColors * 3];
                    Buffer.BlockCopy(colors, 0, Colors, 0, nColors * 3);
                    if (nVertices != nColors)
                    {
                        double[] vertices = Vertices;
                        Vertices = new double[nColors * 3];
                        if (nVertices > 0)
                            for (int i = 0; i < nColors; i++)
                            {
                                int id = i % nVertices;
                                Vertices[i * 3] = vertices[id * 3];
                                Vertices[i * 3 + 1] = vertices[id * 3 + 1];
                                Vertices[i * 3 + 2] = vertices[id * 3 + 2];
                            }
                        nVertices = nColors;
                    }
                }
                public override void WriteElements()
                {
                    GL.Enable(EnableCap.ProgramPointSize);
                    GL.PointSize(10);
                    GL.DrawArrays(Mode, 0, nVertices);
                }
            }
            public class Selectable : Primitive
            {
                public bool Selected = false;
                byte[] ID = new byte[3] { 255, 255, 255 };

                public void SetID(int id)
                {
                    ID = BitConverter.GetBytes(id);
                }
                public int GetID()
                {
                    return BitConverter.ToInt32(ID, 0);
                }

                public override void WriteColor()
                {
                    if (Selected)
                    {
                        byte[] NewColors = new byte[nColors * 3];
                        for (int i = 0; i < nColors; i++)
                        {
                            NewColors[i * 3] = (byte)(255 - Colors[i * 3]);
                            NewColors[i * 3 + 1] = (byte)(255 - Colors[i * 3 + 1]);
                            NewColors[i * 3 + 2] = (byte)(255 - Colors[i * 3 + 2]);
                        }
                        GL.ColorPointer(3, ColorPointerType.UnsignedByte, 0, NewColors.Pointer());
                    }
                    else
                        GL.ColorPointer(3, ColorPointerType.UnsignedByte, 0, Colors.Pointer());
                }

                public class Line : Selectable
                {
                    public Line() : this(0, 0, 0, 0, 0, 0) { }
                    public Line(double[] p) : this(p[0], p[1], p[2], p[3], p[4], p[5]) { }
                    public Line(Vector<Dimensions.III> start, Vector<Dimensions.III> end) : this(start[0], start[1], start[2], end[0], end[1], end[2]) { }
                    public Line(double startx, double starty, double startz, double endx, double endy, double endz)
                    {
                        nVertices = 2;
                        nColors = nVertices;
                        Mode = PrimitiveType.Lines;
                        Vertices = new double[nVertices * 3];
                        Colors = new byte[nColors * 3];
                        Indices = new uint[nVertices].Select((I, i) => (uint)i).ToArray();
                        Update(startx, starty, startz, endx, endy, endz);
                    }
                    public void Update(double startx, double starty, double startz, double endx, double endy, double endz)
                    {
                        Update(new double[3] { startx, starty, startz }, new double[3] { endx, endy, endz });
                    }
                    public void Update(double[] p)
                    {
                        Update(new double[3] { p[0], p[1], p[2] }, new double[3] { p[3], p[4], p[5] });
                    }
                    public void Update(double[] start, double[] end)
                    {
                        Vertices[0] = start[0];
                        Vertices[1] = start[1];
                        Vertices[2] = start[2];
                        Vertices[3] = end[0];
                        Vertices[4] = end[1];
                        Vertices[5] = end[2];
                    }
                }
            }

            public class CoordinateSystem : Primitive
            {
                public CoordinateSystem()
                {
                    nVertices = 4;
                    nColors = nVertices;
                    Mode = PrimitiveType.Lines;
                    Vertices = new double[4 * 3];
                    Colors = new byte[4 * 3] {0, 0, 0,
                                      255, 0, 0,
                                      0, 255, 0,
                                      0, 0, 255};
                    Indices = new uint[6] { 0, 1, 0, 2, 0, 3 };
                    Update(new double[3], new double[3] { 1, 0, 0 }, new double[3] { 0, 1, 0 }, new double[3] { 0, 0, 1 });
                }

                public void Update(double[] center, double[] axis1, double[] axis2, double[] axis3)
                {
                    Vertices[0] = center[0];
                    Vertices[1] = center[1];
                    Vertices[2] = center[2];

                    Vertices[3] = center[0] + axis1[0];
                    Vertices[4] = center[1] + axis1[1];
                    Vertices[5] = center[2] + axis1[2];

                    Vertices[6] = center[0] + axis2[0];
                    Vertices[7] = center[1] + axis2[1];
                    Vertices[8] = center[2] + axis2[2];

                    Vertices[9] = center[0] + axis3[0];
                    Vertices[10] = center[1] + axis3[1];
                    Vertices[11] = center[2] + axis3[2];
                }
            }
            public class Plane : Primitive
            {
                float[] UV;
                int texture = -1;
                public Plane(double[] TopLeft, double[] TopRight, double[] BottomRight, double[] BottomLeft)
                {
                    Mode = PrimitiveType.Quads;
                    nVertices = 4;
                    nColors = 4;
                    UV = new float[] { 0, 0, 1, 0, 1, 1, 0, 1 };
                    Colors = new byte[nColors * 3];
                    Update(TopLeft, TopRight, BottomRight, BottomLeft);
                }

                public void Update(double[] TopLeft, double[] TopRight, double[] BottomRight, double[] BottomLeft)
                {
                    Vertices = new double[4 * 3]{TopLeft[0], TopLeft[1],TopLeft[2],
                    TopRight[0],TopRight[1],TopRight[2],
                    BottomRight[0],BottomRight[1],BottomRight[2],
                    BottomLeft[0],BottomLeft[1],BottomLeft[2]};
                    Indices = new uint[4] { 0, 1, 2, 3 };
                }

                public override void WriteAll()
                {
                    if (texture != -1)
                    {
                        int i = 0;
                        GL.Enable(EnableCap.Texture2D);
                        GL.BindTexture(TextureTarget.Texture2D, texture);

                        GL.Begin(PrimitiveType.Quads);

                        GL.TexCoord2(0, 0);
                        GL.Vertex3(Vertices[i++], Vertices[i++], Vertices[i++]);

                        GL.TexCoord2(1, 0);
                        GL.Vertex3(Vertices[i++], Vertices[i++], Vertices[i++]);

                        GL.TexCoord2(1, 1);
                        GL.Vertex3(Vertices[i++], Vertices[i++], Vertices[i++]);

                        GL.TexCoord2(0, 1);
                        GL.Vertex3(Vertices[i++], Vertices[i++], Vertices[i++]);

                        GL.End();

                        GL.Disable(EnableCap.Texture2D);
                    }
                    else
                        base.WriteAll();
                }

                public void SetTexture(Bitmap bmp, bool PixelInterpolation = true)
                {
                    if (texture == -1)
                    {
                        GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
                        GL.GenTextures(1, out texture);
                    }
                    GL.BindTexture(TextureTarget.Texture2D, texture);

                    System.Drawing.Imaging.BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                    if (bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, data.Width, data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, data.Scan0);
                    else
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, data.Width, data.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);

                    bmp.UnlockBits(data);

                    if (PixelInterpolation)
                    {
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    }
                    else
                    {
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    }
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                }
            }
            public class Disk : Primitive
            {
                Vector<Dimensions.III> Center;
                Vector<Dimensions.III>[] Axis;
                double Radius;
                double[] VerticesNormalized;
                public Disk(Vector<Dimensions.III> Center, Vector<Dimensions.III> Normal, double Radius)
                {
                    Mode = PrimitiveType.TriangleFan;
                    nVertices = 40;
                    nColors = nVertices;
                    Colors = new byte[nColors * 3];
                    Indices = new uint[nVertices];

                    this.Center = Center;
                    this.Radius = Radius;
                    Axis = Normal.Normals();

                    VerticesNormalized = new double[nVertices * 3];
                    Vertices = new double[nVertices * 3];
                    for (int i = 1; i < nVertices; i++)
                    {
                        Indices[i] = (uint)i;
                        double angle = 2.0 * Math.PI * (i - 1) / (nVertices - 2);
                        double angle_cos = Math.Cos(angle);
                        double angle_sin = Math.Sin(angle);

                        for (int d = 0; d < 3; d++)
                            VerticesNormalized[i * 3 + d] = Axis[0][d] * angle_cos + Axis[1][d] * angle_sin;
                    }
                    Update();
                }

                void Update()
                {
                    for (int i = 0; i < nVertices; i++)
                        for (int d = 0; d < 3; d++)
                            Vertices[i * 3 + d] = Radius * (VerticesNormalized[i * 3 + d]) + Center[d];
                }

                public override Vector<Dimensions.III> GetCenter()
                {
                    return Center;
                }
                public override void SetCenter(Vector<Dimensions.III> newCenter)
                {
                    Center = newCenter;
                    Update();
                }
                public override void Translate(double x, double y, double z)
                {
                    Center[0] += x;
                    Center[1] += y;
                    Center[2] += z;
                    SetCenter(Center);
                }
                public override void Scale(double s)
                {
                    Radius *= s;
                    Update();
                }
            }

            public class Cube : Primitive
            {
                public Cube(Vector<Dimensions.III> center, Matrix<Dimensions.III> basis)
                {
                    Mode = PrimitiveType.Quads;
                    nVertices = 8;
                    nColors = 8;
                    Vertices = new double[nVertices * 3];
                    Colors = new byte[nColors * 4];

                    Indices = new uint[6 * 4]
                    {
                        0,1,2,3,
                        5,4,7,6,
                        1,5,6,2,
                        4,0,3,7,
                        4,5,1,0,
                        3,2,6,7
                    };
                    Update(center,basis);
                }

                int[] corners = new int[3 * 8] {
                    -1,-1,1,
                    1,-1,1,
                    1,1,1,
                    -1,1,1,
                    -1,-1,-1,
                    1,-1,-1,
                    1,1,-1,
                    -1,1,-1};
                public void Update(Vector<Dimensions.III> center, Matrix<Dimensions.III> basis)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector<Dimensions.III> v = center + corners[i * 3] * basis.XAxis + corners[i * 3 + 1] * basis.YAxis + corners[i * 3 + 2] * basis.ZAxis;
                        Vertices[i * 3] = v.X;
                        Vertices[i * 3 + 1] = v.Y;
                        Vertices[i * 3 + 2] = v.Z;
                    }
                }

                public override void WriteColor()
                {
                    GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, Colors.Pointer());
                }

                public override void SetColor(byte r, byte g, byte b)
                {
                    for (int i = 0; i < nColors; i++)
                    {
                        Colors[i * 4] = r;
                        Colors[i * 4 + 1] = g;
                        Colors[i * 4 + 2] = b;
                        Colors[i * 4 + 3] = 200;
                    }
                }
            }
            public class Cylinder : Primitive
            {
                public Cylinder(Vector<Dimensions.III> start, Vector<Dimensions.III> end, double radius)
                {
                    Mode = PrimitiveType.Quads;
                    nVertices = 100;
                    nColors = 100;
                    Colors = new byte[nColors * 4];
                    Update(start, end, radius);
                }

                public void Update(Vector<Dimensions.III> start, Vector<Dimensions.III> end, double radius)
                {
                    Vector<Dimensions.III> dir = end - start;
                    Vector<Dimensions.III>[] normals = dir.Normals();

                    List<uint> indices = new List<uint>();
                    List<double[]> vertices = new List<double[]>();
                    for (int i = 0; i < nVertices / 2; i++)
                    {
                        double s = Math.Sin((double)i / nVertices * Math.PI * 4);
                        double c = Math.Cos((double)i / nVertices * Math.PI * 4);
                        Vector<Dimensions.III> vertex1 = (normals[0] * c + normals[1] * s) * radius + start;
                        Vector<Dimensions.III> vertex2 = (normals[0] * c + normals[1] * s) * radius + end;
                        vertices.Add(vertex1.Data);
                        indices.Add((uint)vertices.Count - 1);
                        vertices.Add(vertex2.Data);
                        indices.Add((uint)vertices.Count - 1);
                        if (i == nVertices / 2 - 1)
                        {
                            indices.Add(1);
                            indices.Add(0);
                        }
                        else
                        {
                            indices.Add((uint)vertices.Count + 2 - 1);
                            indices.Add((uint)vertices.Count + 1 - 1);
                        }
                    }
                    Vertices = vertices.SelectMany(v => v).ToArray();
                    Indices = indices.ToArray();
                }

                public override void WriteColor()
                {
                    GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, Colors.Pointer());
                }

                public override void SetColor(byte r, byte g, byte b)
                {
                    for (int i = 0; i < nColors; i++)
                    {
                        Colors[i * 4] = r;
                        Colors[i * 4 + 1] = g;
                        Colors[i * 4 + 2] = b;
                        Colors[i * 4 + 3] = 200;
                    }
                }
            }

            public class Sphere : Primitive
            {
                Vector<Dimensions.III> Center;
                double Radius;

                int nEquator = 40;
                int nMeridian = 20;
                List<double[]> VerticesNormalized = new List<double[]>();
                public Sphere(Vector<Dimensions.III> Center, double Radius)
                {
                    Mode = PrimitiveType.Quads;
                    nVertices = nEquator * nMeridian + 2;
                    nColors = nVertices;
                    Colors = new byte[nColors * 3];

                    this.Center = Center;
                    this.Radius = Radius;

                    List<uint> lstIndices = new List<uint>();
                    uint ind = 0;
                    VerticesNormalized = new List<double[]>();
                    for (int i = 0; i < nMeridian; i++)
                    {
                        double angle1 = Math.PI * i / (nMeridian-1);
                        double c1 = Math.Cos(angle1);
                        double s1 = Math.Sin(angle1);
                        for (int j = 0; j < nEquator; j++)
                        {
                            double angle2 = 2.0 * Math.PI * i / nEquator;
                            double c2 = Math.Cos(angle2);
                            double s2 = Math.Sin(angle2);

                            double x = s1 * c2;
                            double y = s1 * s2;
                            double z = c1;
                            VerticesNormalized.Add(new double[] { x, y, z });
                            if (i > 0)
                            {
                                lstIndices.Add(ind);
                                lstIndices.Add(ind - 1);
                                lstIndices.Add((uint)(ind - 1 - nEquator));
                                lstIndices.Add((uint)(ind - nEquator));
                            }
                            ind++;
                        }
                    }
                    Indices = lstIndices.ToArray();
                    
                    Update();
                }

                void Update()
                {
                    for (int i = 0; i < nVertices; i++)
                        for (int d = 0; d < 3; d++)
                            Vertices[i * 3 + d] = Radius * (VerticesNormalized[i][d]) + Center[d];
                }

                public override Vector<Dimensions.III> GetCenter()
                {
                    return Center;
                }
                public override void SetCenter(Vector<Dimensions.III> newCenter)
                {
                    Center = newCenter;
                    Update();
                }
                public override void Translate(double x, double y, double z)
                {
                    Center[0] += x;
                    Center[1] += y;
                    Center[2] += z;
                    SetCenter(Center);
                }
                public override void Scale(double s)
                {
                    Radius *= s;
                    Update();
                }
            }
            public class TriSurf : Primitive
            {
                public TriSurf(double[] Vertices, uint[] Indices, byte[] Colors)
                {
                    this.Vertices = Vertices;
                    this.Indices = Indices;
                    Mode = PrimitiveType.Triangles;
                    nVertices = Vertices.Length / 3;
                    if (Colors.Length == 3)
                    {
                        byte[] colors = (byte[])Colors.Clone();
                        Colors = new byte[3 * nVertices];
                        for (int i = 0; i < nVertices; i++)
                        {
                            Colors[i * 3] = colors[0];
                            Colors[i * 3 + 1] = colors[1];
                            Colors[i * 3 + 2] = colors[2];
                        }
                    }
                    this.Colors = Colors;
                }
            }
        }

        public class SceneControl : GLControl
        {
            public double FovY = 43.0 / 180.0 * Math.PI;
            public double Aspect = 4.0 / 3.0;
            public double NearZ = 0.1;
            public double FarZ = 50.0;

            double TranslationSpeed = 0.1;
            double SlowFactor = 1.0;

            Color BGColor = Color.CornflowerBlue;

            Camera MainCamera = new Camera();
            Primitive.CoordinateSystem WorldCS;
            List<Primitive> SceneObjects = new List<Primitive>();
            bool isLoaded = false;
            bool isDestroyed = false;
            public SceneControl()
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                    return;
                MenuStrip mnu = new MenuStrip();
                ToolStripMenuItem ChooseBackColor = new ToolStripMenuItem();
                ChooseBackColor.Text = "Color";
                KnownColor[] colors = (KnownColor[])Enum.GetValues(typeof(KnownColor));
                foreach (KnownColor knowColor in colors)
                    ChooseBackColor.DropDownItems.Add(knowColor.ToString());
                ChooseBackColor.DropDownItemClicked += (s, e) =>
                {
                    BGColor = Color.FromName(e.ClickedItem.Text);
                    GL.ClearColor(BGColor);
                    DrawNow();
                };
                mnu.Items.Add(ChooseBackColor);

                ToolStripMenuItem SaveSnap = new ToolStripMenuItem();
                SaveSnap.Text = "Take Snapshot";
                SaveSnap.Click += (s,e)=>
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
                    if(sfd.ShowDialog() == DialogResult.OK)
                        ScreenShot().Save(sfd.FileName);
                };
                mnu.Items.Add(SaveSnap);

                ContextMenu = new ContextMenu();

                Controls.Add(mnu);

                HandleDestroyed += (s, e) =>
                  {
                      isDestroyed = true;
                  };

                MouseWheel += MainView_MouseWheel;
                MouseMove += MainView_MouseMove;
                MouseDown += MainView_MouseDown;
                MouseUp += MainView_MouseUp;
                Load += (s, e) =>
                {
                    MakeCurrent();
                    GL.Viewport(0, 0, Width, Height);

                    Matrix4d matrixProjection = Matrix4d.Perspective(FovY, Aspect, NearZ, FarZ);
                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadMatrix(ref matrixProjection);
                    GL.ClearColor(BGColor);

                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthRange(NearZ, FarZ);

                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    GL.Enable(EnableCap.CullFace);

                    GL.LineWidth(2f);

                    GL.EnableClientState(ArrayCap.VertexArray);
                    GL.EnableClientState(ArrayCap.ColorArray);
                    isLoaded = true;
                };
                Paint += (s, e) =>
                {
                    DrawNow();
                };
                Resize += (s, e) =>
                {
                    GL.Viewport(0, 0, Width, Height);
                };

                WorldCS = new Primitive.CoordinateSystem();

                // Construct Timer to Catch Keyboard Events
                Timer tmrUserHandler = new Timer();
                //tmrUserHandler.Tick += tmrUserHandler_Tick;
                tmrUserHandler.Interval = 30;
                tmrUserHandler.Start();

                this.Show();
                this.Location = new Point(0, 0);
            }

            public void SetMainCamera(Vector<Dimensions.III> Origo, Vector<Dimensions.III> XAxis, Vector<Dimensions.III> YAxis, Vector<Dimensions.III> ZAxis)
            {
                if(Origo != null)
                    MainCamera.Origo = ToVector3d(Origo);
                if (XAxis != null) 
                    MainCamera.XAxis = ToVector3d(XAxis);
                if (YAxis != null) 
                    MainCamera.YAxis = ToVector3d(YAxis);
                if (ZAxis != null) 
                    MainCamera.ZAxis = ToVector3d(ZAxis);
            }
            public void SetMainCamera(IO.XML xml)
            {
                MainCamera.Origo = XML2Vector3d(xml.getElement("Origo"));
                MainCamera.XAxis = XML2Vector3d(xml.getElement("XAxis"));
                MainCamera.YAxis = XML2Vector3d(xml.getElement("YAxis"));
                MainCamera.ZAxis = XML2Vector3d(xml.getElement("ZAxis"));
            }
            public IO.XML GetMainCamera()
            {
                return new IO.XML("MainCamera",
                    Vector3d2XML("Origo", MainCamera.Origo),
                    Vector3d2XML("XAxis", MainCamera.XAxis),
                    Vector3d2XML("YAxis", MainCamera.YAxis),
                    Vector3d2XML("ZAxis", MainCamera.ZAxis));
            }
            List<GLControl> SecondaryCameras = new List<GLControl>();
            /*public GLControl AddCamera()
            {
                GLControl glControl = new GLControl();
                SecondaryCameras.Add(glControl);
            }*/
            public void ShowSceneObjects(bool show)
            {
                foreach (Primitive sceneobject in SceneObjects)
                    sceneobject.Show = show;
            }
            public void AddSceneObject(Primitive primitive)
            {
                SceneObjects.Add(primitive);
            }
            public void RemoveAllSceneObjects()
            {
                SceneObjects.Clear();
            }
            public void RemoveSceneObject(Primitive primitive)
            {
                if (SceneObjects.Contains(primitive))
                    SceneObjects.Remove(primitive);
            }

            public enum TransformModes {Game,Origo,OrigoFixed}
            public TransformModes TransformMode = TransformModes.Game;
            Point prevMouseLocation;
            Vector3d Origin = Vector3d.Zero;
            public event EventHandler<IO.XML> MainCameraChanged = null;
            private void MainView_MouseMove(object sender, MouseEventArgs e)
            {
                if (!prevMouseLocation.IsEmpty)
                {
                    Point diffLocation = new Point(e.X - prevMouseLocation.X, e.Y - prevMouseLocation.Y);
                    switch (TransformMode)
                    {
                        case TransformModes.Game:
                            if (e.Button == MouseButtons.Left)
                            {
                                MainCamera.RotateY(-diffLocation.X * Math.PI / 2 / Width);
                                MainCamera.RotateX(diffLocation.Y * Math.PI / 2 / Height);
                            }
                            else if (e.Button == MouseButtons.Right)
                            {
                                MainCamera.Translate(diffLocation.X * 10.0 / Width, diffLocation.Y * 10.0 / Height, 0);
                            }
                            break;
                        case TransformModes.Origo:
                            
                            if (e.Button == MouseButtons.Left)
                            {
                                MainCamera.RotateAroundY(-diffLocation.X * Math.PI / Width, Origin);
                                MainCamera.RotateAroundX(diffLocation.Y * Math.PI / Height, Origin);
                            }
                            else if (e.Button == MouseButtons.Right)
                            {
                                Origin += MainCamera.Translate(diffLocation.X * 10.0 / Width, diffLocation.Y * 10.0 / Height, 0);
                            }
                            break;
                        case TransformModes.OrigoFixed:
                            if (e.Button == MouseButtons.Left)
                            {
                                MainCamera.RotateAroundY(-diffLocation.X * Math.PI / Width, Origin);
                                MainCamera.RotateAroundX(diffLocation.Y * Math.PI / Height, Origin);
                                MainCamera.XAxis = Vector3d.Cross(Vector3d.UnitZ, MainCamera.ZAxis);
                                if (MainCamera.XAxis.LengthSquared == 0)
                                    MainCamera.XAxis = Vector3d.UnitX;
                                MainCamera.YAxis = Vector3d.Cross(MainCamera.ZAxis, MainCamera.XAxis);
                            }
                            else if (e.Button == MouseButtons.Right)
                            {
                                Origin += MainCamera.Translate(diffLocation.X * 10.0 / Width, diffLocation.Y * 10.0 / Height, 0);
                            }
                            break;
                    }
                    DrawNow();
                }
                prevMouseLocation = e.Location;
            }
            private void MainView_MouseDown(object sender, MouseEventArgs e)
            {
                prevMouseLocation = e.Location;
            }
            private void MainView_MouseUp(object sender, MouseEventArgs e)
            {
                prevMouseLocation = Point.Empty;
                MainCameraChanged?.Invoke(this, GetMainCamera());
            }
            void MainView_MouseWheel(object sender, MouseEventArgs e)
            {
                if (e.Delta > 0)
                    MainCamera.Translate(0, 0, TranslationSpeed);
                else
                    MainCamera.Translate(0, 0, -TranslationSpeed);
                DrawNow();
            }

            public Bitmap ScreenShot()
            {
                return ScreenShotBGR().ToBitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            }

            public byte[] ScreenShotBGR()
            {
                DrawNow();
                DrawNow(); //Twice to ensure draw ?????
                MakeCurrent();

                GL.ClearColor(Color.Black);
                byte[] BGR = new byte[Width * Height * 3];
                GL.ReadPixels<byte>(0, 0, Width, Height, PixelFormat.Bgr, PixelType.UnsignedByte, BGR);

                return BGR;
            }

            public void DrawNow()
            {
                if (isLoaded && !isDestroyed)
                {
                    MakeCurrent();
                    Draw(MainCamera);
                    SwapBuffers();
                }
            }
            void Draw(Camera camera)
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.MatrixMode(MatrixMode.Modelview);
                Matrix4d LookAtMatrix;
                LookAtMatrix = Matrix4d.LookAt(camera.Origo, camera.Origo + camera.ZAxis, camera.YAxis);
                GL.LoadMatrix(ref LookAtMatrix);

                foreach (Primitive primitive in SceneObjects)
                    primitive.Draw();
                WorldCS.Draw();
            }

            /*static void tmrUserHandler_Tick(object sender, EventArgs e)
        {
            Camera camera = MainCamera;
            Form form = MainWindow;

            if (form.ContainsFocus && !IsChoosingColor)
            {
                // Reset
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.F5))
                    camera.Reset();

                Vector3d translation = new Vector3d();
                // Translate Eye Point (Walk Around)
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.W))
                    translation.Z += TranslationSpeed;
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.S))
                    translation.Z -= TranslationSpeed;
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.D))
                    translation.X -= TranslationSpeed;
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.A))
                    translation.X += TranslationSpeed;
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.Space))
                    translation.Y += TranslationSpeed;
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
                    translation.Y -= TranslationSpeed;

                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift))
                    SlowFactor = 0.1;
                else
                    SlowFactor = 1.0;

                camera.Translate(translation * SlowFactor);

                // Rotate (Look Around)
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.Left))
                    camera.RotateY(RotationSpeed);
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.Right))
                    camera.RotateY(-RotationSpeed);
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.Up))
                    camera.RotateX(-RotationSpeed);
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.Down))
                    camera.RotateX(RotationSpeed);
                //Paint(MainView);
                Paint((GLControl)form.Controls[0]);
            }
        }*/
        }
    }
    
}
