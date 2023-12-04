This repository contains mods that offer minor quality-of-life improvements over the base game of Cities: Skylines II.

**Disclaimer: These modifications are highly experimental. Your game may crash more frequently, and your save files could be corrupted.**

## Mod

* Custom Radio

## Installation

1. If you already have BepInEx 5, proceed to step 4
2. Install [BepInEx 5](https://github.com/BepInEx/BepInEx/releases)
   * Download `BepInEx_x64_5.4.22.0.zip` (or a newer version), and unzip all of its contents into the game's installation directory, typically `C:/Steam/steamapps/common/Cities Skylines II`
   * The installation directory should now have the `BepInEx` folder, the `doorstop_config.ini` file, and the `winhttp.dll` file
3. Run the game once, then close it. You can close it when the main menu appears
4. Download the mod [https://github.com/dragonofmercy/cs2-customradio/releases](https://github.com/dragonofmercy/cs2-customradio/releases)
5. Unzip the downloaded file into the `Cities Skylines II/BepInEx/plugins` folder
   * Don't forget to uninstall the old version of the mod to avoid conflicts
6. Launch the game, the mod should be loaded automatically

## Customize Radios

In the plugin folder you will find a Folder named `Radios`, this is the base folder that create a new Network in the game (just next to "Commercial radios").  
The network name and description can be change using the `meta.json` file inside the `Radios` folder, you can change the SVG file if you want to change the icon.

In the same folder you will find a `Lounge Music` folder, that's a radio sample, you can modifiy it in the same way as the network. This is where you add the various pieces of music you want to listen to.  
**Be careful, the files must be in OGG format**

You can add more Radios if you want, just by create a new folder like `Lounge Music`

### Folder structure

````
├── CustomRadio.dll
├── Radios
│   ├── icon.svg
│   ├── meta.json
│   ├── Lounge Music
│   │   ├── icon.svg
│   │   ├── meta.json
````
## Important Warning

* **Experimental Mod:** Please note that CustomRadio is an experimental mod.

## Feature roadmap

✔️ Loading music files on demand  
✔️ Loading Meta tag from the audio file. (Title, Author, ...)  
✔️ Randomise the clips.  
✔️ Add many Radios as you want

## Thanks

[Cities2Modding](https://github.com/optimus-code/Cities2Modding): An example mod for starting modding in Cities: Skylines II  
[BepInEx](https://github.com/BepInEx/BepInEx): Unity / XNA game patcher and plugin framework  
[Harmony](https://github.com/pardeike/Harmony): A library for patching, replacing and decorating .NET and Mono methods during runtime  
[AlphaGaming7780](https://github.com/AlphaGaming7780): Made the first mod of custom radio where this mod is based on  
