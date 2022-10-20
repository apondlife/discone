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

consider contact offset not related to normal, there's no surfaces in contact offset world, just points

we are not sphere casting, there's no way to make sure we are always contact offset away from stuff

some thoughts:
  around 263: moveDst = hitCapsuleCenter - cast.Direction * m_ContactOffset;
