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
            pManager.AddIntegerParameter("Polyhedron type", "t", "0 Bysimmetric Hendecahedron\n1 Rhombic Dodecahedron\n2 Sphenoid Hendecahedron\n3 Truncated Octahedron", GH_ParamAccess.item, 0);
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
                vList = (Grasshopper.Kernel.Special.GH_ValueList) Params.Input[1].Sources[0];
                // check if the list must be created
                if (!vList.NickName.Equals("Polyhedron type"))
                {
                    vList.ClearData();
                    vList.ListItems.Clear();
                    vList.NickName = "Polyhedron type";
                    var item1 = new Grasshopper.Kernel.Special.GH_ValueListItem("Bisymmetric Hendecahedron", "0");
                    var item2 = new Grasshopper.Kernel.Special.GH_ValueListItem("Rhombic Dodecahedron", "1");
                    var item3 = new Grasshopper.Kernel.Special.GH_ValueListItem("Sphenoid Hendecahedron", "2");
                    var item4 = new Grasshopper.Kernel.Special.GH_ValueListItem("Truncated Octahedron", "3");

                    vList.ListItems.Add(item1);
                    vList.ListItems.Add(item2);
                    vList.ListItems.Add(item3);
                    vList.ListItems.Add(item4);

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
            DA.SetDataList(1, b.fc);
        }

        public class PolyHedra
        {

            public Plane basePt;
            public Mesh mesh;
            public List<Polyline> fc;
            public int type;
            public double scale;

            public PolyHedra(Plane basePt, int type, double scale)
            {
                this.basePt = basePt;
                this.type = type;
                this.scale = scale;
                fc = new List<Polyline>();
                switch (type)
                {
                    case 0:
                        mesh = BisymmetricHendecahedron();
                        GetFacesPolylines();
                        break;
                    case 1:
                        mesh = RhombicDodecahedron();
                        GetFacesPolylines();
                        break;
                    case 2:
                        mesh = SphenoidHendecahedron();
                        GetFacesPolylines();
                        break;
                    case 3:
                        TruncatedOctahedron();
                        break;
                    default:
                        mesh = new Mesh();
                        GetFacesPolylines();
                        break;
                }

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
                mesh.Faces.AddFace(10, 9, 6, 8);
                mesh.Faces.AddFace(8, 5, 2, 6);
                mesh.Faces.AddFace(6, 9, 3);
                mesh.Faces.AddFace(7, 4, 9, 10);
                mesh.Faces.AddFace(0, 3, 4, 1);
                mesh.Faces.AddFace(9, 4, 3);
                mesh.Faces.AddFace(7, 4, 1);

                mesh.UnifyNormals();

                mesh.Scale(scale);

                Transform orient;
                orient = Transform.PlaneToPlane(Plane.WorldXY, basePt);
                mesh.Transform(orient);

                return mesh;
            }

            void TruncatedOctahedron()
            {
                mesh = new Mesh();
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
        new int[]{22,17,16,20}, new int[]{0, 6, 10, 1}, // quad faces bottom-top
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
                mesh.Faces.AddFace(16, 20, 22, 17); // bottom
                mesh.Faces.AddFace(8, 21, 14, 7); // E
                mesh.Faces.AddFace(15, 23, 12, 13); // N
                mesh.Faces.AddFace(11, 18, 3, 2); // W
                mesh.Faces.AddFace(4, 19, 9, 5); // S

                mesh.UnifyNormals();
                mesh.RebuildNormals();

                foreach (int[] cont in contours)
                {
                    Polyline pc = new Polyline();
                    foreach (int i in cont) pc.Add(vertices[i]);
                    pc.Add(vertices[cont[0]]);
                    fc.Add(pc);
                }

                Transform scaleT = Transform.Scale(new Point3d(), scale);
                Transform orient = Transform.PlaneToPlane(Plane.WorldXY, basePt);

                mesh.Transform(scaleT);
                mesh.Transform(orient);
                foreach (Polyline f in fc)
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

                mesh.UnifyNormals();
                mesh.RebuildNormals();
                mesh.Scale(scale * 0.5);

                Transform orient;
                orient = Transform.PlaneToPlane(Plane.WorldXY, basePt);
                mesh.Transform(orient);

                return mesh;
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
                mesh.Faces.AddFace(7, 1, 9, 10); // H B K L
                mesh.Faces.AddFace(7, 10, 8, 3); // H L J D
                mesh.Faces.AddFace(4, 5, 8, 3); // E F J D

                // tri faces
                mesh.Faces.AddFace(6, 9, 5); // G K F
                mesh.Faces.AddFace(6, 1, 9); // G B K
                mesh.Faces.AddFace(2, 3, 7); // C D H
                mesh.Faces.AddFace(2, 7, 1); // C H B

                mesh.UnifyNormals();
                mesh.RebuildNormals();
                mesh.Scale(scale * 0.5);
                Point3d center = mesh.GetBoundingBox(false).Center;
                Plane cPlane = Plane.WorldZX;
                cPlane.Rotate(Math.PI, Vector3d.YAxis);
                cPlane.Translate((Vector3d)center);

                Transform orient;

                orient = Transform.PlaneToPlane(cPlane, basePt);
                mesh.Transform(orient);

                return mesh;
            }



            public void GetFacesPolylines()
            {
                Polyline faceContour = new Polyline();
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
                    fc.Add(faceContour);
                }
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