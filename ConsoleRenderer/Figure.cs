using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRenderer {
    public delegate double F_xy(double x, double y);
    public delegate double F_x(double x);

    struct Triangle {
        public Vector a, b, c;
        public Color? color;

        public Triangle(Vector a, Vector b, Vector c, Color? color) {
            this.a = a;
            this.b = b;
            this.c = c;

            this.color = color;
        }
    }

    struct Line {
        public Vector a, b;
        public Color? color;

        public Line(Vector a, Vector b, Color? color) {
            this.a = a;
            this.b = b;

            this.color = color;
        }
    }

    abstract class Figure {
        protected List<Triangle> triangles;
        protected List<Line> lines;
        protected bool filled;

        protected Figure() {
            triangles = new List<Triangle>();
            lines = new List<Line>();
            filled = true;
        }

        public void Draw(Renderer renderer) {
            if (filled) {
                for (int i = 0; i < triangles.Count; i++)
                    renderer.MakeTriangle(triangles[i]);
            }
            else {
                for (int i = 0; i < lines.Count; i++) {
                    renderer.MakeLine(lines[i].a, lines[i].b, lines[i].color);
                }
            }
        }

        public static double map(double x, double in_min, double in_max, double out_min, double out_max) {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
    }

    class Model : Figure {
        List<Vector> v;
        List<Vector> n;

        public Model(string path, Color color, double sx, double sy, double sz) {
            StreamReader reader = new StreamReader(path);

            v = new List<Vector>();
            n = new List<Vector>();

            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                string[] splited = line.Split(' ');

                if (splited[0] == "v") {
                    double x = double.Parse(splited[1].Replace(".", ","));
                    double y = double.Parse(splited[2].Replace(".", ","));
                    double z = double.Parse(splited[3].Replace(".", ","));

                    v.Add(new Vector(x / sx, y / sy, z / sz));
                }
                else if (splited[0] == "f") {
                    string[] va = splited[1].Split('/');
                    string[] vb = splited[2].Split('/');
                    string[] vc = splited[3].Split('/');                  

                    Vector a = v[int.Parse(va[0]) - 1];
                    Vector b = v[int.Parse(vb[0]) - 1];
                    Vector c = v[int.Parse(vc[0]) - 1];

                    triangles.Add(new Triangle(a, b, c, color));

                    lines.Add(new Line(a, b, color));
                    lines.Add(new Line(b, c, color));
                    lines.Add(new Line(c, a, color));

                    if (splited.Length > 4 && splited[4] != "") {
                        string[] vd = splited[4].Split('/');
                        Vector d = v[int.Parse(vd[0]) - 1];

                        triangles.Add(new Triangle(a, c, d, color));

                        lines.Add(new Line(a, c, color));
                        lines.Add(new Line(c, d, color));
                        lines.Add(new Line(d, a, color));
                    }
                }
            }

            reader.Close();
        }
    }

    class Coord {
        Vector ox, oy, oz, o;

        public Coord(double len) {
            ox = new Vector(len, 0, 0);
            oy = new Vector(0, len, 0);
            oz = new Vector(0, 0, len);

            o = new Vector(0, 0, 0);
        }

        public void Draw(Renderer renderer) {
            renderer.MakeLine(o, ox, Color.Red); 
            renderer.MakeLine(o, oy, Color.Blue); 
            renderer.MakeLine(o, oz, Color.Green);
        }
    }

    class Prism : Figure {
        Vector o1, o2;
        Vector[] s1;
        Vector[] s2;

        public Prism(double x, double y, double z, double r1, double r2, double h, int n, Color c, bool filled = true) {
            this.filled = filled;

            o1 = new Vector(x, y, z);
            o2 = new Vector(x, y + h, z);

            s1 = new Vector[n];
            s2 = new Vector[n];

            double angle = Math.PI * 2 / n;

            for (int i = 0; i < n; i++) {
                double phi = i * angle + angle / 2;
                
                s1[i] = new Vector(x + r1 * Math.Sin(phi), y, z + r1 * Math.Cos(phi));
                s2[i] = new Vector(x + r2 * Math.Sin(phi), y + h, z + r2 * Math.Cos(phi));
            }

            for (int i = 0; i < n; i++) {
                triangles.Add(new Triangle(s1[i], s1[(i + 1) % s1.Length], o1, c));
                triangles.Add(new Triangle(s2[i], o2, s2[(i + 1) % s1.Length], c));

                triangles.Add(new Triangle(s1[i], s2[i], s1[(i + 1) % s1.Length], c));
                triangles.Add(new Triangle(s2[i], s2[(i + 1) % s2.Length], s1[(i + 1) % s1.Length], c));

                lines.Add(new Line(s1[i], s1[(i + 1) % s1.Length], c));
                lines.Add(new Line(s2[i], s2[(i + 1) % s2.Length], c));
                lines.Add(new Line(s1[i], s2[i], c));
            }
        }
    }

    class Sphere : Figure {
        Vector[][] s;

        public Sphere(double x, double y, double z, double r, int n, Color c, int m = 20, bool filled = true) {
            this.filled = filled;

            s = new Vector[m][];

            double angle = Math.PI * 2 / n;

            for (int i = 0; i < m; i++) {
                s[i] = new Vector[n];

                double phi = Math.PI * i / (m - 1);

                for (int j = 0; j < n; j++) {
                    s[i][j] = new Vector(x + r * Math.Sin(phi) * Math.Sin(j * angle), y + r * Math.Cos(phi), z + r * Math.Sin(phi) * Math.Cos(j * angle));
                }
            }

            for (int j = 0; j < s[0].Length; j++) {
                lines.Add(new Line(s[0][j], s[0][(j + 1) % s[0].Length], c));
            }

            for (int i = 1; i < s.Length; i++) {
                for (int j = 0; j < s[i].Length; j++) {
                    triangles.Add(new Triangle(s[i - 1][j], s[i - 1][(j + 1) % s[i].Length], s[i][j], c));
                    triangles.Add(new Triangle(s[i][j], s[i - 1][(j + 1) % s[i].Length], s[i][(j + 1) % s[i].Length], c));

                    lines.Add(new Line(s[i - 1][j], s[i][j], c));
                    lines.Add(new Line(s[i][j], s[i][(j + 1) % s[i].Length], c));
                }
            }
        }
    }

    class Cube : Figure {
        public Cube(double x, double y, double z, double a, Color c, bool filled = true) {
            this.filled = filled;

            double r = a / 2;

            Vector A = new Vector(x - r, y, z - r);
            Vector B = new Vector(x - r, y, z + r);
            Vector C = new Vector(x + r, y, z + r);
            Vector D = new Vector(x + r, y, z - r);

            Vector A1 = new Vector(x - r, y + a, z - r);
            Vector B1 = new Vector(x - r, y + a, z + r);
            Vector C1 = new Vector(x + r, y + a, z + r);
            Vector D1 = new Vector(x + r, y + a, z - r);

            int n = 20;
            int m = 20;

            AddTriangles(A, D, C, B, n, m, c);
            AddTriangles(A1, B1, C1, D1, n, m, c);
            AddTriangles(A, B, B1, A1, n, m, c);
            AddTriangles(D, D1, C1, C, n, m, c);
            AddTriangles(A, A1, D1, D, n, m, c);
            AddTriangles(B, C, C1, B1, n, m, c);
        }

        void AddTriangles(Vector a, Vector b, Vector c, Vector d, int n, int m, Color color) {
            double dh = 1.0 / n;
            double dw = 1.0 / m;

            for (int i = 0; i < n; i++) {
                for (int j = 0; j < m; j++) {
                    double ti = i * dh;
                    double tj = j * dw;

                    Vector v = a + (b - a) * ti + (d - a) * tj;
                    Vector v1 = v;
                    Vector v2 = v + (b - a) * dh;
                    Vector v3 = v + (d - a) * dw;
                    Vector v4 = v + (b - a) * dh + (d - a) * dw;

                    triangles.Add(new Triangle(v2, v1, v3, color));
                    triangles.Add(new Triangle(v2, v3, v4, color));
                }
            }
        }
    }

    class Pyramid : Figure {
        Prism prism;

        public Pyramid(double x, double y, double z, double r, double h, int n, Color c, bool filled = true) {
            prism = new Prism(x, y, z, r, 0, h, n, c, filled);
        }
    }

    class BodyRotation : Figure {
        Vector[][] s;
        
        public BodyRotation(double x, double y, double z, double a, double b, double h, F_x f, Color c, bool fill, bool filled = true, int m = 50) {
            this.filled = filled;

            int n = (int)Math.Ceiling((b - a) / h);

            s = new Vector[n][];

            for (int i = 0; i < n; i++) {
                s[i] = new Vector[m];

                double r = f(a + i * h);
                double angle = Math.PI / m * 2;

                for (int j = 0; j < m; j++) {
                    double x0 = x + r * Math.Sin(j * angle);
                    double y0 = y + i * h;
                    double z0 = z + r * Math.Cos(j * angle);

                    s[i][j] = new Vector(x0, y0, z0);
                }
            }

            for (int j = 0; j < s[0].Length; j++) {
                lines.Add(new Line(s[0][j], s[0][(j + 1) % s[0].Length], c));

                if (fill) {
                    triangles.Add(new Triangle(s[0][j], s[0][(j + 1) % s[0].Length], new Vector(x, y, z), c));
                    triangles.Add(new Triangle(s[n - 1][j], new Vector(x, y + h * (n - 1), z), s[n - 1][(j + 1) % s[n - 1].Length], c));
                }
            }

            for (int i = 1; i < s.Length; i++) {
                for (int j = 0; j < s[i].Length; j++) {
                    triangles.Add(new Triangle(s[i - 1][j], s[i][j], s[i - 1][(j + 1) % s[i].Length], c));
                    triangles.Add(new Triangle(s[i][j], s[i][(j + 1) % s[i].Length], s[i - 1][(j + 1) % s[i].Length], c));

                    lines.Add(new Line(s[i - 1][j], s[i][j], c));
                    lines.Add(new Line(s[i][j], s[i][(j + 1) % s[i].Length], c));
                }
            }
        }
    }
}
