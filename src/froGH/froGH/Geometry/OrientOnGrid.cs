using froGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace froGH
{
    public class OrientOnGrid : GH_Component
    {

        private bool rowFirst;

        /// <summary>
        /// Initializes a new instance of the SortOnGrid class.
        /// </summary>
        public OrientOnGrid()
          : base("OrientOnGrid", "f_OrientGrid",
              "Orient Geometries along a grid\nGrid cell size is computed from the largest Bounding Box X & Y sizes",
              "froGH", "Geometry")
        {
            rowFirst = true;
            UpdateMessage();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to sort", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Base Plane", "P", "Base Plane for Grid", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddIntegerParameter("Primary Dimension items", "Di", "Number of Rows/Columns for the Grid", GH_ParamAccess.item, 5);
            pManager.AddNumberParameter("X Scale", "Xs", "% Scale of cell along X\nmust be >= 1", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Y Scale", "Ys", "% Scale of cell along Y\nmust be >= 1", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Row First", "Rf", "Row First (true) or Column First (false) layout order", GH_ParamAccess.item, true);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Oriented Geometry", "G", "The Geometry oriented along the grid cells", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Base Planes", "P", "Base Planes  at Cell Centers", GH_ParamAccess.list);
            pManager.AddVectorParameter("Cell Size", "Cs", "Cell Size as vector", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IGH_GeometricGoo> geometry = new List<IGH_GeometricGoo>();
            IGH_GeometricGoo[] gridGeometry = new IGH_GeometricGoo[geometry.Count];

            // Get Geometry
            if (!DA.GetDataList(0, geometry)) return;
            // initialize array for parallel processing
            gridGeometry = geometry.Select(g => g=null).ToArray();

            // Get other data
            Plane basePlane = Plane.WorldXY;
            DA.GetData("Base Plane", ref basePlane);
            int primaryDimension = 5;// formerly nRows
            DA.GetData("Primary Dimension items", ref primaryDimension);
            double xScale = 1.0, yScale = 1.0;
            DA.GetData("X Scale", ref xScale);
            DA.GetData("Y Scale", ref yScale);
            xScale = Math.Max(xScale, 1.0);
            yScale = Math.Max(yScale, 1.0);
            //bool rowFirst = false;
            DA.GetData("Row First", ref rowFirst);
            UpdateMessage();

            // Compute grid cell size
            Vector3d cellSize = Vector3d.Zero;
            Vector3d geometrySize;
            BoundingBox[] boundingBoxes = new BoundingBox[geometry.Count];
            BoundingBox boundingBox;
            for (int i = 0; i < geometry.Count; i++)
            {
                boundingBox = geometry[i].Boundingbox;
                boundingBoxes[i] = boundingBox;
                geometrySize = boundingBox.Diagonal;
                if (geometrySize.X > cellSize.X) cellSize.X = geometrySize.X;
                if (geometrySize.Y > cellSize.Y) cellSize.Y = geometrySize.Y;
            }
            cellSize.X *= xScale;
            cellSize.Y *= yScale;
            // Offset cell center so that the base plane is at the lower-left corner of the cell
            Vector3d cellOffset = cellSize * 0.5;
            basePlane.Transform(Transform.Translation(Vector3d.Add(basePlane.XAxis * cellOffset.X, basePlane.YAxis * cellOffset.Y)));
            
            // Compute destination planes & orient geometry
            Plane[] destinationPlanes = new Plane[geometry.Count];
            Parallel.For(0, geometry.Count, i =>
            {
                int rowIndex = i % primaryDimension;
                int columnIndex = (i - rowIndex) / primaryDimension;
                int primaryIndex, secondaryIndex;
                if (rowFirst) 
                {
                    primaryIndex = rowIndex;
                    secondaryIndex = columnIndex;
                } 
                else
                {
                    primaryIndex = columnIndex;
                    secondaryIndex = rowIndex;
                }
                Plane currentPlane = basePlane.Clone();
                currentPlane.Transform(Transform.Translation(Vector3d.Add(currentPlane.XAxis * cellSize.X * secondaryIndex, currentPlane.YAxis * cellSize.Y * primaryIndex)));
                destinationPlanes[i] = currentPlane;
                Plane geometryPlane = Plane.WorldXY;
                geometryPlane.Origin = boundingBoxes[i].PointAt(0.5, 0.5, 0);
                gridGeometry[i] = geometry[i].DuplicateGeometry().Transform(Transform.PlaneToPlane(geometryPlane, currentPlane));
            });

            // Output data
            DA.SetDataList("Oriented Geometry", gridGeometry);
            DA.SetDataList("Base Planes", destinationPlanes);
            DA.SetData("Cell Size", cellSize);
        }

        private void UpdateMessage()
        {
            Message = rowFirst ? "Row First" : "Column First";
        }

        /// <summary>
        /// Exposure override for position in the Subcategory (options primary to septenary)
        /// https://apidocs.co/apps/grasshopper/6.8.18210/T_Grasshopper_Kernel_GH_Exposure.htm
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
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
                return Resources.OrientOnGrid_GH;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("CDCF217A-0B15-4B77-8921-6157BD944F53"); }
        }
    }
}