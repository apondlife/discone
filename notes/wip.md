what is projected
---

# scrap notes from businessman
> is the blob shadow from discone or third person? if TP make sure its portable
> character drive event to fix how the camera currently works, its horrible
>

# wall
> don't wall on small speeds (janky falling animation when walking near walls)
> no regrab wall gravity
> tiny jump / ollie?

# crouch
> min speed is so big
> special animation (or lack of) on crouch/jumpsquat land (bhop?)
> input frame has default values, is that bad?

# flowers
> convert from test to real world
> delete when not finding ground

# camera
> start follow from corrected position

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