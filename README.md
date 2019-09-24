# AC Ink Integration

## Quick Start

1. Setup an Adventure Creator project as usual or open an existing one.
1. Add the Ink Unity package from the Unity Asset Store.
1. Add the AC Ink Intergration package.
1. Attach the ACInkIntegration script to the Persistant Engine.
1. Drag your Ink story JSON file to Main Story in the ACInkIntegration inspector on the Persistant Engine.
1. Create a string global variable in the variables manager and note its ID number
1. Set Var ID to the ID number in the ACInkIntegration inspector on the Persistant Engine.
1. Use the Ink Run Script action in your Action Lists.

## Introduction

This package allows the use of your Ink scripts with Adventure Creator. It is designed to minimally impact Adventure Creator so no files from the Adventure Creator asset are modified.

You can play your Ink scripts as you would expect, and you can also run certain actions from your Ink script by using certain tags. You can play character animation, move characters, face characters in different directions, wait, play sound effects and music.

It's still a work in progress, so use at your own discression.

## Examples

In the Examples folder short scripts demonstrating the integration will be made available.

## Third Party: Ink Script Action

This package gives you a new action located in Third Party and Ink Script. The Unity package will place it in the correct location. If you add the script manually then it must be located in 'Assets/AdventureCreator/Scripts/Actions/'. 

It has several fields:

### New Story?

Ticking this will reset the state of your story, ideal for starting a new game.

### Knot/Stich

Allows you to specify where in your story to begin. If left blank the story will run from the top.

### Conversation

A ***blank conversation object is required*** in your scene. It should have no dialogue options, they will be generated from the Ink story. In the inspector it's interaction source must be set to "Custom Script", Auto-play lone option must be unticked, the script currently auto plays lone options. 

### Number of Speakers

Set this to the number of characters that will take part in the story. It will create blank fields for each speaker, and then link them to the characters in your scene. **It's not nessesary to add the player.**

### Number of Markers

If you move characters around from the Ink script then you need to pass the markers to the action. Input the number of markers you will use and link accordingly.

### Number of Sounds

If you play and sound FX from the Ink script then you attach them here. Input the number of sounds you will use and link accordingly.

## Third Party: Ink: Check Visit Count

Another action in the package lets you check the visit count of a knot/stitch. It's located in Third Party and Ink: Check Visit Count

It has the following fields:

### Knot/Stitch

The name of the knot or stitch to check.

### Condition

The operation to perform, either eqauls to, not equals, greater than or less than.

### Number

The number to compare with.

### Condition Met/Not Met

The flow of the action list continues based of the result of this action.

## ACInkIntegration class

This class is required to save the story state in Adventure Creator's save system. Open up the **Persistant Engine** and attach the ACInkIntegration script. In the inspector two fields are availble to you:

Your main JSON file from your Ink story goes in the Main Story field. It is assumed that you will use one monolithic story for your game.

To enable saving you need to create a global variable in the Variables Manager of the string type. You can call it anything, but note its ID number and set Var ID to that number.

## Setting Default Behaviour

It's expected that speech and actions in your game have a standard way of presentation. You can set the default behaviour by changing the initialisation of these variables in ActionInkRunScript.cs. To get a different behaviour you will need to add extra tags or parameters in your Ink script, see below for more information on tags. 

In all cases if you want the default behaviour then you will not need to specify with tags in your Ink script.

```c#
static AnimOptions defaultAnimOptions = new AnimOptions(0, 0, false, true);
static FaceOptions defaultFaceOptions = new FaceOptions(true, false);
static MoveOptions defaultMoveOptions = new MoveOptions(false, true, true);
static MusicOptions defaultMusicOptions = new MusicOptions(MusicAction.Play, false, false, false, false, 0.0f);
static SoundOptions defaultSoundOptions = new SoundOptions(false);
static SpeechOptions defaultSpeechOptions = new SpeechOptions(false, true, false);
static SwitchCameraOptions defaultCameraOptions = new SwitchCameraOptions(0.0f, MoveMethod.Linear, false, false);
```

## Dialogue Lines

When writing your script with Ink, start the line with the character speaking and a colon.

```
Player: Hi everyone, let's play a game!
```

If no speaker is specified it will be considered as narration by Adventure Creator.

You can add tags to the line, as per the Ink documentation tags can appear before a line, or on the same line. Whitespace and case are not important, but they must be spelt correctly. Tagging is not required where the default behaviour is required.

### BG/noBG

Let's you specify whether a line will play in the background or not.

```
Player: This is a background line. #BG
Player: This is not a background line. #noBG
```

### skip/noSkip

Let's you specify whether you can or cannot skip a line.

```
Player: You can skip this line. #skip
Player: You can't skip this line. #noSkip
```

### anim/noAnim

Let's you specify if the speaker should use its talking animation.

```
Player: My talking animation will play. #anim
Player: My talking animation will not play. #noAnim
```

### audio

Let's you specify the audio file name, file extension is not required. They should be kept in 'Rescources/Speech/'

```
Player: This line has voice over. #audio = Player1
```

Note on audio: There's no real clean way to integrate audio with Adventure Creator, this integration plays the audio but the Speech class is unaware that there is audio. When the audio is done it moves on to the next line of dialogue. If your minimum display time is shorter than the length of the audio then it will be cut early. One solution is, in the Speech Manager set Display Subtitles Forever to on. This will still move on to the next line automatically once speech has finished, but if there is no audio then it will display until the player manually skips.

In the ACInkIntegration class there is a commented out menu item function that will go through your ink files add a lineID tag and add them to the Speech Manager, this can give the desired behavoir but it's unfinished and only reccomended if you understand what it is doing.

## Scripting actions for Adventure Creator in Ink

Using tags you can run other actions from your Ink script, this will allow you to stay in your script for longer and potentially cut down the size of your action lists. The tags are passed to the action in a list and will execute in order and before the line that they are attached to is displayed, some of the tags also have optional parameters so you can specify the behaviour. These mirror the options on the respective action in Adventure Creator.

Parameters must be split with commas.

### Animate

Plays an animation on a character.

```
#animate Player, WaveToCamera
```

You can add the following parameters:

* layer - Mechanim layer
* fadeTime - Transition time
* idle/noIdle - Reset to idle after.
* wait/noWait - Wait for the animation to finish.

```
#animate Player, WaveToCamera, layer = 0, fadeTime = 0, idle, noWait
```

### Move

Moves a character to a marker.

```
#move Player, MyMarker
```

You can add the following parameters:

* run/noRun - Run or walk
* path/noPath - Use pathfinding or not
* wait/noWait - Wait for character to finish moving.

### Face

Turn a character to face a direction.

```
#face Player, left
```

Possible directions are: down, left, right, up, downLeft, downRight, upLeft, upRight

You can add the following parameters:

* instant/noInstant - Turn instantly or not.
* wait/noWait - Wait to turn or not.

### Set

Set a Global Variable in the Vairiable manager to a value. Uses the label name of the variable.

```
#set playerHealth = 100
```

### Wait

Wait for a period of time.

```
#wait 1.0
```

### Music

Plays, stops, resumes or crossfades music. Music must be in the Music Storage Window.

```
#music myMusicFileName
```

Settng the track name to 'stop' will stop the current track.
Setting the track name to 'resume' will resume the current track.

You can add the following parameters.

* loop/noLoop - loops the track or plays once
* queue/noQueue - plays the track after the current track, or interupts
* resume/noResume - Resumes if was played before, or starts from the beginning.
* wait/noWait - waits for track to finish or not
* fade - sets a fade time or crossfade time e.g. fade = 0.5
* play - sets the method to play
* crossfade - sets the method to crossfade

### Sound

Plays a sound file in the same way as the Play Sound One Shot action.

```
#sound mySoundFile
```

You can add the following parameters.

* wait/noWait - wait for the sound to finish or not.

### Camera

Switchs to another camera in the scene.

```
#camera newCamera
```

You can add the following parameters.

* time - transition time e.g. time = 3.5
* linear/smooth/curved/easeIn/easeOut - sets the move method (not case sensitive)
* retainSpeed/noRetainSpeed = smooths the transition from previous camera
* waitFinish/noWaitFinish = wait for transition to finish, or not.

### Inventory

Add or remove items from the current players inventory.

```
#inventory add, Key

#inventory remove, Fish
```

### ToScene

Switchs the scene by name.

```
#toScene MyNewScene
```

## Get Adventure Creator Global Variables in Ink

You can read global variables from the Variable Manager in your Ink scripts. Add the following to the top of your Ink story file:

```
EXTERNAL getValue(a)
```

This takes an integer which is the variable ID in the Variables Manager.

## Check if an object is in the players inventory in Ink

You can check for an item in the players inventory in Ink scripts. Add the following to the top of your Ink story file:

```
EXTERNAL inventoryContains(item)
```

Item is the name of the object which must match the name in the Inventory Manager, it returns a bool.


