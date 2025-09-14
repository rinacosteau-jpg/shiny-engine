using System.Collections.Generic;

public static class ItemIds {
    public const string InventoryArtefact = "InventoryArtefact"; // без "item_"
    public const string Gun = "Gun"; 
    public const string HarmonicRow = "HarmonicRow";
    public const string SonoceramicShard = "SonoceramicShard";
    public const string SonusGuideTube = "SonusGuideTube";
    public const string ReceiptWhisperer = "ReceiptWhisperer";
    public const string WaxStoppers = "WaxStoppers";
    public const string MaintScrollHum = "MaintScrollHum";
    public const string VentFiddle = "VentFiddle";
    public const string EarPressureReports = "EarPressureReports";
    public const string TestCube = "TestCube";
    public const string JapaneseSweets = "JapaneseSweets";

    public static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>
    {
        { InventoryArtefact, "wandering artifact scribbles" },
        { TestCube, "cool cube" },
        { JapaneseSweets, "wow truly magnificent artefact" },
        { Gun, "super gun" },
        { HarmonicRow, "gobbledygook melody of squirrels" },
        { SonoceramicShard, "fragment of whispering teapots" },
        { SonusGuideTube, "tube guiding sounds of marshmallows" },
        { ReceiptWhisperer, "coupon mumbo jumbo mosaic" },
        { WaxStoppers, "sticky nonsense of waxy blobs" },
        { MaintScrollHum, "humming scroll of baffling fumes" },
        { VentFiddle, "perplexing fiddle for vents" },
        { EarPressureReports, "reports full of earwig gibberish" }
    };

    public static readonly Dictionary<string, string> ImagePaths = new Dictionary<string, string>
    {
        { InventoryArtefact, "Images/InventoryArtefact" },
        { TestCube, "Images/TestCube" },
        { JapaneseSweets, "Images/JapaneseSweets" },
        { Gun, "Images/Gun" },
        { HarmonicRow, "Images/HarmonicRow" },
        { SonoceramicShard, "Images/SonoceramicShard" },
        { SonusGuideTube, "Images/SonusGuideTube" },
        { ReceiptWhisperer, "Images/ReceiptWhisperer" },
        { WaxStoppers, "Images/WaxStoppers" },
        { MaintScrollHum, "Images/MaintScrollHum" },
        { VentFiddle, "Images/VentFiddle" },
        { EarPressureReports, "Images/EarPressureReports" }
    };
}
