using System;
using System.Collections.Generic;
using System.IO;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class LoadSTLBinaryFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the LoadSTLBinaryFile class.
        /// </summary>
        public LoadSTLBinaryFile()
          : base("Load STL Binary File", "f_STLBinLoad",
              "Loads a Binary STL Mesh file",
              "froGH", "File I-O")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "F", "Path to the STL file to read", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weld Angle", "Wa", "Adjacent faces with angle below this threshold will be welded\nleave empty for maximum welding", GH_ParamAccess.item, Math.PI);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The loaded Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Load Binary STL mesh
            // original VB.net code by Ola Jaensson - C# adaptation by Alessio Erioli
            // see http://www.grasshopper3d.com/forum/topics/import-an-stl-file-direct-in-grasshopper

            string F = null;
            if (!DA.GetData(0, ref F)) return;

            if (!File.Exists(F)) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File does not exist");
            double wA = 0;
            DA.GetData(1, ref wA);

            if (wA == 0) wA = Math.PI;

            byte[] stl = System.IO.File.ReadAllBytes(F);
            //Print("File length: " + stl.Length + " bytes");
            long numtris = BitConverter.ToInt32(stl, 80);
            //Print("Number of triangles: " + numtris);
            int c = 0;
            int pos = 0;
            Mesh tempmesh = new Mesh();
            Vector3f tempvect = default(Vector3f);
            if (!(numtris * 50 + 84 == stl.Length)) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "STL file length does not match the number of triangle faces");

            //Print("STL file length matches the number of triangle faces");
            for (c = 0; c <= numtris - 1; c++)
            {
                pos = c * 50 + 84;
                tempvect = readVect3f(ref stl, ref pos);
                Mesh _with1 = tempmesh;
                _with1.Vertices.SetVertex(c * 3, readPoint3f(ref stl, ref pos));
                _with1.Vertices.SetVertex(c * 3 + 1, readPoint3f(ref stl, ref pos));
                _with1.Vertices.SetVertex(c * 3 + 2, readPoint3f(ref stl, ref pos));
                _with1.Faces.SetFace(c, c * 3, c * 3 + 1, c * 3 + 2);
                _with1.Normals.SetNormal(c * 3, tempvect);
                _with1.Normals.SetNormal(c * 3 + 1, tempvect);
                _with1.Normals.SetNormal(c * 3 + 2, tempvect);
            }
            tempmesh.Weld(wA);

            DA.SetData(0, tempmesh);

        }

        private float readSingle(ref byte[] stl, ref int pos)
        {
            float functionReturnValue = 0;
            functionReturnValue = BitConverter.ToSingle(stl, pos);
            pos = pos + 4;
            return functionReturnValue;
        }

        private Vector3f readVect3f(ref byte[] stl, ref int pos)
        {
            return new Vector3f(readSingle(ref stl, ref pos), readSingle(ref stl, ref pos), readSingle(ref stl, ref pos));
        }

        private Point3f readPoint3f(ref byte[] stl, ref int pos)
        {
            return new Point3f(readSingle(ref stl, ref pos), readSingle(ref stl, ref pos), readSingle(ref stl, ref pos));
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.load_binary_STL_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ae032737-0ea4-4fa0-9f1f-7262902c2640"); }
        }
    }
}