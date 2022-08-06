using System;

/// the characters ability to perceive others
[Serializable]
public struct CharacterPerception {
    /// how far the character can see others
    public float VisionRadius;

    /// how far the character can hear others
    public float HearingRadius;

    /// how far the character can talk to others
    public float TalkingRadius;
}
