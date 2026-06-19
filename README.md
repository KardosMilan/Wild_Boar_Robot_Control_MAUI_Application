# wild Boar Robot — Control Client & Simulator

A full-stack robot diagnostics system built with **.NET MAUI** (client) and **ASP.NET Core Web API** (simulator), enabling real-time robot control and telemetry visualization through a cross-platform graphical interface.

> Developed as a university project for the *Application Development* course at BME.

---

## Overview

This project consists of two components working together over HTTP:

- **RobotDiagnostics** — an ASP.NET Core Web API that simulates a robot's physics, sensor readings, battery state, and obstacle management
- **VadkanRobotClient** — a .NET MAUI cross-platform client app that connects to the API, visualizes the robot's environment on a 2D canvas, and sends control commands

---

## Features

**Robot Simulator (ASP.NET Core API)**
- Real-time robot state: position (X/Y), heading angle, speed, battery level, status
- Distance sensor simulation with a configurable field-of-view (30° cone) and view distance
- Obstacle placement, collision detection, and automatic removal on contact
- Self-test endpoint with simulated delay
- RESTful API using GET, POST, and DELETE verbs

**MAUI Client**
- 2D top-down canvas rendering of the robot and obstacles using `GraphicsView` / `IDrawable`
- Live telemetry display: position, battery (color-coded progress bar), sensor distance, heading
- Movement controls: forward / backward / rotate left / rotate right
- Tap on canvas to place obstacles interactively
- Settings page: configurable server URL, speed, rotation step, sensor range, collision distance
- Settings persisted to JSON file via serialization; loaded on startup
- Battery charge action and self-test trigger
- Smooth async polling with `Task` / `async-await` to keep the UI responsive
- Dependency Injection via `MauiProgram.cs`
- MVVM architecture with `ICommand`, `INotifyPropertyChanged`, and `IValueConverter` bindings

---

## Architecture

```
┌─────────────────────────────┐         HTTP REST         ┌──────────────────────────┐
│     VadkanRobotClient       │ ◄──────────────────────► │    RobotDiagnostics      │
│        (.NET MAUI)          │                           │  (ASP.NET Core Web API)  │
│                             │                           │                          │
│  Views (XAML)               │   GET  /api/Robot/state   │  RobotController         │
│    └─ MainPage              │   POST /api/Robot/move    │  RobotService            │
│    └─ RobotSettingsPage     │   POST /api/Robot/rotate  │  RobotState (model)      │
│  ViewModels                 │   POST /api/Robot/charge  │  Obstacle (model)        │
│    └─ MainViewModel         │   DELETE /api/Robot/...   │                          │
│    └─ RobotSettingsViewModel│                           │                          │
│  Services                   │                           │                          │
│    └─ RobotApiService       │                           │                          │
│    └─ SettingsService       │                           │                          │
│  Converters                 │                           │                          │
│    └─ BatteryToColor        │                           │                          │
│    └─ BatteryToProgressbar  │                           │                          │
└─────────────────────────────┘                           └──────────────────────────┘
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Client UI | .NET MAUI (C#, XAML) |
| Backend / Simulator | ASP.NET Core 8 Web API |
| Communication | HTTP/REST (`HttpClient`, `System.Net.Http.Json`) |
| UI Pattern | MVVM (`ICommand`, `INotifyPropertyChanged`) |
| Serialization | `System.Text.Json` |
| Persistence | JSON file via `SettingsService` |
| Rendering | MAUI `GraphicsView` / `IDrawable` canvas |
| DI | Built-in .NET DI (`MauiProgram`, `WebApplication.Builder`) |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 with the **.NET MAUI** workload installed
- Windows 10/11 (for Windows target; Android/iOS require respective SDKs)

### Run the simulator

```bash
cd RobotDiagnostics
dotnet run
# API available at http://localhost:5116
```

### Run the client

Open `Vadkan_robot.sln` in Visual Studio 2022 and set `VadkanRobotClient` as the startup project, then run on your desired target (Windows Machine / Android Emulator).

The client defaults to `http://localhost:5116`. The server URL can be changed in **Robot Settings** within the app.

---

## API Reference

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/Robot/state` | Get full robot telemetry |
| `POST` | `/api/Robot/move?distance={d}` | Move robot forward/backward |
| `POST` | `/api/Robot/rotate` | Rotate by angle (JSON body) |
| `POST` | `/api/Robot/charge` | Recharge battery to 100% |
| `POST` | `/api/Robot/selftest` | Run self-test routine |
| `GET` | `/api/Robot/obstacles` | List all obstacles |
| `POST` | `/api/Robot/obstacles` | Add an obstacle |
| `DELETE` | `/api/Robot/obstacles` | Clear all obstacles |
| `DELETE` | `/api/Robot/obstacles/collided` | Remove obstacle nearest to robot |
| `GET` | `/api/Robot/settings` | Get robot parameters |
| `POST` | `/api/Robot/settings` | Update robot parameters |

---

## Project Structure

```
Vadkan_robot/
├── RobotDiagnostics/           # ASP.NET Core Web API (simulator)
│   ├── Controllers/
│   │   └── RobotController.cs
│   ├── Models/
│   │   ├── RobotState.cs
│   │   ├── MoveCommand.cs
│   │   ├── RotateCommand.cs
│   │   └── Obstacle.cs
│   └── Services/
│       └── RobotService.cs
│
└── VadkanRobotClient/          # .NET MAUI cross-platform client
    ├── ViewModels/
    │   ├── MainViewModel.cs
    │   ├── RobotSettingsViewModel.cs
    │   └── ViewModelBase.cs
    ├── Services/
    │   ├── RobotApiService.cs
    │   └── SettingsService.cs
    ├── Converters/
    │   ├── BatteryToColor.cs
    │   └── BatteryToProgressbar.cs
    ├── Models/
    │   ├── RobotState.cs
    │   ├── RobotSettings.cs
    │   └── Obstacle.cs
    ├── MainPage.xaml / .cs
    └── RobotSettingsPage.xaml / .cs
```

---

## Notable Implementation Details

- **Sensor simulation**: the robot's distance sensor casts a directional ray within a ±30° cone, finding the nearest obstacle within the configured view distance using dot-product filtering.
- **Async UI**: all API calls use `async`/`await` with `Task`, ensuring the MAUI UI thread is never blocked.
- **IValueConverter**: `BatteryToColor` and `BatteryToProgressbar` translate the battery integer into a UI-ready color and progress value via XAML bindings.
- **Settings persistence**: `SettingsService` serializes `RobotSettings` to a local JSON file and raises a `SettingsChanged` event consumed by the ViewModel, decoupling storage from UI.
- **Canvas rendering**: `MainViewModel` implements `IDrawable` directly, drawing the robot sprite, heading indicator, sensor cone, and obstacles frame-by-frame onto a MAUI `GraphicsView`.

---

## Author

**Kardos Milan**  
Electrical / Electronics Engineer  
*Specialising in Power Electronics, EMC Validation, Hardware Design, and Embedded Systems*

---

## License

This repository is shared for portfolio and reference purposes. All rights reserved.
