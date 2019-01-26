using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRenderer {
    class PointLight {
        Vector light;
        Vector init;

        public PointLight(double x, double y, double z) {
            light = new Vector(x, y, z);
            init = new Vector(x, y, z);
        }

        public double GetPoint(Vector mid, Vector n) {
            return n * (mid - light).Normalize();
        }

        public void Move(double dx, double dy, double dz) {
            light = light.Translation(dx, dy, dz);
        }

        public void Reset() {
            light.x = init.x;
            light.y = init.y;
            light.z = init.z;
        }

        public Vector GetV() {
            return light;
        }
    }

    class DirectedLight {
        Vector light;

        public DirectedLight(double x, double y, double z) {
            light = new Vector(x, y, z);
        }

        public void ChangeIntensity(double intensity) {
            light.x *= intensity;
            light.y *= intensity;
            light.z *= intensity;
        }

        public void Rotate(double x, double y, double z) {
            light = light.RotateX(x).RotateY(y).RotateZ(z).Normalize();
        }

        public double GetAngle(Vector n) {
            return n * light;
        }

        public Vector GetV() {
            return light;
        }
    }
}
