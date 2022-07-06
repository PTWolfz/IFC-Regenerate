using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IFC_Regenerate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class Regenerate : IExternalCommand
    {
        Regex regexNo = new Regex(@"\d*\b");
        Regex regexIFC = new Regex(@"\bIFC\w*");
        Regex regexDecimal = new Regex(@"-*?\d*?\.\d*");
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            string filePath = @"D:\PTT\05. Download\Document Test\project.ifc";

            IfcFile ifcFile = new IfcFile(filePath, doc);
            List<IfcElement> ifcElements = ifcFile.IfcElements;

            using (Transaction regenerateTrans = new Transaction(doc, "Ifc Regenerate"))
            {
                regenerateTrans.Start();
                foreach (IfcElement ifcElem in ifcElements)
                {
                    if (ifcElem.ECategory==ECategory.StructuralFoundations||ifcElem is IfcSlab)
                    {
                        createFoundation(ifcElem, doc);
                    }
                    else if (ifcElem.ECategory==ECategory.StructuralColumns||ifcElem is IfcColumn)
                    {
                        createColumn(ifcElem , doc);
                    }
                    else if (ifcElem is IfcBeam)
                    {
                        createBeam(ifcElem as IfcBeam, doc);
                    }
                }
                regenerateTrans.Commit();
            }
            return Result.Succeeded;
        }
        void createFoundation(IfcElement ifcSlab, Autodesk.Revit.DB.Document doc)
        {
            FamilySymbol foundationSlab = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).
                OfCategory(BuiltInCategory.OST_StructuralFoundation).Cast<FamilySymbol>().
                Where(x => x.Name==ifcSlab.TypeName).
                Where(x => x.FamilyName == ifcSlab.FamilyName).First();
            XYZ coordinate = ifcSlab.IfcProductDefinitionShape.IfcShapeRepresentaion.IfcExtrudedAreaSolid.IfcAxis2Placement3D.IfcCatesianPoint.XYZ;
            XYZ convertedCoordinate = new XYZ(coordinate.X._MmToFeet(), coordinate.Y._MmToFeet(),
                coordinate.Z._MmToFeet()+foundationSlab.LookupParameter("Foundation Thickness").AsDouble());
            doc.Create.NewFamilyInstance(convertedCoordinate, foundationSlab, ifcSlab.Level, Autodesk.Revit.DB.Structure.StructuralType.Footing).LookupParameter("Mark").
                Set(ifcSlab.ElementId.ToString());

        }
        void createColumn(IfcElement ifcColumn, Autodesk.Revit.DB.Document doc)
        {
            FamilySymbol columnFam = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).
               OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilySymbol>().
               Where(x => x.Name==ifcColumn.TypeName).
               Where(x => x.FamilyName == ifcColumn.FamilyName).First();
            XYZ coordinate = ifcColumn.IfcProductDefinitionShape.IfcShapeRepresentaion.IfcMappedItem.IfcRepresentaionMap.IfcShapeRepresentation.IfcExtrudedAreaSolid.IfcAxis2Placement3D.IfcCatesianPoint.XYZ;
                 XYZ convertedCoordinate = new XYZ(coordinate.X._MmToFeet(), coordinate.Y._MmToFeet(), coordinate.Z._MmToFeet());
            doc.Create.NewFamilyInstance(convertedCoordinate, columnFam, ifcColumn.Level, Autodesk.Revit.DB.Structure.StructuralType.Column).LookupParameter("Mark").
                Set(ifcColumn.ElementId.ToString());
        }
        void createBeam(IfcBeam ifcBeam, Autodesk.Revit.DB.Document doc)
        {

        }
    }
}

