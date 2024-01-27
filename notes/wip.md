what is projected
---

# bugs/problems
- grip pulls you over corners weirdly (on the frame collision misses) (should it pull you towards the hit point?)
- cant let go of wall when pulling back

# surface
- upwards/downwards upgrip?

- unrealized motion (minmove) be converted into inertia?
  - set velocity to zero on min_move?

- maybe? move off wall on jump?

# jump

- canceling some proportion of momentum in input opposing velocity
- air jump charging
- some kind of tiny bad air jump
- real jump charge (small jump -> tangent, big charge -> normal, or maybe the other way around)
- jump di?

# crouch

- wip

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