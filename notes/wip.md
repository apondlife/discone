what is projected
---

# inertia & movement

- unrealized motion (minmove) be converted into inertia? 
  - set velocity to zero on min_move?
- lose inertia on wall transfer?
- negative walls feel like they have higher gravity

- ground velocity vs. surface velocity (friction on walls? and jumping?)
- wall slide as floating state in movement?
- wall system preserves momentum into wall (decaying)

- maybe? move off wall on jump?

# particles 

- wall particle proportional to wall influence
- wall particle indicates state (attack, hold button)
- wall particle indicates direction of transfer

- checkpoint system and state overhaul:
- disable all colliders in character on load (how to do this in a nice way)
  - hairbones: since they get detached from the the body, they need to respond to some sort of event (the hair container could listen to)
- intermediary fun, make it possible to rotate ice cream around in the bubble

- look into crouching sliding down ramps
~ ui save paths are not accurate