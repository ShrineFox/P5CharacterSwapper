# P5CharacterSwapper
Batch-replaces P5 character models/animations by ID.

Can retarget models and animations if specified, or fallback to a certain GMD.

Uses TGE's [GFDLibrary](https://github.com/ShrineFox/P5CharacterSwapper/releases) and [SimpleCommandLine](https://github.com/TGEnigma/SimpleCommandLine). No input files will be overwritten.

![](https://i.imgur.com/mePBYGB.png)
# Using
1. Download the [latest release](https://github.com/ShrineFox/P5CharacterSwapper/releases). 
2. Run P5CharacterSwapper.exe from the command prompt to see options.
3. Enter your command and wait for it to finish replacing and/or retargeting.
4. Copy the newly generated folder to your mod and Build!
5. As this tool uses GMD ID 051 by default to retarget animations, you'll want to use the [P5 Modding Community Patches](https://shrinefox.com/PatchCreator) (and [this softlock fix mod](https://cdn.discordapp.com/attachments/681270126657798295/765992608929021972/p5_community_patches_softlock_fix.7z)) to disable other GAPs that would otherwise interfere with these changes.
## Example
`P5CharacterSwapper.exe -o "D:\Games\Persona\Backups\Persona 5\data\model\character\0001" -n "D:\Games\Persona\Backups\Persona 5\data\model\character\0006" -gmd-r -gap-r -gap-rt`
This would replace all of the protagonist's GMDs and GAPs with the Makoto's, retargeting any of Makoto's animations to the protagonist's default model (051) when no matches are found. Doing so allows you to use the NEW character in place of the OLD character while using as many of their compatible animations as possible. Note that due to height differences and missing animations, not everything will look right, but it's a good place to start for manual editing.
`P5CharacterSwapper.exe -o "D:\Games\Persona\Backups\Persona 5\data\model\character\0001" -n "D:\Games\Persona\Backups\Persona 5\data\model\character\0006" -gmd-rt`
This will retarget all of Makoto's models to the protagonist's and replace them, leaving animations untouched. This allows for full compatibility with all of Joker's animations.
# Building
Import this project into your [GFD Studio solution cloned from here](https://github.com/TGEnigma/GFD-Studio) in order to build it.
