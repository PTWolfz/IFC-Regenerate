using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IFC_Regenerate
{
    public class IfcFile
    {
        Regex regexNo = new Regex(@"\d*\b");
        Regex regexIFC = new Regex(@"\bIFC\w*");
        Regex regexDecimal = new Regex(@"-*?\d*?\.\d*");
        public IfcFile(string filePath, Document doc)
        {
            FilePath=filePath;
            IfcLines=new List<IfcLine>();
            string ifcString = System.IO.File.ReadAllText(filePath);
            string ifcData = ifcString.Split(new string[] { "DATA;\r\n" }, StringSplitOptions.None)[1];
            List<string> ifcLineContents = ifcData.Split(new string[] { ";\r\n" }, StringSplitOptions.None).ToList();
            foreach (string ifcLineContent in ifcLineContents)
            {
                if (ifcLineContent.Length>0&&ifcLineContent.Substring(0, 1)==@"#")
                {
                    IfcLine ifcLine = new IfcLine(ifcLineContent);
                    IfcLines.Add(ifcLine);
                }
            }
            IfcElements=getIfcElements();
            assignLevel(IfcElements, doc);
        }
        List<IfcElement> getIfcElements()
        {
            List<IfcElement> ifcElements = new List<IfcElement>();
            foreach (IfcLine ifcLine in IfcLines)
            {
                if (ifcLine.IfcTypeName=="IFCSLAB")
                {
                    IfcElement ifcElem = new IfcElement(ifcLine);
                    ifcElem.Category="Structural Foundations";
                    ifcElem.ECategory=ECategory.StructuralFoundations;
                    ifcElements.Add(ifcElem);
                }
                else if (ifcLine.IfcTypeName=="IFCCOLUMN")
                {
                    IfcElement ifcElem = new IfcElement(ifcLine);
                    ifcElem.Category="Structural Columns";
                    ifcElem.ECategory=ECategory.StructuralColumns;
                    ifcElements.Add(ifcElem);
                }
                else if (ifcLine.IfcTypeName=="IFCBEAM")
                {
                    IfcElement ifcElem = new IfcElement(ifcLine);
                    ifcElem.Category="Structural Framing";
                    ifcElem.ECategory=ECategory.StructuralFraming;
                    ifcElements.Add(ifcElem);
                }
            }
            return ifcElements;
        }
        void assignLevel(List<IfcElement> ifcElements, Document doc)
        {
            List<Level> levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().ToList();
            Regex regexSharpNo = new Regex(@"#\d*\b");
            List<IfcLine> spatialContents = IfcLines.Where(x => x.IfcTypeName=="IFCRELCONTAINEDINSPATIALSTRUCTURE").ToList();
            foreach (string spatialContent in spatialContents.Select(x => x.FullLine))
            {
                List<string> lineNumbers = regexSharpNo.Matches(spatialContent)._ToListString().Select(x => x.Substring(1, x.Length-1)).ToList();
                string levelNo = lineNumbers.Last();
                string levelContent = IfcLines.First(x => x.LineNo.ToString()==levelNo).FullLine;
                Level level = levels.First(x => x.Name==levelContent.Split('\'')[3]);
                for (int i = 2; i<lineNumbers.Count()-1; i++)
                {
                    string lineNumber = lineNumbers[i];
                    string objectContent = IfcLines.First(x => x.LineNo.ToString()==lineNumber).FullLine;
                    string objectId = objectContent.Split('\'')[7];
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
        public List<IfcElement> IfcElements;
        public static IfcLine ContentAtIndex(int index)
        {
            return IfcLines.FirstOrDefault(x => x.LineNo == index);
        }
        string filePath;
        public string FilePath { get { return filePath; } set { filePath = value; } }
        public static List<IfcLine> IfcLines;
    }
    public enum ECategory
    {
        StructuralFoundations,
        StructuralColumns,
        StructuralFraming
    }
    public class IfcLine
    {
        public IfcLine(string input)
        {
            Regex regexNo = new Regex(@"\d*\b");
            Regex regexSharpNo = new Regex(@"#\d*\b");
            Regex regexIFC = new Regex(@"\bIFC\w*");
            FullLine=input;
            LineNo=Int32.Parse(regexNo.Match(input).ToString());
            IfcTypeName=regexIFC.Match(input).ToString();
            LinesNo=regexSharpNo.Matches(input)._ToListString().Select(x => Int32.Parse(x.Substring(1, x.Length-1))).ToList();
        }
        string fullLine;
        public string FullLine { get { return fullLine; } set { fullLine = value; } }
        string ifcTypeName;
        public string IfcTypeName { get { return ifcTypeName; } set { ifcTypeName = value; } }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        List<int> linesNo;
        public List<int> LinesNo { get { return linesNo; } set { linesNo = value; } }
    }
    public class IfcElement
    {
        public IfcElement(string typeName, string familyName)
        {
            TypeName = typeName;
            FamilyName = familyName;
        }
        public IfcElement(IfcLine ifcLine)
        {
            string ifcInfo = ifcLine.FullLine.Split('\'')[3];
            TypeName = ifcInfo.Split(':')[1];
            FamilyName = ifcInfo.Split(':')[0];
            ElementId = int.Parse(ifcInfo.Split(':')[2]);
            LineNo=ifcLine.LineNo;
            IfcLocalPlacement = new IfcLocalPlacement(IfcFile.ContentAtIndex(ifcLine.LinesNo[2]));
            IfcProductDefinitionShape=new IfcProductDefinitionShape(IfcFile.ContentAtIndex(ifcLine.LinesNo[3]));
        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        string typeName;
        public string TypeName { get { return typeName; } set { typeName = value; } }
        string familyName;
        public string FamilyName { get { return familyName; } set { familyName = value; } }
        int elementId;
        public int ElementId { get { return elementId; } set { elementId = value; } }
        string category;
        public string Category { get { return category; } set { category = value; } }
        ECategory eCategory;
        public ECategory ECategory { get { return eCategory; } set { eCategory = value; } }
        Level level;
        public Level Level { get { return level; } set { level = value; } }
        IfcLocalPlacement ifcLocalPlacement;
        public IfcLocalPlacement IfcLocalPlacement { get { return ifcLocalPlacement; } set { ifcLocalPlacement = value; } }
        IfcProductDefinitionShape ifcProductDefinitionShape;
        public IfcProductDefinitionShape IfcProductDefinitionShape { get { return ifcProductDefinitionShape; } set { ifcProductDefinitionShape = value; } }
    }
    public class IfcLocalPlacement
    {
        public IfcLocalPlacement(IfcLine ifcLine)
        {
            LineNo=ifcLine.LineNo;
            IfcAxis2Placement3D=new IfcAxis2Placement3D(IfcFile.ContentAtIndex(ifcLine.LinesNo[2]));
        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        IfcLocalPlacement ifcLPlacement;
        public IfcLocalPlacement IfcLPlacement { get { return ifcLPlacement; } set { ifcLPlacement = value; } }
        IfcAxis2Placement3D ifcAxis2Placement3D;
        public IfcAxis2Placement3D IfcAxis2Placement3D { get { return ifcAxis2Placement3D; } set { ifcAxis2Placement3D = value; } }
    }
    public class IfcAxis2Placement3D
    {
        public IfcAxis2Placement3D(IfcLine ifcLine)
        {
            Regex regexNo = new Regex(@"\d*");
            LineNo = ifcLine.LineNo;
            IfcCatesianPoint=new IfcCatesianPoint(IfcFile.ContentAtIndex(ifcLine.LinesNo[1]));
        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        IfcCatesianPoint ifcCatesianPoint;
        public IfcCatesianPoint IfcCatesianPoint { get { return ifcCatesianPoint; } set { ifcCatesianPoint = value; } }
    }
    public class IfcCatesianPoint
    {
        public IfcCatesianPoint(IfcLine ifcLine)
        {
            Regex regexNo = new Regex(@"\d*");
            LineNo = ifcLine.LineNo;
            Regex regexDecimal = new Regex(@"-*?\d*?\.\d*");
            List<double> coordinates = regexDecimal.Matches(ifcLine.FullLine)._ToListString().Select(x => double.Parse(x)).ToList();
            if (coordinates.Count==3)
            {
                XYZ = new XYZ(coordinates[0], coordinates[1], coordinates[2]);
            }
            else if (coordinates.Count==2)
            {
                UV = new UV(coordinates[0], coordinates[1]);
            }

        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        XYZ xyz;
        public XYZ XYZ
        {
            get { return xyz; }
            set { xyz = value; }
        }
        UV uv;
        public UV UV
        {
            get { return uv; }
            set { uv = value; }
        }
    }
    public class IfcProductDefinitionShape
    {
        public IfcProductDefinitionShape(IfcLine ifcLine)
        {
            LineNo = ifcLine.LineNo;
            IfcShapeRepresentaion=new IfcShapeRepresentation(IfcFile.ContentAtIndex(ifcLine.LinesNo[1]));
        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        IfcShapeRepresentation ifcShapeRepresentaion;
        public IfcShapeRepresentation IfcShapeRepresentaion { get { return ifcShapeRepresentaion; } set { ifcShapeRepresentaion = value; } }
    }
    enum EIfcShapeRepresentation
    {
        SweptSolid,
        MappedRepresentation
    }
    public class IfcShapeRepresentation
    {
        public IfcShapeRepresentation(IfcLine ifcLine)
        {
            LineNo = ifcLine.LineNo;
            Regex text = new Regex(@"\w*\b");
            string type = text.Matches(ifcLine.FullLine)._ToListString()[4];
            if (type==EIfcShapeRepresentation.SweptSolid.ToString())
            {
                IfcExtrudedAreaSolid=new IfcExtrudedAreaSolid(IfcFile.ContentAtIndex(ifcLine.LinesNo[2]));
            }
            else if (type==EIfcShapeRepresentation.MappedRepresentation.ToString())
            {
                IfcMappedItem=new IfcMappedItem(IfcFile.ContentAtIndex(ifcLine.LinesNo[2]));
            }
        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        IfcMappedItem ifcMappedItem;
        public IfcMappedItem IfcMappedItem { get { return ifcMappedItem; } set { ifcMappedItem = value; } }
        IfcExtrudedAreaSolid ifcExtrudedAreaSolid;
        public IfcExtrudedAreaSolid IfcExtrudedAreaSolid { get { return ifcExtrudedAreaSolid; } set { ifcExtrudedAreaSolid = value; } }
    }
    public class IfcMappedItem
    {
        public IfcMappedItem(IfcLine ifcLine)
        {
            LineNo = ifcLine.LineNo;
            IfcRepresentaionMap=new IfcRepresentaionMap(IfcFile.ContentAtIndex(ifcLine.LinesNo[1]));
        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        IfcRepresentaionMap ifcRepresentaionMap;
        public IfcRepresentaionMap IfcRepresentaionMap { get { return ifcRepresentaionMap; } set { ifcRepresentaionMap = value; } }
    }
    public class IfcExtrudedAreaSolid
    {
        public IfcExtrudedAreaSolid(IfcLine ifcLine)
        {
            LineNo = ifcLine.LineNo;
            IfcAxis2Placement3D=new IfcAxis2Placement3D(IfcFile.ContentAtIndex(ifcLine.LinesNo[2]));
        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        IfcAxis2Placement3D ifcAxis2Placement3D;
        public IfcAxis2Placement3D IfcAxis2Placement3D { get { return ifcAxis2Placement3D; } set { ifcAxis2Placement3D = value; } }
    }    
    public class IfcRepresentaionMap
    {
        public IfcRepresentaionMap(IfcLine ifcLine)
        {
            LineNo = ifcLine.LineNo;
            IfcAxis2Placement3D=new IfcAxis2Placement3D(IfcFile.ContentAtIndex(ifcLine.LinesNo[1]));
            IfcShapeRepresentation=new IfcShapeRepresentation(IfcFile.ContentAtIndex(ifcLine.LinesNo[2]));
        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        IfcAxis2Placement3D ifcAxis2Placement3D;
        public IfcAxis2Placement3D IfcAxis2Placement3D { get { return ifcAxis2Placement3D; } set { ifcAxis2Placement3D = value; } }
        IfcShapeRepresentation ifcShapeRepresentation;
        public IfcShapeRepresentation IfcShapeRepresentation { get { return ifcShapeRepresentation; } set { ifcShapeRepresentation = value; } }
    }
    public class IfcSlab : IfcElement
    {
        public IfcSlab(int elementId, string typeName, string familyName, XYZ coordinate) : base(typeName, familyName)
        {
            ElementId = elementId;
            TypeName = typeName;
            FamilyName = familyName;
            Coordinate = coordinate;
        }
        XYZ coordinate;
        public XYZ Coordinate { get { return coordinate; } set { coordinate = value; } }

    }
    public class IfcColumn : IfcElement
    {
        public IfcColumn(int elementId, string typeName, string familyName, XYZ coordinate, double height) : base(typeName, familyName)
        {
            ElementId = elementId;
            TypeName = typeName;
            FamilyName = familyName;
            Coordinate = coordinate;
            Height = height;
        }
        XYZ coordinate;
        public XYZ Coordinate { get { return coordinate; } set { coordinate = value; } }
        double height;
        public double Height { get { return height; } set { height = value; } }
    }
    public class IfcBeam : IfcElement
    {
        public IfcBeam(string typeName, string familyName) : base(typeName, familyName)
        {
        }
    }
}
