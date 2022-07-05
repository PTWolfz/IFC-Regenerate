//using Autodesk.Revit.ApplicationServices;
//using Autodesk.Revit.Attributes;
//using Autodesk.Revit.Creation;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
//using Autodesk.Revit.UI.Selection;
//using GeometryGym.Ifc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace IFC_Regenerate
//{
//    [Transaction(TransactionMode.Manual)]
//    [Regeneration(RegenerationOption.Manual)]
//    [Journaling(JournalingMode.NoCommandData)]
//    public class Regenerate_GeoGym : IExternalCommand
//    {
//        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
//        {
//            UIApplication uiapp = commandData.Application;
//            UIDocument uidoc = uiapp.ActiveUIDocument;
//            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
//            Autodesk.Revit.DB.Document doc = uidoc.Document;
//            Selection sel = uidoc.Selection;

//            string filePath = @"D:\PTT\05. Download\Document Test\project.ifc";
//            //string fileName = @"C:\Users\ptthanh\Documents\Project1.ifc";
//            DatabaseIfc db = new DatabaseIfc(filePath);
//            IfcProject project = db.Project;
//            List<IfcBuiltElement> elems = project.Extract<IfcBuiltElement>();
//            using(Transaction regenerate=new Transaction(doc,"Regenerate"))
//            {
//                regenerate.Start();
//                foreach (IfcBuiltElement element in elems)
//                {
//                    if (element is IfcSlab)
//                    {
//                        IfcSlab ifcSlab = (IfcSlab)element;
//                         createFoundation(element as IfcSlab, doc);
//                    }
//                    else if (element is IfcBeam)
//                    {

//                    }
//                    else if (element is IfcColumn)
//                    {

//                    }
//                }
//                regenerate.Commit();
//            }
      

//            return Result.Succeeded;
//        }
//        void createFoundation(IfcSlab ifcSlab, Autodesk.Revit.DB.Document doc)
//        {
//            List<XYZ> coordinations = new List<XYZ>();
//            List<IfcCartesianPoint> locations = new List<IfcCartesianPoint>();
//            List<IfcAxis2Placement3D> placements = new List<IfcAxis2Placement3D>();
//            FamilySymbol foundationSlab = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFoundation).
//                Cast<FamilySymbol>().First(x => x.Name=="2400 x 1800 x 900mm");
//            Level level1 = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().First();
//            foreach (IfcObjectPlacement ifcObjectPlacement in ifcSlab.ObjectPlacement.PlacementRelTo.ReferencedByPlacements)
//            {
//                IfcLocalPlacement ifcLocalPlacement = ifcObjectPlacement as IfcLocalPlacement;
//                IfcAxis2Placement3D ifcAxis2Placement3D = ifcLocalPlacement.RelativePlacement as IfcAxis2Placement3D;
//                placements.Add(ifcAxis2Placement3D);
//                locations.Add(ifcAxis2Placement3D.Location);
//                XYZ coordinate = new XYZ(ifcAxis2Placement3D.Location.CoordinateX._MmToFeet(), 
//                    ifcAxis2Placement3D.Location.CoordinateY._MmToFeet(), 
//                    ifcAxis2Placement3D.Location.CoordinateZ._MmToFeet());
//                coordinations.Add(coordinate);
//                try
//                {
//                    doc.Create.NewFamilyInstance(coordinate, foundationSlab, level1, Autodesk.Revit.DB.Structure.StructuralType.Footing);
//                }
//                catch (Exception ex)
//                {
//                }
//            }
//        }
//    void createColumn(IfcColumn ifcColumn)
//    {
//        string objectType = ifcColumn.ObjectType;
//    }
//    void createBeam()
//    {

//    }
//}
//}
