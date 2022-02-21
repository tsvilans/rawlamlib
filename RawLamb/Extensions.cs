using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using RawLamNet;

namespace RawLambCommon
{
    public static class RhinoExtensions
    {
        public static Mesh ToRhinoMesh(this QuadMesh qm)
        {
            var verts = qm.Vertices;
            var faces = qm.Faces;

            var mesh = new Mesh();
            for (int i = 0; i < verts.Length / 3; ++i)
            {
                mesh.Vertices.Add(verts[i * 3], verts[i * 3 + 1], verts[i * 3 + 2]);
            }

            for (int i = 0; i < faces.Length / 4; ++i)
            {
                int a = faces[i * 4], b = faces[i * 4 + 1], c = faces[i * 4 + 2], d = faces[i * 4 + 3];
                if (a == d)
                {
                    mesh.Faces.AddFace(c, b, a);
                }
                else
                    mesh.Faces.AddFace(d, c, b, a);
            }

            return mesh;
        }

        public static Mesh ToRhinoMesh(this CtLog log, double isovalue, bool cleanup=true)
        {
            var qm = log.ToMesh((float)isovalue);
            if (!cleanup)
                return qm.ToRhinoMesh();

            var log_mesh = qm.ToRhinoMesh();
            log_mesh.Vertices.CombineIdentical(true, true);
            log_mesh.Faces.CullDegenerateFaces();
            log_mesh.Vertices.CullUnused();

            var pieces = log_mesh.SplitDisjointPieces();

            double max_volume = 0;
            int index = -1;

            for (int i = 0; i < pieces.Length; ++i)
            {
                var vmp = VolumeMassProperties.Compute(pieces[i]);
                if (vmp.Volume > max_volume)
                {
                    max_volume = vmp.Volume;
                    index = i;
                }
            }

            log_mesh = pieces[index];

            log_mesh.Compact();

            return log_mesh;
        }

        public static void Transform(this CtLog log, Transform xform)
        {
            log.Transform = xform.ToFloatArray(true);
        }
    }
}
