using System.Collections.Generic;

namespace CrateBoss.Levels
{
    /// <summary>One parked car of the "parking" block (Ana Belge §6): its two cells and its colour name.</summary>
    public class CarData
    {
        public int[][] cells;
        public string color;

        public static CarData FromJson(object node, int index)
        {
            if (!(node is Dictionary<string, object> obj)) throw new LevelFormatException($"parking.cars {index} must be an object.");
            var c = new CarData();
            if (obj.TryGetValue("cells", out var cells)) c.cells = JsonCells.List(cells, $"parking.cars {index} cells");
            if (obj.TryGetValue("color", out var color)) c.color = color as string;
            return c;
        }
    }
}
