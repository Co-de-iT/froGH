using Rhino.Geometry;
using System;
using System.Linq;

namespace froGH.Utils
{
    /// <summary>
    /// Check Modifications:
    /// . ElongatedDodecahedron & TruncatedOctahedron return as out params an array of indexes for faceContours
    /// . make on overload of MakeContours that takes said array of indexes
    /// . after generating the mesh, scale and orient the mesh first, then call Generate Contours in the appropriate case
    /// </summary>
    public class PolyHedra
    {
        internal Mesh[] allMeshes;
        internal Polyline[][] allFaceContours;
        internal Mesh mesh;
        internal Polyline[] faceContours;
        internal string[] names = new string[]
        {
        "Bisymmetric Hendecahedron",
        "Rhombic Dodecahedron",
        "Elongated Dodecahedron",
        "Sphenoid Hendecahedron",
        "Truncated Octahedron",
        "Gyrobifastigium"
        };

        public PolyHedra()
        {
            int[][] contoursIndexes;

            allMeshes = new Mesh[6];
            allFaceContours = new Polyline[6][];

            allMeshes[0] = BisymmetricHendecahedron();
            allFaceContours[0] = MakeContours(allMeshes[0]);

            allMeshes[1] = RhombicDodecahedron();
            allFaceContours[1] = MakeContours(allMeshes[1]);

            allMeshes[2] = ElongatedDodecahedron(out contoursIndexes);
            allFaceContours[2] = MakeContours(allMeshes[2], contoursIndexes);

            allMeshes[3] = SphenoidHendecahedron();
            allFaceContours[3] = MakeContours(allMeshes[3]);

            allMeshes[4] = TruncatedOctahedron(out contoursIndexes);
            allFaceContours[4] = MakeContours(allMeshes[4], contoursIndexes);

            allMeshes[5] = Gyrobifastigium();
            allFaceContours[5] = MakeContours(allMeshes[5]);

            for (int i = 0; i < allMeshes.Length; i++)
            {
                allMeshes[i].UnifyNormals();
                allMeshes[i].RebuildNormals();
                allMeshes[i].Unweld(0, true);
            }
        }

        public PolyHedra(Plane basePlane, int type, double scale)
        {
            int[][] contoursIndexes = new int[1][];
            switch (type)
            {
                case 0:
                    mesh = BisymmetricHendecahedron();
                    goto case 99;
                case 1:
                    mesh = RhombicDodecahedron();
                    goto case 99;
                case 2:
                    mesh = ElongatedDodecahedron(out contoursIndexes);
                    goto case 98;
                case 3:
                    mesh = SphenoidHendecahedron();
                    goto case 99;
                case 4:
                    mesh = TruncatedOctahedron(out contoursIndexes);
                    goto case 98;
                case 5:
                    mesh = Gyrobifastigium();
                    goto case 99;
                case 98:
                    ScaleAndOrientMesh(mesh, scale, basePlane);
                    faceContours = MakeContours(mesh, contoursIndexes);
                    break;
                case 99:
                    ScaleAndOrientMesh(mesh, scale, basePlane);
                    faceContours = MakeContours(mesh);
                    break;
                default:
                    mesh = new Mesh();
                    faceContours = null;
                    break;
            }

            mesh.UnifyNormals();
            mesh.RebuildNormals();
            mesh.Unweld(0, true);

        }

        public void GetPolyhedra(int type, out Mesh pMesh, out Polyline[] pFaceContours)
        {
            pMesh = GetMesh(type);
            pFaceContours = GetFaceContours(type);
        }

        public Mesh GetMesh(int type)
        {
            return allMeshes[type].DuplicateMesh();
        }

        public Polyline[] GetFaceContours(int type)
        {
            return allFaceContours[type].Select(p => p.Duplicate()).ToArray();
        }

        Mesh BisymmetricHendecahedron()
        {
            Mesh mesh = new Mesh();

            Point3d[] vertices =
              new Point3d[]{new Point3d(0, 0, 1),new Point3d(1, 0.5, 0.5),new Point3d(0, -0.5, 0.5),new Point3d(-1, 0.5, 0.5),
              new Point3d(0, 1, 0),new Point3d(0.5, -0.5, 0),new Point3d(-0.5, -0.5, 0),new Point3d(1, 0.5, -0.5),
              new Point3d(0, -0.5, -0.5),new Point3d(-1, 0.5, -0.5),new Point3d(0, 0, -1)};

            MeshFace[] faces =
                new MeshFace[] {
                new MeshFace(0, 3, 6, 2),
                new MeshFace(0, 2, 5, 1),
                new MeshFace(1, 5, 7),
                new MeshFace(7, 5, 8, 10),
                new MeshFace(8, 6, 9, 10),
                new MeshFace(8, 5, 2, 6),
                new MeshFace(3, 9, 6),
                new MeshFace(10, 9, 4, 7),
                new MeshFace(1, 4, 3, 0),
                new MeshFace(3, 4, 9),
                new MeshFace(7, 4, 1)};

            mesh.Vertices.AddVertices(vertices);
            mesh.Faces.AddFaces(faces);

            return mesh;
        }

        Mesh TruncatedOctahedron(out int[][] contours)
        {
            Mesh mesh = new Mesh();
            Point3d[] vertices =
              new Point3d[] {new Point3d(0, -0.25, 0.5),new Point3d(-0.25, 0, 0.5),new Point3d(-0.5, 0, 0.25),
        new Point3d(-0.5, -0.25, 0), new Point3d(-0.25, -0.5, 0),new Point3d(0, -0.5, 0.25), new Point3d(0.25, 0, 0.5),
        new Point3d(0.5, 0, 0.25), new Point3d(0.5, -0.25, 0), new Point3d(0.25, -0.5, 0), new Point3d(0, 0.25, 0.5),
        new Point3d(-0.5, 0.25, 0), new Point3d(-0.25, 0.5, 0),new Point3d(0, 0.5, 0.25), new Point3d(0.5, 0.25, 0), new Point3d(0.25, 0.5, 0),
        new Point3d(0, -0.25, -0.5), new Point3d(-0.25, 0, -0.5), new Point3d(-0.5, 0, -0.25), new Point3d(0, -0.5, -0.25),
        new Point3d(0.25, 0, -0.5), new Point3d(0.5, 0, -0.25), new Point3d(0, 0.25, -0.5), new Point3d(0, 0.5, -0.25)};

            MeshFace[] faces = new MeshFace[]
            { 
                // hex faces
                new MeshFace(0, 1, 2, 5),
                new MeshFace(5, 2, 3, 4), // full face: 0, 1, 2, 3, 4, 5
                new MeshFace(5, 7, 6, 0),
                new MeshFace(5, 9, 8, 7), // full face: 5, 9, 8, 7, 6, 0
                new MeshFace(7, 13, 10, 6),
                new MeshFace(7, 14, 15, 13),
                new MeshFace(13, 2, 1, 10),
                new MeshFace(13, 12, 11, 2),
                new MeshFace(19, 21, 8, 9),
                new MeshFace(19, 16, 20, 21),
                new MeshFace(21, 23, 15, 14),
                new MeshFace(21, 20, 22, 23),
                new MeshFace(23, 18, 11, 12),
                new MeshFace(23, 22, 17, 18),
                new MeshFace(18, 19, 4, 3),
                new MeshFace(18, 17, 16, 19),
                // quad faces
                new MeshFace(0, 6, 10, 1), // top
                new MeshFace(20, 16, 17, 22), // bottom
                new MeshFace(8, 21, 14, 7), // E
                new MeshFace(15, 23, 12, 13), // N
                new MeshFace(11, 18, 3, 2), // W
                new MeshFace(4, 19, 9, 5) // S
        };

            contours =
new int[][]{new int[]{0, 1, 2, 3, 4, 5},new int[]{0, 5, 9, 8, 7, 6}, new int[]{6,7,14,15,13,10},  new int[]{10,13,12,11,2,1},// hex faces up
        new int[]{3,18,17,16,19,4},new int[]{9,19,16,20,21,8}, new int[]{14,21,20,22,23,15},  new int[]{12,23,22,17,18,11}, // hex faces down
        new int[]{20,16,17,22}, new int[]{0, 6, 10, 1}, // quad faces bottom-top
        new int[]{11, 18, 3, 2}, new int[]{8, 21, 14, 7}, new int[]{4, 19, 9, 5},  new int[]{15, 23, 12, 13}};// quad faces W-E-S-N

            mesh.Vertices.AddVertices(vertices);
            mesh.Faces.AddFaces(faces);
            //faceContours = MakeContours(mesh, contours);

            return mesh;
        }

        Mesh RhombicDodecahedron()
        {
            Mesh mesh = new Mesh();

            Point3d[] vertices =
              new Point3d[] {new Point3d(0, 0, -1),new Point3d(0.5, 0.5, -0.5),new Point3d(1, 0, 0),new Point3d(0.5, -0.5, -0.5),
        new Point3d(0, -1, 0),new Point3d(-0.5, -0.5, -0.5),new Point3d(-1, 0, 0),new Point3d(-0.5, 0.5, -0.5),new Point3d(0, 0, 1),
        new Point3d(0.5, -0.5, 0.5),new Point3d(0.5, 0.5, 0.5),new Point3d(-0.5, -0.5, 0.5),new Point3d(-0.5, 0.5, 0.5),new Point3d(0, 1, 0)
              };
            MeshFace[] faces = new MeshFace[]
            {
                new MeshFace(0, 1, 2, 3),
                new MeshFace(0, 3, 4, 5),
                new MeshFace(0, 5, 6, 7),
                new MeshFace(8, 9, 2, 10),
                new MeshFace(8, 11, 4, 9),
                new MeshFace(8, 12, 6, 11),
                new MeshFace(0, 7, 13, 1),
                new MeshFace(4, 3, 2, 9),
                new MeshFace(6, 5, 4, 11),
                new MeshFace(13, 12, 8, 10),
                new MeshFace(13, 7, 6, 12),
                new MeshFace(2, 1, 13, 10)};

            mesh.Vertices.AddVertices(vertices);
            mesh.Faces.AddFaces(faces);

            return mesh;
        }

        Mesh ElongatedDodecahedron(out int[][] contours)
        {
            Mesh mesh = new Mesh();

            Point3d[] vertices = new Point3d[] { new Point3d(-0.25, 0.25, -0.408), new Point3d(0, 0.5, -0.204), new Point3d(0.25, -0.25, -0.408),
                    new Point3d(0, 0, -0.612), new Point3d(0.25, 0.25, -0.408), new Point3d(0, -0.5, -0.204), new Point3d(-0.5, 0, -0.204),
                    new Point3d(-0.25, -0.25, -0.408), new Point3d(-0.25, 0.25, 0.408), new Point3d(0.25, 0.25, 0.408), new Point3d(0, 0, 0.612),
                    new Point3d(-0.25, -0.25, 0.408), new Point3d(0, 0.5, 0.204), new Point3d(0.5, 0, -0.204), new Point3d(0.5, 0, 0.204),
                    new Point3d(-0.5, 0, 0.204), new Point3d(0.25, -0.25, 0.408), new Point3d(0, -0.5, 0.204) };

            MeshFace[] faces = new MeshFace[]
            {
                new MeshFace(4, 3, 0, 1),
                new MeshFace(2, 3, 4, 13),
                new MeshFace(7, 6, 0, 3),
                new MeshFace(2, 5, 7, 3),

                new MeshFace(14, 16, 2, 13),
                new MeshFace(17, 5, 2, 16),
                new MeshFace(12, 1, 0, 8),
                new MeshFace(0, 6, 15, 8),
                new MeshFace(7, 5, 17, 11),
                new MeshFace(15, 6, 7, 11),
                new MeshFace(4, 1, 12, 9),
                new MeshFace(14, 13, 4, 9),

                new MeshFace(9, 12, 8, 10),
                new MeshFace(16, 14, 9, 10),
                new MeshFace(11, 10, 8, 15),
                new MeshFace(16, 10, 11, 17)
        };

            contours =
    new int[][]{new int[]{2,13,14,16,17,5},new int[]{4,1,12,9,14,13}, new int[]{0,6,15,8,12,1},  new int[]{7,5,17,11,15,6},// hex faces
        new int[]{4, 3, 0, 1}, new int[]{2, 3, 4, 13}, new int[]{7, 6, 0, 3}, new int[]{2, 5, 7, 3},// quad faces bottom
        new int[]{9, 12, 8, 10}, new int[]{16, 14, 9, 10}, new int[]{11, 10, 8, 15},  new int[]{16, 10, 11, 17}};// quad faces top

            mesh.Vertices.AddVertices(vertices);
            mesh.Faces.AddFaces(faces);
            //faceContours = MakeContours(mesh, contours);

            return mesh;
        }

        Mesh SphenoidHendecahedron()
        {
            Mesh mesh = new Mesh();
            double a = 13.0 / 7.0, b = Math.Sqrt(3), c = 3 * b / 7.0, d = b * 0.5, e = b * 0.25;
            Point3d[] vertices = new Point3d[]
            {
                new Point3d(a, c, 1), // A - 0
                new Point3d(1, b, 0), // B - 1
                new Point3d(2, b, 0.5), // C - 2
                new Point3d(2.5, d, 0), // D - 3
                new Point3d(2.25, e, 0.5), // E - 4
                new Point3d(2, 0, 0), // F - 5
                new Point3d(0, 0, 0.5), // G - 6
                new Point3d(2, b, -0.5), // H - 7
                new Point3d(2.25, e, -0.5), // J - 8
                new Point3d(0, 0, -0.5), // K - 9
                new Point3d(a, c, -1) // L - 10
        };

            MeshFace[] faces = new MeshFace[]
            { 
                // quad faces
                new MeshFace(0, 6, 5, 4), // A G F E
                new MeshFace(0, 2, 1, 6), // A C B G
                new MeshFace(0, 4, 3, 2), // A E D C
                new MeshFace(5, 9, 10, 8), // F K L J
                new MeshFace(10, 9, 1, 7), // L K B H
                new MeshFace(3, 8, 10, 7), // D J L H
                new MeshFace(4, 5, 8, 3), // E F J D
                // tri faces
                new MeshFace(6, 9, 5), // G K F
                new MeshFace(6, 1, 9), // G B K
                new MeshFace(2, 3, 7), // C D H
                new MeshFace(2, 7, 1) // C H B
        };

            mesh.Vertices.AddVertices(vertices);
            mesh.Faces.AddFaces(faces);

            // orient to WorldXY center
            Point3d center = mesh.GetBoundingBox(false).Center;
            mesh.Rotate(Math.PI * 0.5, Vector3d.XAxis, center);
            mesh.Rotate(-Math.PI * 0.5, Vector3d.ZAxis, center);
            mesh.Translate((Vector3d)(-center));

            return mesh;
        }

        // https://en.wikipedia.org/wiki/Gyrobifastigium
        Mesh Gyrobifastigium()
        {
            Mesh mesh = new Mesh();

            Point3d[] vertices = new Point3d[] { new Point3d(0.5, 0.5, 0), new Point3d(0.5, -0.5, 0),
                    new Point3d(0, -0.5, 0.8660255), new Point3d(-0.5, -0.5, 0), new Point3d(0, 0.5, 0.8660255),
                    new Point3d(-0.5, 0.5, 0), new Point3d(-0.5, 0, -0.8660255), new Point3d(0.5, 0, -0.8660255) };

            MeshFace[] faces = new MeshFace[]
            {
                new MeshFace(2, 1, 0, 4),
                new MeshFace(1, 2, 3),
                new MeshFace(4, 5, 3, 2),
                new MeshFace(5, 4, 0),
                new MeshFace(7, 0, 1),
                new MeshFace(7, 6, 5, 0),
                new MeshFace(7, 1, 3, 6),
                new MeshFace(6, 3, 5)
        };

            mesh.Vertices.AddVertices(vertices);
            mesh.Faces.AddFaces(faces);

            return mesh;
        }

        Polyline[] MakeContours(Mesh mesh, int[][] contours)
        {
            Polyline[] faceContours = new Polyline[contours.GetLength(0)];
            for (int i = 0; i < contours.GetLength(0); i++)
            {
                int[] cont = contours[i];
                Polyline pc = new Polyline();
                foreach (int j in cont) pc.Add(mesh.Vertices[j]);
                pc.Add(mesh.Vertices[cont[0]]);
                faceContours[i] = pc;
            }

            return faceContours;
        }

        Polyline[] MakeContours(Mesh mesh)
        {
            Polyline[] faceContours = new Polyline[mesh.Faces.Count];
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
                faceContours[i] = faceContour;
            }
            return faceContours;
        }

        void ScaleAndOrient(Mesh mesh, Polyline[] faceContours, double scale, Plane basePlane)
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

        void ScaleAndOrientMesh(Mesh mesh, double scale, Plane basePlane)
        {
            mesh.Scale(scale * 0.5);
            Transform orient;
            orient = Transform.PlaneToPlane(Plane.WorldXY, basePlane);
            mesh.Transform(orient);
        }
    }
}
