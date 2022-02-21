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
        public byte[] Serialize()
        {
            byte[] data = new byte[sizeof(int) * 2 + sizeof(bool) + sizeof(double) * 9];

            var index = 0;
            Array.Copy(BitConverter.GetBytes(LogIndex), 0, data, index, sizeof(int));
            index += sizeof(int);

            Array.Copy(BitConverter.GetBytes(BoardIndex), 0, data, index, sizeof(int));
            index += sizeof(int);

            Array.Copy(BitConverter.GetBytes(Placed), 0, data, index, sizeof(bool));
            index += sizeof(bool);

            Array.Copy(BitConverter.GetBytes(Plane.Origin.X), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.Origin.Y), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.Origin.Z), 0, data, index, sizeof(double));
            index += sizeof(double);

            Array.Copy(BitConverter.GetBytes(Plane.XAxis.X), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.XAxis.Y), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.XAxis.Z), 0, data, index, sizeof(double));
            index += sizeof(double);

            Array.Copy(BitConverter.GetBytes(Plane.YAxis.X), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.YAxis.Y), 0, data, index, sizeof(double));
            index += sizeof(double);
            Array.Copy(BitConverter.GetBytes(Plane.YAxis.Z), 0, data, index, sizeof(double));
            index += sizeof(double);

            return data;
        }

        public static LamellaPlacement Deserialize(byte[] data)
        {
            var lp = new LamellaPlacement();

            var indexStep = 0;


            var LI = BitConverter.ToInt32(data, indexStep);
            indexStep += sizeof(int);

            var BI = BitConverter.ToInt32(data, indexStep);
            indexStep += sizeof(int);

            var P = BitConverter.ToBoolean(data, indexStep);
            indexStep += sizeof(bool);



            Point3d PtO = new Point3d(
              BitConverter.ToDouble(data, indexStep),
              BitConverter.ToDouble(data, indexStep + sizeof(double)),
              BitConverter.ToDouble(data, indexStep + sizeof(double) * 2)
              );
            indexStep += (sizeof(double) * 3);

            Vector3d Vx = new Vector3d(
              BitConverter.ToDouble(data, indexStep),
              BitConverter.ToDouble(data, indexStep + sizeof(double)),
              BitConverter.ToDouble(data, indexStep) + (sizeof(double) * 2)
              );
            indexStep += (sizeof(double) * 3);

            Vector3d Vy = new Vector3d(
              BitConverter.ToDouble(data, indexStep),
              BitConverter.ToDouble(data, indexStep + sizeof(double)),
              BitConverter.ToDouble(data, indexStep) + (sizeof(double) * 2)
              );

            indexStep += (sizeof(double) * 3);

            Plane Pl = new Plane(PtO, Vx, Vy);
            // TO DO
            lp.LogIndex = LI;
            lp.BoardIndex = BI;
            lp.Placed = P;
            lp.Plane = Pl;

            return lp;
        }

        public override string ToString()
        {
            return string.Format("LamellaPlacement({0} {1} {2} {3})", LogIndex, BoardIndex, Placed, Plane);
        }
    }

}
