using System;

// -- types --
/// the key for a character
public enum CharacterKey {
    Icecream,
    Ivan,
    Frog,
    Clockboi
};

/// -- impls --
public static class CharacterKeyExt {
    public static string Name(this CharacterKey key) {
        return Enum.GetName(typeof(CharacterKey), key);
    }
}