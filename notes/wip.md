what is projected
---

# inertia & movement

- wall gravity too strong ? (see negative walls)
- inertia transfer really strong
- lose inertia on wall transfer?
  - need half life on inertia decay
  - investigate interaction w/ perceived transfer scale
- unrealized motion (minmove) be converted into inertia? 
  - set velocity to zero on min_move?
- negative walls feel like they have higher gravity

- ground velocity vs. surface velocity (friction on walls? and jumping?)
- wall slide as floating state in movement?
- wall system preserves momentum into wall (decaying)

- maybe? move off wall on jump?

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

- checkpoint system and state overhaul:
- disable all colliders in character on load (how to do this in a nice way)
  - hairbones: since they get detached from the the body, they need to respond to some sort of event (the hair container could listen to)
- intermediary fun, make it possible to rotate ice cream around in the bubble


- look into crouching sliding down ramps
~ ui save paths are not accurate