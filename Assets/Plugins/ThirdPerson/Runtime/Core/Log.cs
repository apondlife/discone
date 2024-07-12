using Soil;

namespace ThirdPerson {

// TODO: inherit log tags w/ codegen: [Extends(typeof(Soil.Log))]
/// a set of log tags
public static class Log {
    public static readonly Tag Camera     = new("camera", "#ca3e8a");
    public static readonly Tag Character  = new("chrctr", "#ffff00");
    public static readonly Tag Controller = new("cntrlr", "#ff00ff");
    public static readonly Tag Editor     = Soil.Log.Editor;
    public static readonly Tag Model      = new("cmodel", "#fabada");
    public static readonly Tag Player     = new("player", "#ff00aa");
    public static readonly Tag Temp       = Soil.Log.Temp;
}

}