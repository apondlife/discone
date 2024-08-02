using UnityEngine;

namespace ThirdPerson {

/// the base class for a character effect
public abstract class CharacterEffect: MonoBehaviour {
    // -- props --
    /// the character container
    protected CharacterContainer c;

    // -- lifecycle --
    protected virtual void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>();
    }

    // -- commands --
    /// prepares the particle system to use the color texture
    protected void InitColorTexture(ParticleSystem particles) {
        var shape = particles.shape;
        shape.textureBilinearFiltering = false;
        shape.textureAlphaAffectsParticles = false;
        shape.textureColorAffectsParticles = true;
        shape.textureClipChannel = ParticleSystemShapeTextureChannel.Alpha;
        shape.textureClipThreshold = 0f;
    }

    /// updates the color texture for this particle system (call this before emitting)
    /// see: https://discussions.unity.com/t/procedural-particlesystem-shape-texture-not-working/890524/8
    protected void SyncColorTexture(ParticleSystem particles) {
        var shape = particles.shape;
        shape.texture = c.Effects.ColorTexture;
    }
}

}