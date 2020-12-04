# Scene Hierarchy Keeper

[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/brunomikoski/SceneHierarchyKeeper/blob/develop/LICENSE)
[![openupm](https://img.shields.io/npm/v/com.brunomikoski.scenehierarchykeeper?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.brunomikoski.scenehierarchykeeper/) 

[![](https://img.shields.io/github/followers/brunomikoski?label=Follow&style=social)](https://github.com/brunomikoski) [![](https://img.shields.io/twitter/follow/brunomikoski?style=social)](https://twitter.com/brunomikoski)

Sometimes when you are working on a specific feature or a part of a level inside a scene, and you want to tweak see it between Editor/Play time, unity always reset the hierarchy expand from all the items? *Scene Hierarchy Keeper* is here to rescue!
*Scene Hierarchy Keeper* stores the hierarchy status for each scene, keeping exactly as you left before switching scenes/playing the game/editing the game.
Stop losing your flow searching for things! 


![example](/Documentation~/example-usage.gif)


## Features
- Store hierarchy state for all the items between play/editor mode
- Also store when regularly editing multiple scenes, you always gonna see it how you left it.

## How to use
1. Toggle the *SceneHierarchyKeeper* by the menu `Tools/Scene Hierarchy Keeper/Toggle Scene Hierarchy Keeper`

## System Requirements
Unity 2018.4.0 or later versions


## Installation

### OpenUPM
The package is available on the [openupm registry](https://openupm.com). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

```
openupm add com.brunomikoski.scenehierarchykeeper
```

### Manifest
You can also install via git URL by adding this entry in your **manifest.json**
```
"com.brunomikoski.scenehierarchykeeper": "https://github.com/brunomikoski/SceneHierarchyKeeper.git"
```

### Unity Package Manager
```
from Window->Package Manager, click on the + sign and Add from git: https://github.com/brunomikoski/SceneHierarchyKeeper.git
```
