using System.Text;

namespace CrateBoss.Levels
{
    /// <summary>Writes a level in the same hand-readable layout as the files in Resources/Levels.</summary>
    public static class LevelJsonWriter
    {
        public static string ToJson(LevelData d)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append($"  \"id\": {d.id},\n");
            sb.Append($"  \"name\": {Quote(d.name)},\n");
            AppendRows(sb, "ground", d.ground);
            sb.Append(",\n");
            AppendRows(sb, "objects", d.objects);
            if (d.HasCraneList)
            {
                sb.Append(",\n  \"cranes\": [\n");
                for (int c = 0; c < d.cranes.Length; c++)
                {
                    var crane = d.cranes[c];
                    sb.Append("    { ");
                    if (!string.IsNullOrEmpty(crane.id)) sb.Append($"\"id\": {Quote(crane.id)}, ");
                    if (!string.IsNullOrEmpty(crane.side)) sb.Append($"\"side\": {Quote(crane.side)}, ");
                    sb.Append($"\"every\": {crane.every}, \"drops\": [\n");
                    AppendDrops(sb, crane.drops, "        ");
                    sb.Append($"      ] }}{(c < d.cranes.Length - 1 ? "," : "")}\n");
                }
                sb.Append("  ]");
            }
            else if (d.HasOldCrane)
            {
                sb.Append($",\n  \"crane\": {{\n    \"every\": {d.crane.every},\n    \"drops\": [\n");
                AppendDrops(sb, d.crane.drops, "      ");
                sb.Append("    ]\n  }");
            }
            if (d.goal != null && d.goal.Length > 0)
            {
                sb.Append(",\n  \"goal\": [");
                for (int i = 0; i < d.goal.Length; i++) sb.Append(i > 0 ? ", " : "").Append(Quote(d.goal[i]));
                sb.Append("]");
            }
            if (d.trucks != null && d.trucks.bay != null)
            {
                sb.Append($",\n  \"trucks\": {{ \"count\": {d.trucks.count}, \"bay\": [");
                for (int i = 0; i < d.trucks.bay.Length; i++) sb.Append(i > 0 ? ", " : "").Append($"[{d.trucks.bay[i][0]},{d.trucks.bay[i][1]}]");
                sb.Append("] }");
            }
            if (d.deliveries != null && d.deliveries.Length > 0)
            {
                sb.Append(",\n  \"deliveries\": [\n");
                for (int i = 0; i < d.deliveries.Length; i++)
                {
                    var e = d.deliveries[i];
                    string box = e.box != null ? $"[{e.box[0]},{e.box[1]}]" : "null";
                    string via = string.IsNullOrEmpty(e.via) ? "null" : Quote(e.via);
                    sb.Append($"    {{ \"box\": {box}, \"area\": {Quote(e.area)}, \"via\": {via}{(e.heavy ? ", \"heavy\": true" : "")} }}{(i < d.deliveries.Length - 1 ? "," : "")}\n");
                }
                sb.Append("  ]");
            }
            if (d.parking != null)
            {
                var p = d.parking;
                sb.Append(",\n  \"parking\": {\n    \"cars\": [");
                var cars = p.cars ?? new CarData[0];
                for (int i = 0; i < cars.Length; i++)
                    sb.Append(i > 0 ? ", " : "").Append($"{{ \"cells\": {Cells(cars[i].cells)}, \"color\": {Quote(cars[i].color)} }}");
                sb.Append($"],\n    \"exits\": {Cells(p.exits)},\n    \"truckRoute\": {Cells(p.truckRoute)},\n    \"bay\": {Cells(p.bay)},\n    \"trucks\": {p.trucks}");
                if (p.docks != null && p.docks.Length > 0)
                {
                    sb.Append(",\n    \"docks\": [");
                    for (int i = 0; i < p.docks.Length; i++)
                        sb.Append(i > 0 ? ", " : "").Append($"{{ \"truckRoute\": {Cells(p.docks[i].truckRoute)}, \"bay\": {Cells(p.docks[i].bay)}, \"trucks\": {p.docks[i].trucks} }}");
                    sb.Append("]");
                }
                sb.Append("\n  }");
            }
            if (d.league > 0) sb.Append($",\n  \"league\": {d.league}");
            if (!string.IsNullOrEmpty(d.solution)) sb.Append($",\n  \"solution\": {Quote(d.solution)}");
            if (!string.IsNullOrEmpty(d.taskCard)) sb.Append($",\n  \"taskCard\": {Quote(d.taskCard)}");
            if (d.design != null)
            {
                sb.Append(",\n  \"design\": {\n");
                sb.Append($"    \"idea\": {Quote(d.design.idea)},\n");
                sb.Append("    \"mechanics\": [");
                var mechanics = d.design.mechanics ?? new string[0];
                for (int i = 0; i < mechanics.Length; i++) sb.Append(i > 0 ? ", " : "").Append(Quote(mechanics[i]));
                sb.Append("],\n");
                sb.Append($"    \"tier\": {Quote(d.design.tier)}\n");
                sb.Append("  }");
            }
            sb.Append("\n}\n");
            return sb.ToString();
        }

        private static void AppendRows(StringBuilder sb, string key, string[] rows)
        {
            sb.Append($"  \"{key}\": [\n");
            for (int i = 0; i < rows.Length; i++)
                sb.Append($"    \"{rows[i]}\"{(i < rows.Length - 1 ? "," : "")}\n");
            sb.Append("  ]");
        }

        private static void AppendDrops(StringBuilder sb, CraneDropData[] drops, string indent)
        {
            for (int i = 0; i < drops.Length; i++)
            {
                var drop = drops[i];
                string index = drop.type == "D" ? $", \"deliveryIndex\": {drop.deliveryIndex}" : "";
                sb.Append($"{indent}{{ \"x\": {drop.x}, \"y\": {drop.y}, \"type\": {Quote(drop.type)}{index} }}{(i < drops.Length - 1 ? "," : "")}\n");
            }
        }

        private static string Cells(int[][] cells)
        {
            if (cells == null || cells.Length == 0) return "[]";
            var sb = new StringBuilder("[");
            for (int i = 0; i < cells.Length; i++) sb.Append(i > 0 ? ", " : "").Append($"[{cells[i][0]},{cells[i][1]}]");
            return sb.Append(']').ToString();
        }

        private static string Quote(string s) => "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
