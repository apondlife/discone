what is projected
---
- negative walls feel like they have higher gravity

- wall particle proportional to wall influence
- wall particle indicates state (attack, hold button)
- wall particle indicates direction of transfer

- ground velocity vs. surface velocity (friction on walls? and jumping?)
- wall slide as floating state in movement?
- wall system preserves momentum into wall (decaying)

- maybe? wall system no wall gravity (all friction??)
- maybe? cache wall transfer on transfer to prevent adding magnet and input every frame
- maybe? move off wall on jump?
- maybe? vertical attack on wall-to-wall transfer

- checkpoint system and state overhaul:
  - disable all colliders in character on load (how to do this in a nice way)
    - hairbones: since they get detached from the the body, they need to respond to some sort of event (the hair container could listen to)
  - intermediary fun, make it possible to rotate ice cream around in the bubble

- look into crouching sliding down ramps
~ ui save paths are not accurate

