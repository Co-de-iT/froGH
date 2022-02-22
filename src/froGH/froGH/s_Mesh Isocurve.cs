using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

// <Custom using>
using System.Drawing;
using System.Threading.Tasks;
// </Custom using>


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
    #region Utility functions
    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
    /// <param name="text">String to print.</param>
    private void Print(string text) { __out.Add(text); }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments, 
    /// Output parameters as ref arguments. You don't have to assign output parameters, 
    /// they will have a default value.
    /// </summary>
    private void RunScript(Mesh M, int ch, double iso, ref object C)
    {
        // <Custom code>

        /*
        froGH - a sparse collection of (in)utilities for Grasshopper

        Mesh IsoCurves
        an interpretation of the marching squares algorithm to get isocurves on a quad mesh

        code by Alessio Erioli

        froGH by Co-de-iT is licensed under a Creative Commons Attribution 4.0 International License.
        http://creativecommons.org/licenses/by/4.0/

        */

        initComp();

        // return on null
        if (M == null) return;
        // if Mesh has no colors or if not all faces are quads throw an error
        if (M.VertexColors.Count == 0)
        {
            Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh must have vertex colors");
            return;
        }

        if (M.Faces.QuadCount < M.Faces.Count)
        {
            Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh must have only quad faces");
            return;
        }

        C = ToLineList(isoCurvesOnColoredMesh(M, ch, iso));

        // </Custom code>
    }

    // <Custom additional code> 

    // constants and LUTs
    private readonly int[] vPowers = new int[] { 8, 4, 2, 1 }; // from MSB (Most Significant Bit) to LSB (Least Significant Bit)
    // table of cases (1 to 14, case 0 and 15 leave the face empty)
    // cases are the indexes of the connected edges (see Marching Squares algorithm)
    private readonly int[][] cases = new int[][] { new int[] { 2, 3 }, new int[] { 1, 2 }, new int[] { 1, 3 }, new int[] { 0, 1 }, new int[] { 0,1,2,3 }, new int[] { 0, 2}, new int[] { 0, 3 },
        new int[] { 0, 3 }, new int[] { 0 , 2 }, new int[] { 0, 1, 2, 3 }, new int[] { 0 , 1 }, new int[] { 1, 3 }, new int[] { 1, 2 }, new int[] { 2, 3 }};


    void initComp()
    {
        Component.Name = "Mesh isoCurve";
        Component.NickName = "f_MIso";
        Component.Description = "Generates isocurves on colored quad meshes";
        Component.Params.Input[0].Name = "Mesh";
        Component.Params.Input[0].Description = "The colored quad Mesh";
        Component.Params.Input[1].Name = "Channel";
        Component.Params.Input[1].Description = "The color channel to use: 0-R, 1-G, 2-B";
        Component.Params.Input[2].Name = "Isovalue";
        Component.Params.Input[2].Description = "The isovalue to search for (0-1)";
        Component.Params.Output[0].Name = "Curves";
        Component.Params.Output[0].Description = "Isovalue Curves as line segments";
    }

    Line[][] isoCurvesOnColoredMesh(Mesh M, int channel, double iso)
    {
        // Variables
        bool[] nakedStatus = M.GetNakedEdgePointStatus();
        int[] isoStatus = new int[M.Vertices.Count];
        double[] normVValues = new double[M.Vertices.Count];


        // compile vertices values and status
        Parallel.For(0, M.Vertices.Count, i =>
        {
            // store normalized color values per vertex
            Color c = M.VertexColors[i];
            switch (channel)
            {
                case 0:
                    normVValues[i] = c.R / 255.0;
                    break;
                case 1:
                    normVValues[i] = c.G / 255.0;
                    break;
                case 2:
                    normVValues[i] = c.B / 255.0;
                    break;
                default:
                    goto case 1;
            }
            // find vertex iso status (0 = below iso, 1 = above iso)
            isoStatus[i] = Convert.ToInt32(normVValues[i] > iso);

        });

        //DataTree<Line> LineSegments = new DataTree<Line>();
        Line[][] LineSegmentsArray = new Line[M.Faces.Count][];

        // Performing algorithm
        Parallel.For(0, M.Faces.Count, i =>
        //for (int i = 0; i < M.Faces.Count; i++)
        {

            //List<Line> faceIsolines = new List<Line>();

            // Vertices indexes by face
            int[] ind = new int[4];

            ind[0] = M.Faces[i].A;
            ind[1] = M.Faces[i].B;
            ind[2] = M.Faces[i].C;
            ind[3] = M.Faces[i].D;

            // find face case
            int caseIndex = 0; // marching squares case
            for (int j = 0; j < 4; j++)
                caseIndex += vPowers[j] * isoStatus[ind[j]];

            //caseIndexes[i] = caseIndex;
            if (caseIndex > 0 && caseIndex < 15) // excluding null cases
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
                    int[] edgeIndexes = cases[caseIndex - 1];
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


    List<GH_Line> ToLineList(Line[][] LineJaggedArray)
    {
        List<GH_Line> lines = new List<GH_Line>();
        for (int i = 0; i < LineJaggedArray.Length; i++)
            if (LineJaggedArray[i] != null)
                for (int j = 0; j < LineJaggedArray[i].Length; j++)
                    lines.Add(new GH_Line(LineJaggedArray[i][j]));
        return lines;
    }


    // </Custom additional code> 

    private List<string> __err = new List<string>(); //Do not modify this list directly.
    private List<string> __out = new List<string>(); //Do not modify this list directly.
    private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
    private IGH_ActiveObject owner;                  //Legacy field.
    private int runCount;                            //Legacy field.

    public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
    {
        //Prepare for a new run...
        //1. Reset lists
        this.__out.Clear();
        this.__err.Clear();

        this.Component = owner;
        this.Iteration = iteration;
        this.GrasshopperDocument = owner.OnPingDocument();
        this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

        this.owner = this.Component;
        this.runCount = this.Iteration;
        this.doc = this.RhinoDocument;

        //2. Assign input parameters
        Mesh M = default(Mesh);
        if (inputs[0] != null)
        {
            M = (Mesh)(inputs[0]);
        }

        int channel = default(int);
        if (inputs[1] != null)
        {
            channel = (int)(inputs[1]);
        }

        double iso = default(double);
        if (inputs[2] != null)
        {
            iso = (double)(inputs[2]);
        }



        //3. Declare output parameters
        object isoCurve = null;


        //4. Invoke RunScript
        RunScript(M, channel, iso, ref isoCurve);

        try
        {
            //5. Assign output parameters to component...
            if (isoCurve != null)
            {
                if (GH_Format.TreatAsCollection(isoCurve))
                {
                    IEnumerable __enum_isoCurve = (IEnumerable)(isoCurve);
                    DA.SetDataList(1, __enum_isoCurve);
                }
                else
                {
                    if (isoCurve is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(isoCurve));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(1, isoCurve);
                    }
                }
            }
            else
            {
                DA.SetData(1, null);
            }

        }
        catch (Exception ex)
        {
            this.__err.Add(string.Format("Script exception: {0}", ex.Message));
        }
        finally
        {
            //Add errors and messages... 
            if (owner.Params.Output.Count > 0)
            {
                if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
                {
                    List<string> __errors_plus_messages = new List<string>();
                    if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
                    if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
                    if (__errors_plus_messages.Count > 0)
                        DA.SetDataList(0, __errors_plus_messages);
                }
            }
        }
    }
}