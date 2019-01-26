using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRenderer {
    class Camera {
        const double movement = 0.5;
        bool flight = true;

        readonly Vector initPoint;
        readonly Vector initAngle;

        Vector position;
        Vector angle;

        public Camera(Vector position, Vector angle) {
            this.position = position;
            this.angle = angle;

            initPoint = new Vector(position.x, position.y, position.z);
            initAngle = new Vector(angle.x, angle.y, angle.z);
        }

        public string PointInfo() {
            return position.ToString();
        }

        public string AngleInfo() {
            return angle.ToString();
        }

        public Vector GetPoint() {
            return position;
        }

        public Vector GetAngle() {
            return angle;
        }

        public void Left() {
            position.x += Math.Cos(angle.x) * movement;
            position.z += Math.Sin(angle.x) * movement;
        }

        public void Right() {
            position.x -= Math.Cos(angle.x) * movement;
            position.z -= Math.Sin(angle.x) * movement;
        }

        public void Forward() {
            if (flight) {
                position.y += Math.Sin(angle.y) * movement;
                position.x += Math.Cos(angle.x + Math.PI / 2) * Math.Cos(angle.y) * movement;
                position.z += Math.Sin(angle.x + Math.PI / 2) * Math.Cos(angle.y) * movement;
            }
            else {
                position.x += Math.Cos(angle.x + Math.PI / 2) * movement;
                position.z += Math.Sin(angle.x + Math.PI / 2) * movement;
            }
        }

        public void Backward() {
            if (flight) {
                position.y -= Math.Sin(angle.y) * movement;
                position.x += Math.Cos(angle.x - Math.PI / 2) * Math.Cos(angle.y) * movement;
                position.z += Math.Sin(angle.x - Math.PI / 2) * Math.Cos(angle.y) * movement;
            }
            else {
                position.x += Math.Cos(angle.x - Math.PI / 2) * movement;
                position.z += Math.Sin(angle.x - Math.PI / 2) * movement;
            }
        }

        public void Up() {
            position.y += 1;
        }

        public void Down() {
            position.y -= 1;
        }

        public void Reset() {
            position.x = initPoint.x;
            position.y = initPoint.y;
            position.z = initPoint.z;

            angle.x = initAngle.x;
            angle.y = initAngle.y;
            angle.z = initAngle.z;
        }

        public void RotateUp() {
            angle.y = (angle.y + 0.1 + Math.PI * 2) % (Math.PI * 2);
        }

        public void RotateDown() {
            angle.y = (angle.y - 0.1 + Math.PI * 2) % (Math.PI * 2);
        }

        public void RotateLeft() {
            angle.x = (angle.x - 0.1 + Math.PI * 2) % (Math.PI * 2);
        }

        public void RotateRight() {
            angle.x = (angle.x + 0.1 + Math.PI * 2) % (Math.PI * 2);
        }

        public void Move(double dx, double dy, double dz) {
            position.x += dx; 
            position.y += dy; 
            position.z += dz;
        }
    }
}
