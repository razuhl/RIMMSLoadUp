# RIMMSLoadUp

This mod adds community solutions for improving load times in Rimworld.

Corresponding [discussion](https://ludeon.com/forums/index.php?topic=47478.30) on ludeon forum.

### Features
- The lookup for types referenced in xml is cached so that the work is not repeated.
- The preview images for mods were loaded during startup but never used. The images are now only loaded when looking at the mod list.
- Corrected xml iterations to avoid unnecessary searches.
- Earlier releasing of xml patch resources to reduce memory impact.

## Load Order
Does not matter for functionality. Mods are loaded in multiple phases and inside each phase by load order. This mod registers it's changes in the earliest phase and affects the later phases.

## No Change
It is possible that the methods employed here are already tucked away in some mod that you already use. They have been layed out in the beginning of 2019 as shown in the linked discussion about load times. E.g. the mod [Multiplayer](https://github.com/Zetrith/Multiplayer) by Zetrith was cited by notfood as the source for the idea of type caching which is already implemented in that mod.

## Disclaimer
Any resources based on or referncing rimworld resources are also subject to rimworlds eula agreement. This includes images, audio but also decompiled sections repeated in this mod. If the origin is unclear it must be assumed that the rimworld eula applies.
