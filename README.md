# Zone Zero Arena 🎮

A third-person **3D mobile arena shooter** built in Unity (C#) with NavMesh AI enemies, a state machine player controller, and mobile-first controls.

---

## 🕹️ Gameplay

Survive waves of AI-driven enemies in a 3D arena environment. Move, sprint, jump, and shoot — all optimized for mobile touchscreen input.

**Platforms:** Android · WebGL  
**Engine:** Unity 2022.3 (LTS) · C#

---

## ⚙️ Technical Highlights

### 🧠 PlayerStateMachine
Clean state-based player controller handling:
- Idle / Move / Sprint / Jump / Shoot states
- Smooth transitions with no spaghetti logic
- Fully decoupled from UI and AI systems

### 🤖 NavMesh AI
- Enemies navigate dynamically around obstacles using Unity's NavMesh system
- AI agents chase, pathfind, and react to player position in real time

### 📱 Mobile Input System
- Virtual joystick for movement
- On-screen action buttons (Shoot, Jump)
- Custom `MobileInputHandler` script decoupled from player logic

### 🖥️ HUD System
- Real-time health bar
- Score display
- Clean canvas-based UI optimized for mobile resolutions

### ☁️ PlayFab Backend
- Player authentication via Microsoft PlayFab
- Session and data management integrated

---

## 📁 Project Structure

```
Assets/
├── _project/
│   ├── Scripts/
│   │   ├── Core/         # Game loop, base classes
│   │   ├── Bot/          # AI NavMesh logic
│   │   ├── Events/       # Event system
│   │   ├── Managers/     # Game, UI, Audio managers
│   │   ├── Network/      # PlayFab integration
│   │   └── Interfaces/   # Shared contracts
│   ├── Scenes/           # Arena scenes
│   ├── Prefabs/          # Player, Enemy, HUD prefabs
│   └── ScriptableObjects/
├── Photon/               # Multiplayer support
├── PlayFabSDK/           # Backend integration
└── StarterAssets/        # Character controller base
```

---

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 LTS or higher
- Android Build Support module installed

### Run Locally
```bash
git clone https://github.com/Mishalbhasim/Zone-Zero.git
```
1. Open project in Unity Hub
2. Open `Assets/_project/Scenes/Arena_01`
3. Press Play in the Editor

### Android Build
1. File → Build Settings → Android
2. Switch Platform → Build and Run

---

## 🎯 What I Learned Building This

- Architecting a **PlayerStateMachine** to manage complex state transitions cleanly
- Implementing **NavMesh AI** with dynamic obstacle avoidance
- Designing **mobile-first input** systems decoupled from core gameplay logic
- Integrating a **backend service (PlayFab)** for auth and session management
- Optimizing Unity scenes for **Android performance**

---

## 👨‍💻 Developer

**Mishal Bhasim T M**  
Unity Game Developer  
🌐 [Portfolio](https://mishal-bhasim-t-m.itch.io) · [GitHub](https://github.com/Mishalbhasim) · [LinkedIn](https://linkedin.com/in/mishal-bhasim)  
📧 Mishalbhasim5@gmail.com

---

*Built at Brototype Game Dev Bootcamp · 2024–2025*
