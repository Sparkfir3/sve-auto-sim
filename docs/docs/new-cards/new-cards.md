---
title: Creating New Cards
nav_order: 3
---
To add new cards, they must be written in an SVE script `.txt` file, imported into Unity, and compiled, and the image downloader needs to be updated to support the new cards.

---
# Updating the Image Downloader
The image downloader should be updated before compiling new SVE script cards, otherwise new cards will not have any images after they are added.

- In the `DatabaseController` prefab (in the prefab editor, *not* the scene object inspector):
	- Select the `DatabaseImageDownloader` child game object in the hierarchy
	- In the inspector, update the `Download Settings` variable with the new card ID ranges to download

\[TODO - add image of download settings here\]

- Launch and play the `MainMenu` scene
- If the "Downloading Card Images" window appears and no errors appear in the Unity console, the new images are downloading properly
	- If there are errors in the console, the download settings are likely incorrect
	- Check that the ID ranges are valid

Downloaded images can be found in the [Unity Persistent Data Path folder][Persistent Data Path], under the subfolder path: `uSVE Sim Team/Unofficial SVE Simulator/SVESimData/Images`

---
# Compiling SVE Scripts
To compile and test SVE scripts:
- Place the script (as a `.txt` file) into the `Data/Resources/SveScripts` folder
	- Place into the appropriate subfolder, or create a new one if needed
- In the `MainMenu` scene:
	- Select the `DatabaseController` game object in the hierarchy
	- In the inspector, press the `Generate Library` button
	- If no errors have appeared in the Unity console, the scripts have compiled successfully. Otherwise, fix the errors and try again
	- If the scene is marked as dirty after compiling, do not save/discard all scene changes
- Load the game, and check that the cards appear in the deck builder
	- If the image downloader was not updated beforehand, the cards will appear but without an image

If new effects/keywords/etc are not compiling, make sure that the SVE script parser has been updated to handle the new items correctly.

[Persistent Data Path]: https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Application-persistentDataPath.html
