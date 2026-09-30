# Gorilla Cosmetics

Custom hats and materials for the current PC version of Gorilla Tag.

## Installation

Requires BepInEx 5. The game supplies Newtonsoft.Json; Utilla and MonkeMapLoader are no longer required.

Extract the release ZIP into the game folder, or copy the built DLL to `BepInEx/plugins/GorillaCosmetics/GorillaCosmetics.dll`.

Put `.hat` and `.ghat` packages in `BepInEx/plugins/GorillaCosmetics/Hats`. Put `.material` and `.gmat` packages in `BepInEx/plugins/GorillaCosmetics/Materials`.

Press **CUSTOM** beside the wardrobe hat button. Use **HATS**, **MATERIALS**, and the page arrows to choose cosmetics. Press a selected cosmetic again to remove it. Press **CUSTOM** again to restore the normal wardrobe.

Selections are saved in PlayerPrefs. Other players need this mod and matching cosmetic packages to see them. Packages marked as disabled in public lobbies are only applied offline or in private rooms.

Invalid packages are logged and left untouched. Old bundles may need rebuilding for the current Unity version.

## Building

Requires the .NET SDK and an installed, modded game.

```powershell
dotnet build GorillaCosmetics.sln -c Release
```

For another install location, pass `-p:GamePath="D:\Games\Gorilla Tag"`.

Output: `GorillaCosmetics/bin/Release/netstandard2.1/GorillaCosmetics.dll`.

Run `GorillaCosmetics/MakeRelease.ps1` to package the DLL and any local Hats and Materials folders. Builds only copy the DLL to the game when `-p:DeployToGame=true` is supplied.

## Cosmetic creators

Packages contain `package.json` and the PC asset bundle specified by `pcFileName`. Hats use a `_Hat` prefab; materials use a `_Material` prefab with a renderer. Metadata includes `descriptor.objectName`, `author`, and `description`; optional config fields are `customColors` and `disableInPublicLobbies`.

## Disclaimer

Not affiliated with, endorsed by, or sponsored by Gorilla Tag or Another Axiom LLC. Portions of the materials are property of Another Axiom LLC.
