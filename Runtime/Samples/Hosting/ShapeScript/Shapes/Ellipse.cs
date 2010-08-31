using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ShapeScript;

namespace Shapes {
    public class Ellipse : Shape {
        protected int _xCenter, _yCenter, _xradius, _yradius;
        public Ellipse(int xCenter, int yCenter, int xradius, int yradius) {
            _xCenter = xCenter; _yCenter = yCenter; _xradius = xradius; _yradius = yradius;
            Draw();
        }

        #region Shape Members

        //public void Draw() {
        //    Graphics g1 = Form1.Canvas.CreateGraphics();
        //    Pen pen = new Pen(Color.YellowGreen, 2);
        //    Draw(g1, pen);
        //}

        public override void Draw(Graphics g, Pen pen) {
            g.DrawEllipse(pen, _xCenter - _xradius, _yCenter - _yradius, 2 * _xradius, 2 * _yradius);
            g.DrawString("s1", new Font("Arial", 16), new SolidBrush(pen.Color), new PointF(_xCenter, _yCenter));
        }

        public override void Move(int x, int y) {
            Erase();
            _xCenter += x;
            _yCenter += y;
            Draw();
        }

        private void Erase() {
            Pen pen = new Pen(Form1.Canvas.BackColor, 2);
            Draw(Form1.Canvas.CreateGraphics(), pen);
        }

        #endregion
    }
}
