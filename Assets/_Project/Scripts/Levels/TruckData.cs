using System.Collections.Generic;

namespace CrateBoss.Levels
{
    /// <summary>The "trucks" block (Ana Belge §5.5): how many trucks come and the loading area cells [[x, y], ...].</summary>
    public class TruckData
    {
        public int count = 1;
        public int[][] bay;

        public static TruckData FromJson(object node)
        {
            if (!(node is Dictionary<string, object> obj)) throw new LevelFormatException("'trucks' must be an object.");
            var t = new TruckData();
            if (obj.TryGetValue("count", out var c) && c is double d) t.count = (int)d;
            if (obj.TryGetValue("bay", out var b)) t.bay = JsonCells.List(b, "trucks.bay");
            return t;
        }
    }
}
