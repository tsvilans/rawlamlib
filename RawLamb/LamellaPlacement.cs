using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace RawLambCommon
{
    public class LamellaPlacement
    {
        public int LogIndex = -1;
        public int BoardIndex = -1;
        public bool Placed = false;
        public Plane Plane;

        public LamellaPlacement()
        {
            Plane = Plane.Unset;
        }
    }
}
