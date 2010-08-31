using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ShapeScript;

namespace Shapes {
    public class Rectangle : Shape {
        int _x1, _y1, _x2, _y2;
        public Rectangle(int x1, int y1, int x2, int y2) {
            _x1 = x1; _y1 = y1; _x2 = x2; _y2 = y2;
            Draw();
        }

        //public void Draw() {
        //    Graphics g1 = Form1.Canvas.CreateGraphics();
        //    Pen pen = new Pen(Color.YellowGreen, 2);
        //    Draw(g1, pen);
        //}

        #region Shape Members

        public override void Draw(Graphics g, Pen pen) {
            g.DrawRectangle(pen, _x1, _y1, _x2-_x1, _y2-_y1);
        }

        public override void Move(int x, int y) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
