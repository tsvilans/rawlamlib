/*
 * RawLamb
 * Copyright 2022 Tom Svilans
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */

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
        public static Transform ToRhinoTransform(this IList<float> val)
        {
            if (val.Count != 16) throw new ArgumentException("ToTransform(this float[] val): Transform needs exactly 16 values.");

            var xform = Rhino.Geometry.Transform.Identity;
            xform.M00 = val[0];
            xform.M01 = val[4];
            xform.M02 = val[8];
            xform.M03 = val[12];

            xform.M10 = val[1];
            xform.M11 = val[5];
            xform.M12 = val[9];
            xform.M13 = val[13];

            xform.M20 = val[2];
            xform.M21 = val[6];
            xform.M22 = val[10];
            xform.M23 = val[14];

            xform.M30 = val[3];
            xform.M31 = val[7];
            xform.M32 = val[11];
            xform.M33 = val[15];

            return xform;
        }

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

        public static Mesh ToRhinoMesh(this Grid log, double isovalue, bool cleanup=true)
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

        public static void Transform(this Grid log, Transform xform)
        {
            log.Transform = xform.ToFloatArray(false);
        }

        public static Transform ToTransform(this float[] val)
        {
            if (val.Length != 16) throw new ArgumentException("ToTransform(this float[] val): Transform needs exactly 16 values.");
            var xform = Rhino.Geometry.Transform.Identity;
            xform.M00 = val[0];
            xform.M01 = val[4];
            xform.M02 = val[8];
            xform.M03 = val[12];

            xform.M10 = val[1];
            xform.M11 = val[5];
            xform.M12 = val[9];
            xform.M13 = val[13];

            xform.M20 = val[2];
            xform.M21 = val[6];
            xform.M22 = val[10];
            xform.M23 = val[14];

            xform.M30 = val[3];
            xform.M31 = val[7];
            xform.M32 = val[11];
            xform.M33 = val[15];

            return xform;
        }
    }
}
