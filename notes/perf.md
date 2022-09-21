perf
---

notes on perf; generally things i don't understand and would like to investigate

# general

with the profiler on, i never really get to 30fps on my computer as host and am often not even 
at 15fps.

# scripts

on the worst frames, we have a cpu-bound perf issue in our scripts (e.g. 257ms just for `FixedUpdate`).
the problems in particular are `DisconeCharacter` & `Character`. a headless server could only be only doing 
this and still struggle. scripts do hit 16ms sometimes :).

> `DisconeCharacter` (281/415ms)

273/281ms is in `Server_SyncState`. of this fn, 53/273ms is spent serializing data. i
think we can alleviate this by some combination of:

* minimizing the data in `CharacterState.Frame`
  * or better, serializing the frame to/from a different struct that we send over the network that  minimizes data
* serializing field separately so that only what's changed is sent over the network 
  * may incur more overhead, not sure what mirror does

digging down into the call stack, the other end of the bottleneck is deserializing all the fields back
into the frame on the client. i think minimizing network data transfer is important. it spends a lot of
time on vectors, charactercollision, and quaternion.

> `Character` (88/415ms)

the pain here is mostly in the controller (not surprising) and maybe in allocations of state frames, can't
really tell.

> `CharacterModel` / `CharacterDust` (17ms/415ms each)

we can turn off everything but the character simulation for characers that aren't in view.

# rendering

terrain doesn't seem to occlude anything, which is a problem for a large world w/ many objects in it. e.g. 
you can be in the forest and it is making draw calls for clearly invisible stuff in the salt flats.

* not occluding static objects? static occlusion)
* not occluding dynamic objects? (dynamic occlusion)

dialog arrows for all characters draw every frame even when hidden.

# unity physics

4-11% (4-40ms) cpu every frame on `FixedUpdate.PhysicsFixedUpdate` which i think we only use for region 
detection, dialogue range, and player perception checks, seems easy and/or possible to optimize out. (the 
capsule casts for character collision are under `FixedUpdate.ScriptRunBehaviorFixedUpdate`, so i don't think 
those contribute to this number). seems like it should be <1ms every frame.

e.g. we could have a single script process all the characters and regions in a batch and doing purpose-
built collision, we don't need arbitrary collider-intersection (at least not for dialogue).
