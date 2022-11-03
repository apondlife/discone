what is projected
---

# crouch
> make slide affect drag
> min speed is so big
> special animation (or lack of) on crouch/jumpsquat land (bhop?)
> input frame has default values, is that bad?

# flowers
> convert from test to real world
> delete when not finding ground

# tools
> test pt script on windows
> bundle dev world.json into build, create user's world.json from bundled copy if none


# cast thoughts
cast offset seems to me to be more of a property of the casting engine than that of the character controller.
its very weird that it currently affects the character anyway though

we are not sphere casting, there's no way to make sure we are always contact offset away from stuff

we should test the capsule collider in a context in which velocity is not sensitive. i tried setting velocity to 0 on every move and got interesting results...
  contact offset still generates velocity, and we have to deal with that.

we should probably recalculate velocity every collision step, and just have an output velocity, instead of calculating velocity in the very end.
  - assuming linear velocity within a move step, we should be able to know on collision, how much of that move step was elapsed. then we project velocity onto the collision surface and move the rest of the frame in that direction.
    - i tried briefly doing this and still didn't figure it out completely, but the fact that it hasn't really exploded makes me confident in taking this approach


i don't think when we are moving on the ground we should ever hit any part of the capsule that is not the bottom, and looking quickly into it it seems like we do