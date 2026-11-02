using System.Collections.Generic;

namespace CrateBoss.Levels
{
    /// <summary>
    /// One entry of the "deliveries" list (Ana Belge §6): the delivery crate's cell (null when a crane brings it), its area
    /// ("B" or "1"–"3"), an optional way stop ("M") and whether it is a heavy delivery crate.
    /// </summary>
    public class DeliveryData
    {
        public int[] box;
        public string area = "B";
        public string via;
        public bool heavy;

        public static DeliveryData FromJson(object node, int index)
        {
            if (!(node is Dictionary<string, object> obj)) throw new LevelFormatException($"deliveries {index} must be an object.");
            var d = new DeliveryData();
            if (obj.TryGetValue("box", out var b) && b != null) d.box = JsonCells.Pair(b, $"deliveries {index} box");
            if (obj.TryGetValue("area", out var a) && a != null)
                d.area = a as string ?? (a is double n ? ((int)n).ToString() : throw new LevelFormatException($"deliveries {index} area must be a name."));
            if (obj.TryGetValue("via", out var v) && v != null) d.via = v as string ?? throw new LevelFormatException($"deliveries {index} via must be \"M\" or null.");
            if (obj.TryGetValue("heavy", out var h) && h is bool hb) d.heavy = hb;
            return d;
        }
    }
}
