# Hue Shifter

This mod replaces most of the level objects' shaders with variants that rotate the textures around the colour wheel.

If you want to use it as a cool PaletteSwapper alternative, just set the phase or
enable per-room or per-area phase randomization.

If you want it to make everything majestic rainbows, whack the X and/or Y frequencies to at least +/- 4, but be warned that this setting may make animated sprites flash different colours.
The time frequency setting will animate through the colours of the rainbow.

The "Respect Lighting" setting controls whether sprites that are normally tinted by the ambient light in vanilla still are.
Turning it off will prevent the default lighting from damping down the colours but it will break PaletteSwapper.
This setting only takes affect on scene reload.

TODOs:
+ Add grass support (will require replicating the game's grass waving shaders).
+ Replace the current rainbow options with an effect that won't cause issues with animated textures.
+ Maybe hue shift the scene lighting too.
+ Eventually maybe make CK but for shaders?