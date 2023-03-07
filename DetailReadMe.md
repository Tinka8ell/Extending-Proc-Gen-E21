# Extending Proc gen E21
Extend  Sebastian Lague's Procedural Landmass Generation (Proc Gen E21) to generate islands

I am looking to create a "Survival" RPG based on generated islands.

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
* Couldn't get my head around Mesh and how to edit / create a more simple plane mesh,
so not going to try for now
* I did discover that my Sea Prefab as using the night material so I now have a Day and Night Sea prefabs
  * Difficult to tell the difference to be honest!
* I really don't like the stiff robot like motion of the StarterAssets Third Person Controller
  * Should I look at adding other / better animations?

### Better animations

Started looking at other anmations so my UMA does not look like a robot!
* Got the StarterAssets to work with new input system
* Want to replace Starter Assets and Standard Assets with the Devion Inventory System

### Devion Inventory System

* Import and get working Devion Inventory and 3rd person Controller with UMA
* Ok, that did not go too well!
  * The inventory system, I have working, ok without items, but ...
  * The Devion 3rd person controller misbehaved
    * Did not seem to cope with the ground, kind kept slipping
    * Meant it would not move.  It turned, but not moved
    * Put it on a plane, and it would move and walk off ok
    * But falling off onto the real ground, went back to not moving
    * Just found it not easy to debug what was wrong
    * Still based around the old Input Manager
  * Back to starter assets, but with inventory - result!
* Now we need to extra annimations from Devion ...

### Back to Annimations

* Well that was not as expected:
  * Lets try and extend Starter Assets 3rd Person to Devion animations
    * After lots of research and all else, did not get anywhere
  * Also look at Getting UI to use new input manager
    * Also much research, and I can see how it could be done, but
      * there are lots of places where Input.Get... would need to be replaced
      * there is so much tied into the Devion structure and MotionState and ...
* Got the Devion 3rd Person Controller to work
  * There was an issue with "falling" and resolved it by re-instating grounding -> stop motion
  * Discovered that there were mutiple "attachements" the the default avatar rig
* UMA-ified the Devion Default Avatar
  * Replace the rig with the UMA one
  * Added the Left and Right Hand Item slots
  * Added the "face cam" to provide the icon picture for the stats UI as well
  * Only thing that does not work correctly is the attachment of equipment
    * Goes to the right place, but the orientation is all wrong, 
    but that is common - got to re-watch the YouTube video on it.
* Now I want to get the "look" right
  * tatty cloths
  * more realistic body (less muscular)
  * Would be nice to add a create avatar scene to select features
    * limit available features to "pale" and "unfit" options
    * so during game time you get more fit and weathered ;-)
    * may be consider starting as a child / teen and grow up during the game?
* Also want to introduce "floppy sleep"
  * When the game starts, you are flopped where you we last / start using rag-doll
  * As you "wake up" you stand up (realistically)
  * The idea is start of game (or if you get disconnected when in water / below high tide) you "survive" on a pallet
  * Intention is that ideally you leave the game by sleeping on a bed
  * but if not you just collapse (Avatar-like) where you are (last saved) and start from there
* Also want to introduce the biomes and tides and day / night

### Day and Night

* Initially just the day / night cycle from the Survival Game
  * Implemented, but has limitations
    * There does not seem to be a moon as the SkyBox only uses one light source
  * Found another system that also generates stars ...
    * [ReCogMission Tutorial](https://www.youtube.com/watch?v=mPS_nRwh_dM)
    * [ReCogMission Code](https://github.com/ReCogMission/FirstTutorials)
    * Incoporated the DayNightController script with additions:
      * Use computer time * speed to get game time
        * Change Update() to be a coroutine, so we can reduce the number of updates per second!
      * Initialise the sun driection using localEulerAngles, so we can put it in a box and turn it.
        * The system works fine moving the sun from North to South, but we want to go East to West
        * By mouning the "Sun" in a GameObject (EastWest), and rotating that object (0, -90, 0) we now go E-W!
      * Modularised the motion and added moon with different cycle lenght
      * Lighting is funny.  
        * Some items glow in the dark (e.g. bushes and the ground) and others go black!
        * Need to fix this
      * Added the Sky at Night too
        * Can't see the stars in the sky, but can reflected in the sea?
      * Using pub / sub to notify ScatterMyStars when they have moved
      * Expanced and reorged the classes to create: 
        * a GameManager class to control time
        * the DayNightController that subscribes to the GameManager.GameClockTickEvent to control: 
          * the sun, 
          * the moon and 
          * the stars
        * the Tides class that that subscribes to the DayNightController.TideMovedEvent to control:
          * whatever it is on (SeaObject) raise and lower it by the tide movement

### Time to add scenes

* Want to add Game Start Scene
  * Initial screen:
    * Use a simple (unmoving main Camera), pointing east at:
    * Generate Terrain for a known scene as background
    * Modify the "Menu" UI to create "Start" menu
      * add an extra "New" button
      * change "LoadSave" into just "Load"
    * Make the initial GameManager the "player" location for the MapGenerator
    * The "X" does the "Exit" action
    * Close on "Options" and "Load" to return to "Start" menu properly
    * Wired up the "New" button to go to the New Screne (Avatar Creator Scene)
  * Added an Avatar creator screen
    * Initially just a clone of the UMA sample screen
  * Added world load and save
    * Just associating a name with HeightmapSettings
    * Added links from GameManger through TerrainGenerator to HightMapSettings
  * To do:
    * Design the Game load and save and new
      * The world
      * The avatar
      * The current time - what is our base (temp and long term)
    * Wire up the "Load" button, one the above it sorted

### Game Load and Save

Design the Game load and save and new
* The world
  * Is defined by:
    * MeshSettings - fixed by game
    * TextureData - fixed by game
    * HeightMapSettings 
      * could be changed to give new world type
      * needs to be serialisable and so loadable 
      * will need a Design Scene, pobabaly using the Preview GameObject with a load and save option
  * What state
    * Record regenerate data per chunk, with a when (see below)
    * Do we need to remember the spaces outside of the island?  (All Sea Chunks)
    * What happens if we manage to get to a new island?
  * When was it last seen
  * Is that per chunck, per island?
* The avatar
  * Need to associate Player with World and World Instance
  * Where and when and inventory
  * What about camp stores?
* The current time - what is our base (temp and long term)

### Update on skunk works

Seems that I have not been planning but just going for it ...

#### Completed UI for Game-Start scene

* Renamed the scenes
* Completed / got working:
  * New Game UI with:
    * Character creator
    * Name character (basically save for re-use)
    * Select world (only default at the moment)
    * Start - change to Game-Main scene with world and character selected

#### Sorted out a lot of missing GameObjects

* Mostly due to do not destroy and singleton mis-matches

#### Looked at returning to StarterAssets

* The frustration with the Devonian code
  * It is far to easy to break
    * Missing objects
    * Just getting the default 3rd person avatar to work
    * Too many ways to drop out of expected environments (e.g. screen size!)
* Code is cleaver and extensible but as clear as mud!
* Struggle to get head round the way Devonian 3rd person controller does actions / animations
* StarterAssets have their own issues
  * Use the CinemachinBrain
  * Possibly using different shader / renderer - led to "pink objects"!
  * Limited animations and they still looked very robotic

#### Explored more on animations

* Found a really helpful YouTube course on [Unity's Animation System](https://www.youtube.com/playlist?list=PLwyUzJb_FNeTQwyGujWRLqnfKpV-cj-eO)
* Now have a better idea of:
  * how animations can interact and 
  * some of the compexity that was being done with Devion code
    * I don't forgive them (no comments and bad error checking), but an apprecaiation of what they were dealing with
  * Retargetting animations and avitars

## Future or Wild ideas

### The next stage

This is my bucket of things I think of...

#### Crate Avatar Scene

* A start up scene, where we can select
  * Body shape
  * Hair style
  * Face features?
  * Initially where I can try out cloth sets
* May be use the Previewer for the ground to look upon

#### Floppy sleep

* Enable exit
  * Avatar is saved
  * Avatar goes floppy
* Enable start / restart
  * Avatar is created from save
  * Avatar goes floppy
  * Camera switches on (may be blurry to start with)
  * Avater stands up 

#### Day / Night / Tides

* Use Survival Game to provide the lighting / sky as a basis - done
* Use clock (from day / night cycle) to adjust height of sea - done
* Enable sea as trigger for the avatar to swim
* Look at introducing the floating emulation code to create waves?
* Add the ability for items to float and be caried by current
* Love to expand the tides - running at 12.5h (1h 25m) cycle time
  * Consider adding Spring and Neap tides ...
  * Consider effect of storm surge?
* Love to add swim trigger
  * concider swim as more tiring than walking
  * treading water is slow recovery?
  * what happens when exhausted?

#### Generate equipment from the Survival Game

* this is more an exercise in the ease of building things
* use the items from the game
* create their prefabs and presence in the datase
* include the building, fire and crafting table
* look at "sleeping" on the bed!

#### Clothing and Decoration

* It would be good to play with the UMA clothing and make some ragged geens and top
  * Get a relistic UMA character to play with
  * Ideas on this would be to add a UMA character creation screen
  * Thought on some evolving ideas ....
    * Over time the character should get a little more tanned - they are now living outside
    * If the character starts of flabby or scinny, then they get "fitter" as they exercise
* Like to add "decoration" -  trees and grass and stuff (part of the Zenva 3D RPG initially)
  * Over tiem use some of the free vegitation resources that I have found, but start with the simple ones
  * Add a game memory system 
    * Use Player Prefs to store and retrieve the initail decoration
    * Generate (for a new island, etc) some random placing of stuff, and then save it
      * Initial "biomes" idea would be to use heights to get % coverage of these decorations
* Switch to the Devion version of the "water" (or my own) and make sure it allows my avatar to swim

#### Inventory System

* Since I last worked on this, I have been exploring free assets ...
  * One of these is [Item & Inventory System](https://assetstore.unity.com/packages/tools/gui/item-inventory-system-45568)
by [Devion Games](https://deviongames.com/)
  * Unfortunately this has not been maintained for a while and is not that inuitive
  * But it does seem to give a pretty good, clean and simple look inventory system
  * As a bonus there is a third person player (he's a bit funny) with some extra animations
* I'd like to replace the StartUp Asset 3rd person animator with the Devion games one
* To go with this, temporarily, add the assets from the Zenva 3D RPG game I trained on
* This should give a good active basics to see were I can go

#### Better animations

Start looking at other anmations so my UMA does not look like a robot!
* I already have the ["free" Basic Motions by Kevin Iglesias](https://assetstore.unity.com/packages/3d/animations/basic-motions-free-154271),
but need to see how to integrate
  * When I have the money I might consider the other packages: 
    * [Basic Motions full version](http://assetstore.unity.com/packages/3d/animations/basic-motions-pro-pack-157744)
    * [Animations Pack Mega Bundle](http://assetstore.unity.com/packages/3d/animations/mega-animations-pack-162341)
* Might be an opportunity to understand the new input system and so replace the StarterAssets with my own
* Also an opportunity to look into annimations

#### Standard assets

* The Standard Assets are not being kept up to date, so I may need to modify them and add them as my own assets and so clean up the add-ins
  * Started this process
* I may also add UMA and get some survival cloths, just thoughts for now
  * Added UMA and is working with StarterAssets

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

