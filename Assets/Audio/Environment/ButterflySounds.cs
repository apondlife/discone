using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Linq;

[RequireComponent(typeof(ParticleSystem))]
public class ButterflySounds : MonoBehaviour
{
    ParticleSystem _ps;
    Dictionary<uint, StudioEventEmitter> _emitters; // store use Particle.randomSeed as id/key

    ParticleSystem.Particle[] _particles;

    bool _initialized;

    public EventReference fmodEvent;

    public Transform listener;
    public float listenRadius = 10f;

    void OnEnable()
    {
        _ps = GetComponent<ParticleSystem>();
        _particles = new ParticleSystem.Particle[_ps.main.maxParticles];
        _emitters = new Dictionary<uint, StudioEventEmitter>();

        _initialized = false;
    }

    void Update()
    {
        if (!_initialized){
            // Initialize emitters
            _ps.GetParticles(_particles);
            foreach (ParticleSystem.Particle p in _particles) {
                GameObject o = new GameObject("Butterfly");
                o.transform.parent = this.transform;
                StudioEventEmitter emitter = o.AddComponent<StudioEventEmitter>();
                emitter.EventReference = fmodEvent;
                // emitter.Play();

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
            List<uint> deadEmitterKeys = _emitters.Keys.Where(k => !liveKeys.Contains(k)).ToList();
            // deadEmitters belonged to particles that died last frame, reassign them to any new particles

            for(int i = 0; i < _particles.Length; i++) {
                uint k = _particles[i].randomSeed;
                if (!_emitters.ContainsKey(k)) {
                    // new particle; reuse a dead emitter
                    uint deadK = deadEmitterKeys[0];
                    deadEmitterKeys.RemoveAt(0);
                    _emitters[k] = _emitters[deadK];
                    _emitters.Remove(deadK);
                }
                _emitters[k].transform.position = _particles[i].position;

                // Only play sounds close to the listener
                // (this is a workaround for limitations in FMOD's 'max instances'/'stealing' beahvior,
                //  namely that it doesn't work for continuously-playing instances that change position)
                // TODO: a better and generic and cheap way of doing this [maybe a trigger sphere on the listener object?]
                if (Vector3.Distance(_particles[i].position, listener.position) < listenRadius) {
                    if (!_emitters[k].IsPlaying()) {
                        _emitters[k].Play();
                    }
                } else {
                    _emitters[k].Stop();
                }
            }
            // TODO: should remove particles that are no longer active from the dict
        }
    }
}
