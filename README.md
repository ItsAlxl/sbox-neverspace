# Neverspace
A short puzzle-ish game made for [s&box](https://sbox.game/pwegg/neverspace) featuring mechanics that bend the rules of physics.

- Portals: Teleport from one portal to another. Each portal defines a target portal, so they can be one-way, two-way, chained, etc.
- Gravity: Planetoids and walkways can affect the gravity of nearby objects.
- Perception: Events are triggered when an object enters/exits/occludes the player's view.

## License
The code is released under the [MIT license](LICENSE), which is a permissive license. Here's the [TLDR](https://www.tldrlegal.com/license/mit-license).

## Issues
There are few issues and hacky workarounds present. You should be aware of these if you intend to reuse any of the code.

### AfterUI
This is a really, really bad one. [Look at it](code/ForceAfterUI.cs), it's bad. I'm sorry.

Neverspace forces **all ModelRenderers** to set the After UI render flag. It does this *every frame*, which is completely unnecessary, but it was the easiest way for me to guarantee that it gets set for every ModelRenderer.

So why did I do this? Well, at the time of writing, s&box has a bug that affects the color of unlit materials. This means that a portal would be significantly darker than the objects it was depicting (the stuff on the other side of the portal). As a result, the portal looked out of place and the user would see a very noticeable jump in brightness when passing through a portal.

Weirdly, this problem doesn't occur if the portals are set to render After UI - but then they render on top of everything else, including other stuff in 3D, which is no good. So if the only way to get portals to look right was making them render After UI, then *everything* had to render After UI.

I believe this also means that they'll all draw on top of the UI, so you can't have a UI. Similarly, you can't have world panels - they get drawn behind the other 3D stuff.

Anyway, here are a couple issues from the s&box github so you in the future can investigate whether or not the problem has been fixed: [#6111](https://github.com/Facepunch/sbox-issues/issues/6111), [#6450](https://github.com/Facepunch/sbox-issues/issues/6450).

### Portal Cutoff
Ideally, as an object is passing through a portal, it will not render any part of itself beyond the portal. This *is* implemented in Neverspace, but only kinda.

On the first implementation, when an object was partway through a portal, there was a razor-thin gap between the object and the portal. It was extremely small, but noticeable (particularly in motion), which ruined the whole effect.

I believe the small gap was created by two different effects working together. One is the clip shader (which "cuts" the model) and the other is the oblique camera matrix for the portal. So to close the gap, I've added a "buffer" in those two places, so that the object and its clone on the other side of the paired gateway overlap slightly. There's a hardcoded `+ 0.2` in the clip shader and a `MIN_OBLIQUE_GAP` constant in `Gateway.cs` that create these buffers.

However, this negatively impacts both effects. Putting an object halfway through a portal and walking around to the other side, you will be able to see a thin slice of the object poking through the portal (a result of the clip shader's buffer). If you put an object next to a portal, walk through it, and walk around to the other side, you will be able to see a little bit of that object, even though you'd now be looking at the side of the portal that shouldn't see it at all (a result of `MIN_OBLIQUE_GAP`).

In theory, this would be solvable by figuring out which objects *should* get overdrawn, and only draw the extra buffers for them, but that's more difficult than I was willing to do, as the current implementation works for everything except some edge cases that are mostly unattainable in Neverspace due to the level design.

### Walkway Out-of-Bounds
It's pretty easy to fall out of bounds when walking off of a walkway, particularly if you're upside-down. This is probably as simple as tweaking the GravHop code present in `GravityPawnPlayer.cs`.

### Rooms
The room system is a probably-unnecessary optimization. I was worried about all those portals, each with their own camera, all rendering at the same time - so the room system enables/disables portals as needed, to reduce that work.

Unfortunately, there's a bad side-effect. The visuals of one of the portals in Neverspace completely breaks if it's disabled and then enabled. I believe it's an issue with the camera, but this *only* occurs in-game, and *not* in the editor (which made it really annoying to debug!).

As a workaround, the gateways have a `ForceUntilUnloaded` property, which prevents it from being disabled by the room system until after the first time it's been enabled by the system.
