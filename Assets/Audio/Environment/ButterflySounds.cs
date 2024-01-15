using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Linq;

[RequireComponent(typeof(ParticleSystem))]
public class ButterflySounds: MonoBehaviour {
    ParticleSystem _ps;
    Dictionary<uint, StudioEventEmitter> _emitters; // use Particle.randomSeed as id/key

    ParticleSystem.Particle[] _particles;

    bool _initialized;

    public EventReference fmodEvent;

    public Transform listener;
    public float listenRadius = 10f;

    void OnEnable() {
        _ps = GetComponent<ParticleSystem>();
        _particles = new ParticleSystem.Particle[_ps.main.maxParticles];
        _emitters = new Dictionary<uint, StudioEventEmitter>();

        _initialized = false;
    }

    void Update() {
        if (!_initialized) {
            // Initialize emitters
            _ps.GetParticles(_particles);
            foreach (ParticleSystem.Particle p in _particles) {
                GameObject o = new GameObject("Butterfly");
                o.transform.parent = this.transform;
                StudioEventEmitter emitter = o.AddComponent<StudioEventEmitter>();
                emitter.EventReference = fmodEvent;

                emitter.transform.position = p.position;

                uint k = p.randomSeed;
                _emitters[k] = emitter;
            }
            Debug.Log($"Butterflies: {_emitters.Count}");
            _initialized = true;
        } else {
            // update positions
            _ps.GetParticles(_particles);
            var liveKeys = _particles.Select(p => p.randomSeed);
            List<uint> deadEmitterKeys = _emitters.Keys.Except(liveKeys).ToList();
            // deadEmitters belonged to particles that died last frame, reassign them to any new particles

            var newParticleKeys = _particles.Select(p => p.randomSeed).Except(_emitters.Keys);

            foreach (var k in newParticleKeys) {
                // new particle; reuse a dead emitter
                uint deadK = deadEmitterKeys[0];
                deadEmitterKeys.RemoveAt(0);
                _emitters.Add(k, _emitters[deadK]);
                _emitters.Remove(deadK);
            }

            foreach (var p in _particles) {
                uint k = p.randomSeed;
                _emitters[k].transform.position = p.position;

                // Only play sounds close to the listener
                // (this is a workaround for limitations in FMOD's 'max instances'/'stealing' beahvior,
                //  namely that it doesn't work for continuously-playing instances that change position)
                // TODO: this logic should be unified with the character distance check that's happening in EntityPerception.cs
                // [I guess we want a SoundSource entity?]
                if (Vector3.Distance(p.position, listener.position) < listenRadius) {
                    if (!_emitters[k].IsPlaying()) {
                        _emitters[k].Play();
                    }
                } else {
                    _emitters[k].Stop();
                }
            }
        }
    }
}