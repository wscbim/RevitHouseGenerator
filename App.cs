using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RevitHouseGenerator
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            CreateRibbon(application);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

        private void CreateRibbon(UIControlledApplication application)
        {
            const string tabName = "DevWSC";
            try { application.CreateRibbonTab(tabName); } catch { }

            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Generator");
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            var buttonData = new PushButtonData(
                "GenHouseBtn",
                "Solid",
                assemblyPath,
                "RevitHouseGenerator.Commands.GenerateHouseCommand")
            {
                ToolTip = "Generate a house structure on a 120×120 cm module grid."
            };

            PushButton button = panel.AddItem(buttonData) as PushButton;
            button.LargeImage = CreateGIcon(32);
            button.Image = CreateGIcon(16);

            // About panel
            RibbonPanel aboutPanel = application.CreateRibbonPanel(tabName, "About");

            var aboutBtn = new PushButtonData(
                "AboutBtn",
                "About",
                assemblyPath,
                "RevitHouseGenerator.Commands.AboutCommand")
            {
                ToolTip = "Author: Wojciech Ciep\u0142ucha\n" +
                          "wojciech.cieplucha@pk.edu.pl\n\n" +
                          "Chair of Architectural Design\n" +
                          "Faculty of Architecture\n" +
                          "Cracow University of Technology"
            };

            aboutPanel.AddItem(aboutBtn);
        }

        private BitmapSource CreateGIcon(int size)
        {
            var visual = new DrawingVisual();
            using (DrawingContext ctx = visual.RenderOpen())
            {
                ctx.DrawRectangle(
                    new SolidColorBrush(Color.FromRgb(0, 102, 179)),
                    null,
                    new Rect(0, 0, size, size));

                var text = new FormattedText(
                    "G",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                    size * 0.72,
                    Brushes.White,
                    1.0);

                ctx.DrawText(text, new Point(
                    (size - text.Width) / 2,
                    (size - text.Height) / 2));
            }

            var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            return bitmap;
        }
    }
}
