using System;
using System.Collections.Generic;
using System.Security.Permissions;
using froGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace froGH
{
    public class SpaceFillingPolyGen : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SpaceFillingPolyGen class.
        /// </summary>
        public SpaceFillingPolyGen()
          : base("Space-Filling Polyhedra Generator", "f_SPHGen",
              "Creates a space filling Polyhedron",
              "froGH", "Mesh-Create")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Base Plane", "P", "Base Plane for Polyhedron", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntegerParameter("Polyhedron type", "t",
                "0 Bysimmetric Hendecahedron" +
                "\n1 Rhombic Dodecahedron" +
                "\n2 Elongated Dodecahedron" +
                "\n3 Sphenoid Hendecahedron" +
                "\n4 Truncated Octahedron" +
                "\n5 Gyrobifastigium", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Scale", "s", "Polyhedron scale (center on base point)", GH_ParamAccess.item, 1);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Polyhedron as Mesh", GH_ParamAccess.item);
            pManager.AddCurveParameter("Face Polylines", "P", "Face contours as polylines", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane P = new Plane();
            int type = 0;
            double scale = 1.0;
            DA.GetData(0, ref P);
            DA.GetData(1, ref type);
            DA.GetData(2, ref scale);

            if (!P.IsValid || P == null) P = Plane.WorldXY;
            if (scale == 0.0) scale = 1.0;

            // __________________ autoList part __________________

            // variable for the list
            Grasshopper.Kernel.Special.GH_ValueList vList;
            // tries to cast input as list
            try
            {

                // if the list is not the first parameter then change Input[0] to the corresponding value
                vList = (Grasshopper.Kernel.Special.GH_ValueList)Params.Input[1].Sources[0];
                // check if the list must be created
                if (!vList.NickName.Equals("Polyhedron type"))
                {
                    vList.ClearData();
                    vList.ListItems.Clear();
                    vList.NickName = "Polyhedron type";
                    var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("Bisymmetric Hendecahedron", "0");
                    var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Rhombic Dodecahedron", "1");
                    var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("Elongated Dodecahedron", "2");
                    var item4 = new Grasshopper.Kernel.Special.GH_ValueListItem("Sphenoid Hendecahedron", "3");
                    var item5 = new Grasshopper.Kernel.Special.GH_ValueListItem("Truncated Octahedron", "4");
                    var item6 = new Grasshopper.Kernel.Special.GH_ValueListItem("Gyrobifastigium", "5");

                    vList.ListItems.Add(item1);
                    vList.ListItems.Add(item2);
                    vList.ListItems.Add(item3);
                    vList.ListItems.Add(item4);
                    vList.ListItems.Add(item5);
                    vList.ListItems.Add(item6);

                    vList.ListItems[0].Value.CastTo(out type);
                }
            }
            catch
            {
                // handles anything that is not a value list
            }

            PolyHedra b;
            b = new PolyHedra(P, type, scale);

            DA.SetData(0, b.mesh);
            DA.SetDataList(1, b.faceContours);
        }

        public class PolyHedra
        {

            public Mesh mesh;
            public List<Polyline> faceContours;
            private Plane basePlane;
            private double scale;

            public PolyHedra(Plane basePlane, int type, double scale)
            {
                this.basePlane = basePlane;
                this.scale = scale;
                faceContours = new List<Polyline>();
                switch (type)
                {
                    case 0:
                        mesh = BisymmetricHendecahedron();
                        goto case 99;
                    //ScaleAndOrient(mesh, scale, basePlane);
                    //faceContours = GetFaceContours(mesh);
                    //break;
                    case 1:
                        mesh = RhombicDodecahedron();
                        goto case 99;
                    //faceContours = GetFaceContours(mesh);
                    //break;
                    case 2:
                        ElongatedDodecahedron(out mesh, out faceContours);
                        break;
                    case 3:
                        mesh = SphenoidHendecahedron();
                        goto case 99;
                    //faceContours = GetFaceContours(mesh);
                    //break;
                    case 4:
                        TruncatedOctahedron(out mesh, out faceContours);
                        break;
                    case 5:
                        mesh = Gyrobifastigium();
                        goto case 99;
                    case 99:
                        ScaleAndOrient(mesh, scale, basePlane);
                        faceContours = GetFaceContours(mesh);
                        break;
                    default:
                        mesh = new Mesh();
                        faceContours = GetFaceContours(mesh);
                        break;
                }

                mesh.UnifyNormals();
                mesh.RebuildNormals();
                mesh.Unweld(0, true);

            }

            Mesh BisymmetricHendecahedron()
            {
                Mesh mesh = new Mesh();

                Point3d[] vertices =
                  new Point3d[]{new Point3d(0, 0, 0.5),new Point3d(0.5, 0.25, 0.25),new Point3d(0, -0.25, 0.25),new Point3d(-0.5, 0.25, 0.25),
        new Point3d(0, 0.5, 0),new Point3d(0.25, -0.25, 0),new Point3d(-0.25, -0.25, 0),new Point3d(0.5, 0.25, -0.25),
        new Point3d(0, -0.25, -0.25),new Point3d(-0.5, 0.25, -0.25),new Point3d(0, 0, -0.5)};

                mesh.Vertices.AddVertices(vertices);

                mesh.Faces.AddFace(0, 3, 6, 2);
                mesh.Faces.AddFace(0, 2, 5, 1);
                mesh.Faces.AddFace(1, 5, 7);
                mesh.Faces.AddFace(7, 5, 8, 10);
                mesh.Faces.AddFace(8, 6, 9, 10);
                mesh.Faces.AddFace(8, 5, 2, 6);
                mesh.Faces.AddFace(3, 9, 6);
                mesh.Faces.AddFace(10, 9, 4, 7);
                mesh.Faces.AddFace(1, 4, 3, 0);
                mesh.Faces.AddFace(3, 4, 9);
                mesh.Faces.AddFace(7, 4, 1);

                //mesh.Scale(scale);

                //Transform orient;
                //orient = Transform.PlaneToPlane(Plane.WorldXY, basePlane);
                //mesh.Transform(orient);

                return mesh;
            }

            void TruncatedOctahedron(out Mesh mesh, out List<Polyline> faceContours)
            {
                mesh = new Mesh();
                faceContours = new List<Polyline>();
                Point3d[] vertices =
                  new Point3d[] {new Point3d(0, -0.25, 0.5),new Point3d(-0.25, 0, 0.5),new Point3d(-0.5, 0, 0.25),
        new Point3d(-0.5, -0.25, 0), new Point3d(-0.25, -0.5, 0),new Point3d(0, -0.5, 0.25), new Point3d(0.25, 0, 0.5),
        new Point3d(0.5, 0, 0.25), new Point3d(0.5, -0.25, 0), new Point3d(0.25, -0.5, 0), new Point3d(0, 0.25, 0.5),
        new Point3d(-0.5, 0.25, 0), new Point3d(-0.25, 0.5, 0),new Point3d(0, 0.5, 0.25), new Point3d(0.5, 0.25, 0), new Point3d(0.25, 0.5, 0),
        new Point3d(0, -0.25, -0.5), new Point3d(-0.25, 0, -0.5), new Point3d(-0.5, 0, -0.25), new Point3d(0, -0.5, -0.25),
        new Point3d(0.25, 0, -0.5), new Point3d(0.5, 0, -0.25), new Point3d(0, 0.25, -0.5), new Point3d(0, 0.5, -0.25)};

                mesh.Vertices.AddVertices(vertices);

                int[][] contours =
                  new int[][]{new int[]{0, 1, 2, 3, 4, 5},new int[]{0, 5, 9, 8, 7, 6}, new int[]{6,7,14,15,13,10},  new int[]{10,13,12,11,2,1},// hex faces up
        new int[]{3,18,17,16,19,4},new int[]{9,19,16,20,21,8}, new int[]{14,21,20,22,23,15},  new int[]{12,23,22,17,18,11}, // hex faces down
        new int[]{20,16,17,22}, new int[]{0, 6, 10, 1}, // quad faces bottom-top
        new int[]{11, 18, 3, 2}, new int[]{8, 21, 14, 7}, new int[]{4, 19, 9, 5},  new int[]{15, 23, 12, 13}};// quad faces W-E-S-N

                // hex faces
                mesh.Faces.AddFace(0, 1, 2, 5);
                mesh.Faces.AddFace(5, 2, 3, 4); // full face: 0, 1, 2, 3, 4, 5
                mesh.Faces.AddFace(5, 7, 6, 0);
                mesh.Faces.AddFace(5, 9, 8, 7); // full face: 5, 9, 8, 7, 6, 0
                mesh.Faces.AddFace(7, 13, 10, 6);
                mesh.Faces.AddFace(7, 14, 15, 13);
                mesh.Faces.AddFace(13, 2, 1, 10);
                mesh.Faces.AddFace(13, 12, 11, 2);
                mesh.Faces.AddFace(19, 21, 8, 9);
                mesh.Faces.AddFace(19, 16, 20, 21);
                mesh.Faces.AddFace(21, 23, 15, 14);
                mesh.Faces.AddFace(21, 20, 22, 23);
                mesh.Faces.AddFace(23, 18, 11, 12);
                mesh.Faces.AddFace(23, 22, 17, 18);
                mesh.Faces.AddFace(18, 19, 4, 3);
                mesh.Faces.AddFace(18, 17, 16, 19);
                // quad faces
                mesh.Faces.AddFace(0, 6, 10, 1); // top
                mesh.Faces.AddFace(20, 16, 17, 22); // bottom
                mesh.Faces.AddFace(8, 21, 14, 7); // E
                mesh.Faces.AddFace(15, 23, 12, 13); // N
                mesh.Faces.AddFace(11, 18, 3, 2); // W
                mesh.Faces.AddFace(4, 19, 9, 5); // S

                foreach (int[] cont in contours)
                {
                    Polyline pc = new Polyline();
                    foreach (int i in cont) pc.Add(vertices[i]);
                    pc.Add(vertices[cont[0]]);
                    faceContours.Add(pc);
                }

                Transform scaleT = Transform.Scale(new Point3d(), scale);
                Transform orient = Transform.PlaneToPlane(Plane.WorldXY, basePlane);

                mesh.Transform(scaleT);
                mesh.Transform(orient);
                foreach (Polyline f in faceContours)
                {
                    f.Transform(scaleT);
                    f.Transform(orient);
                }
            }

            List<Polyline> MakeContours(Mesh mesh, int[][] contours)
            {
                List<Polyline> faceContours = new List<Polyline>();
                foreach (int[] cont in contours)
                {
                    Polyline pc = new Polyline();
                    foreach (int i in cont) pc.Add(mesh.Vertices[i]);
                    pc.Add(mesh.Vertices[cont[0]]);
                    faceContours.Add(pc);
                }

                return faceContours;
            }

            void ScaleOrient(ref Mesh mesh, ref List<Polyline> faceContours)
            {
                Transform scaleT = Transform.Scale(new Point3d(), scale);
                Transform orient = Transform.PlaneToPlane(Plane.WorldXY, basePlane);

                mesh.Transform(scaleT);
                mesh.Transform(orient);
                foreach (Polyline f in faceContours)
                {
                    f.Transform(scaleT);
                    f.Transform(orient);
                }
            }

            Mesh RhombicDodecahedron()
            {
                Mesh mesh = new Mesh();

                Point3d[] vertices =
                  new Point3d[] {new Point3d(0, 0, -1),new Point3d(0.5, 0.5, -0.5),new Point3d(1, 0, 0),new Point3d(0.5, -0.5, -0.5),
        new Point3d(0, -1, 0),new Point3d(-0.5, -0.5, -0.5),new Point3d(-1, 0, 0),new Point3d(-0.5, 0.5, -0.5),new Point3d(0, 0, 1),
        new Point3d(0.5, -0.5, 0.5),new Point3d(0.5, 0.5, 0.5),new Point3d(-0.5, -0.5, 0.5),new Point3d(-0.5, 0.5, 0.5),new Point3d(0, 1, 0)
                  };

                mesh.Vertices.AddVertices(vertices);

                mesh.Faces.AddFace(0, 1, 2, 3);
                mesh.Faces.AddFace(0, 3, 4, 5);
                mesh.Faces.AddFace(0, 5, 6, 7);
                mesh.Faces.AddFace(8, 9, 2, 10);
                mesh.Faces.AddFace(8, 11, 4, 9);
                mesh.Faces.AddFace(8, 12, 6, 11);
                mesh.Faces.AddFace(0, 7, 13, 1);
                mesh.Faces.AddFace(4, 3, 2, 9);
                mesh.Faces.AddFace(6, 5, 4, 11);
                mesh.Faces.AddFace(13, 12, 8, 10);
                mesh.Faces.AddFace(13, 7, 6, 12);
                mesh.Faces.AddFace(2, 1, 13, 10);

                //mesh.Scale(scale * 0.5);

                //Transform orient;
                //orient = Transform.PlaneToPlane(Plane.WorldXY, basePlane);
                //mesh.Transform(orient);

                return mesh;
            }

            void ElongatedDodecahedron(out Mesh mesh, out List<Polyline> faceContours)
            {
                mesh = new Mesh();
                faceContours = new List<Polyline>();

                Point3d[] vertices = new Point3d[] { new Point3d(-0.25, 0.25, -0.4079729), new Point3d(-9.758178E-09, 0.5, -0.2039729), new Point3d(0.25, -0.25, -0.4079729), new Point3d(0, 0, -0.6119729), new Point3d(0.25, 0.25, -0.4079729), new Point3d(0, -0.5, -0.204), new Point3d(-0.5, 3.439183E-08, -0.204), new Point3d(-0.25, -0.25, -0.4079729), new Point3d(-0.25, 0.25, 0.4076668), new Point3d(0.25, 0.25, 0.4080271), new Point3d(0, 0, 0.6120271), new Point3d(-0.25, -0.25, 0.4080271), new Point3d(-9.758178E-09, 0.5, 0.2040271), new Point3d(0.5, 0, -0.204), new Point3d(0.5, 0, 0.204), new Point3d(-0.5000001, -3.439183E-08, 0.204), new Point3d(0.25, -0.25, 0.4080271), new Point3d(0, -0.5, 0.204) };

                mesh.Vertices.AddVertices(vertices);

                mesh.Faces.AddFace(4, 3, 0, 1);
                mesh.Faces.AddFace(2, 3, 4, 13);
                mesh.Faces.AddFace(7, 6, 0, 3);
                mesh.Faces.AddFace(2, 5, 7, 3);

                mesh.Faces.AddFace(14, 16, 2, 13);
                mesh.Faces.AddFace(17, 5, 2, 16);
                mesh.Faces.AddFace(12, 1, 0, 8);
                mesh.Faces.AddFace(0, 6, 15, 8);
                mesh.Faces.AddFace(7, 5, 17, 11);
                mesh.Faces.AddFace(15, 6, 7, 11);
                mesh.Faces.AddFace(4, 1, 12, 9);
                mesh.Faces.AddFace(14, 13, 4, 9);

                mesh.Faces.AddFace(9, 12, 8, 10);
                mesh.Faces.AddFace(16, 14, 9, 10);
                mesh.Faces.AddFace(11, 10, 8, 15);
                mesh.Faces.AddFace(16, 10, 11, 17);




                int[][] contours =
        new int[][]{new int[]{2,13,14,16,17,5},new int[]{4,1,12,9,14,13}, new int[]{0,6,15,8,12,1},  new int[]{7,5,17,11,15,6},// hex faces
        new int[]{4, 3, 0, 1}, new int[]{2, 3, 4, 13}, new int[]{7, 6, 0, 3}, new int[]{2, 5, 7, 3},// quad faces bottom
        new int[]{9, 12, 8, 10}, new int[]{16, 14, 9, 10}, new int[]{11, 10, 8, 15},  new int[]{16, 10, 11, 17}};// quad faces top

                faceContours = MakeContours(mesh, contours);
                ScaleOrient(ref mesh, ref faceContours);

                mesh.UnifyNormals();
                mesh.RebuildNormals();
                mesh.Unweld(0, true);
            }

            Mesh SphenoidHendecahedron()
            {
                Mesh mesh = new Mesh();

                mesh.Vertices.Add(new Point3d(13.0 / 7.0, 3 * Math.Sqrt(3) / 7, 1)); // A - 0
                mesh.Vertices.Add(new Point3d(1, Math.Sqrt(3), 0)); // B - 1
                mesh.Vertices.Add(new Point3d(2, Math.Sqrt(3), 0.5)); // C - 2
                mesh.Vertices.Add(new Point3d(2.5, Math.Sqrt(3) / 2, 0)); // D - 3
                mesh.Vertices.Add(new Point3d(2.25, Math.Sqrt(3) / 4, 0.5)); // E - 4
                mesh.Vertices.Add(new Point3d(2, 0, 0)); // F - 5
                mesh.Vertices.Add(new Point3d(0, 0, 0.5)); // G - 6
                mesh.Vertices.Add(new Point3d(2, Math.Sqrt(3), -0.5)); // H - 7
                mesh.Vertices.Add(new Point3d(2.25, Math.Sqrt(3) / 4, -0.5)); // J - 8
                mesh.Vertices.Add(new Point3d(0, 0, -0.5)); // K - 9
                mesh.Vertices.Add(new Point3d(13.0 / 7.0, 3 * Math.Sqrt(3) / 7, -1)); // L - 10

                // quad faces
                mesh.Faces.AddFace(0, 6, 5, 4); // A G F E
                mesh.Faces.AddFace(0, 2, 1, 6); // A C B G
                mesh.Faces.AddFace(0, 4, 3, 2); // A E D C
                mesh.Faces.AddFace(5, 9, 10, 8); // F K L J
                mesh.Faces.AddFace(10, 9, 1, 7); // L K B H
                mesh.Faces.AddFace(3, 8, 10, 7); // D J L H
                mesh.Faces.AddFace(4, 5, 8, 3); // E F J D

                // tri faces
                mesh.Faces.AddFace(6, 9, 5); // G K F
                mesh.Faces.AddFace(6, 1, 9); // G B K
                mesh.Faces.AddFace(2, 3, 7); // C D H
                mesh.Faces.AddFace(2, 7, 1); // C H B

                // orient to WorldXY center
                Point3d center = mesh.GetBoundingBox(false).Center;
                mesh.Rotate(Math.PI * 0.5, Vector3d.XAxis, center);
                mesh.Rotate(-Math.PI * 0.5, Vector3d.ZAxis, center);
                mesh.Translate((Vector3d)(-center));

                //mesh.Scale(scale * 0.5);
                //Point3d center = mesh.GetBoundingBox(false).Center;
                //Plane cPlane = Plane.WorldZX;
                //cPlane.Rotate(Math.PI, Vector3d.YAxis);
                //cPlane.Translate((Vector3d)center);

                //Transform orient;

                //orient = Transform.PlaneToPlane(Plane.WorldZX, basePlane);
                //mesh.Transform(orient);

                return mesh;
            }

            // https://en.wikipedia.org/wiki/Gyrobifastigium
            Mesh Gyrobifastigium()
            {
                Mesh mesh = new Mesh();

                Point3d[] vertices = new Point3d[] { new Point3d(0.5, 0.5, 0), new Point3d(0.5, -0.5, 0),
                    new Point3d(0, -0.5, 0.8660255), new Point3d(-0.5, -0.5, 0), new Point3d(0, 0.5, 0.8660255),
                    new Point3d(-0.5, 0.5, 0), new Point3d(-0.5, 0, -0.8660255), new Point3d(0.5, 0, -0.8660255) };

                mesh.Vertices.AddVertices(vertices);

                mesh.Faces.AddFace(2, 1, 0, 4);
                mesh.Faces.AddFace(1, 2, 3);
                mesh.Faces.AddFace(4, 5, 3, 2);
                mesh.Faces.AddFace(5, 4, 0);
                mesh.Faces.AddFace(7, 0, 1);
                mesh.Faces.AddFace(7, 6, 5, 0);
                mesh.Faces.AddFace(7, 1, 3, 6);
                mesh.Faces.AddFace(6, 3, 5);

                mesh.UnifyNormals();
                mesh.RebuildNormals();

                mesh.Unweld(0, true);

                return mesh;
            }


            List<Polyline> GetFaceContours(Mesh mesh)
            {
                List<Polyline> faceContours = new List<Polyline>();
                Polyline faceContour;
                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    faceContour = new Polyline();
                    Point3f[] pf;
                    if (mesh.Faces[i].IsQuad)
                    {
                        pf = new Point3f[4];
                        mesh.Faces.GetFaceVertices(i, out pf[0], out pf[1], out pf[2], out pf[3]);
                    }
                    else
                    {
                        pf = new Point3f[3];
                        mesh.Faces.GetFaceVertices(i, out pf[0], out pf[1], out pf[2], out pf[2]);
                    }
                    for (int j = 0; j < pf.Length; j++)
                    {
                        faceContour.Add(pf[j].X, pf[j].Y, pf[j].Z);
                    }
                    faceContour.Add(pf[0].X, pf[0].Y, pf[0].Z); // adds again first point to generate a closed polyline
                    faceContours.Add(faceContour);
                }
                return faceContours;
            }

            Mesh ScaleAndOrient(Mesh mesh, double scale, Plane basePlane)
            {
                mesh.Scale(scale * 0.5);
                Transform orient;
                orient = Transform.PlaneToPlane(Plane.WorldXY, basePlane);
                mesh.Transform(orient);
                return mesh;
            }
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
                return Resources.SPFPolyhedra_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("76e4284b-40c8-4712-9235-0de1acb3e7e2"); }
        }
    }
}