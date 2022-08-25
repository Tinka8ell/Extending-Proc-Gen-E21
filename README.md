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

#### A Plan!

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
* May also consider any changes so value\[x, y\] relates to cordinate (x, y) and Vector3(x, value\[x, y\], y)
* Finally consider a find an island method, where we start at (0, 0) and go west in chunks till we find land, 
and then back east 1 chunk and move west to find it's shore?  Just a thought.
