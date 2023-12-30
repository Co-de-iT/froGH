using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace froGH
{
    /*
        Several options come from a Daniel Piker sample posted here:
        https://discourse.mcneel.com/t/proper-mesh-offset/148952/8
    */
    public class MeshOffsetExtended : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MeshOffsetAdvanced class.
        /// </summary>
        public MeshOffsetExtended()
          : base("Mesh Offset Extended", "f_MeshOffEx",
              "Offsets a Mesh using several possible options\n" +
                "Integrates algorithms by Daniel Piker, from this post:\n" +
                "https://discourse.mcneel.com/t/proper-mesh-offset/148952/8",
              "froGH", "Mesh-Create")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Mesh to offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset Distance", "D", "Offset Distance", GH_ParamAccess.item, 0.1);
            pManager.AddIntegerParameter("Distance Type", "DT", "Distance Type\n" +
                "0 - Face-Face at least\n" +
                "1 - Face-Face at most\n" +
                "2 - Face-Face average\n" +
                "3 - Vertex-vertex constant (old component default)\n" +
                "Attach a Value list for autolist", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("Direction Type", "dT", "Direction Type\n" +
                "0 - Average of Face Normals\n" +
                "1 - Angle weighted average of Face Normals\n" +
                "2 - Angle + area weighted average of Face Normals (old component default)\n" +
                "Attach a Value list for autolist", GH_ParamAccess.item, 2);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The offset Mesh", GH_ParamAccess.item);
            pManager.AddVectorParameter("Normals", "N", "Normal Vectors", GH_ParamAccess.list);
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

            // combine identical vertices
            mesh.Vertices.CombineIdentical(true, true);
            Mesh offset = mesh.DuplicateMesh();
            // triangulate Mesh & compute Face normals
            mesh.Faces.ConvertQuadsToTriangles();
            mesh.FaceNormals.ComputeFaceNormals();

            double dist = 0;
            DA.GetData(1, ref dist);

            int distType = 3, dirType = 0;

            DA.GetData("Distance Type", ref distType);
            DA.GetData("Direction Type", ref dirType);

            // __________________ distance type autoList __________________

            // variable for the list
            Grasshopper.Kernel.Special.GH_ValueList distTypeList;
            // tries to cast input as list
            string nickName = "Distance Type";
            try
            {
                // if the list is not the first parameter then change Input[0] to the corresponding value
                distTypeList = (Grasshopper.Kernel.Special.GH_ValueList)Params.Input[2].Sources[0];

                if (!distTypeList.NickName.Equals(nickName))
                {
                    distTypeList.ClearData();
                    distTypeList.ListItems.Clear();
                    distTypeList.NickName = nickName;
                    var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("Face-Face at least", "0");
                    var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Face-Face at most", "1");
                    var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("Face-Face average", "2");
                    var item4 = new Grasshopper.Kernel.Special.GH_ValueListItem("Vertex-vertex constant", "3");

                    distTypeList.ListItems.Add(item1);
                    distTypeList.ListItems.Add(item2);
                    distTypeList.ListItems.Add(item3);
                    distTypeList.ListItems.Add(item4);

                    distTypeList.ListItems[0].Value.CastTo(out distType);
                }
            }
            catch
            {
                // handles anything that is not a value list
            }

            // __________________ direction type autoList __________________

            // variable for the list
            Grasshopper.Kernel.Special.GH_ValueList dirTypeList;
            // tries to cast input as list
            nickName = "Direction Type";
            try
            {
                // if the list is not the first parameter then change Input[0] to the corresponding value
                dirTypeList = (Grasshopper.Kernel.Special.GH_ValueList)Params.Input[3].Sources[0];

                if (!dirTypeList.NickName.Equals(nickName))
                {
                    dirTypeList.ClearData();
                    dirTypeList.ListItems.Clear();
                    dirTypeList.NickName = nickName;
                    var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("Average of Face Normals", "0");
                    var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Angle weighted average of Face Normals", "1");
                    var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("Angle + area weighted average of Face Normals", "2");

                    dirTypeList.ListItems.Add(item1);
                    dirTypeList.ListItems.Add(item2);
                    dirTypeList.ListItems.Add(item3);

                    dirTypeList.ListItems[0].Value.CastTo(out dirType);
                }
            }
            catch
            {
                // handles anything that is not a value list
            }


            Vector3d[] newNormals = new Vector3d[mesh.Vertices.Count];
            Point3d[] vertices = mesh.Vertices.ToPoint3dArray();

            // Compute new normals
            newNormals = ComputeNormals(mesh, dirType);

            // Compute distances
            double[] distances = ComputeDistances(mesh, dist, distType, newNormals);

            // offset Mesh
            for (int i = 0; i < offset.Vertices.Count; i++)
            {
                offset.Vertices.SetVertex(i, vertices[i] + newNormals[i] * distances[i]);// -dist);
            }

            DA.SetData(0, offset);
            DA.SetDataList(1, newNormals);

        }

        // Mesh weighted normals implemented from the tips at the folowing pages:
        // https://stackoverflow.com/questions/25100120/how-does-blender-calculate-vertex-normals
        // http://www.bytehazard.com/articles/vertnorm.html
        /*
         pseudocode
        for each face A in mesh
        {
         n = face A facet normal
 
         // loop through all vertices in face A
         for each vert in face A
         {
          for each face B in mesh
          {
           // ignore self
           if face A == face B then skip
   
           // criteria for hard-edges
           if face A and B smoothing groups match {
   
            // accumulate normal
            // v1, v2, v3 are the vertices of face A
            if face B shares v1 {
             angle = angle_between_vectors( v1 - v2 , v1 - v3 )
             n += (face B facet normal) * (face B surface area) * angle // multiply by angle
            }
            if face B shares v2 {
             angle = angle_between_vectors( v2 - v1 , v2 - v3 )
             n += (face B facet normal) * (face B surface area) * angle // multiply by angle
            }
            if face B shares v3 {
             angle = angle_between_vectors( v3 - v1 , v3 - v2 )
             n += (face B facet normal) * (face B surface area) * angle // multiply by angle
            }
     
           }
          }
  
          // normalize vertex normal
          vn = normalize(n)
         }
        }
         */

        private Vector3d[] ComputeWeightedNormals(Mesh mesh)
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
                faceArea = 1.0;//  MeshFaceArea(mesh, i); // do not weight by area
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

        // test: this should follow the original algorithm more closely
        private Vector3d[] ComputeWeightedNormalsTriangulate(Mesh mesh)
        {
            // triangulate Mesh
            mesh.Faces.ConvertQuadsToTriangles();

            Vector3d[] weightedNormals = new Vector3d[mesh.Vertices.Count];
            Point3d v1, v2, v3;
            double faceAngle, faceArea;
            int[] connectedFaces;
            int tVertInd;

            //initialize newNormals
            for (int i = 0; i < weightedNormals.Length; i++)
                weightedNormals[i] = Vector3d.Zero;

            // Compute new normals per vertices
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                tVertInd = mesh.TopologyVertices.TopologyVertexIndex(i);
                v1 = mesh.Vertices[i];
                v2 = new Point3d();
                v3 = new Point3d();
                // scan vertex connected faces
                connectedFaces = mesh.TopologyVertices.ConnectedFaces(tVertInd);
                if (connectedFaces.Length == 0) continue;

                for (int j = 0; j < connectedFaces.Length; j++)
                {
                    // initialize normal
                    Vector3d cumulativeNormal = mesh.FaceNormals[connectedFaces[j]];

                    // find other 2 face vertices
                    if (mesh.TopologyVertices.TopologyVertexIndex(mesh.Faces[connectedFaces[j]].A) == tVertInd)
                    {
                        v2 = mesh.Vertices[mesh.Faces[connectedFaces[j]].B];
                        v3 = mesh.Vertices[mesh.Faces[connectedFaces[j]].C];
                    }
                    else if (mesh.TopologyVertices.TopologyVertexIndex(mesh.Faces[connectedFaces[j]].B) == tVertInd)
                    {
                        v2 = mesh.Vertices[mesh.Faces[connectedFaces[j]].A];
                        v3 = mesh.Vertices[mesh.Faces[connectedFaces[j]].C];
                    }
                    else if (mesh.TopologyVertices.TopologyVertexIndex(mesh.Faces[connectedFaces[j]].C) == tVertInd)
                    {
                        v2 = mesh.Vertices[mesh.Faces[connectedFaces[j]].B];
                        v3 = mesh.Vertices[mesh.Faces[connectedFaces[j]].A];
                    }

                    // compute faceAngle
                    faceAngle = Vector3d.VectorAngle(v1 - v2, v1 - v3);

                    //accumulate vertex normal
                    for (int k = 0; k < connectedFaces.Length; k++)
                    {
                        // skip same face
                        if (connectedFaces[k] == connectedFaces[j]) continue;

                        faceArea = MeshFaceArea(mesh, connectedFaces[k]);
                        cumulativeNormal += (Vector3d)mesh.FaceNormals[connectedFaces[k]] * faceAngle * faceArea;
                    }
                    weightedNormals[i] += cumulativeNormal;
                }
            }

            // Unitize newNormals (do NOT use a foreach loop for this)
            for (int i = 0; i < weightedNormals.Length; i++) weightedNormals[i].Unitize();

            return weightedNormals;
        }

        private double MeshFaceArea(Mesh m, int i)
        {
            double area = 0;
            MeshFace f = m.Faces[i];

            if (f.IsTriangle)
                area = TriangleArea(m.Vertices[f.A], m.Vertices[f.B], m.Vertices[f.C]);
            else
                area = TriangleArea(m.Vertices[f.A], m.Vertices[f.B], m.Vertices[f.C]) + TriangleArea(m.Vertices[f.D], m.Vertices[f.A], m.Vertices[f.C]);

            return area;
        }

        private double TriangleArea(Point3d A, Point3d B, Point3d C)
        {
            return Math.Abs(A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) * 0.5;
        }

        private Vector3d[] ComputeNormals(Mesh mesh, int type)
        {
            Vector3d[] normals = new Vector3d[mesh.Vertices.Count];
            if (type == 0) //unweighted average of face normals
            {
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    int[] faces = mesh.Vertices.GetVertexFaces(i);
                    Vector3d v = new Vector3d();
                    for (int j = 0; j < faces.Length; j++)
                    {
                        v += mesh.FaceNormals[faces[j]];
                    }
                    v.Unitize();
                    normals[i] = v;
                }
            }

            if (type == 1) //area weighted average of face normals (Piker)
            {
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    Vector3d v = new Vector3d();
                    Point3d p = mesh.Vertices.Point3dAt(i);
                    Point3d[] neighbours = sortedNeighbourPts(mesh, i);
                    for (int j = 0; j < neighbours.Length; j++)
                    {
                        Vector3d va = neighbours[j] - p;
                        Vector3d vb = neighbours[(j + 1) % neighbours.Length] - p;
                        //local face normal at this corner - better than using standard face normal if face is very non planar:
                        Vector3d fn = Vector3d.CrossProduct(va, vb);
                        fn.Unitize();
                        v += fn * Vector3d.VectorAngle(va, vb);
                    }
                    v.Unitize();
                    normals[i] = v;
                }
            }

            if (type == 2) //area weighted average of face normals
                normals = ComputeWeightedNormals(mesh);

            if (type == 3) //testing if this method is any different
                normals = ComputeWeightedNormalsTriangulate(mesh);

            return normals;
        }

        private double[] ComputeDistances(Mesh m, double distance, int distanceType, Vector3d[] normals)
        {
            double[] distances = new double[m.Vertices.Count];

            if (distanceType == 0) //at least this distance between corresponding faces
            {
                for (int i = 0; i < m.Vertices.Count; i++)
                {
                    Point3d p = m.Vertices.Point3dAt(i);
                    Point3d[] neighbours = sortedNeighbourPts(m, i);
                    double dmin = 0;
                    for (int j = 0; j < neighbours.Length; j++)
                    {
                        Vector3d va = neighbours[j] - p;
                        Vector3d vb = neighbours[(j + 1) % neighbours.Length] - p;
                        Vector3d fn = Vector3d.CrossProduct(va, vb);
                        fn.Unitize();
                        double d = distance / Math.Max(0, (fn * normals[i])); //(fn * normals[i]);
                        if (d > dmin) dmin = d;
                    }
                    distances[i] = dmin;
                }
            }

            if (distanceType == 1) //at most this distance between corresponding faces
            {
                for (int i = 0; i < m.Vertices.Count; i++)
                {
                    Point3d p = m.Vertices.Point3dAt(i);
                    Point3d[] neighbours = sortedNeighbourPts(m, i);
                    double dmax = double.MaxValue;
                    for (int j = 0; j < neighbours.Length; j++)
                    {
                        Vector3d va = neighbours[j] - p;
                        Vector3d vb = neighbours[(j + 1) % neighbours.Length] - p;
                        Vector3d fn = Vector3d.CrossProduct(va, vb);
                        fn.Unitize();
                        double d = distance / Math.Max(0, (fn * normals[i]));
                        if (d < dmax) dmax = d;
                    }
                    distances[i] = dmax;
                }
            }

            if (distanceType == 2) //average distance between corresponding faces
            {
                for (int i = 0; i < m.Vertices.Count; i++)
                {
                    Point3d p = m.Vertices.Point3dAt(i);
                    Point3d[] neighbours = sortedNeighbourPts(m, i);
                    double dAvg = 0;
                    for (int j = 0; j < neighbours.Length; j++)
                    {
                        Vector3d va = neighbours[j] - p;
                        Vector3d vb = neighbours[(j + 1) % neighbours.Length] - p;
                        Vector3d fn = Vector3d.CrossProduct(va, vb);
                        fn.Unitize();
                        double d = distance / (fn * normals[i]);
                        d = Math.Max(0, d);
                        dAvg += d;
                    }
                    distances[i] = dAvg / neighbours.Length;
                }
            }

            if (distanceType == 3) //vertex-vertex, all the same distance
            {
                for (int i = 0; i < m.Vertices.Count; i++) distances[i] = distance;
            }

            return distances;
        }

        private Point3d[] sortedNeighbourPts(Mesh m, int i)
        {
            int ti = m.TopologyVertices.TopologyVertexIndex(i);
            int[] neighbours = m.TopologyVertices.ConnectedTopologyVertices(ti, true);
            Point3d[] neighbourPts = new Point3d[neighbours.Length];
            for (int j = 0; j < neighbours.Length; j++)
                neighbourPts[j] = m.Vertices.Point3dAt(m.TopologyVertices.MeshVertexIndices(neighbours[j])[0]);
            return neighbourPts;
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
            get { return new Guid("9D068F7A-5C5E-4D9D-BCEF-D1425960B28A"); }
        }
    }
}