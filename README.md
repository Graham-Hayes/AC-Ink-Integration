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

## Third Party: Ink Script Action

This package gives you a new action located in Third Party and Ink Script. The Unity package will place it in the correct location. If you add the script manually then it must be located in 'Assets/AdventureCreator/Scripts/Actions/'. 

It has several fields:

### New Story?

Ticking this will reset the state of your story, ideal for starting a new game.

### Knot/Stich

Allows you to specify where in your story to begin. If left blank the story will run from the top.

### Conversation

A ***blank conversation object is required*** in your scene. It should have no dialogue options, they will be generated from the Ink story.

### Number of Speakers

Set this to the number of characters that will take part in the story. It will create blank fields for each speaker, and then link them to the characters in your scene. **It's not nessesary to add the player.**

### Number of Markers

If you move characters around from the Ink script then you need to pass the markers to the action. Input the number of markers you will use and link accordingly.

### Number of Sounds

If you play and sound FX from the Ink script then you attach them here. Input the number of sounds you will use and link accordingly.

## ACInkIntegration class

This class is required to save the story state in Adventure Creator's save system. Open up the **Persistant Engine** and attach the ACInkIntegration script. In the inspector two fields are availble to you:

Your main JSON file from your Ink story goes in the Main Story field. It is assumed that you will use one monolithic story for your game.

To enable saving you need to create a global variable in the Variables Manager of the string type. You can call it anything, but note its ID number and set Var ID to that number.

## Setting Default Behaviour

It's expected that speech and actions in your game have a standard way of presentation. You can set the default behaviour by changing the initialisation of these variables. To get a different behaviour you will need to add extra tags or parameters in your Ink script, see below for more information on tags. 

In all cases if you want the default behaviour then you will not need to specify with tags in your Ink script.

```c#
static AnimOptions defaultAnimOptions = new AnimOptions(0, 0, false, true);
static FaceOptions defaultFaceOptions = new FaceOptions(true, false);
static MoveOptions defaultMoveOptions = new MoveOptions(false, true, true);
static MusicOptions defaultMusicOptions = new MusicOptions(MusicAction.Play, false, false, false, false, 0.0f);
static SoundOptions defaultSoundOptions = new SoundOptions(false);
static SpeechOptions defaultSpeechOptions = new SpeechOptions(false, true, false);        
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

Let's you specify the audio file name, file extension is not required. They should be kept in 'Rescources/Speech'

```
Player: This line has voice over. #audio = Player1
```

## Scripting actions for Adventure Creator in Ink

Using tags you can run other actions from your Ink script, this will allow you to stay in your script for longer and potentially cut down the size of your action lists. The tags are passed to the action in a list and will execute in order, some of the tags also have optional parameters so you can specify the behaviour. These mirror the options on the respective action in Adventure Creator.

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

### Wait

### Music

### Sound

## Get Adventure Creator global variables in Ink
