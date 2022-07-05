using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IFC_Regenerate
{
    public class IfcElement
    {
        public IfcElement(string typeName, string familyName)
        {
            TypeName = typeName;
            FamilyName = familyName;
        }
        public IfcElement(string ifcLine)
        {
            string ifcInfo = ifcLine.Split('\'')[3];
            TypeName = ifcInfo.Split(':')[1];
            FamilyName = ifcInfo.Split(':')[0];
            ElementId = int.Parse(ifcInfo.Split(':')[2]);
        }
        string typeName;
        public string TypeName { get { return typeName; } set { typeName = value; } }
        string familyName;
        public string FamilyName { get { return familyName; } set { familyName = value; } }
        int elementId;
        public int ElementId { get { return elementId; } set { elementId = value; } }
        string category;
        public string Category { get { return category; } set { category = value; } }
        Level level;
        public Level Level { get { return level; } set { level = value; } }
    }
    public class IfcLocalPlacement
    {
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        IfcLocalPlacement ifcLPlacement;
        public IfcLocalPlacement IfcLPlacement { get { return ifcLPlacement; } set { ifcLPlacement = value; } }
        IfcAxis2Placement3D ifcAxis2Placement3D;
        public IfcAxis2Placement3D IfcAxis2Placement3D { get { return ifcAxis2Placement3D; } set { ifcAxis2Placement3D = value; } }
    }
    public class IfcAxis2Placement3D
    {
        public IfcAxis2Placement3D(string ifcLine)
        {
            Regex regexNo = new Regex(@"\d*");
            LineNo = int.Parse(regexNo.Match(ifcLine).ToString());
        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        IfcCatesianPoint ifcCatesianPoint;
        public IfcCatesianPoint IfcCatesianPoint { get { return ifcCatesianPoint; } set { ifcCatesianPoint = value; } }
    }
    public class IfcCatesianPoint
    {
        public IfcCatesianPoint(string ifcLine)
        {
            Regex regexNo = new Regex(@"\d*");
            LineNo = int.Parse(regexNo.Match(ifcLine).ToString());
            Regex regexDecimal = new Regex(@"-*?\d*?\.\d*");
            List<double> coordinates = regexDecimal.Matches(ifcLine)._ToListString().Select(x => double.Parse(x)).ToList();
            XYZ = new XYZ(coordinates[0], coordinates[1], coordinates[2]);
        }
        int lineNo;
        public int LineNo { get { return lineNo; } set { lineNo = value; } }
        XYZ xyz;
        public XYZ XYZ
        {
            get { return xyz; }
            set { xyz = value; }
        }
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
