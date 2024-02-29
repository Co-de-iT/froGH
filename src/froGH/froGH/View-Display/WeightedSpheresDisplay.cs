using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Threading.Tasks;
using System.Drawing;
using froGH.Properties;
using Rhino;
using Rhino.DocObjects;

namespace froGH
{

    public class WeightedSpheresDisplay : GH_Component
    {
        private BoundingBox _clip;
        private Mesh[] _meshes;
        private Mesh _mesh;
        private readonly Mesh[] baseSpheres;
        private Rhino.Display.DisplayMaterial _blackMatte = new Rhino.Display.DisplayMaterial(Color.Black, Color.Black, Color.Black, Color.Black, 0.0, 0.0);

        /// <summary>
        /// Initializes a new instance of the WeightedSpheresDisplay class.
        /// </summary>
        public WeightedSpheresDisplay()
          : base("Weighted Spheres Cloud Display", "f_WSCD",
              "Render-compatible Weighted Sphere Cloud display\nSuitable for volumetric scalar field display",
              "froGH", "View/Display")
        {
            baseSpheres = new Mesh[] { MakeSphereBFr0(), MakeSphereBFr1(), MakeSphereBFr2() };
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points to Display", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "R", "Radiuses (one value for each point)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Resolution", "r", "0 - coarse, 1 - fine, 2 - extra-fine", GH_ParamAccess.item, 0);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> P = new List<Point3d>();
            List<double> R = new List<double>();
            if (!DA.GetDataList(0, P)) return;
            if (!DA.GetDataList(1, R)) return;

            int res = 0;
            DA.GetData(2, ref res);

            if (P == null || R == null || P.Count == 0 || P.Count != R.Count) return;

            //    int step;
            Mesh blackSphere;
            // steps: 1, 2, 4, 8
            //       32, 16, 8, 4 as UV params

            blackSphere = baseSpheres[res % 3];

            _meshes = new Mesh[P.Count];
            _mesh = new Mesh();

            Parallel.For(0, P.Count, i =>
            {
                Mesh sphere = blackSphere.DuplicateMesh();
                sphere.Scale(R[i]);
                sphere.Translate((Vector3d)P[i]);
                _meshes[i] = sphere;
            });

            _mesh.Append(_meshes);
            _clip = _mesh.GetBoundingBox(false);
        }

        /// <summary>
        /// This method will be called once every solution, before any calls to RunScript.
        /// </summary>
        protected override void BeforeSolveInstance()
        {
            _clip = BoundingBox.Empty;
            _meshes = null;
            _mesh = null;
        }

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get { return _clip; }
        }

        //Draw all meshes in this method.
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (_mesh == null || _meshes == null) return;

            args.Display.DrawMeshShaded(_mesh, _blackMatte);
        }

        public override void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            if (att == null)
                att = doc.CreateDefaultAttributes();

            doc.Objects.AddMesh(_mesh, att);
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
                return Resources.Custom_Sphere_Display_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("53c7f9c7-51a9-4c58-8619-8870c9394309"); }
        }

        // more data
        // data for Mesh r0
        private Point3d[] mVerticesr0 = new Point3d[]{
  new Point3d(0, 0, -1),
  new Point3d(-1, 0, 0),
  new Point3d(0, -1, 0),
  new Point3d(1, 0, 0),
  new Point3d(0, 1, 0),
  new Point3d(0, 0, 1)
  };

        private MeshFace[] mFacesr0 = new MeshFace[]{
  new MeshFace(0, 2, 1),
  new MeshFace(5, 1, 2),
  new MeshFace(0, 3, 2),
  new MeshFace(5, 2, 3),
  new MeshFace(0, 4, 3),
  new MeshFace(5, 3, 4),
  new MeshFace(0, 1, 4),
  new MeshFace(5, 4, 1)
  };

        // data for Mesh r1
        private Point3d[] mVerticesr1 = new Point3d[]{
  new Point3d(0, 0, -1),
  new Point3d(-0.707107, 0, -0.707107),
  new Point3d(-0.5, -0.5, -0.707107),
  new Point3d(0, -0.707107, -0.707107),
  new Point3d(0.5, -0.5, -0.707107),
  new Point3d(0.707107, 0, -0.707107),
  new Point3d(0.5, 0.5, -0.707107),
  new Point3d(0, 0.707107, -0.707107),
  new Point3d(-0.5, 0.5, -0.707107),
  new Point3d(-1, 0, 0),
  new Point3d(-0.707107, -0.707107, 0),
  new Point3d(0, -1, 0),
  new Point3d(0.707107, -0.707107, 0),
  new Point3d(1, 0, 0),
  new Point3d(0.707107, 0.707107, 0),
  new Point3d(0, 1, 0),
  new Point3d(-0.707107, 0.707107, 0),
  new Point3d(-0.707107, 0, 0.707107),
  new Point3d(-0.5, -0.5, 0.707107),
  new Point3d(0, -0.707107, 0.707107),
  new Point3d(0.5, -0.5, 0.707107),
  new Point3d(0.707107, 0, 0.707107),
  new Point3d(0.5, 0.5, 0.707107),
  new Point3d(0, 0.707107, 0.707107),
  new Point3d(-0.5, 0.5, 0.707107),
  new Point3d(0, 0, 1)
  };

        private MeshFace[] mFacesr1 = new MeshFace[]{
  new MeshFace(1, 2, 10, 9),
  new MeshFace(2, 3, 11, 10),
  new MeshFace(3, 4, 12, 11),
  new MeshFace(4, 5, 13, 12),
  new MeshFace(5, 6, 14, 13),
  new MeshFace(6, 7, 15, 14),
  new MeshFace(7, 8, 16, 15),
  new MeshFace(8, 1, 9, 16),
  new MeshFace(9, 10, 18, 17),
  new MeshFace(10, 11, 19, 18),
  new MeshFace(11, 12, 20, 19),
  new MeshFace(12, 13, 21, 20),
  new MeshFace(13, 14, 22, 21),
  new MeshFace(14, 15, 23, 22),
  new MeshFace(15, 16, 24, 23),
  new MeshFace(16, 9, 17, 24),
  new MeshFace(0, 2, 1),
  new MeshFace(25, 17, 18),
  new MeshFace(0, 3, 2),
  new MeshFace(25, 18, 19),
  new MeshFace(0, 4, 3),
  new MeshFace(25, 19, 20),
  new MeshFace(0, 5, 4),
  new MeshFace(25, 20, 21),
  new MeshFace(0, 6, 5),
  new MeshFace(25, 21, 22),
  new MeshFace(0, 7, 6),
  new MeshFace(25, 22, 23),
  new MeshFace(0, 8, 7),
  new MeshFace(25, 23, 24),
  new MeshFace(0, 1, 8),
  new MeshFace(25, 24, 17)
  };

        // data for Mesh r2
        private Point3d[] mVerticesr2 = new Point3d[]{
  new Point3d(0, 0, -1),
  new Point3d(-0.382683, 0, -0.92388),
  new Point3d(-0.353553, -0.146446, -0.92388),
  new Point3d(-0.270598, -0.270598, -0.92388),
  new Point3d(-0.146446, -0.353553, -0.92388),
  new Point3d(0, -0.382683, -0.92388),
  new Point3d(0.146446, -0.353553, -0.92388),
  new Point3d(0.270598, -0.270598, -0.92388),
  new Point3d(0.353553, -0.146446, -0.92388),
  new Point3d(0.382683, 0, -0.92388),
  new Point3d(0.353553, 0.146446, -0.92388),
  new Point3d(0.270598, 0.270598, -0.92388),
  new Point3d(0.146446, 0.353553, -0.92388),
  new Point3d(0, 0.382683, -0.92388),
  new Point3d(-0.146446, 0.353553, -0.92388),
  new Point3d(-0.270598, 0.270598, -0.92388),
  new Point3d(-0.353553, 0.146446, -0.92388),
  new Point3d(-0.707107, 0, -0.707107),
  new Point3d(-0.653282, -0.270598, -0.707107),
  new Point3d(-0.5, -0.5, -0.707107),
  new Point3d(-0.270598, -0.653282, -0.707107),
  new Point3d(0, -0.707107, -0.707107),
  new Point3d(0.270598, -0.653282, -0.707107),
  new Point3d(0.5, -0.5, -0.707107),
  new Point3d(0.653282, -0.270598, -0.707107),
  new Point3d(0.707107, 0, -0.707107),
  new Point3d(0.653282, 0.270598, -0.707107),
  new Point3d(0.5, 0.5, -0.707107),
  new Point3d(0.270598, 0.653282, -0.707107),
  new Point3d(0, 0.707107, -0.707107),
  new Point3d(-0.270598, 0.653282, -0.707107),
  new Point3d(-0.5, 0.5, -0.707107),
  new Point3d(-0.653282, 0.270598, -0.707107),
  new Point3d(-0.92388, 0, -0.382683),
  new Point3d(-0.853554, -0.353553, -0.382683),
  new Point3d(-0.653282, -0.653282, -0.382683),
  new Point3d(-0.353553, -0.853554, -0.382683),
  new Point3d(0, -0.92388, -0.382683),
  new Point3d(0.353553, -0.853554, -0.382683),
  new Point3d(0.653282, -0.653282, -0.382683),
  new Point3d(0.853554, -0.353553, -0.382683),
  new Point3d(0.92388, 0, -0.382683),
  new Point3d(0.853554, 0.353553, -0.382683),
  new Point3d(0.653282, 0.653282, -0.382683),
  new Point3d(0.353553, 0.853554, -0.382683),
  new Point3d(0, 0.92388, -0.382683),
  new Point3d(-0.353553, 0.853554, -0.382683),
  new Point3d(-0.653282, 0.653282, -0.382683),
  new Point3d(-0.853554, 0.353553, -0.382683),
  new Point3d(-1, 0, 0),
  new Point3d(-0.92388, -0.382683, 0),
  new Point3d(-0.707107, -0.707107, 0),
  new Point3d(-0.382683, -0.92388, 0),
  new Point3d(0, -1, 0),
  new Point3d(0.382683, -0.92388, 0),
  new Point3d(0.707107, -0.707107, 0),
  new Point3d(0.92388, -0.382683, 0),
  new Point3d(1, 0, 0),
  new Point3d(0.92388, 0.382683, 0),
  new Point3d(0.707107, 0.707107, 0),
  new Point3d(0.382683, 0.92388, 0),
  new Point3d(0, 1, 0),
  new Point3d(-0.382683, 0.92388, 0),
  new Point3d(-0.707107, 0.707107, 0),
  new Point3d(-0.92388, 0.382683, 0),
  new Point3d(-0.92388, 0, 0.382683),
  new Point3d(-0.853554, -0.353553, 0.382683),
  new Point3d(-0.653282, -0.653282, 0.382683),
  new Point3d(-0.353553, -0.853554, 0.382683),
  new Point3d(0, -0.92388, 0.382683),
  new Point3d(0.353553, -0.853554, 0.382683),
  new Point3d(0.653282, -0.653282, 0.382683),
  new Point3d(0.853554, -0.353553, 0.382683),
  new Point3d(0.92388, 0, 0.382683),
  new Point3d(0.853554, 0.353553, 0.382683),
  new Point3d(0.653282, 0.653282, 0.382683),
  new Point3d(0.353553, 0.853554, 0.382683),
  new Point3d(0, 0.92388, 0.382683),
  new Point3d(-0.353553, 0.853554, 0.382683),
  new Point3d(-0.653282, 0.653282, 0.382683),
  new Point3d(-0.853554, 0.353553, 0.382683),
  new Point3d(-0.707107, 0, 0.707107),
  new Point3d(-0.653282, -0.270598, 0.707107),
  new Point3d(-0.5, -0.5, 0.707107),
  new Point3d(-0.270598, -0.653282, 0.707107),
  new Point3d(0, -0.707107, 0.707107),
  new Point3d(0.270598, -0.653282, 0.707107),
  new Point3d(0.5, -0.5, 0.707107),
  new Point3d(0.653282, -0.270598, 0.707107),
  new Point3d(0.707107, 0, 0.707107),
  new Point3d(0.653282, 0.270598, 0.707107),
  new Point3d(0.5, 0.5, 0.707107),
  new Point3d(0.270598, 0.653282, 0.707107),
  new Point3d(0, 0.707107, 0.707107),
  new Point3d(-0.270598, 0.653282, 0.707107),
  new Point3d(-0.5, 0.5, 0.707107),
  new Point3d(-0.653282, 0.270598, 0.707107),
  new Point3d(-0.382683, 0, 0.92388),
  new Point3d(-0.353553, -0.146446, 0.92388),
  new Point3d(-0.270598, -0.270598, 0.92388),
  new Point3d(-0.146446, -0.353553, 0.92388),
  new Point3d(0, -0.382683, 0.92388),
  new Point3d(0.146446, -0.353553, 0.92388),
  new Point3d(0.270598, -0.270598, 0.92388),
  new Point3d(0.353553, -0.146446, 0.92388),
  new Point3d(0.382683, 0, 0.92388),
  new Point3d(0.353553, 0.146446, 0.92388),
  new Point3d(0.270598, 0.270598, 0.92388),
  new Point3d(0.146446, 0.353553, 0.92388),
  new Point3d(0, 0.382683, 0.92388),
  new Point3d(-0.146446, 0.353553, 0.92388),
  new Point3d(-0.270598, 0.270598, 0.92388),
  new Point3d(-0.353553, 0.146446, 0.92388),
  new Point3d(0, 0, 1)
  };
        private MeshFace[] mFacesr2 = new MeshFace[]{
  new MeshFace(1, 2, 18, 17),
  new MeshFace(2, 3, 19, 18),
  new MeshFace(3, 4, 20, 19),
  new MeshFace(4, 5, 21, 20),
  new MeshFace(5, 6, 22, 21),
  new MeshFace(6, 7, 23, 22),
  new MeshFace(7, 8, 24, 23),
  new MeshFace(8, 9, 25, 24),
  new MeshFace(9, 10, 26, 25),
  new MeshFace(10, 11, 27, 26),
  new MeshFace(11, 12, 28, 27),
  new MeshFace(12, 13, 29, 28),
  new MeshFace(13, 14, 30, 29),
  new MeshFace(14, 15, 31, 30),
  new MeshFace(15, 16, 32, 31),
  new MeshFace(16, 1, 17, 32),
  new MeshFace(17, 18, 34, 33),
  new MeshFace(18, 19, 35, 34),
  new MeshFace(19, 20, 36, 35),
  new MeshFace(20, 21, 37, 36),
  new MeshFace(21, 22, 38, 37),
  new MeshFace(22, 23, 39, 38),
  new MeshFace(23, 24, 40, 39),
  new MeshFace(24, 25, 41, 40),
  new MeshFace(25, 26, 42, 41),
  new MeshFace(26, 27, 43, 42),
  new MeshFace(27, 28, 44, 43),
  new MeshFace(28, 29, 45, 44),
  new MeshFace(29, 30, 46, 45),
  new MeshFace(30, 31, 47, 46),
  new MeshFace(31, 32, 48, 47),
  new MeshFace(32, 17, 33, 48),
  new MeshFace(33, 34, 50, 49),
  new MeshFace(34, 35, 51, 50),
  new MeshFace(35, 36, 52, 51),
  new MeshFace(36, 37, 53, 52),
  new MeshFace(37, 38, 54, 53),
  new MeshFace(38, 39, 55, 54),
  new MeshFace(39, 40, 56, 55),
  new MeshFace(40, 41, 57, 56),
  new MeshFace(41, 42, 58, 57),
  new MeshFace(42, 43, 59, 58),
  new MeshFace(43, 44, 60, 59),
  new MeshFace(44, 45, 61, 60),
  new MeshFace(45, 46, 62, 61),
  new MeshFace(46, 47, 63, 62),
  new MeshFace(47, 48, 64, 63),
  new MeshFace(48, 33, 49, 64),
  new MeshFace(49, 50, 66, 65),
  new MeshFace(50, 51, 67, 66),
  new MeshFace(51, 52, 68, 67),
  new MeshFace(52, 53, 69, 68),
  new MeshFace(53, 54, 70, 69),
  new MeshFace(54, 55, 71, 70),
  new MeshFace(55, 56, 72, 71),
  new MeshFace(56, 57, 73, 72),
  new MeshFace(57, 58, 74, 73),
  new MeshFace(58, 59, 75, 74),
  new MeshFace(59, 60, 76, 75),
  new MeshFace(60, 61, 77, 76),
  new MeshFace(61, 62, 78, 77),
  new MeshFace(62, 63, 79, 78),
  new MeshFace(63, 64, 80, 79),
  new MeshFace(64, 49, 65, 80),
  new MeshFace(65, 66, 82, 81),
  new MeshFace(66, 67, 83, 82),
  new MeshFace(67, 68, 84, 83),
  new MeshFace(68, 69, 85, 84),
  new MeshFace(69, 70, 86, 85),
  new MeshFace(70, 71, 87, 86),
  new MeshFace(71, 72, 88, 87),
  new MeshFace(72, 73, 89, 88),
  new MeshFace(73, 74, 90, 89),
  new MeshFace(74, 75, 91, 90),
  new MeshFace(75, 76, 92, 91),
  new MeshFace(76, 77, 93, 92),
  new MeshFace(77, 78, 94, 93),
  new MeshFace(78, 79, 95, 94),
  new MeshFace(79, 80, 96, 95),
  new MeshFace(80, 65, 81, 96),
  new MeshFace(81, 82, 98, 97),
  new MeshFace(82, 83, 99, 98),
  new MeshFace(83, 84, 100, 99),
  new MeshFace(84, 85, 101, 100),
  new MeshFace(85, 86, 102, 101),
  new MeshFace(86, 87, 103, 102),
  new MeshFace(87, 88, 104, 103),
  new MeshFace(88, 89, 105, 104),
  new MeshFace(89, 90, 106, 105),
  new MeshFace(90, 91, 107, 106),
  new MeshFace(91, 92, 108, 107),
  new MeshFace(92, 93, 109, 108),
  new MeshFace(93, 94, 110, 109),
  new MeshFace(94, 95, 111, 110),
  new MeshFace(95, 96, 112, 111),
  new MeshFace(96, 81, 97, 112),
  new MeshFace(0, 2, 1),
  new MeshFace(113, 97, 98),
  new MeshFace(0, 3, 2),
  new MeshFace(113, 98, 99),
  new MeshFace(0, 4, 3),
  new MeshFace(113, 99, 100),
  new MeshFace(0, 5, 4),
  new MeshFace(113, 100, 101),
  new MeshFace(0, 6, 5),
  new MeshFace(113, 101, 102),
  new MeshFace(0, 7, 6),
  new MeshFace(113, 102, 103),
  new MeshFace(0, 8, 7),
  new MeshFace(113, 103, 104),
  new MeshFace(0, 9, 8),
  new MeshFace(113, 104, 105),
  new MeshFace(0, 10, 9),
  new MeshFace(113, 105, 106),
  new MeshFace(0, 11, 10),
  new MeshFace(113, 106, 107),
  new MeshFace(0, 12, 11),
  new MeshFace(113, 107, 108),
  new MeshFace(0, 13, 12),
  new MeshFace(113, 108, 109),
  new MeshFace(0, 14, 13),
  new MeshFace(113, 109, 110),
  new MeshFace(0, 15, 14),
  new MeshFace(113, 110, 111),
  new MeshFace(0, 16, 15),
  new MeshFace(113, 111, 112),
  new MeshFace(0, 1, 16),
  new MeshFace(113, 112, 97)
  };

        private Mesh MakeSphereBFr0()
        {
            Mesh S = new Mesh();
            S.Vertices.AddVertices(mVerticesr0);
            S.Faces.AddFaces(mFacesr0);
            return S;
        }

        private Mesh MakeSphereBFr1()
        {
            Mesh S = new Mesh();
            S.Vertices.AddVertices(mVerticesr1);
            S.Faces.AddFaces(mFacesr1);
            return S;
        }

        private Mesh MakeSphereBFr2()
        {
            Mesh S = new Mesh();
            S.Vertices.AddVertices(mVerticesr2);
            S.Faces.AddFaces(mFacesr2);
            return S;
        }

    }
}