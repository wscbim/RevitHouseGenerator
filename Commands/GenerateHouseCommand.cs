using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitHouseGenerator.UI;
using System.Collections.Generic;
using System.Linq;

namespace RevitHouseGenerator.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GenerateHouseCommand : IExternalCommand
    {
        // Level name, elevation (m), wall height (m), floor type name
        private static readonly (string Name, double ElevM, double WallHeightM, string FloorTypeName)[] LevelData =
        {
            ("Ground Floor", 0.0,  3.6, "Concept - 60"),
            ("1st Floor",    3.6,  3.6, "Concept - 35"),
            ("2nd Floor",    7.2,  3.6, "Concept - 35"),
            ("Attic",       10.8,  1.2, "Concept - 60"),
        };

        private const double ModuleSizeM = 1.2;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var dialog = new HouseConfigDialog();
            if (dialog.ShowDialog() != true)
                return Result.Cancelled;

            int modulesX = dialog.SelectedModulesX;
            int modulesY = dialog.SelectedModulesY;

            Document doc = commandData.Application.ActiveUIDocument.Document;

            using (var tx = new Transaction(doc, "GenHouse"))
            {
                tx.Start();

                EnsureConceptWallTypes(doc);
                EnsureConceptFloorTypes(doc);
                var levels = EnsureLevels(doc);

                foreach (var level in levels)
                    CreateModuleGrid(doc, level, modulesX, modulesY);

                WallType wallType = GetWallType(doc, "Concept - 50");
                if (wallType != null)
                {
                    CreateWalls(doc, levels, modulesX, modulesY, wallType);
                    CreateFloors(doc, levels, modulesX, modulesY, wallType);
                }

                tx.Commit();
            }

            double w = modulesX * ModuleSizeM;
            double d = modulesY * ModuleSizeM;
            TaskDialog.Show("GenHouse",
                $"Done!\n\nGrid: {modulesX}×{modulesY} modules = {w:F1}×{d:F1} m\n" +
                $"Levels: Ground Floor, 1st Floor, 2nd Floor, Attic\n" +
                $"Walls: Concept - 50  |  Floors: Concept-60 / 35 / 60 / 60");

            return Result.Succeeded;
        }

        // ── Wall types ──────────────────────────────────────────────────────────

        private void EnsureConceptWallTypes(Document doc)
        {
            var conceptTypes = new (string Name, double ThicknessM)[]
            {
                ("Concept - 50", 0.50),
                ("Concept - 25", 0.25),
                ("Concept - 12", 0.12),
            };

            WallType sourceType = new FilteredElementCollector(doc)
                .OfClass(typeof(WallType))
                .Cast<WallType>()
                .First(wt => wt.Kind == WallKind.Basic);

            foreach (var (name, thicknessM) in conceptTypes)
            {
                bool exists = new FilteredElementCollector(doc)
                    .OfClass(typeof(WallType))
                    .Cast<WallType>()
                    .Any(wt => wt.Name == name);

                if (exists) continue;

                double thicknessFt = UnitUtils.ConvertToInternalUnits(thicknessM, UnitTypeId.Meters);
                WallType newType = sourceType.Duplicate(name) as WallType;
                CompoundStructure cs = newType.GetCompoundStructure();
                IList<CompoundStructureLayer> existing = cs.GetLayers();
                ElementId matId = existing.Count > 0 ? existing[0].MaterialId : ElementId.InvalidElementId;
                cs.SetLayers(new List<CompoundStructureLayer>
                {
                    new CompoundStructureLayer(thicknessFt, MaterialFunctionAssignment.Structure, matId)
                });
                newType.SetCompoundStructure(cs);
            }
        }

        private WallType GetWallType(Document doc, string name) =>
            new FilteredElementCollector(doc)
                .OfClass(typeof(WallType))
                .Cast<WallType>()
                .FirstOrDefault(wt => wt.Name == name);

        // ── Floor types ─────────────────────────────────────────────────────────

        private void EnsureConceptFloorTypes(Document doc)
        {
            var conceptTypes = new (string Name, double ThicknessM)[]
            {
                ("Concept - 60", 0.60),
                ("Concept - 35", 0.35),
            };

            FloorType sourceType = new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .Cast<FloorType>()
                .First(ft => ft.IsFoundationSlab == false);

            foreach (var (name, thicknessM) in conceptTypes)
            {
                bool exists = new FilteredElementCollector(doc)
                    .OfClass(typeof(FloorType))
                    .Cast<FloorType>()
                    .Any(ft => ft.Name == name);

                if (exists) continue;

                double thicknessFt = UnitUtils.ConvertToInternalUnits(thicknessM, UnitTypeId.Meters);
                FloorType newType = sourceType.Duplicate(name) as FloorType;
                CompoundStructure cs = newType.GetCompoundStructure();
                IList<CompoundStructureLayer> existing = cs.GetLayers();
                ElementId matId = existing.Count > 0 ? existing[0].MaterialId : ElementId.InvalidElementId;
                cs.SetLayers(new List<CompoundStructureLayer>
                {
                    new CompoundStructureLayer(thicknessFt, MaterialFunctionAssignment.Structure, matId)
                });
                newType.SetCompoundStructure(cs);
            }
        }

        private FloorType GetFloorType(Document doc, string name) =>
            new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .Cast<FloorType>()
                .FirstOrDefault(ft => ft.Name == name);

        // ── Levels ──────────────────────────────────────────────────────────────

        private List<Level> EnsureLevels(Document doc)
        {
            var existingByElev = new Dictionary<double, Level>();
            using (var col = new FilteredElementCollector(doc))
                foreach (Level lvl in col.OfClass(typeof(Level)))
                    existingByElev[lvl.Elevation] = lvl;

            var levels = new List<Level>();
            foreach (var (name, elevM, _, _) in LevelData)
            {
                double elevFt = UnitUtils.ConvertToInternalUnits(elevM, UnitTypeId.Meters);

                if (existingByElev.TryGetValue(elevFt, out Level existing))
                {
                    levels.Add(existing);
                    continue;
                }

                Level level = Level.Create(doc, elevFt);
                level.Name = name;
                levels.Add(level);
            }

            return levels;
        }

        // ── Module grid (model lines) ────────────────────────────────────────────

        private void CreateModuleGrid(Document doc, Level level, int modulesX, int modulesY)
        {
            double moduleFt = UnitUtils.ConvertToInternalUnits(ModuleSizeM, UnitTypeId.Meters);
            double W = modulesX * moduleFt;
            double D = modulesY * moduleFt;
            double z = level.Elevation;

            SketchPlane sketchPlane = SketchPlane.Create(doc,
                Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, z)));

            for (int i = 0; i <= modulesX; i++)
            {
                double x = i * moduleFt;
                doc.Create.NewModelCurve(
                    Line.CreateBound(new XYZ(x, 0, z), new XYZ(x, D, z)), sketchPlane);
            }
            for (int j = 0; j <= modulesY; j++)
            {
                double y = j * moduleFt;
                doc.Create.NewModelCurve(
                    Line.CreateBound(new XYZ(0, y, z), new XYZ(W, y, z)), sketchPlane);
            }
        }

        // ── Floors ──────────────────────────────────────────────────────────────

        private void CreateFloors(Document doc, IList<Level> levels, int modulesX, int modulesY, WallType wallType)
        {
            double moduleFt = UnitUtils.ConvertToInternalUnits(ModuleSizeM, UnitTypeId.Meters);
            double W  = modulesX * moduleFt;
            double D  = modulesY * moduleFt;
            double ht = wallType.GetCompoundStructure().GetWidth() / 2.0;

            for (int li = 0; li < levels.Count; li++)
            {
                FloorType floorType = GetFloorType(doc, LevelData[li].FloorTypeName);
                if (floorType == null) continue;

                double z = levels[li].Elevation;

                var loop = new CurveLoop();
                loop.Append(Line.CreateBound(new XYZ(ht,   ht,   z), new XYZ(W-ht, ht,   z)));
                loop.Append(Line.CreateBound(new XYZ(W-ht, ht,   z), new XYZ(W-ht, D-ht, z)));
                loop.Append(Line.CreateBound(new XYZ(W-ht, D-ht, z), new XYZ(ht,   D-ht, z)));
                loop.Append(Line.CreateBound(new XYZ(ht,   D-ht, z), new XYZ(ht,   ht,   z)));

                Floor.Create(doc, new List<CurveLoop> { loop }, floorType.Id, levels[li].Id);
            }
        }

        // ── Exterior walls ───────────────────────────────────────────────────────

        private void CreateWalls(Document doc, IList<Level> levels, int modulesX, int modulesY, WallType wallType)
        {
            double moduleFt = UnitUtils.ConvertToInternalUnits(ModuleSizeM, UnitTypeId.Meters);
            double W  = modulesX * moduleFt;
            double D  = modulesY * moduleFt;
            double ht = wallType.GetCompoundStructure().GetWidth() / 2.0;

            var segments = new[]
            {
                (new XYZ(0,    ht,   0), new XYZ(W,    ht,   0)),  // South
                (new XYZ(W-ht, 0,    0), new XYZ(W-ht, D,    0)),  // East
                (new XYZ(W,    D-ht, 0), new XYZ(0,    D-ht, 0)),  // North
                (new XYZ(ht,   D,    0), new XYZ(ht,   0,    0)),  // West
            };

            for (int li = 0; li < levels.Count; li++)
            {
                double heightFt = UnitUtils.ConvertToInternalUnits(LevelData[li].WallHeightM, UnitTypeId.Meters);

                foreach (var (start, end) in segments)
                {
                    Curve curve = Line.CreateBound(start, end);
                    Wall.Create(doc, curve, wallType.Id, levels[li].Id, heightFt, 0, false, false);
                }
            }
        }
    }
}
