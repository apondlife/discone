gonna start this to keep track of why tunables were changed

2022.10.26.1

**Camera Follow(third person)**

- Base Distance 10->13: camera felt too close
- Increased Local Speed 13->50: local speed is a weird parameter, and maybe should go, but this was so that the camera would turn around faster


**Icecream**
- Max Lateral Speed 0.15->0.03: becuse of drag decrease you could get too much speed
- Positive Kinectic Friction (0,0)->(1.2, 1.2): still needs to have some friction so that icecream doesnt slide forever
- Positive Drag (0.1, 0.05) -> (0.1, 0.02): make it more slidey!

**Checkpoint(Discone Character)**

- Delay 0.3->0.48: "closing eyes too fast" (mut, ty, julian)
- Smell Duration 0.8 -> 0.97: to follow increase in delay

