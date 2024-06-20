what is projected
---

# ik legs
- inclines: surface velocity for legs? going downhill looks bad
- sliding down slope sideways (surface angle?)
- move stride ellipse so it's not centered on the leg (e.g. right leg can extend more to the right)

# ik arms
- move search dir according to...velocity? input?

# surface
- unrealized motion (minmove) be converted into inertia?
  - set velocity to zero on min_move?

- maybe? move off wall on jump?
- indication of change in perception (particle effect), to help with knowing that you missed a wall

# jump
- jump ik legs 
- jump di? (input direction)
- canceling some proportion of momentum in input opposing velocity (di?)
- jump leniency on surface transfer (perception?)
- real jump charge (small jump -> tangent, big charge -> normal, or maybe the other way around)
- cooldown on surfaces (?)

# crouch
- tilt character when crouched
- aerial crouch rotation
- interpolate between crouch & ik legs
- crouch ik slide
- look into crouching sliding down ramps
- crouch gravity (previously, fastfall, fastslide)? (per jump)
- tuning

# pivot
- make pivot faster (or at least be able to do so)

# flower
- flower disabling zones

# particles
- wall particle proportional to wall influence
- wall particle indicates state (attack, hold button)
- wall particle indicates direction of transfer

# tools
- Vector3 drawer that rounds to a reasonable number of digits
- fix character state preview in shortcuts so it's more useful
- make fields toggleable
- make fields readonly

# others
- wall gravity doesn't take yoshiing/regrabbing into account
- falling while holding jump (no jump squat) doesn't trigger fall gravity

- checkpoint system and state overhaul
- disable all colliders in character on load (how to do this in a nice way)
  - hairbones: since they get detached from the the body, they need to respond to some sort of event (the hair container could listen to)
- intermediary fun, make it possible to rotate ice cream around in the bubble

~ ui save paths are not accurate