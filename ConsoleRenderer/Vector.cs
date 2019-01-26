using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRenderer {
    class Vector {
        public double x;
        public double y;
        public double z;
        public double w;

        public Vector(double x = 0, double y = 0, double z = 0, double w = 1) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        // получение длины вектора
        public double GetLength() {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        // нормализация вектора
        public Vector Normalize() {
            double len = GetLength();

            return len == 0 ? new Vector(0, 0, 0) : new Vector(x / len, y / len, z / len);
        }

        public Color ToColor() {
            Vector norm = Normalize();
            
            int r = (int) Math.Max(0, 255 * Math.Abs(norm.x));
            int g = (int) Math.Max(0, 255 * Math.Abs(norm.y));
            int b = (int) Math.Max(0, 255 * Math.Abs(norm.z));

            return Color.FromArgb(r, g, b);
        }

        // перемещение вектора
        public Vector Translation(double dx = 0, double dy = 0, double dz = 0) {
            Vector p0 = new Vector();

            p0.x = x + dx * w;
            p0.y = y + dy * w;
            p0.z = z + dz * w;
            p0.w = w;

            return p0;
        }

        // растяжение вектора
        public Vector Scale(double x_scale = 1, double y_scale = 1, double z_scale = 1) {
            return new Vector(x * x_scale, y * y_scale, z * z_scale, w);
        }

        // вращение около оси X
        public Vector RotateX(double alpha) {
            Vector p = new Vector();

            p.x = x;
            p.y = Math.Cos(alpha) * y - Math.Sin(alpha) * z;
            p.z = Math.Sin(alpha) * y + Math.Cos(alpha) * z;
            p.w = w;

            return p;
        }

        // вращение около оси Y
        public Vector RotateY(double alpha) {
            Vector p = new Vector();

            p.x = Math.Cos(alpha) * x - Math.Sin(alpha) * z;
            p.y = y;
            p.z = Math.Sin(alpha) * x + Math.Cos(alpha) * z;
            p.w = w;

            return p;
        }
        
        // вращение около оси Z
        public Vector RotateZ(double alpha) {
            Vector p = new Vector();

            p.x = Math.Cos(alpha) * x - Math.Sin(alpha) * y;
            p.y = Math.Sin(alpha) * x + Math.Cos(alpha) * y; ;
            p.z = z;
            p.w = w;

            return p;
        }

        public static Vector operator*(Vector v, double d) {
            return new Vector(v.x * d, v.y * d, v.z * d, v.w);
        }

        public static Vector operator /(Vector v, double d) {
            return new Vector(v.x / d, v.y / d, v.z / d, v.w);
        }

        public static Vector operator +(Vector v1, Vector v2) {
            return new Vector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector operator-(Vector v1, Vector v2) {
            return new Vector(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector operator ^(Vector v1, Vector v2) {
            double x = v1.y * v2.z - v1.z * v2.y;
            double y = v1.z * v2.x - v1.x * v2.z;
            double z = v1.x * v2.y - v1.y * v2.x;

            return new Vector(x, y, z);
        }

        public static double operator*(Vector v1, Vector v2) {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }

        public override string ToString() {
            return x.ToString("0.####") + " " + y.ToString("0.####") + " " + z.ToString("0.####") + " " + w.ToString("0.####");
        }
    }
}