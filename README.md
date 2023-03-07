# Extending Proc gen E21
Extend  Sebastian Lague's Procedural Landmass Generation (Proc Gen E21) to generate islands

I am looking to create a "Survival" RPG based on generated islands.

## [What to change and why](./DetailReadMe.md#the-details)

1. [Islands are the wrong scale](./DetailReadMe.md#islands-are-the-wrong-scale)
   * Did not realise this until I added some scaling blobs which made visible how low they were!
1. [Use perlin noise to make islands](./DetailReadMe.md#use-perlin-noise-to-make-islands)
   * Kinda worked, but then they were hard to find and looked too low again!
1. [Change the islandifying parameters](./DetailReadMe.md#change-the-islandifying-parameters)
   * Better but th coasts are too steep!
1. [Sort out the coasts](./DetailReadMe.md#sort-out-the-coasts)
   * Looks better in preview mode, but not when we start the game
1. [Need to get preview / map and game to match](./DetailReadMe.md#need-to-get-preview-/-map-and-game-to-match)
   * Sort of got it working, but need a tidy up
1. [ Add some sea to every terrain chunck](./DetailReadMe.md#add-some-sea-to-every-terrain-chunck)
   * Got that working, but switching between 1st person and Ethan is frought!
1. [Use 1st and 3rd person prefabs](./DetailReadMe.md#use-1st-and-3rd-person-prefabs)
   * Installed and working StarterAssets:
     * First Person Controller
     * Third Person Controller
   * Also got third person replaced with an UMA!
   * Now I really don't like the water - to shimmery - possible too detailed plain
1. [Make the water nicer](./DetailReadMe.md#make-the-water-nicer)
   * Seems to difficult for me at the moment
1. [Better animations](./DetailReadMe.md#better-animations)
1. [Devion Inventory System](./DetailReadMe.md#devion-inventory-system)
   * Managed to get inventory working with UMA, but not got Devion 3rd Person Working
1. [Back to Annimations](./DetailReadMe.md#back-to-annimations)
   * Lets try and extend Starter Assets 3rd Person to Devion animations
   * Also look at Getting UI to use new input manager
1. [Day and Night](./DetailReadMe.md#day--night--tides)
   * Got the day / night cycle working with tides
1. [Time to Add Scenes](#time-to-add-scenes)
   * Start Game
   * Create Avatar
1. [Game Load and Save](./DetailReadMe.md#game-load-and-save)
   * Initialise world data
1. [Update on skunk works](./DetailReadMe.md#update-on-skunk-works)
   * [Completed UI for Game-Start scene](./DetailReadMe.md#completed-ui-for-game-start-scene)
   * [Sorted out a lot of missing GameObjects](./DetailReadMe.md#sorted-out-a-lot-of-missing-gameobjects)
   * [Looked at returning to StarterAssets](./DetailReadMe.md#looked-at-returning-to-starterassets)
   * [Explored more on animations](./DetailReadMe.md#explored-more-on-animations)

## [Future or Wild ideas](./DetailReadMe.md#future-or-wild-ideas)

1. [The next stage](./DetailReadMe.md#the-next-stage)
   * [Clothing and Decoration](./DetailReadMe.md#clothing-and-decoration)
   * [Inventory System](./DetailReadMe.md#inventory-system)
   * [Better animations](./DetailReadMe.md#better-animations-1)
   * [Standard assets](./DetailReadMe.md#standard-assets)
   * [Biomes](./DetailReadMe.md#biomes)
   * [Game start](./DetailReadMe.md#game-start)
1. [Back burner](./DetailReadMe.md#back-burner)

