using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Linq;

[RequireComponent(typeof(ParticleSystem))]
public class ButterflySounds : MonoBehaviour
{
    ParticleSystem _ps;
    List<StudioEventEmitter> _emitters;

    ParticleSystem.Particle[] _particles;

    bool _initialized;

    public EventReference fmodEvent;

    // Start is called before the first frame update
    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        _particles = new ParticleSystem.Particle[_ps.main.maxParticles];

        _initialized = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Initialize emitters
        if (!_initialized){
            _ps.GetParticles(_particles);
            _emitters = _particles.Select(p => {
                GameObject o = new GameObject("Butterfly");
                o.transform.parent = this.transform;
                StudioEventEmitter emitter = o.AddComponent<StudioEventEmitter>();
                emitter.EventReference = fmodEvent;
                emitter.Play();

                // emitter.transform.position = p.position;
                // [Unfortunately seems like we can't get the startFrame (ie phase offset) of the texture sheet animation -
                //  if we could then we could set the loop offset in fmod correspondingly. Instead for now just randomize it independently]
                return emitter;
            }).ToList();
            _initialized = true;
        }

        // update positions
        _ps.GetParticles(_particles);
        for(int i = 0; i < _particles.Length; i++) {
            _emitters[i].transform.position = _particles[i].position;
        }
    }
}
