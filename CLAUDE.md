# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**GameJam3** is a 2D action-adventure narrative game in Unity 6000.6.0f1 (Unity 6). Player controls **Michelle Sato**, a mother on a revenge quest, exploring a Japanese city. The only scene in the build is `In game.unity`.

## Build & Run

- Open in Unity Editor (open `GameJam3` folder). No separate build step needed for development.
- Play: press Play in the Editor — the `In game` scene is the only build scene.
- Build for iOS: `File > Build Profiles > iOS` (XCode project exports to `Builds/`).
- Target platform: iOS.

## Architecture

### Singleton Pattern
Most managers are singletons with a static `Instance` property enforced in `Awake()`:
- `PlayerMovement` — player controller
- `DialogueManager` — dialogue system
- `InventoryManager` — 9-slot inventory
- `AudioManager` — music/SFX/voice
- `SceneController` — scene loading, fullscreen enforcement
- `PlayerCombat` — melee attack system

### State Machines
- `BeggarTrigger.QuestState`: `NotStarted → LookingForFood → YakuzaFight → QuestCompleted`
- `BlacksmithTrigger.BlacksmithState`: `Locked → ReadyForTalk → InCombat → CombatFinished → QuestCompleted`

### Event-based
`CharacterHealth.OnDeath` event — subscribed by `BeggarTrigger` (tracks yakuza kills) and `BlacksmithTrigger` (increments kill count).

### ScriptableObjects
`ItemData` (with `[CreateAssetMenu]`) for inventory items (yakitori, katana, medkit).

## Scripts Location

All C# scripts live in `Assets/Script/` organized by category:
- `/` — player, combat
- `Mechanics/Dialogue/` — dialogue system, NPC triggers (Beggar, Blacksmith, Pedagang)
- `Mechanics/Interaction/` — inventory, items
- `Mechanics/Props/` — health, pickups, enemy AI
- `Movement/` — camera, joystick
- `Service/` — audio, scene loading
- `Story/` — onboarding sequence
- `Environments/` — roof cutout, tilemap fade
- `Main Menu/` — menu controller

## Key Systems

### Dialogue
`DialogueManager` drives all NPC conversations. `DialogueTrigger` (on NPCs) + `DialogueSentence` data. Typewriter effect (0.02s/char). E or Space to advance. Walking away from an NPC calls `CancelDialogue()`.

### Quest Flow
1. Talk to Beggar (Tanaka Koji) → find food quest
2. Buy yakitori from Pedagang → return to Beggar
3. Yakuza shakedown spawns → defeat them
4. Beggar gives intel → directs to Blacksmith (Ito Shun)
5. Talk to Blacksmith → 3-yakuza wave; defeat 2, 3rd flees → receive katana
6. Katana equippable via inventory (toggles weapon sprite on player)

### Player Controls
- WASD / Arrow keys / Virtual Joystick: movement
- J: melee attack (25 damage, cooldown)
- K: shuriken throw
- E / Space: interact / advance dialogue
- Inventory click: use/equip items

### Sprite Animation
Manual frame-cycling via `Sprite[]` arrays + timers (no Mecanim/Animator for characters). Enemies, NPCs, and player all use this pattern.

## Notable Quirks

- `QualitySettings.vSyncCount = 0` and `Application.targetFrameRate = 60` set in `DialogueManager.Awake()`.
- `VirtualJoystick` uses reflection to find axis values — fragile but works across input systems.
- `RoofCutoutController` updates roof shader uniforms every frame for a player-under-roof shadow effect.
- `Story.cs` recurses with `index++` at end of `PlayStatement()` — index 14 loads `InGame` scene.
- Two dialogue data structures exist: `DialogueSentence` (used) and `DialogueLine` (appears unused).

## Code Patterns

- **No DI framework** — components reference each other via `FindFirstObjectByType`, `GetComponent`, or serialized fields.
- **No test suite** — no test files exist.
- **VS Code** is the configured editor (`.vscode/launch.json` has "Attach to Unity" debug config).
- Both old Input Manager and new Input System are installed — `PlayerMovement` reads both.
