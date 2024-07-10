using Soil;

namespace Discone {

/// a set of log tags
public static class Log {
    public static readonly Tag App       = new("appppp", "#00aabb");
    public static readonly Tag Audio     = new("audioo", "#a0d110");
    public static readonly Tag Character = ThirdPerson.Log.Character;
    public static readonly Tag Debug     = new("debugg", "#046920");
    public static readonly Tag Dialog    = new("dialog", "#bbccaa");
    public static readonly Tag Editor    = Soil.Log.Editor;
    public static readonly Tag Flower    = new("flower", "#410034");
    public static readonly Tag Game      = new("gameee", "#548833");
    public static readonly Tag Interest  = new("intrst", "#00aabb");
    public static readonly Tag Mechanic  = new("mechnk", "#ddaaff");
    public static readonly Tag Menu      = new("menuuu", "#420420");
    public static readonly Tag Online    = new("online", "#ff00ff");
    public static readonly Tag Player    = ThirdPerson.Log.Player;
    public static readonly Tag Region    = new("region", "#112211");
    public static readonly Tag Store     = new("storee", "#00aabb");
    public static readonly Tag Temp      = Soil.Log.Temp;
    public static readonly Tag Unknown   = new("unknwn", "#000000");
    public static readonly Tag World     = new("worldd", "#00ff00");
}

}