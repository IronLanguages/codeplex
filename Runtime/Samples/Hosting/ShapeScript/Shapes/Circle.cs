using System;
using System.Collections.Generic;
using System.Text;
using Shapes;

namespace Shapes{
    public class Circle : Ellipse {
        public Circle(int x, int y, int radius) : base(x,y,radius,radius) {

        }
    }
}
