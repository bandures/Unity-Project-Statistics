/*
roots
- scenes
- assetbundles
- resources folder


goals:
- find duplication? assetbundles - scene/resources


Battlestate
- shadow mesh and normal mesh - compare, are they identical?
- shadow mesh - potentially downsides is that on high distance it can be more detailed than lowest lod
- memory consumption / culling / hierarchy complexity


- BC7 vs DXT5 - improve quality, maybe consider reduce textures size?
- crunch compression - can improve loading time
- temporal antialiasing (follow spotlight guides, turn it on?)


scene
- check objects do they have LODs or not
- report objects without LODs and number of verts/tris on them


Modify Unity
- More profiler tags

Plarium
- Cache
*/