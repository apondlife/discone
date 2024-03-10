Script Execution Order
===

i think we lose this, so just to make sure, we should also note here what the order should be, just in case.

maybe in the future we can convert this file into some format and use it instead of unity's solution

- ProvideGameObject asap : (a lot of stuff depends on a game object being provided)
- *Characters before DisconeCharacter* : (spawn event has to be subscribed, and maybe not great to replay it)
- *Online before OnlineInterest* : both singletons, should destroy Online before Checking for duplicate interest