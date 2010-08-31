using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ShapeScript;

namespace Shapes {
    public class Graph :Shape{
        public delegate float MapperFunc(float x);

        MapperFunc _func;
        const int MAX_X = 800;
        const int MAX_Y = 350;

        List<PointF> _points;

        public Graph(MapperFunc func) {
            _func = func;
            _points = new List<PointF>();
            Draw();
        }
        
        public override void Draw(System.Drawing.Graphics g, System.Drawing.Pen pen) {
            for (float x = 0; x < MAX_X; x=x+0.5f) {
                float y = _func(x);
                _points.Add( new PointF( x, y));
            }
            
            g.DrawCurve(pen, _points.ToArray());
            
        }

        //public void Draw() {
        //    Graphics g1 = Form1.Canvas.CreateGraphics();
        //    Pen pen = new Pen(Color.YellowGreen, 2);
        //    Draw(g1, pen);
        //}

        public override void Move(int x, int y) {
            throw new NotImplementedException();
        }
    }
}
