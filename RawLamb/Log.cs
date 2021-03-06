using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using StudioAvw.Geometry;
using RawLamNet;

namespace RawLambCommon
{
    public class Log
    {
        public Guid Id;
        public string Name;
        public Grid CtLog;
        public Mesh Mesh;
        public Plane Plane;
        public List<Knot> Knots;
        public Polyline Pith;
        public BoundingBox BoundingBox;

        public List<Board> Boards;

        public Log()
        {
            Id = Guid.NewGuid();
            Boards = new List<Board>();
            Knots = null;
            Plane = Plane.WorldXY;
            CtLog = null;
            Mesh = null;
            Pith = null;
        }

        public Log(string name, Grid ctlog) : base()
        { 
            CtLog = ctlog;
            Name = name;
        }

        public void ReadInfoLog(string path)
        {
            InfoLog ilog = new InfoLog();
            ilog.Read(path);

            Pith = new Polyline();

            for (int i = 0; i < ilog.Pith.Length; i += 2)
            {
                Pith.Add(new Point3d(ilog.Pith[i], ilog.Pith[i + 1], i * 5));
            }

            Knots = new List<Knot>();


            for (int i = 0; i < ilog.Knots.Length; i += 6)
            {
                var knot = new Knot();
                var line = new Line(
                    new Point3d(ilog.Knots[i], ilog.Knots[i + 1], ilog.Knots[i + 2]),
                    new Point3d(ilog.Knots[i + 3], ilog.Knots[i + 4], ilog.Knots[i + 5]));
                knot.Axis = line;
                knot.Length = line.Length;
                Knots.Add(knot);
            }
        }

        public static Log LoadCtLog(string path, Transform xform, bool create_mesh=false, double resample_resolution = 30.0, double mesh_isovalue=0.7)
        {
            if (!System.IO.File.Exists(path))
                throw new Exception($"File '{path}' does not exist.");

            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            Grid ctlog = null;
            
            try
            {
                ctlog = Grid.Read(path);
            }
            catch (Exception e)
            {
                throw new Exception($"Loading CtLog failed: {e.Message}");
            }
            
            ctlog.Transform(xform);

            var log = new Log(name, ctlog);

            if (create_mesh)
            {
                var rlog = ctlog.Resample(resample_resolution);
                log.Mesh = rlog.ToRhinoMesh(mesh_isovalue, true);
            }

            return log;
        }
        
        public void Transform(Transform xform)
        {
            Mesh.Transform(xform);
            Plane.Transform(xform);
            for (int i = 0; i < Boards.Count; ++i)
            {
                Boards[i].Transform(xform);
            }
        }

        public Log Duplicate()
        {
            var log = new Log() { Name = Name, Plane = Plane, Mesh = Mesh.DuplicateMesh(), Pith = Pith.Duplicate(), BoundingBox = BoundingBox };
            for (int i = 0; i < Boards.Count; ++i)
            {
                var brd = Boards[i].Duplicate();
                brd.Log = log;
                log.Boards.Add(brd);
            }

            for (int i = 0; i < Knots.Count; ++i)
            {
                log.Knots.Add(Knots[i]);
            }

            return log;
        }

        /// <summary>
        /// IN PROGRESS.
        /// </summary>
        /// <param name="cutting_planes"></param>
        /// <param name="names"></param>
        /// <param name="kerf"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public List<Board> CutBoards(IList<Plane> cutting_planes, IList<string> names, double kerf = 3.0, double offset=0.0)
        {
            var boards = new List<Board>();

            for (int i = 0; i < cutting_planes.Count - 1; ++i)
            {
                string name = string.Format("Board_{0:00}", i);
                if (names != null && i < names.Count)
                {
                    name = names[i];
                }

                var brd = CutBoard(cutting_planes[i], cutting_planes[i + 1], name, offset);
                boards.Add(brd);
            }

            Boards.AddRange(boards);
            return boards;
        }

        public Board CutBoard(Plane top, Plane bottom, string name="Board", double offset=0.0)
        {
            if (Mesh == null)
                throw new Exception("Log has no mesh.");

            Vector3d v = top.Origin - bottom.Origin;

            double thickness = Math.Abs(v * top.ZAxis);
            var brd = new Board() { Name = name, Thickness = thickness, Plane = top, LogId = Id, Log = this };

            if (top.ZAxis * v < 0)
                top = new Plane(top.Origin, -top.XAxis, top.YAxis);
            if (bottom.ZAxis * v > 0)
                bottom = new Plane(bottom.Origin, -bottom.XAxis, bottom.YAxis);

            brd.TopPlane = top;
            brd.BottomPlane = bottom;

            // Intersect planes with log mesh


            int index = 0;
            double max_area = 0;
            List<Polyline> pout1, pout2;

            var res = Rhino.Geometry.Intersect.Intersection.MeshPlane(Mesh, top);
            if (res != null && res.Length > 0)
            {


                if (res.Length > 1)
                {
                    for (int j = 0; j < res.Length; ++j)
                    {
                        var amp = AreaMassProperties.Compute(res[j].ToNurbsCurve());
                        if (amp == null) continue;

                        if (amp.Area > max_area)
                        {
                            index = j;
                            max_area = amp.Area;
                        }
                    }
                }

                Polyline3D.Offset(new Polyline[] { res[index] },
                  Polyline3D.OpenFilletType.Butt, Polyline3D.ClosedFilletType.Miter,
                  offset,
                  brd.Plane,
                  0.01,
                  out pout1,
                  out pout2);

                if (pout1.Count > 0)
                    brd.Top = pout1;
                else if (pout2.Count > 0)
                    brd.Top = pout2;
                else
                    brd.Top = new List<Polyline> { res[index] };
            }

            // Cut bottom plane
            res = Rhino.Geometry.Intersect.Intersection.MeshPlane(Mesh, bottom);
            if (res != null && res.Length > 0)
            {
                index = 0;
                max_area = 0;

                if (res.Length > 1)
                {
                    for (int j = 0; j < res.Length; ++j)
                    {
                        var amp = AreaMassProperties.Compute(res[j].ToNurbsCurve());
                        if (amp == null) continue;

                        if (amp.Area > max_area)
                        {
                            index = j;
                            max_area = amp.Area;
                        }
                    }
                }

                Polyline3D.Offset(new Polyline[] { res[index] },
                  Polyline3D.OpenFilletType.Butt, Polyline3D.ClosedFilletType.Miter,
                  offset,
                  brd.Plane,
                  0.01,
                  out pout1,
                  out pout2);

                if (pout1.Count > 0)
                    brd.Bottom = pout1;
                else if (pout2.Count > 0)
                    brd.Bottom = pout2;
                else
                    brd.Bottom = new List<Polyline> { res[index] };
            }

            Boards.Add(brd);

            return brd;
        }

        public Board CutBoard(Plane p, string name="Board", double thickness = 45.0, double offset=0.0)
        {
            if (Mesh == null)
                throw new Exception("Log has no mesh.");

            var brd = new Board() { Name = name, Thickness = thickness, Plane = p, LogId = Id, Log = this };

            var res = Rhino.Geometry.Intersect.Intersection.MeshPlane(Mesh, p);
            if (res == null || res.Length < 1) return null;

            int index = 0;
            double max_area = 0;

            if (res.Length > 1)
            {
                for (int j = 0; j < res.Length; ++j)
                {
                    var amp = AreaMassProperties.Compute(res[j].ToNurbsCurve());
                    if (amp == null) continue;

                    if (amp.Area > max_area)
                    {
                        index = j;
                        max_area = amp.Area;
                    }
                }
            }

            List<Polyline> pout1, pout2;
            Polyline3D.Offset(new Polyline[] { res[index] },
              Polyline3D.OpenFilletType.Butt, Polyline3D.ClosedFilletType.Miter,
              offset,
              brd.Plane,
              0.01,
              out pout1,
              out pout2);

            if (pout1.Count > 0)
                brd.Centre = pout1[0];
            else if (pout2.Count > 0)
                brd.Centre = pout2[0];
            else
                brd.Centre = res[index];

            brd.Centre.ReduceSegments(3);
            brd.LogId = Id;

            Boards.Add(brd);

            return brd;
        }

        public float[] Sample(Mesh mesh, int sample_type = 0)
        {
            var coords = mesh.Vertices.ToFloatArray();
            return CtLog.Evaluate(coords, sample_type);
        }
    }
}
