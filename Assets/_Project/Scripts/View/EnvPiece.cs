namespace CrateBoss.View
{
    /// <summary>
    /// Surroundings pieces (Ana Belge §2.5). Each has a placeholder prefab at
    /// Art/Environment/&lt;district&gt;/Resources/Environment/&lt;District&gt;/&lt;Piece&gt;.prefab; replacing that file with a real
    /// model changes the look without code changes. Pivot at the bottom centre, front facing -Z (toward the camera).
    /// </summary>
    public enum EnvPiece
    {
        Building,
        Shop,
        Tree,
        StreetLamp,
        Car,
        Truck,
        ContainerStack,
        Warehouse,
        CargoPlane,
        BaggageTrain,
        Tower,
        RunwayLight,
        // Cold store and customs (Ana Belge §2)
        ColdStorage,
        ReeferTruck,
        CustomsHouse,
        Checkpoint,
        Flag,
        // More variety round the yard (owner 2026-10-30)
        Apartment,
        Park,
        ParkingLot,
        GantryCrane,
        Ship,
    }
}
