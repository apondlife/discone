mired in debt
---

# bugs/problems
- going back to bubble offsets model position
- **ICE CREAMS HEAD IS FUCKEE+D***=
- icecream can stand on the curve of the capsule
- inertia sometimes explodes when character is stuck
- make log.i go to correct place in code (dll with log & code excluded from project)
- grip (or something) pulls you over corners weirdly (on the frame collision misses; should it pull you towards the hit point?)
- can't get into jumpsquat after first aerial jump (if there's not infinite air jumps)
- jumping in a tight corner (wall & ground) sometimes has no effect (happens in birthplace)
- transfer in a vpipe doesn't work nicely
- recover broken overworld terrain (git history)
- movement system uses coyote time
- interaction between eyes closed dialogue and dream dialogue
    - closing eyed before finishing dream tutorial, breaks sequence (see Intro_Skip)
- transition from fixed cameras to character camera has bad rotation in in-between state
- teleport shortcut is inconsistent
- visuals for adsr
- if you stop on static friction, decreasing the friction doesn't start moving the character

# debt
- need to manually enable input actions
- head ik direction only works for some models
- default speed of head ik is poorly tuned
- having zero jumps is not considered
- rename character* to model* (generally reconsider 3p namespaces)
- compiling stuff out in debug/development (but still want it for testing maybe)
- add code generation for equatable (jumpid, charactercollision)