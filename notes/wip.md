what is projected
---

# wall
> dont wall on small speends (janky falling animation when walking near walls)
  > apparently harder than imagined, collision system has this
> no regrab wall gravity
> boost speed when hitting wall
> more boost when hittin wall
> better magnet
> tiny jump / ollie?

# crouch
> min speed is so big
> special animation (or lack of) on crouch/jumpsquat land (bhop?)
> input frame has default values, is that bad?

# flowers
> convert from test to real world
> delete when not finding ground

# tools
> test pt script on windows
> bundle dev world.json into build, create user's world.json from bundled copy if none

# icecream model
> make arms better (more taper, close it down towards the end of arms)
> more skew
> make it one mesh for better deformation

# load bubble
> mask everything

# world
> make most colliders convex if possible

# depth shader
> make the dissolve be world space
  > i spent a couple hours trying to get this to work, the most promising version was copying the [code from keijiro](https://github.com/keijiro/DepthInverseProjection/blob/master/Assets/InverseProjection/Resources/InverseProjection.shader), however there's also this [URP solution](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@11.0/manual/writing-shaders-urp-reconstruct-world-position.html), and this [library implementing the urp code in non urp](https://github.com/wave-harmonic/crest/blob/master/crest/Assets/Crest/Crest/Shaders/Helpers/BIRP/Common.hlsl). there also seems to be [this thread improving on keijiros code](https://forum.unity.com/threads/solved-clip-space-to-world-space-in-a-vertex-shader.531492/). an experiment is in the branch worldspacefuzz


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