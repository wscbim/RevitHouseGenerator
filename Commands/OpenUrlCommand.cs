using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitHouseGenerator.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class AboutCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var dlg = new TaskDialog("RevitHouseGenerator v0.1")
            {
                MainInstruction = "RevitHouseGenerator",
                MainContent =
                    "Author: Wojciech Ciep\u0142ucha\n" +
                    "wojciech.cieplucha@pk.edu.pl\n\n" +
                    "Chair of Architectural Design\n" +
                    "Faculty of Architecture\n" +
                    "Cracow University of Technology"
            };
            dlg.Show();
            return Result.Succeeded;
        }
    }
}
