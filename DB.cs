using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFC_Regenerate
{
    public class IfcElement
    {
        public IfcElement(string typeName, string familyName)
        {
            TypeName=typeName;
            FamilyName=familyName;
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
    public class IfcSlab : IfcElement
    {
        public IfcSlab(int elementId,string typeName, string familyName, XYZ coordinate) : base(typeName, familyName)
        {
            ElementId=  elementId;
            TypeName=typeName;
            FamilyName=familyName;
            Coordinate=coordinate;
        }
        XYZ coordinate;
        public XYZ Coordinate { get { return coordinate; } set { coordinate = value; } }
      
    }
    public class IfcColumn : IfcElement
    {
        public IfcColumn(int elementId, string typeName, string familyName, XYZ coordinate, double height) : base(typeName, familyName)
        {
            ElementId=  elementId;
            TypeName=typeName;
            FamilyName=familyName;
            Coordinate=coordinate;
            Height=height;
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
