# Scene Keeper

[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/brunomikoski/SceneKeeper/blob/develop/LICENSE)
[![openupm](https://img.shields.io/npm/v/com.brunomikoski.scenekeeper?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.brunomikoski.scenekeeper/)

[![](https://img.shields.io/github/followers/brunomikoski?label=Follow&style=social)](https://github.com/brunomikoski) [![](https://img.shields.io/twitter/follow/brunomikoski?style=social)](https://twitter.com/brunomikoski)

Sometimes when you are working on a specific feature or a part of a level inside a scene, and you want to tweak see it between Editor/Play time, unity always reset the hierarchy expand from all the items? *Scene Keeper* is here to rescue!
*Scene Keeper* stores the hierarchy status for each scene, keeping exactly as you left before switching scenes/playing the game/editing the game.
Stop losing your flow searching for things! 


![example](/Documentation~/example-usage.gif)


## Features
- Store hierarchy state for all the items between play/editor mode
- Also store when regularly editing multiple scenes, you always gonna see it how you left it.
- Save your last selection on the scenes
- Force especific game objects to be expanded all the time by `Right Click/Scene Keeper/Always Expanded`

## FAQ
### How do I use this?
Thats the cool part about it, you don't have to do anything, after adding the package to your project every time a scene is opened / closed all the expanded items and selection will be restored.

### How I can turn off?
If its annoying for some reason, or someone don't want to use on your project you can quickly disable it by the menu `Tools/Scene Keeper/Hierarchy/Keep Hierarchy`, this will disable it.

### Can I disable just the selection?
For sure the options is there on the same menu `Tools/Scene Keeper/Selection/Keep Selection`

### Can I disable the selection at runtime?
Yes! If you don't want your selection be stored from runtime you can disable it here `Tools/Scene Keeper/Selection/Ignore Playtime Selection`, so seleciton will be only stored / restored at editor time.


## System Requirements
Unity 2018.4.0 or later versions


## How to install

<details>
<summary>Add from OpenUPM <em>| via scoped registry, recommended</em></summary>

This package is available on OpenUPM: https://openupm.com/packages/com.brunomikoski.scenekeeper

To add it the package to your project:

- open `Edit/Project Settings/Package Manager`
- add a new Scoped Registry:
  ```
  Name: OpenUPM
  URL:  https://package.openupm.com/
  Scope(s): com.brunomikoski
  ```
- click <kbd>Save</kbd>
- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `com.brunomikoski.scenekeeper`
- click <kbd>Add</kbd>
</details>

<details>
<summary>Add from GitHub | <em>not recommended, no updates :( </em></summary>

You can also add it directly from GitHub on Unity 2019.4+. Note that you won't be able to receive updates through Package Manager this way, you'll have to update manually.

- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `https://github.com/brunomikoski/SceneKeeper.git`
- click <kbd>Add</kbd>
</details>
