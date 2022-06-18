# Hue Shifter

This mod replaces most of the level objects' shaders with variants that rotate the textures around the colour wheel.

If you want to use it as a cool PaletteSwapper alternative, just set the "phase angle" or
enable per-room or per-area phase randomization. Note that since the phase angle setting is in degrees 
setting it to 360 wraps around back to vanilla.

If you want it to make everything majestic rainbows, use the X and Y and Z settings to control the gradient strength
in each of the three axes. The Z rainbow setting in particular colors objects based on how deep they are into the background.
The effect can also be animated using the "Animate Speed" slider.

The "Respect Lighting" setting controls whether sprites that are normally tinted by the ambient light in vanilla still are.
Turning it off will prevent the default lighting from damping down the colours but it will break PaletteSwapper.
This setting only takes affect on scene reload.

TODOs:
+ Add grass support (will require replicating the game's grass waving shaders).
+ Maybe hue shift the scene lighting too.
+ Eventually maybe make CK but for shaders?

## Screenshot Gallery

![](https://user-images.githubusercontent.com/106181028/173234232-19ce8379-2dea-40cb-86e4-51dceadeab07.png)
![](https://user-images.githubusercontent.com/106181028/173234464-283515e1-bce1-4570-8c2e-32500b98a06b.png)
![](https://user-images.githubusercontent.com/106181028/173234238-9f0ceae9-11d3-47a3-86bc-ded545be2cc2.png)
![](https://user-images.githubusercontent.com/106181028/173234246-7aa7baae-4dfd-4b84-86d5-b5e7f1301e74.png)
![](https://user-images.githubusercontent.com/106181028/173234234-afd14cf1-eeef-4a18-b7e3-ea243e31d7db.png)
