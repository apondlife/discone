what is projected
---

# bugs/problems
- landing transfer on vertical jump regression
- grip pulls you over corners weirdly (on the frame collision misses) (should it pull you towards the hit point?)

# surface
- unrealized motion (minmove) be converted into inertia?
  - set velocity to zero on min_move?

- maybe? move off wall on jump?

# jump
- cooldown on surfaces
- tuning proportional to charge time
- jump di? (input direction)
- uncharged jump is a cute hop
- canceling some proportion of momentum in input opposing velocity
- real jump charge (small jump -> tangent, big charge -> normal, or maybe the other way around)

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