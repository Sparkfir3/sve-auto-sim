# Unofficial SVE Auto Simulator
An unofficial automated simulator for Shadowverse: Evolve

Developed using Unity version 2022.3.62f3

## Project Cloning & Set Up
IMPORTANT: This repository is *not* the complete Unity project folder for the simulator, and is only the main assets folder for the project, containing all custom scripts/data/etc, without any third-party plugins. Due to the use of paid/commercial third-party plugins, which cannot be distributed in open source repos, the complete project folder is not publicly available.

To set up the project:
- Create a new, empty Unity project (version 2022.3.62f3)
   - Make sure to import Text Mesh Pro and its essentials
- Import all required [external plugins](#external-plugin-list) into the project
   - Do not include the CCG Kit demo folder
- Clone this repository into the project's Assets folder, ideally into a subfolder
   - The subfolder can be named anything you want, but personally I use `_Main` (the underscore sorts the folder to the top)

Your folder structure inside Unity should look like this:
```
Assets
├── _Main
│   └─ *this repository here*
├── CCGKit
├── com.rlabrecque.steamworks.net
├── Mirror
├── Plugins
│   ├─ Demigant
│   │  ├─ DemiLib
│   │  └─ DOTWeen
│   └─ Sirenix
└─ TextMesh Pro
```

### External Plugin List
Required:
- <a href="https://assetstore.unity.com/packages/tools/network/mirror-129321">Mirror</a>
- <a href="https://github.com/Chykary/FizzySteamworks/releases">FizzySteamworks</a> 6.0.0
- <a href="https://github.com/rlabrecque/Steamworks.NET/releases">Steamworks.NET</a> 2024.8.0
- <a href="https://assetstore.unity.com/packages/templates/systems/ccg-kit-52739">CCG Kit</a> (paid asset, documentation available <a href="https://www.gamevanillawiki.com/ccg-kit/introduction/">here</a>)
- <a href="https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041">Odin Inspector</a> (paid asset)
- <a href="https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676">DOTween</a>

Recommended:
- <a href="https://github.com/VeriorPies/ParrelSync/releases">ParrelSync 1.5.2</a>
- <a href="https://assetstore.unity.com/packages/tools/utilities/rainbow-folders-2-143526">Rainbow Folders</a> (paid asset)
