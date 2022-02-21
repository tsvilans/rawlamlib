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
        public CtLog CtLog;
        public Mesh Mesh;
        public Plane Plane;

        public List<Board> Boards;

        public Log()
        {
            Id = Guid.NewGuid();
            Boards = new List<Board>();
            Plane = Plane.WorldXY;
            CtLog = null;
            Mesh = null;
        }

        public Log(string name, CtLog ctlog) : base()
        { 
            CtLog = ctlog;
            Name = name;
        }

        public static Log LoadCtLog(string path, Transform xform, bool create_mesh=false, double resample_resolution = 30.0, double mesh_isovalue=0.7)
        {
            if (!System.IO.File.Exists(path))
                throw new Exception($"File '{path}' does not exist.");

            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            var ctlog = new CtLog();
            try
            {
                ctlog.ReadVdb(path);
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
            var log = new Log() { Name = Name, Plane = Plane, Mesh = Mesh.DuplicateMesh() };
            for (int i = 0; i < Boards.Count; ++i)
            {
                var brd = Boards[i].Duplicate();
                brd.Log = log;
                log.Boards.Add(brd);
            }

            return log;
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
