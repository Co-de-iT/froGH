using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace froGH
{
    public class MeshOffsetWeighted : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshOffsetWeighted class.
        /// </summary>
        public MeshOffsetWeighted()
          : base("Mesh Offset Weighted", "f_MeshOffsetTheProperWayWhyIsThisNotTheNorm",
              "Offsets a Mesh using weighted normals",
              "froGH", "Mesh-Create")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Mesh to offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset Distance", "d", "Offset Distance\npositive values to offset inward", GH_ParamAccess.item, 0.1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Mesh to offset", GH_ParamAccess.item);
            pManager.AddVectorParameter("Weighted Normals", "Nw", "Weighted Normal Vectors", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Mesh mesh = new Mesh();

            if (!DA.GetData(0, ref mesh)) return;

            if (!mesh.IsValid || mesh == null) return;

            // compute normals if none are present
            if (mesh.Normals.Count == 0)
                mesh.RebuildNormals();
            if (mesh.FaceNormals.Count == 0)
                mesh.FaceNormals.ComputeFaceNormals();


            double dist = 0;
            DA.GetData(1, ref dist);

            Mesh offset = new Mesh();
            offset.CopyFrom(mesh);

            Vector3d[] newNormals = new Vector3d[mesh.Vertices.Count];
            Point3d[] vertices = mesh.Vertices.ToPoint3dArray();

            //initialize newNormals
            for (int i = 0; i < newNormals.Length; i++)
                newNormals[i] = Vector3d.Zero;

            // Compute new normals
            newNormals = ComputeWeightedNormals(mesh);

            // Unitize newNormals
            foreach (Vector3d n in newNormals) n.Unitize();

            // offset Mesh
            for (int i = 0; i < offset.Vertices.Count; i++)
            {
                offset.Vertices.SetVertex(i, vertices[i] + newNormals[i] * -dist);
            }

            DA.SetData(0, offset);
            DA.SetDataList(1, newNormals);

        }

        // Mesh weighted normals implemented from the tips at the folowing pages:
        // https://stackoverflow.com/questions/25100120/how-does-blender-calculate-vertex-normals
        // http://www.bytehazard.com/articles/vertnorm.html
        Vector3d[] ComputeWeightedNormals(Mesh mesh)
        {

            Vector3d[] weightedNormals = new Vector3d[mesh.Vertices.Count];
            Point3d A, B, C, D;
            double faceAngle, faceArea;

            //initialize newNormals
            for (int i = 0; i < weightedNormals.Length; i++)
                weightedNormals[i] = Vector3d.Zero;

            // Compute new normals
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                faceArea = 1.0;// MeshFaceArea(mesh, i); // do not weight by area
                A = mesh.Vertices[mesh.Faces[i].A];
                B = mesh.Vertices[mesh.Faces[i].B];
                C = mesh.Vertices[mesh.Faces[i].C];
                if (mesh.Faces[i].IsTriangle)
                {
                    faceAngle = Vector3d.VectorAngle(B - A, C - A);
                    weightedNormals[mesh.Faces[i].A] += (Vector3d)mesh.FaceNormals[i] * faceAngle * faceArea;
                    faceAngle = Vector3d.VectorAngle(A - B, C - B);
                    weightedNormals[mesh.Faces[i].B] += (Vector3d)mesh.FaceNormals[i] * faceAngle * faceArea;
                    faceAngle = Vector3d.VectorAngle(A - C, B - C);
                    weightedNormals[mesh.Faces[i].C] += (Vector3d)mesh.FaceNormals[i] * faceAngle * faceArea;

                }
                else
                {
                    D = mesh.Vertices[mesh.Faces[i].D];
                    faceAngle = Vector3d.VectorAngle(B - A, D - A);
                    weightedNormals[mesh.Faces[i].A] += (Vector3d)mesh.FaceNormals[i] * faceAngle * faceArea;
                    faceAngle = Vector3d.VectorAngle(A - B, C - B);
                    weightedNormals[mesh.Faces[i].B] += (Vector3d)mesh.FaceNormals[i] * faceAngle * faceArea;
                    faceAngle = Vector3d.VectorAngle(D - C, B - C);
                    weightedNormals[mesh.Faces[i].C] += (Vector3d)mesh.FaceNormals[i] * faceAngle * faceArea;
                    faceAngle = Vector3d.VectorAngle(A - D, C - D);
                    weightedNormals[mesh.Faces[i].D] += (Vector3d)mesh.FaceNormals[i] * faceAngle * faceArea;
                }
            }

            // Unitize newNormals (do NOT use a foreach loop for this)
            for (int i = 0; i < weightedNormals.Length; i++) weightedNormals[i].Unitize();

            return weightedNormals;
        }



        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
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
                return Resources.Mesh_Offset_Weighted_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("027acacd-f756-418f-b975-891f40467bbc"); }
        }
    }
}