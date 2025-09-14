using System.Collections.Generic;

public static class ItemIds {
    public const string InventoryArtefact = "InventoryArtefact"; // без "item_"
    public const string Gun = "Gun"; // на будущее под hasGun
    public const string HarmonicRow = "HarmonicRow";
    public const string SonoceramicShard = "SonoceramicShard";
    public const string SonusGuideTube = "SonusGuideTube";
    public const string ReceiptWhisperer = "ReceiptWhisperer";
    public const string WaxStoppers = "WaxStoppers";
    public const string MaintScrollHum = "MaintScrollHum";
    public const string VentFiddle = "VentFiddle";
    public const string EarPressureReports = "EarPressureReports";

    public static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>
    {
        { InventoryArtefact, "wandering artifact scribbles" },
        { HarmonicRow, "gobbledygook melody of squirrels" },
        { SonoceramicShard, "fragment of whispering teapots" },
        { SonusGuideTube, "tube guiding sounds of marshmallows" },
        { ReceiptWhisperer, "coupon mumbo jumbo mosaic" },
        { WaxStoppers, "sticky nonsense of waxy blobs" },
        { MaintScrollHum, "humming scroll of baffling fumes" },
        { VentFiddle, "perplexing fiddle for vents" },
        { EarPressureReports, "reports full of earwig gibberish" }
    };
}
