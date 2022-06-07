# Hue Shifter

**Warning: I'm not good at shader stuff, might cause graphical glitches or performance issues**

This mod applies a hue shift shader to all things that normally have the Sprites/Default or Sprites/Lit shaders
(most static map objects)

If you want to use it as a cool PaletteSwapper alternative, you can just set a global phase or
enable per-room or per-area phase randomization.

If you want it to make everything majestic rainbows, whack the X and/or Y frequencies to at least +/- 4, but be warned that this setting may make animated sprites flash different colours.
The time frequency setting will animate through the colours of the rainbow.

The "Respect Lighting" setting controls whether sprites that are normally tinted by the ambient light in vanilla still are.
Turning it off will prevent the lighting from damping down the colours but it will break PaletteSwapper.

TODOs:
+ Add more hooks and things so objects that get created after the scene load are affected
+ Add grass support (will require replicating the game's grass waving shaders)
+ Figure out why a couple specific screens (eg Quirrel bench, hive bench) are laggy for me
+ Maybe hue shift the lights too.
+ Eventually maybe make CK but for shaders?