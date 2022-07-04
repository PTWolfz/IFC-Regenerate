using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IFC_Regenerate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class Regenerate : IExternalCommand
    {
        Regex regexNo = new Regex(@"\w*\b");
        Regex regexIFC = new Regex(@"\bIFC\w*");
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            string filePath = @"D:\PTT\05. Download\Document Test\project.ifc";
            string ifcString = System.IO.File.ReadAllText(filePath);
            string ifcData = ifcString.Split(new string[] { "DATA;\r\n" }, StringSplitOptions.None)[1];
            List<string> ifcLines = ifcData.Split(new string[] { ";\r\n" }, StringSplitOptions.None).ToList();

            
          

            //   MessageBox.Show(regex.Match(ifcLines.First()).ToString());
            //DatabaseIfc db = new DatabaseIfc(filePath);
            //IfcProject project = db.Project;
            //List<IfcBuiltElement> elems = project.Extract<IfcBuiltElement>();
            //using(Transaction regenerate=new Transaction(doc,"Regenerate"))
            //{
            //    regenerate.Start();
            //    foreach (IfcBuiltElement element in elems)
            //    {
            //        if (element is IfcSlab)
            //        {
            //            IfcSlab ifcSlab = (IfcSlab)element;
            //             createFoundation(element as IfcSlab, doc);
            //        }
            //        else if (element is IfcBeam)
            //        {

            //        }
            //        else if (element is IfcColumn)
            //        {

            //        }
            //    }
            //    regenerate.Commit();
            //}


            return Result.Succeeded;
        }
        void findIFCSlab(List<string> ifcLines)
        {
            List<string> ifcSlabItems = ifcLines.Where(x => regexIFC.Match(x).ToString()=="IFCSLAB").ToList();
            
        }
        void createFoundation(string familyName, string typeName, XYZ coordinate, Autodesk.Revit.DB.Document doc)
        {
            Level level = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().First();
            FamilySymbol foundationSlab = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_StructuralFoundation).
                Cast<FamilySymbol>().Where(x => x.Name==typeName).Where(x => x.FamilyName == familyName).First();
            doc.Create.NewFamilyInstance(coordinate, foundationSlab, level, Autodesk.Revit.DB.Structure.StructuralType.Footing);
        }
        void createColumn()
        {
        }
        void createBeam()
        {

        }
    }
    class IFCSlab
    {
        public IFCSlab(string typeName, string familyName, XYZ coordinate)
        {
            TypeName=typeName;
            FamilyName=familyName;
            Coordinate=coordinate;
        }
        string typeName;
        public string TypeName { get { return typeName; } set { typeName = value; } }
        string familyName;
        public string FamilyName { get { return familyName; } set { familyName = value; } }
        XYZ coordinate;
        public XYZ Coordinate { get { return coordinate; } set { coordinate = value; } }
    }
}
