# Extending Proc gen E21
Extend  Sebastian Lague's Procedural Landmass Generation (Proc Gen E21) to generate islands

I am looking to create a "Survival" RPG based on generated islands.

## What to change and why

1. [Islands are the wrong scale](#islands-are-the-wrong-scale)
   * Did not realise this until I added some scaling blobs which made visible how low they were!
1. [Use perlin noise to make islands](#use-perlin-noise-to-make-islands)
   * Kinda worked, but then they were hard to find and looked too low again!
1. [Change the islandifying parameters](#change-the-islandifying-parameters)
   * Better but th coasts are too steep!
1. [Sort out the coasts](#sort-out-the-coasts)
   * Looks better in preview mode, but not when we start the game
1. [Need to get preview / map and game to match](#need-to-get-preview-/-map-and-game-to-match)
   * Sort of got it working, but need a tidy up
1. [ Add some sea to every terrain chunck](#add-some-sea-to-every-terrain-chunck)
   * Got that working, but switching between 1st person and Ethan is frought!
1. [Use 1st and 3rd person prefabs](#use-1st-and-3rd-person-prefabs)
   * Installed and working StarterAssets:
     * First Person Controller
     * Third Person Controller
   * Also got third person replaced with an UMA!
   * Now I really don't like the water - to shimmery - possible too detailed plain
1. [Make the water nicer](#make-the-water-nicer)
1. [The next stage](#the-next-stage)
   * Standard assets
   * Biomes
   * Game start
   * Back burner


## The details ...

### Islands are the wrong scale

Did not realise this until I added some scaling blobs which made visible how low they were!
So what was wrong?  The material and grading looks good, but the "mountains" were
little more than small rises.  So I added another scale altogether.

### Use perlin noise to make islands

Kinda worked, but then they were hard to find and looked too low again! 
Even with a greater height scale, everything looks too rounded. 
So added back the original noise as a texture on top of the island noise, 
but it is just too had to find any islands and the sea floor looks wrong - too bumpy!


### Change the islandifying parameters

After adding, or rather trying to add a map for the islands - a scaled down version of the 
island mesh, so I can try to see where ther islands were relative to me, it all got crazy!

#### The plan at this point:

* First start documenting what I am trying to do.  Hense the catch up above and a plan here.
* Move "islandifying" options from HeightMapSettings into NoiseSettings
* Rationalise them:
  * Created Sea Gradient from deepen see and the other one
  * Move the height scaling from the NoiseSettings to the HeightMapSettings, now 0 => don't do this part
  * Add a Sea Level to the NoiseSettings to be able to make islands more sparse!

#### Details

Now that Noise is normalised from 0 to 1, change it to be from -1 to +1 - nominally either side of sea level.
Use Sea Gradient to make the below sea level structures more extreme (if we need to) - 1 => no change.

Add a Sea Level (-1 < seaLevel < 1, default is 0!) to shift the cut off.
Use seaLevel to 1 as the full +ve range, so:
* range = 1 - sealevel
* height > seaLevel => height = (height - seaLevel) / range
* height < seaLevel => height = seaGradient * (height - seaLevel) / range
* height == sealevel => height = 0!

But as the octaves cause the actual range to change from +/- 1 to:
* sum<from i = 0 < octaves>(persistance ^ i)

But that is the extremes and very unlikely (especially as you get to hight octaves). 
If perlin noise was sinusodial then the "average hight" would be the root mean square 
and so both the original range and with octaves would be reduced by a factor of sqrt(2).
So a more realistic range should be factored to some where between? 
So suggest (1 + sqrt(2)) / 2 or about 1.2 (or a factor of about 0.83).
I was going to clamp values to +/- this range, or modified range, to give flat ocean bed 
but this might also give some flat topped mountains - oops! 
In the end I will go with clamping at the HightMapGeneration stage, 
but only clamp the -ve values.  we will see what we get.

#### Perception vs Reality!

I can't believe how much I was incorrect in the above!  Just goes to shw that reality
and our perception are quite different.
* heightMultiplier was already in HeightMapSettings
* MapGenerator only has one HeightMapSettings as I have not yet taken the new look to the full app!
* MapPreview is the only one with two HeightMapSettings
* As the only differences between the Island and Texture HeightMapSettings is the 
NoiseSettings scale and seaGradient and the HeightMapSettings heightMultiplier, 
perhaps a better implementation is MapGenerator and MapPreview will both use an array of HeightMapSettings
then the presence of each one would generate a seperate HeightMap and they will be combined into one
by addition for the GenerateCombinedheightMap() method

At the same time as adding some compexity, I feel I should remove some superfluous stuff!
* We can drop NormalizeMode as we only use Global anyway, and remove the test and any code that was not Global
* The AnimationCurve heightCurve in HeightMapSettings is usually generating a range from 0 to 1, 
so the minHeight is always 0f and maxHeight heightMultiplier(* 1f)!
* We are replacing the falloff generation so that can go too!

So first job is to clear the dross above and then do the new code, and add some proper TDD tests as we go!

### Sort out the coasts

The statndard annimation cure is intended to do the smoothingm but when applying it to the full range, 
from sea to land, it does the opposite and makes the coasts steeper.  Solution is to use the curve for
just the land and then look at doing the same for twice the range going into the sea as we know 
the sea is likely to be at least twice as deep as the land is high!.  This may need to be modified for
the sea gradient as well as this extends the maximun depth of the sea.

### Need to get preview / map and game to match

The game mode start location does not match either island or terrain mesh views in the Map Preview.
The island map preview does not seem to match the island view either so we can't use it to "move" to
a better location.  On top of this we have not yet implemented the combined map generation in the game mode.
For that matter we have not made it more generic to use the array idea.  So much to do, but where to start?

#### last iteration - I do need some better naming

* KISS - so lets simplify
  * Move combined to use the array, not that we plan to do more than 2, but so we can easily do less
  * Combined, will become just generate, and we will start with a simple (zero height) map and add any that we generate
  * The old generate becomes partial, and as each partial will assume the min = - max so combined can just add these
  * Also make sure they all use one size rather than width and height
  * Steps above completed!
* Add a sample location for the Preview so we can move them around more easily
  * added
* Also work out whether noise sample centre works the same way
  * In GenerateNoiseMap, NoiseSettings.offset and sampleCentre are treated as equals (plus in x direction and minus in the y (z) direction)
  * The offset / sample is also at the same increments as the height map coordinates
  * Effectively we have offsets into the perlin noise of offset = random(but fixed) + settings.offset + sampleCentre
  * We then sample a square offset +/- (size/2, size/2) all / frequency and then / scale
  * If we only had 1 octave (so frequency would be 1):
    * (offset + sample - half-size) / scale to
    * (offset + sample + half-size) / scale 
  * so to keep the same sample / offset when scale is reduced to give bigger part of map:
    * offset and sample need to be increased by that factor
  * well it's a theory, let's try it!
  * Nope! It didn't, but changing to use a map zoom propogated down to GenerateHeightMap() and GenerateNoise() works!
  * To complete this section also adjusted the CombinedHeightSettigns to get something that looks good!
  * Now need to remove the Unity Assets from the project in GIT!
    * that was easier said than done!  Think it is now working, and added an "Ethan", but controls are funny
  * Also added a bit of "sea" using a water effect.  I like it!  Now to add that to terrain chuncks.

### Add some sea to every terrain chunck

Taking stuff from [The next stage](#the-next-stage):
* Create my own Sea effect (copy of StandardAsset profesional water)
* Add start of Biome creation to force each terrain to get a sea
* Exploring the new run time requires 1st person controller as Ethan not working as I would like
* Swapping to 1st person from Ethan caused chaos (again) as need to move the "Viewer" object
* I have got to fix this, and after seeing a couple of videos, I want to go back and try with the new 1st and 3rd person controllers
  * 1st and 3rd swapper using [Unity Chan](https://assetstore.unity.com/packages/3d/characters/unity-chan-model-18705): 
Jimmy Vegas, [Mini Unity Tutorial - How To Switch First Person & Third Person View](https://www.youtube.com/watch?v=nR5P7AH4aHE)
  * [Kickstart your game with First and Third Person Controllers](https://www.youtube.com/watch?v=jXz5b_9z0Bc)

### Use 1st and 3rd person prefabs

1. Add the assest from store - installed and working StarterAssets:
   * First Person Controller
   * Third Person Controller
1. Create a "Player" using both and write a script to swap
   * Sort of:  created a player with both and added an IAMViewer script to the transform objects of each
   * Made that Player object the initial viewer so we have some terrain to land on
   * Result it that the active one will take over as the viewer
1. Add UMA skeleton etc. 
   * After ages searchiong and trying lots of stuff, when back to 
[UMA 101 - Part 2: Up and Running by Secret Anorak](https://www.youtube.com/watch?v=Wse9I72YJvc&t=600s)
   * Added UMA_GLIB to the scene as it's a kind of prerequisite
   * Going to the ThirdPerson PlayerArmature:
     * Deleted Geometry and Skeleton children (after unpacking the Prefab)
     * Added DynamicCharacterAvatar.cs script as a component to it
   * And it just worked! Moves and is animated - Success!
   * For convenience added a presets folder and a preset for the UMA, so I can quickly dress a new one

### Make the water nicer

*  Look at improving the look of the water - try changing the plane used in the prefab
*  May be should make it on the fly to match the terrain chunk?

### The next stage

This is my bucket of things I think of.

#### Standard assets

* The standard assets are not being kept up to date, so I may need to modify them and add them as my own assets and so clean up the add-ins
* Doing the above, I may also add UMA and get some survival cloths, just thoughts for now

#### Biomes

* Need to think about generating and storing trees and grasses to the land.  May be look at mapping biomes in a chunk
* Also need to think about springs and rivers and the way they modify the land
* Thinking about modifying land - what about long shore drift and sand moving round the coast
  * It can then form sand dunes on land
  * It can also reveal stones, rocks and flints, and of course cover them up again
  * Also pre-empts the flotsum and jetsum arriving on the coast and moving off
  * and that then goes to adding waves (from the west) 
  * and then we have to consider weather (rain and wind), oh and tides following the moon
  * and then we have to consider the day / night cycle and how fast and what is it tied to for when we have multiple visitors

#### Game start

* Finally consider a find an island method, where we start at (0, 0) and go west in chunks till we find land, 
and then back east 1 chunk and move west to find it's shore?  Just a thought.

Further thinking on this (at least for now):
* Sort out the "player":
  * Give it a UI
    * Add a switch between 1st and 3rd person
    * Actually, if this is the only thing for now, add a switch key ('3' and '1'?)
* Start the game (for now, so we can "see" it) in 1st person looking down from the west at max height and no gravity
  * After the LOD=0 terrain chunk we are over is complete:
    * Evaluate it
      * Is it sea (all below sea level / low-tide) - add sea **- Added structure to support this!**
      * Is it no sea (all above sea level / high-tide) - never add sea
      * Land locked (all way round the edge above sea level / high-tide) - may need lake water
    * If we are sea then slide one chunk west and repeat
  * If we are not just sea, then identify the coast
    * Is this viable as an island?
    * if not, move on ...
    * Map the island edge (coast) - all "coast" areas get sea added
    * What about "beach" and "cliffs" or "rocks"?
* This is evolving, but how about an "Explorer" "Player" to start with:
  * 1st person view port, no gravity
  * 66" (1.68m) above "sea level" facing east and looking down a bit
  * If terrain is sea then we can start looking, else jump back one chunk (west) and try again!
  * Drift west (over the sea) until we have land below us (low tide?) 
    * 1st coordinate east of us start of "beach"
    * "Beach" is set of triangles with a coordinate between low and high tide
    * Start to steal all connected triangles 
      * by identifying each coord as low, high or beach
      * "walk" from each beach coordinate to each neighbour clockwise until it becomes low or high until all are mapped
    * Each triangle idetified is added to a mesh with a specific colour and removed from the terrain mesh

### Back burner

This is for things I might do, or were a way forward, but the priorities changed.

* Documenting the ThreadedDataRequester, I notice that all queued data gets processed on a frame
  * This may be better throttle to limit how much data gets actioned each frame
* Consider any changes so value\[x, y\] relates to cordinate (x, y) and Vector3(x, value\[x, y\], y)

