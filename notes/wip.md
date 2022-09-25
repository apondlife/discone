what is projected
---

> camera and walls are jank

> need to disable culling? > stop culling

> input frame has default values, is that bad?
> extending characterinputsource by making the frame type a generic

> do we have 2 different inputs for crouch and save?

> sniff flowers, automatically (bots try sniffing a close flower and planing one otherwise)
> CharacterCheckpoint.Server_OnSimulationChanged should move to discone character (bots smell the flowers)
> flower grab makes less sense with sniffing now (a flower can have multiple owners)
> multiple players on same flower? (isFree needs to be an int)
> flower object pooling, probably just need to sync a list of flowers in the client, and pool locally