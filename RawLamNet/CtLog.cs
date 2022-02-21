using System;
using System.Runtime.InteropServices;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawLamNet
{
    public class CtLog : IDisposable
    {
        public IntPtr Ptr;
        private bool m_valid;

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CtLog_Create();
        public CtLog()
        {
            Ptr = CtLog_Create();
        }

        public CtLog(IntPtr ptr)
        {
            Ptr = ptr;
        }

        ~CtLog()
        {
            Dispose(false);
            Console.WriteLine("Deleting CtLog at {0}", Ptr.ToString());
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CtLog_readVDB(IntPtr ptr, string filename);
        public void ReadVdb(string filename)
        {
            CtLog_readVDB(Ptr, filename);
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CtLog_evaluate(IntPtr ptr, int num_coords, float[] coords, float[] results, int sample_type);
        public float[] Evaluate(float[] coords, int sample_type)
        {
            int N = coords.Length / 3;
            float[] values = new float[N];

            CtLog_evaluate(Ptr, N, coords, values, sample_type);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CtLog_offset(IntPtr ptr, float amount);
        public void Offset(float amount)
        {
            CtLog_offset(Ptr, amount);
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CtLog_filter(IntPtr ptr, int width, int iterations, int type);
        public void Filter(int width, int iterations, int type)
        {
            CtLog_filter(Ptr, width, iterations, type);
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CtLog_bounding_box(IntPtr ptr, float[] min, float[] max);
        public void BoundingBox(out float[] min, out float[] max)
        {
            min = new float[3];
            max = new float[3];
            CtLog_bounding_box(Ptr, min, max);
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CtLog_get_transform(IntPtr ptr, float[] mat);
        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CtLog_set_transform(IntPtr ptr, float[] mat);

        public float[] Transform { 
            
            get
            {
                var mat = new float[16];
                CtLog_get_transform(Ptr, mat);
                return mat;
            } 
            set
            {
                if (value.Length < 16) throw new ArgumentException("Transform array must have at least 16 values.");
                CtLog_set_transform(Ptr, value);
            }
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CtLog_to_mesh(IntPtr ptr, IntPtr mesh_ptr, float isovalue);
        public QuadMesh ToMesh(float isovalue)
        {
            var qm = new QuadMesh();
            CtLog_to_mesh(Ptr, qm.Ptr, isovalue);

            return qm;
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CtLog_resample(IntPtr ptr, float scale);
        public CtLog Resample(double scale)
        {
            return new CtLog(CtLog_resample(Ptr, (float)scale));
        }

        [DllImport(API.RawLamApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CtLog_debug_grid_counter();

        public static int DebugGridCounter
        {
            get
            {
                return CtLog_debug_grid_counter();
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
        private static extern void CtLog_Delete(IntPtr ptr);

        /// <summary>
        /// protected implementation of dispose pattern
        /// </summary>
        /// <param name="bDisposing">holds value indicating if this was called from dispose or finalizer</param>
        protected virtual void Dispose(bool bDisposing)
        {
            if (this.Ptr != IntPtr.Zero)
            {
                // cleanup everything on the c++ side
                CtLog_Delete(this.Ptr);

                // clear pointer
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
