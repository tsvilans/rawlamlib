using System;
using System.Runtime.InteropServices;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawLamNet
{
    public class QuadMesh : IDisposable
    {
        public IntPtr Ptr;
        private bool m_valid;

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr QuadMesh_Create();
        public QuadMesh()
        {
            Ptr = QuadMesh_Create();
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int QuadMesh_num_vertices(IntPtr ptr);
        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int QuadMesh_num_faces(IntPtr ptr);
        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void QuadMesh_get_vertices(IntPtr ptr, float[] data);
        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void QuadMesh_get_faces(IntPtr ptr, int[] data);

        public float[] Vertices
        {
            get
            {
                var verts = new float[QuadMesh_num_vertices(Ptr) * 3];
                QuadMesh_get_vertices(Ptr, verts);

                return verts;
            }
        }

        public int[] Faces
        {
            get
            {
                var faces = new int[QuadMesh_num_faces(Ptr) * 4];
                QuadMesh_get_faces(Ptr, faces);

                return faces;
            }
        }

        /// <summary>
        /// validity property
        /// </summary>
        /// <returns>boolean value of validity</returns>
        public bool IsValid
        {
            get
            {
                if (this.Ptr == IntPtr.Zero) return false;
                return this.m_valid;
            }
            private set
            {
                this.m_valid = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void QuadMesh_Delete(IntPtr ptr);

        /// <summary>
        /// protected implementation of dispose pattern
        /// </summary>
        /// <param name="bDisposing">holds value indicating if this was called from dispose or finalizer</param>
        protected virtual void Dispose(bool bDisposing)
        {
            if (this.Ptr != IntPtr.Zero)
            {
                // cleanup everything on the c++ side
                QuadMesh_Delete(this.Ptr);

                // clear renderer pointer
                this.Ptr = IntPtr.Zero;
                this.IsValid = false;
            }

            // finalize garbage collection
            if (bDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }




    }
}
