# Zone Zero Arena

A third-person 3D mobile arena shooter built in Unity (C#). Still in active development — this is my first attempt at building a complete game with multiplayer, AI enemies, and an event-driven architecture.

---

## What It Is

Players survive in a 3D arena against AI-driven enemies. The zone shrinks over time and deals damage, forcing players to keep moving. Built for Android with mobile touch controls.

Platform: Android, WebGL  
Engine: Unity 2022.3, C#  
Status: Work in Progress

---

## What I Built

### Player Health and State System (PlayerStateMachine.cs)

Manages the player's core survival logic — tracks HP, listens to zone damage ticks via an EventBus, handles shooting by delegating to a WeaponBase component, and triggers death by disabling the movement controller and playing a death animation.

```csharp
void OnEnable() => EventBus.OnZoneDamageTick += TakeDamage;
void OnDisable() => EventBus.OnZoneDamageTick -= TakeDamage;
```

### EventBus Architecture

Decoupled event system that connects zone damage, player health, UI, and game state without tight dependencies between systems.

### NavMesh AI and BotManager

Enemies spawn and navigate dynamically using Unity's NavMesh. BotManager handles spawning logic and bot lifecycle in the arena.

### Terrain

Custom terrain built and painted inside Unity. Getting it centered and optimized for mobile took a few iterations — performance on mid-range Android was the main challenge.

### Mobile Controls

Virtual joystick and on-screen buttons for touch input. Movement uses Unity Starter Assets (ThirdPersonController), extended with custom shooting and health logic.

---

## Still Working On

- Multiplayer sync using Photon
- Full HUD (health bar, zone timer, kill feed)
- Performance optimization for mid-range Android devices
- Complete game loop: match start, win/loss conditions, respawn

---

## Project Structure

```
Assets/
├── _project/
│   ├── Scripts/
│   │   ├── Core/         
│   │   ├── Bot/          
│   │   ├── Events/       
│   │   ├── Managers/     
│   │   ├── Network/      
│   │   └── Interfaces/
│   ├── Scenes/
│   ├── Prefabs/
│   └── ScriptableObjects/
├── Photon/
├── PlayFabSDK/
└── StarterAssets/
```

---

## What I Learned

Building an EventBus to decouple systems cleanly was something I hadn't done before — having zone damage talk to player health without direct references made the code much easier to manage. Terrain creation and optimization for mobile was harder than I expected. I also learned how to extend third-party packages like Starter Assets with custom game logic without breaking the original functionality.

---

## Developer

Mishal Bhasim T M  
itch.io: mishal-bhasim-t-m.itch.io  
GitHub: github.com/Mishalbhasim  
LinkedIn: linkedin.com/in/mishal-bhasim  
Email: Mishalbhasim5@gmail.com

Built at Brototype Game Dev Bootcamp, 2024–present
