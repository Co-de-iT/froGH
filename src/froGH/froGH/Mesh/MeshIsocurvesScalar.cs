using froGH.Properties;
using GH_IO.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace froGH
{
    public class MeshIsocurvesScalar : GH_Component
    {
        // constants and LUTs
        // Quads
        // from MSB (Most Significant Bit) to LSB (Least Significant Bit)
        private readonly int[] vPowersQuad = new int[] { 8, 4, 2, 1 };
        // table of cases for Quad faces (1 to 14, case 0 and 15 leave the face empty)
        // cases are the indexes of the connected edges (see Marching Squares algorithm)
        private readonly int[][] casesQuad = new int[][] { new int[] { 2, 3 }, new int[] { 1, 2 }, new int[] { 1, 3 }, new int[] { 0, 1 }, new int[] { 0,1,2,3 }, new int[] { 0, 2}, new int[] { 0, 3 },
    new int[] { 0, 3 }, new int[] { 0 , 2 }, new int[] { 0, 1, 2, 3 }, new int[] { 0 , 1 }, new int[] { 1, 3 }, new int[] { 1, 2 }, new int[] { 2, 3 }};
        // Tris
        // from MSB (Most Significant Bit) to LSB (Least Significant Bit)
        private readonly int[] vPowersTri = new int[] { 4, 2, 1 };
        // table of cases for Tri faces (1 to 6, case 0 and 7 leave the face empty)
        private readonly int[][] casesTri = new int[][] { new int[] { 1, 2 }, new int[] { 0, 1 }, new int[] { 0, 2 }, new int[] { 0, 2 }, new int[] { 0, 1 }, new int[] { 1, 2 } };

        /// <summary>
        /// Initializes a new instance of the MeshIsocurves class.
        /// </summary>
        public MeshIsocurvesScalar()
          : base("Mesh Isocurves Scalar", "f_MIsoSc",
              "Generates isocurves on Mesh from scalar values",
              "froGH", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The Colored Mesh", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mesh Scalar", "S", "Scalar values for the vertices.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Isovalue", "i", "The isovalue to search for (0-1)", GH_ParamAccess.item, 0.5);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Curve", "C", "Isovalue Curves as line segments", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh M = new Mesh();
            if (!DA.GetData(0, ref M)) return;
            if (!(M.IsValid) || M == null) return;
            List<double> scalarValues = new List<double>();
            if (!DA.GetDataList(1, scalarValues)) return;
            double iso = 0;
            if (!DA.GetData(2, ref iso)) return;

            // if scalar values do not match number of vertices throw an error
            if (M.Vertices.Count != scalarValues.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of values does not match number of Mesh vertices");
                return;
            }

            double[] normalizedValues = NormalizeValues(scalarValues);

            List<Line> isoLines = ToLineList(IsoCurvesOnMesh(M, normalizedValues, iso));

            DA.SetDataList(0, isoLines);
        }

        double[] NormalizeValues(List<double> values)
        {
            double[] normalizedValues = new double[values.Count];
            List<double> sortedValues = new List<double>(values);

            // find values domain
            sortedValues.Sort();

            double denominator = 1 / (sortedValues[sortedValues.Count - 1] - sortedValues[0]);
            // convert values
            //for (int i = 0; i < values.Count; i++)
            Parallel.For(0, values.Count, i =>
            {
                normalizedValues[i] = ((values[i] - sortedValues[0]) * denominator);
            });

            return normalizedValues;
        }

        Line[][] IsoCurvesOnMesh(Mesh M, double[] normVValues, double iso)
        {
            // Variables
            int[] isoStatus = new int[M.Vertices.Count];

            // find vertex iso status (0 = below or equal to iso, 1 = above iso)
            Parallel.For(0, M.Vertices.Count, i =>
            {
                isoStatus[i] = Convert.ToInt32(normVValues[i] > iso);
            });

            Line[][] LineSegmentsArray = new Line[M.Faces.Count][];

            // Performing algorithm
            Parallel.For(0, M.Faces.Count, i =>
            //for (int i = 0; i < M.Faces.Count; i++)
            {
                if (M.Faces[i].IsQuad)
                {
                    // Vertices indexes by face
                    int[] ind = new int[4];

                    ind[0] = M.Faces[i].A;
                    ind[1] = M.Faces[i].B;
                    ind[2] = M.Faces[i].C;
                    ind[3] = M.Faces[i].D;

                    // find face case in marching squares cases
                    int caseIndex = 0;
                    for (int j = 0; j < 4; j++)
                        caseIndex += vPowersQuad[j] * isoStatus[ind[j]];

                    // excluding null cases
                    if (caseIndex > 0 && caseIndex < 15)
                    {
                        // if in a saddle case....
                        if (caseIndex == 5 || caseIndex == 10)
                        {
                            LineSegmentsArray[i] = new Line[2];
                            // find value in face center as corner values average
                            double centerValue = 0;
                            for (int j = 0; j < 4; j++)
                                centerValue += normVValues[ind[j]];

                            centerValue *= 0.25;
                            int centercase = Convert.ToInt32(centerValue > iso);

                            Point3d a, b, c, d;
                            a = LerpPoint(M.Vertices[ind[0]], M.Vertices[ind[1]], normVValues[ind[0]], normVValues[ind[1]], iso);
                            b = LerpPoint(M.Vertices[ind[1]], M.Vertices[ind[2]], normVValues[ind[1]], normVValues[ind[2]], iso);
                            c = LerpPoint(M.Vertices[ind[2]], M.Vertices[ind[3]], normVValues[ind[2]], normVValues[ind[3]], iso);
                            d = LerpPoint(M.Vertices[ind[3]], M.Vertices[ind[0]], normVValues[ind[3]], normVValues[ind[0]], iso);

                            if ((caseIndex == 5 && centercase == 1) || (caseIndex == 10 && centercase == 0))
                            {
                                // connect edges 0-3, 1-2
                                LineSegmentsArray[i][0] = new Line(a, d);
                                LineSegmentsArray[i][1] = new Line(b, c);
                            }
                            else
                            {
                                // connect edges 0-1, 2-3
                                LineSegmentsArray[i][0] = new Line(a, b);
                                LineSegmentsArray[i][1] = new Line(c, d);
                            }
                        }
                        // otherwise (only one line to add)
                        else
                        {
                            int[] edgeIndexes = casesQuad[caseIndex - 1];
                            Point3d[] lineEnds = new Point3d[2];
                            int indA, indB;
                            for (int j = 0; j < 2; j++)
                            {
                                indA = ind[edgeIndexes[j]];
                                indB = ind[(edgeIndexes[j] + 1) % 4];
                                lineEnds[j] = LerpPoint(M.Vertices[indA], M.Vertices[indB], normVValues[indA], normVValues[indB], iso);
                            }

                            LineSegmentsArray[i] = new Line[1];
                            LineSegmentsArray[i][0] = new Line(lineEnds[0], lineEnds[1]);
                        }
                    }
                }
                else if (M.Faces[i].IsTriangle)
                {
                    // Vertices indexes by face
                    int[] ind = new int[3];

                    ind[0] = M.Faces[i].A;
                    ind[1] = M.Faces[i].B;
                    ind[2] = M.Faces[i].C;

                    // find face case in marching triangles cases
                    int caseIndex = 0;
                    for (int j = 0; j < 3; j++)
                        caseIndex += vPowersTri[j] * isoStatus[ind[j]];

                    // excluding null cases
                    if (caseIndex > 0 && caseIndex < 7)
                    {
                        int[] edgeIndexes = casesTri[caseIndex - 1];
                        Point3d[] lineEnds = new Point3d[2];
                        int indA, indB;
                        for (int j = 0; j < 2; j++)
                        {
                            indA = ind[edgeIndexes[j]];
                            indB = ind[(edgeIndexes[j] + 1) % 3];
                            lineEnds[j] = LerpPoint(M.Vertices[indA], M.Vertices[indB], normVValues[indA], normVValues[indB], iso);
                        }

                        LineSegmentsArray[i] = new Line[1];
                        LineSegmentsArray[i][0] = new Line(lineEnds[0], lineEnds[1]);
                    }
                }

            });
            return LineSegmentsArray;
        }

        Point3d LerpPoint(Point3d a, Point3d b, double t)
        {
            return a + (b - a) * t;
        }

        Point3d LerpPoint(Point3d a, Point3d b, double va, double vb, double x)
        {
            double t = (x - va) / (vb - va);
            return LerpPoint(a, b, t);
        }


        List<GH_Line> ToGHLineList(Line[][] LineJaggedArray)
        {
            List<GH_Line> lines = new List<GH_Line>();
            for (int i = 0; i < LineJaggedArray.Length; i++)
                if (LineJaggedArray[i] != null)
                    for (int j = 0; j < LineJaggedArray[i].Length; j++)
                        lines.Add(new GH_Line(LineJaggedArray[i][j].From.X, LineJaggedArray[i][j].From.Y, LineJaggedArray[i][j].From.Z,
                            LineJaggedArray[i][j].To.X, LineJaggedArray[i][j].To.Y, LineJaggedArray[i][j].To.Z));
            return lines;
        }

        List<Line> ToLineList(Line[][] LineJaggedArray)
        {
            List<Line> lines = new List<Line>();
            for (int i = 0; i < LineJaggedArray.Length; i++)
                if (LineJaggedArray[i] != null) lines.AddRange(LineJaggedArray[i]);
            return lines;
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
                return Resources.MeshIsocurvesScalar_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2C46F59B-DAD2-4F74-82F6-355B8861F11F"); }
        }
    }
}