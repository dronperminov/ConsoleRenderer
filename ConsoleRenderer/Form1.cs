using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleRenderer {
    public partial class Form1 : Form {
        Renderer renderer;

        public Form1() {
            InitializeComponent();

            Vector position = new Vector(0, 8, -20);
            Vector angle = new Vector(0, 0, Math.PI);

            renderer = new Renderer(box, position, angle);

            KeyDown += renderer.KeyDown;
            box.MouseClick += renderer.MouseClick;
            box.MouseWheel += renderer.MouseWheel;
            box.MouseMove += renderer.MouseMove;

            DoubleBuffered = true;

            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Tick += ((object s, EventArgs e) => {
                renderer.Render();
                renderer.Draw();
            });

            timer.Start();
        }
    }
}
