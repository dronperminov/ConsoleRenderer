using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleRenderer {
    class Renderer {
        const double FOV = 0.6;
        const double NEAR = 0;
        const double FAR = 40;
        double aspectRatio;

        bool showCoords = true; // C
        bool showNormals = false; // N
        bool gridPoligons = false; // G
        bool lightOn = true; // L

        bool flight = true;

        int height;
        int width;

        double[] matrix = new double[16];
        Camera camera;

        Color[][] colorBuffer; // буфер цветов
        double[][] zBuffer; // буфер глубины

        double z_min, z_max;

        PictureBox box;
        Bitmap bitmap;
        LockBitmap bm;
        Graphics g;

        List<Figure> figures;
        Coord coord; // координатные оси
        PointLight light;
        List<DirectedLight> dirLights;

        bool mousePressed = false;
        Point mousePosition = new Point(0, 0);

        public Renderer(PictureBox box, Vector position, Vector angle, double lightValue = 0.1) {
            this.box = box;

            height = box.Height;
            width = box.Width;

            aspectRatio = (double)width / height;

            bitmap = new Bitmap(width, height);
            bm = new LockBitmap(bitmap);
            g = Graphics.FromImage(bitmap);

            colorBuffer = new Color[height][];
            zBuffer = new double[height][];

            for (int i = 0; i < height; i++) {
                colorBuffer[i] = new Color[width];
                zBuffer[i] = new double[width];
            }

            camera = new Camera(position, angle);

            dirLights = new List<DirectedLight>();

            dirLights.Add(new DirectedLight(0, 0, lightValue));
            dirLights.Add(new DirectedLight(0, 0, -lightValue));

            dirLights.Add(new DirectedLight(0, lightValue, 0));
            dirLights.Add(new DirectedLight(0, -lightValue, 0));

            dirLights.Add(new DirectedLight(lightValue, 0, 0));
            dirLights.Add(new DirectedLight(-lightValue, 0, 0));

            light = new PointLight(-6, 12, -6);

            FiguresInit();

            buildMatrix();
        }

        void FiguresInit() {
            figures = new List<Figure>();

            figures.Add(new Pyramid(0, 0, 5, 2, 4, 8, Color.Red));
            figures.Add(new Cube(0, 0, 0, 4, Color.IndianRed));
            figures.Add(new Cube(4, 0, 2, 4, Color.LightBlue));
            figures.Add(new Cube(-4, 0, 2, 4, Color.MediumSpringGreen));
            figures.Add(new Cube(-5, 0, 5, 2, Color.Blue));
            figures.Add(new Sphere(0, 4, 5, 2, 50, Color.Red, 50));
            figures.Add(new Prism(-10, 0, 7.5, 4, 4, 8, 8, Color.Green));
            
            coord = new Coord(10);
        }

        public void KeyDown(object sender, KeyEventArgs e) {
            if (e.Control) {
                if (e.KeyCode == Keys.NumPad8)
                    light.Move(0, 0, 1);

                if (e.KeyCode == Keys.NumPad2)
                    light.Move(0, 0, -1);

                if (e.KeyCode == Keys.NumPad4)
                    light.Move(1, 0, 0);

                if (e.KeyCode == Keys.NumPad6)
                    light.Move(-1, 0, 0);

                if (e.KeyCode == Keys.NumPad9)
                    light.Move(0, 1, 0);

                if (e.KeyCode == Keys.NumPad3)
                    light.Move(0, -1, 0);

                return;
            }
            
            // повороты камеры
            if (e.KeyCode == Keys.Left)
                camera.RotateLeft();            

            if (e.KeyCode == Keys.Right)
                camera.RotateRight();

            if (e.KeyCode == Keys.Up)
                camera.RotateUp();

            if (e.KeyCode == Keys.Down)
                camera.RotateDown();

            if (e.KeyCode == Keys.R) {
                camera.Reset();
                light.Reset();
            }

            // движение камеры
            if (e.KeyCode == Keys.E)
                camera.Up();

            if (e.KeyCode == Keys.Q)
                camera.Down();

            if (e.KeyCode == Keys.D)
                camera.Right();

            if (e.KeyCode == Keys.A)
                camera.Left();

            if (e.KeyCode == Keys.W)
                camera.Forward();

            if (e.KeyCode == Keys.S)
                camera.Backward();

            // движение по осям
            if (e.KeyCode == Keys.NumPad8)
                camera.Move(0, 0, 1);

            if (e.KeyCode == Keys.NumPad2)
                camera.Move(0, 0, -1);

            if (e.KeyCode == Keys.NumPad4)
                camera.Move(1, 0, 0);

            if (e.KeyCode == Keys.NumPad6)
                camera.Move(-1, 0, 0);

            if (e.KeyCode == Keys.NumPad9)
                camera.Move(0, 1, 0);

            if (e.KeyCode == Keys.NumPad3)
                camera.Move(0, -1, 0);


            if (e.KeyCode == Keys.N)
                showNormals = !showNormals;

            if (e.KeyCode == Keys.G)
                gridPoligons = !gridPoligons;

            if (e.KeyCode == Keys.C)
                showCoords = !showCoords;

            if (e.KeyCode == Keys.L)
                lightOn = !lightOn;

            if (e.KeyCode == Keys.Oemplus) {
                for (int i = 0; i < dirLights.Count; i++)
                    dirLights[i].ChangeIntensity(1.1);
            }

            if (e.KeyCode == Keys.OemMinus) {
                for (int i = 0; i < dirLights.Count; i++)
                    dirLights[i].ChangeIntensity(0.9);
            }
        }

        public void MouseClick(object sender, MouseEventArgs e) {
            mousePressed = !mousePressed;

            if (mousePressed)
                Cursor.Hide();
            else
                Cursor.Show();
        }

        public void MouseMove(object sender, MouseEventArgs e) {
            if (!mousePressed)
                return;

            Point p = Cursor.Position;

            double dx = e.X - mousePosition.X;
            double dy = e.Y - mousePosition.Y;

            if (Math.Abs(dx) > 1) {
                if (dx > 0)
                    camera.RotateRight();
                else
                    camera.RotateLeft();
            }

            if (Math.Abs(dy) > 1) {
                if (dy > 0)
                    camera.RotateDown();
                else
                    camera.RotateUp();
            }

            mousePosition.X = e.X;
            mousePosition.Y = e.Y;
        }

        public void MouseWheel(object sender, MouseEventArgs e) {
            if (e.Delta / 120 > 0)
                camera.Forward();
            else
                camera.Backward();
        }

        void buildMatrix() {
            double frustumDepth = (FAR - NEAR);
            double oneOverDepth = 1.0 / frustumDepth;

            matrix[1] = 0;
            matrix[2] = 0;
            matrix[3] = 0;
            matrix[4] = 0;
            matrix[5] = 1.0 / Math.Tan(FOV * 0.5);

            matrix[0] = matrix[5] / aspectRatio;

            matrix[6] = 0;
            matrix[7] = 0;
            matrix[8] = 0;
            matrix[9] = 0;

            matrix[10] = FAR * oneOverDepth;
            matrix[11] = 1.0;
            matrix[12] = 0;
            matrix[13] = 0;
            matrix[14] = (-FAR * NEAR) * oneOverDepth;
            matrix[15] = 0;
        }

        Vector ToPerspective(Vector point) {
            double ax = camera.GetAngle().x;
            double ay = camera.GetAngle().y;
            double az = camera.GetAngle().z;

            double ix = point.x - camera.GetPoint().x;
            double iy = point.y - camera.GetPoint().y;
            double iz = point.z - camera.GetPoint().z;
            double iw = point.w - camera.GetPoint().w;

            double x1 = iz * Math.Sin(ax) + ix * Math.Cos(ax);
            double y1 = iy;
            double z1 = iz * Math.Cos(ax) - ix * Math.Sin(ax);

            double x11 = x1;
            double y11 = y1 * Math.Cos(ay) - z1 * Math.Sin(ay);
            double z11 = y1 * Math.Sin(ay) + z1 * Math.Cos(ay);

            double x111 = y11 * Math.Sin(az) + x11 * Math.Cos(az);
            double y111 = y11 * Math.Cos(az) - x11 * Math.Sin(az);
            double z111 = z11;

            ix = x111;
            iy = y111;
            iz = z111;

            double ox = matrix[0] * ix + matrix[4] * iy + matrix[8] * iz + matrix[12] * iw;
            double oy = matrix[1] * ix + matrix[5] * iy + matrix[9] * iz + matrix[13] * iw;
            double oz = matrix[2] * ix + matrix[6] * iy + matrix[10] * iz + matrix[14] * iw;
            double ow = matrix[3] * ix + matrix[7] * iy + matrix[11] * iz + matrix[15] * iw;

            Vector vect = new Vector();

            vect.x = (ox * width) / (2 * ow) + (width / 2.0);
            vect.y = (oy * height) / (2 * ow) + (height / 2.0);
            vect.z = oz;
            vect.w = ow;

            return vect;
        }

        public void Render() {
            Stopwatch t = new Stopwatch();
            t.Start();

            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    colorBuffer[i][j] = Color.Black;
                    zBuffer[i][j] = double.MaxValue;
                }
            }

            z_min = double.MaxValue;
            z_max = double.MinValue;

            figures.Add(new Sphere(light.GetV().x, light.GetV().y, light.GetV().z, 0.1, 50, Color.Yellow, 50, false));

            for (int i = 0; i < figures.Count; i++)
                figures[i].Draw(this);

            figures.RemoveAt(figures.Count - 1);

            if (showCoords)
                coord.Draw(this);

            t.Stop();
            Console.WriteLine("Rendering time: {0} ms", t.ElapsedMilliseconds);
        }

        public void Draw() {
            Stopwatch t = new Stopwatch();
            t.Start();

            g.Clear(Color.Black);

            bm.LockBits();
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    if (zBuffer[i][j] == double.MaxValue)
                        continue;

                    bm.SetPixel(j, i, colorBuffer[i][j]);
                }
            }

            bm.UnlockBits();

            g.DrawString("viewpoint: " + camera.PointInfo(), new Font("Arial", 10), Brushes.White, 10, 10);
            g.DrawString("viewangle: " + camera.AngleInfo(), new Font("Arial", 10), Brushes.White, 10, 30);
            g.DrawString("Zmin: " + z_min, new Font("Arial", 10), Brushes.White, 10, 50);
            g.DrawString("Zmax: " + z_max, new Font("Arial", 10), Brushes.White, 10, 70);
            g.DrawString("Light1: " + light.GetV().ToString(), new Font("Arial", 10), Brushes.White, 10, 90);

            if (showNormals)
                g.DrawString("Normals on", new Font("Arial", 10), Brushes.White, 10, 130);

            box.Image = bitmap;

            t.Stop();
            Console.WriteLine("Drawing time: {0} ms", t.ElapsedMilliseconds);
        }

        public void MakeLine(Vector pointA, Vector pointB, Color? color = null) {
            Vector pointAProjected = ToPerspective(pointA);
            Vector pointBProjected = ToPerspective(pointB);

            if (pointAProjected.w <= 0 || pointBProjected.w <= 0)
                return;

            if (pointAProjected.z >= NEAR && pointBProjected.z >= NEAR) {
                DrawLine(pointAProjected, pointBProjected, color);
            }
            else if (pointAProjected.z >= NEAR && pointBProjected.z < NEAR) {
                double n = (pointAProjected.w - NEAR) / (pointAProjected.w - pointBProjected.w);
                double xc = (n * pointAProjected.x) + ((1 - n) * pointBProjected.x);
                double yc = (n * pointAProjected.y) + ((1 - n) * pointBProjected.y);
                double zc = (n * pointAProjected.z) + ((1 - n) * pointBProjected.z);

                pointBProjected.x = xc;
                pointBProjected.y = yc;
                pointBProjected.z = zc;

                DrawLine(pointAProjected, pointBProjected, color);
            }
            else if (pointAProjected.z < NEAR && pointBProjected.z >= NEAR) {
                double n = (pointBProjected.w - NEAR) / (pointBProjected.w - pointAProjected.w);
                double xc = (n * pointBProjected.x) + ((1 - n) * pointAProjected.x);
                double yc = (n * pointBProjected.y) + ((1 - n) * pointAProjected.y);
                double zc = (n * pointBProjected.z) + ((1 - n) * pointAProjected.z);

                pointAProjected.x = xc;
                pointAProjected.y = yc;
                pointAProjected.z = zc;
                DrawLine(pointAProjected, pointBProjected, color);
            }
        }

        public void MakeTriangle(Triangle t) {
            Vector pa = ToPerspective(t.a);
            Vector pb = ToPerspective(t.b);
            Vector pc = ToPerspective(t.c);

            if (pa.w <= 0 || pb.w <= 0 || pc.w <= 0)
                return;

            if (double.IsNaN(pa.x) || double.IsNaN(pa.y) || double.IsNaN(pa.z))
                return;

            if (double.IsNaN(pb.x) || double.IsNaN(pb.y) || double.IsNaN(pb.z))
                return;

            if (double.IsNaN(pc.x) || double.IsNaN(pc.y) || double.IsNaN(pc.z))
                return;

            Vector ab = t.b - t.a;
            Vector ac = t.c - t.a;

            Vector mid = (t.a + t.b + t.c) / 3;
            Vector n = (ab ^ ac).Normalize();
            
            double cos = Math.Max(0, light.GetPoint(mid, n));
            double dirCos = 0;

            if (lightOn) {
                for (int i = 0; i < dirLights.Count; i++)
                    dirCos += Math.Max(0, dirLights[i].GetAngle(n));
            }

            if (t.color != null && !gridPoligons) {
                double R = Math.Min(255, Math.Max(0, t.color.Value.R * (cos + dirCos)));
                double G = Math.Min(255, Math.Max(0, t.color.Value.G * (cos + dirCos)));
                double B = Math.Min(255, Math.Max(0, t.color.Value.B * (cos + dirCos)));

                t.color = Color.FromArgb((int)R, (int)G, (int)B);
            }

            if (gridPoligons) {
                DrawWireframeTriangle(pa, pb, pc, t.color);
            }
            else {
                DrawFilledTriangle(pa, pb, pc, t.color);
            }

            if (showNormals)
                DrawLine(ToPerspective(mid), ToPerspective(mid + n), n.ToColor());
        }

        void swap(ref Vector a, ref Vector b) {
            double x = a.x;
            double y = a.y;
            double z = a.z;

            a.x = b.x;
            a.y = b.y;
            a.z = b.z;

            b.x = x;
            b.y = y;
            b.z = z;
        }

        List<double> Interpolate(double i0, double d0, double i1, double d1) {
            List<double> values = new List<double>();

            if (i0 == i1) {
                values.Add(d0);
                return values;
            }

            double a = (d1 - d0) / (i1 - i0);
            double d = d0;

            if (i1 - i0 > 10000)
                throw new Exception();

            for (int i = (int) i0; i <= (int) i1; i++) {
                values.Add(d);
                d += a;
            }

            return values;
        }

        void DrawLine(Vector v0, Vector v1, Color? color) {
            try {
                if (Math.Abs(v1.x - v0.x) > Math.Abs(v1.y - v0.y)) {
                    if (v0.x > v1.x)
                        swap(ref v0, ref v1);

                    List<double> ys = Interpolate(v0.x, v0.y, v1.x, v1.y);
                    List<double> zs = Interpolate(v0.x, v0.z, v1.x, v1.z);

                    for (int x = (int)v0.x; x <= (int)v1.x; x++) {
                        int y = (int)ys[x - (int)v0.x];

                        if (x < 0 || x >= width || y < 0 || y >= height)
                            continue;

                        set(x, y, zs[x - (int)v0.x], color);
                    }
                }
                else {
                    if (v0.y > v1.y)
                        swap(ref v0, ref v1);

                    List<double> xs = Interpolate(v0.y, v0.x, v1.y, v1.x);
                    List<double> zs = Interpolate(v0.y, v0.z, v1.y, v1.z);

                    for (int y = (int)v0.y; y <= (int)v1.y; y++) {
                        int x = (int)xs[y - (int)v0.y];

                        if (x < 0 || x >= width || y < 0 || y >= height)
                            continue;

                        set(x, y, zs[y - (int)v0.y], color);
                    }
                }
            }
            catch (Exception) {

            }
        }

        void DrawWireframeTriangle(Vector p0, Vector p1, Vector p2, Color? color = null) {
            DrawLine(p0, p1, color);
            DrawLine(p1, p2, color);
            DrawLine(p2, p0, color);
        }

        void DrawFilledTriangle(Vector p0, Vector p1, Vector p2, Color? color = null) {
            if (p1.y < p0.y)
                swap(ref p0, ref p1);

            if (p2.y < p0.y)
                swap(ref p2, ref p0);

            if (p2.y < p1.y)
                swap(ref p2, ref p1);

            try {
                List<double> x01 = Interpolate(p0.y, p0.x, p1.y, p1.x);
                List<double> x12 = Interpolate(p1.y, p1.x, p2.y, p2.x);
                List<double> x02 = Interpolate(p0.y, p0.x, p2.y, p2.x);

                List<double> z01 = Interpolate(p0.y, p0.z, p1.y, p1.z);
                List<double> z12 = Interpolate(p1.y, p1.z, p2.y, p2.z);
                List<double> z02 = Interpolate(p0.y, p0.z, p2.y, p2.z);

                x01.RemoveAt(x01.Count - 1);
                z01.RemoveAt(z01.Count - 1);

                List<double> x012 = new List<double>();
                x012.AddRange(x01);
                x012.AddRange(x12);

                List<double> z012 = new List<double>();
                z012.AddRange(z01);
                z012.AddRange(z12);

                int m = x012.Count / 2;

                List<double> x_left;
                List<double> x_right;

                List<double> z_left;
                List<double> z_right;

                if (x02[m] < x012[m]) {
                    x_left = x02;
                    x_right = x012;

                    z_left = z02;
                    z_right = z012;
                }
                else {
                    x_left = x012;
                    x_right = x02;

                    z_left = z012;
                    z_right = z02;
                }

                int y0 = Math.Max(0, (int)p0.y);
                int y1 = Math.Min(height, (int)p2.y);

                for (int y = y0; y < y1; y++) {
                    int x_l = (int)x_left[y - (int)p0.y];
                    int x_r = (int)x_right[y - (int)p0.y];

                    if (x_r < -width || x_l > width)
                        continue;

                    int x0 = Math.Max(0, x_l);
                    int x1 = Math.Min(width - 1, x_r);

                    List<double> z_segment = Interpolate(x_l, z_left[y - (int)p0.y], x_r, z_right[y - (int)p0.y]);

                    for (int x = x0; x <= x1; x++)
                        set(x, y, z_segment[x - x_l], color);
                }
            }
            catch (Exception) {

            }
        }

        void set(int x, int y, double z, Color? color) {
            if (z >= zBuffer[y][x])
                return;

            if (z < z_min)
                z_min = z;

            if (z > z_max)
                z_max = z;

            zBuffer[y][x] = z;
            colorBuffer[y][x] = color == null ? Color.White : color.Value;
        }
    }
}
