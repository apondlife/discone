what is projected
---

# ik legs
- inclines: surface velocity for legs?
- rotation & placement
- foot height: tune during stride

# ik arms
- fix arms

# surface
- unrealized motion (minmove) be converted into inertia?
  - set velocity to zero on min_move?

- maybe? move off wall on jump?
- indication of change in perception (particle effect), to help with knowing that you missed a wall

# jump
- cooldown on surfaces
- tuning proportional to charge time
- uncharged jump is a cute hop

- canceling some proportion of momentum in input opposing velocity
- jump di? (input direction)
- real jump charge (small jump -> tangent, big charge -> normal, or maybe the other way around)

# crouch
- crouch ik slide

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

- checkpoint system and state overhaul:
- disable all colliders in character on load (how to do this in a nice way)
  - hairbones: since they get detached from the the body, they need to respond to some sort of event (the hair container could listen to)
- intermediary fun, make it possible to rotate ice cream around in the bubble

- look into crouching sliding down ramps
~ ui save paths are not accurate