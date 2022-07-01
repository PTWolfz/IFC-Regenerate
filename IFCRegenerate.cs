using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GeometryGym.Ifc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc2x3.Interfaces;

namespace IFC_Regenerate
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]    
    public class Regenerate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string filePath = @"D:\PTT\05. Download\Document Test\project.ifc";
            //string fileName = @"C:\Users\ptthanh\Documents\Project1.ifc";
            DatabaseIfc db = new DatabaseIfc(filePath);
            IfcProject project = db.Project;
            List<IfcBuilding> buildings = project.Extract<IfcBuilding>();
            IfcBuilding thisBuilding = buildings.FirstOrDefault();


            return Result.Succeeded;
        }
    }
}
