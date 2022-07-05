using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
            List<IfcElement> ifcElements = ifcFileAnalyst(filePath);
            assignLevel(ifcElements, filePath, doc);
            using (Transaction regenerateTrans = new Transaction(doc, "Ifc Regenerate"))
            {
                regenerateTrans.Start();
                foreach (IfcElement ifcElem in ifcElements)
                {
                    if (ifcElem is IfcSlab)
                    {
                        createFoundation(ifcElem as IfcSlab, doc);
                    }
                    else if (ifcElem is IfcColumn)
                    {
                        createColumn(ifcElem as IfcColumn, doc);
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
        List<IfcElement> ifcFileAnalyst(string filePath)
        {
            List<IfcElement> ifcElements = new List<IfcElement>();
            string ifcString = System.IO.File.ReadAllText(filePath);
            string ifcData = ifcString.Split(new string[] { "DATA;\r\n" }, StringSplitOptions.None)[1];
            List<string> ifcLines = ifcData.Split(new string[] { ";\r\n" }, StringSplitOptions.None).ToList();

            int i = 0;
            while (i<ifcLines.Count())
            {
                string ifcContent = ifcLines[i];
                if (regexIFC.Match(ifcContent).ToString()=="IFCSLAB")
                {
                    int j = i;
                    XYZ coordinate;
                    while (true)
                    {
                        j--;
                        string slabContent = ifcLines[j];
                        if (regexIFC.Match(slabContent).ToString()=="IFCAXIS2PLACEMENT3D")
                        {
                            //Skip if not suitable
                            if (regexNo.Matches(slabContent)._ToListString().Count!=4) { continue; }

                            string coordinateNo = regexNo.Matches(slabContent)._ToListString()[1].ToString();

                            string coordinateContent = ifcLines.First(
                              x => regexNo.Match(x).ToString()==coordinateNo);


                            coordinate=new XYZ(double.Parse(regexDecimal.Matches(coordinateContent)[0].ToString())._MmToFeet(),
                                double.Parse(regexDecimal.Matches(coordinateContent)[1].ToString())._MmToFeet(),
                               0);
                            break;
                        }
                    }
                    string ifcInfo = ifcContent.Split('\'')[3];
                    IfcSlab ifcSlab = new IfcSlab(int.Parse(ifcInfo.Split(':')[2]), ifcInfo.Split(':')[1],
                        ifcInfo.Split(':')[0], coordinate);
                    ifcElements.Add(ifcSlab);
                    i++;
                }
                else if (regexIFC.Match(ifcContent).ToString()=="IFCCOLUMN")
                {
                    XYZ coordinate = null;
                    double height = 0;
                    int j = i;
                    while (true)
                    {
                        j--;
                        string slabContent = ifcLines[j];
                        if (coordinate==null&&regexIFC.Match(slabContent).ToString()=="IFCAXIS2PLACEMENT3D")
                        {
                            //Skip if not suitable
                            if (regexNo.Matches(slabContent)._ToListString().Count!=4) { continue; }
                            //if (regexNo.Matches(slabContent)._ToListString()[1]=="6") { continue; }

                            string coordinateNo = regexNo.Matches(slabContent)._ToListString()[1].ToString();

                            string coordinateContent = ifcLines.First(
                              x => regexNo.Match(x).ToString()==coordinateNo);



                            coordinate=new XYZ(double.Parse(regexDecimal.Matches(coordinateContent)[0].ToString())._MmToFeet(),
                                double.Parse(regexDecimal.Matches(coordinateContent)[1].ToString())._MmToFeet(),
                               0);
                        }
                        else if (height==0&&regexIFC.Match(slabContent).ToString()=="IFCEXTRUDEDAREASOLID")
                        {
                            height=double.Parse(regexDecimal.Match(slabContent).ToString());
                        }
                        if (coordinate!=null&&height!=0)
                        {
                            break;
                        }
                    }
                    string ifcInfo = ifcContent.Split('\'')[3];
                    IfcColumn ifcColumn = new IfcColumn(int.Parse(ifcInfo.Split(':')[2]), ifcInfo.Split(':')[1],
                        ifcInfo.Split(':')[0], coordinate, height);
                    ifcElements.Add(ifcColumn);
                    i++;
                }
                else if (regexIFC.Match(ifcContent).ToString()=="IFCBEAM")
                {
                    i++;
                }
                else
                {
                    i++;
                }
            }
            return ifcElements;
        }
        void assignLevel(List<IfcElement> ifcElements, string filePath, Document doc)
        {
            string ifcString = System.IO.File.ReadAllText(filePath);
            string ifcData = ifcString.Split(new string[] { "DATA;\r\n" }, StringSplitOptions.None)[1];
            List<string> ifcLines = ifcData.Split(new string[] { ";\r\n" }, StringSplitOptions.None).ToList();

            List<Level> levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            Regex regexSharpNo = new Regex(@"#\d*\b");
            List<string> spatialContents = ifcLines.Where(x => regexIFC.Match(x).ToString()=="IFCRELCONTAINEDINSPATIALSTRUCTURE").ToList();
            foreach (string spatialContent in spatialContents)
            {
                List<string> lineNumbers = regexSharpNo.Matches(spatialContent)._ToListString().Select(x => x.Substring(1, x.Length-1)).ToList();
                string levelNo = lineNumbers.Last();
                string levelContent = ifcLines.First(x => regexNo.Match(x).ToString()==levelNo);
                Level level = levels.First(x => x.Name==levelContent.Split('\'')[3]);
                for(int i=2;i<lineNumbers.Count()-1;i++)
                {
                    string lineNumber = lineNumbers[i];
                    string objectContent = ifcLines.First(x => regexNo.Match(x).ToString()==lineNumber);
                    string objectId= objectContent.Split('\'')[7];
                    try
                    {
                        ifcElements.First(x => x.ElementId.ToString()==objectId).Level=level;
                    }
                    catch (Exception)
                    {
                    }
                  
                }
            }
        }
        void createFoundation(IfcSlab ifcSlab, Autodesk.Revit.DB.Document doc)
        {
            FamilySymbol foundationSlab = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).
                OfCategory(BuiltInCategory.OST_StructuralFoundation).Cast<FamilySymbol>().
                Where(x => x.Name==ifcSlab.TypeName).
                Where(x => x.FamilyName == ifcSlab.FamilyName).First();
            doc.Create.NewFamilyInstance(ifcSlab.Coordinate, foundationSlab,ifcSlab.Level, Autodesk.Revit.DB.Structure.StructuralType.Footing).LookupParameter("Mark").
                Set(ifcSlab.ElementId.ToString());
     
        }
        void createColumn(IfcColumn ifcColumn, Autodesk.Revit.DB.Document doc)
        {
            //Level level = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().ToList().
            //    First(x => x.LookupParameter("Elevation").AsDouble()-ifcColumn.Coordinate.Z==0.00);
            FamilySymbol columnFam = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).
               OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilySymbol>().
               Where(x => x.Name==ifcColumn.TypeName).
               Where(x => x.FamilyName == ifcColumn.FamilyName).First();
            doc.Create.NewFamilyInstance(ifcColumn.Coordinate, columnFam, ifcColumn.Level, Autodesk.Revit.DB.Structure.StructuralType.Column).LookupParameter("Mark").
                Set(ifcColumn.ElementId.ToString());
        }
        void createBeam(IfcBeam ifcBeam, Autodesk.Revit.DB.Document doc)
        {

        }
    }
}

