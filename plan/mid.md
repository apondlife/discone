mired in debt
---

# bugs/problems
- head is looking down when idle
- flowers spawn from center
- aspect ratio stretches the game
- head gets distorted over time by the hair
- clipping happens on bad normal surfaces
- jumping in a tight corner (wall & ground) sometimes has no effect (happens in birthplace)
- flowers on teleport don't work for other players 
- inertia sometimes explodes when character is stuck
- icecream can stand on the curve of the capsule
- transition from fixed cameras to character camera has bad rotation in in-between state
- start camera is only triggered by the last player 
- grip (or something) pulls you over corners weirdly (on the frame collision misses; should it pull you towards the hit point?)
- clipping happens on regular cubes
- transfer in a vpipe doesn't work nicely
- recover broken overworld terrain (git history)
- movement system uses coyote time
- interaction between eyes closed dialogue and dream dialogue
    - closing eyed before finishing dream tutorial, breaks sequence (see Intro_Skip)
- teleport shortcut is inconsistent
- visuals for adsr
- if you stop on static friction, decreasing the friction doesn't start moving the character
- should forcestate set curr?
- can't get into jumpsquat after first aerial jump (if there's not infinite air jumps)
- head ik rotates too far
- need to manually enable input actions
- having zero jumps is not considered
- default speed of head ik is poorly tuned
- head ik direction only works for some models

# debt
- remove frame copy constructor in favor of assign fn (reuse frames)
- float dynamic ease (replace camera tilt system smoothdamp)
- model should always use Next (or some interpolated version)
- rename character* to model* (generally reconsider 3p namespaces)
- make log.i go to correct place in code (dll with log & code excluded from project)
- compiling stuff out in debug/development (but still want it for testing maybe)
- need to manually enable input actions
- add code generation for equatable (jumpid, charactercollision)
- share state interpolation