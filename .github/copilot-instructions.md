# VMS - Copilot Instructions

## Project Overview

**VMS (Video Management System)** is a professional WPF video management application for Intelbras DVRs, supporting both direct IP connections and remote P2P Cloud access. The architecture uses MVVM with .NET 8.0, Serilog logging, and LiteDB persistence.

**Key Tech Stack:**
- Framework: .NET 8.0 (WPF)
- Architecture: MVVM (Community Toolkit)
- Database: SQLite (Microsoft.Data.Sqlite)
- Logging: Serilog (file-based, daily rolling, 30-day retention)
- External SDK: Intelbras `dhnetsdk.dll` (P/Invoke wrappers in [VMS_AlarmesJahu.App/Sdk/IntelbrasSdk.cs](VMS_AlarmesJahu.App/Sdk/IntelbrasSdk.cs))

---

## Architecture Patterns

### Dependency Injection & Initialization
The application uses `Microsoft.Extensions.DependencyInjection` configured in [App.xaml.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/App.xaml.cs#L40):
- Serilog is initialized before DI (logging is global via static `Log.Logger`)
- SDK initialization happens early via `IntelbrasSdk.Initialize()`
- Database initialization via `Database.Initialize()` creates SQLite schema
- All services are registered as singletons (no transient services)

**Critical pattern:** Services depend on repositories, which depend on `Database.ConnectionString`.

### Device Connection Management
[ConnectionManager](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Services/ConnectionManager.cs) handles all DVR connections:
- Maintains a `ConcurrentDictionary<long, DeviceConnection>` of active login handles
- Supports two connection modes: Direct IP (`Host:Port`) or P2P Cloud (`SerialNumber + Port`)
- Automatic health checks every 30 seconds via `Timer`
- Emits `DeviceStatusChanged` events when connection state changes
- **Critical:** `GetLogin()` returns cached `IntPtr` for existing connections; `Connect()` creates new ones

### Data Access Layer
Repositories follow a consistent pattern ([DeviceRepository](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Data/DeviceRepository.cs), [AiRuleRepository](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Data/AiRuleRepository.cs)):
- Use `Database.GetConnection()` for raw SQLite access
- No ORM—direct SQL with `SqliteCommand` and `SqliteDataReader`
- Repositories are stateless; each method creates its own connection
- Password storage uses `ProtectedData` (DPAPI) encryption at rest

### P2P Cloud Connection Flow
P2P Cloud uses a local tunnel approach (via `libt2u.dll`):
1. **Tunnel Manager** ([P2PTunnelManager](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Services/P2P/P2PTunnelManager.cs)): Singleton that manages local port forwarding
   - Initialized once via `P2PTunnelManager.Instance.EnsureInitialized()` in `App.OnStartup()`
   - Opens tunnel via `OpenTunnel(serialNumber, remotePort=37777)` → returns local port (6000-7000)
   - Closes tunnel via `CloseTunnel(serialNumber)` when disconnecting
2. **Connection Flow**:
   - `ConnectionManager.Connect()` checks `device.ConnectionType == P2PCloud`
   - Calls `P2PTunnelManager.OpenTunnel()` to map remote port to local
   - Uses `IntelbrasSdk.Login("127.0.0.1", localPort, ...)` to login locally
   - If login fails, closes tunnel immediately
   - On disconnect, `ConnectionManager.Disconnect()` closes tunnel
3. **Thread-safety**: Tunnel manager uses `ReaderWriterLockSlim` for init and `ConcurrentDictionary` for active tunnels
4. **Error handling**: Log all P2P failures; device shows as Error status if tunnel open or login fails

---

## Critical Dependencies & External Libraries

### Intelbras SDK (`dhnetsdk.dll`)
- Copied to `VMS_AlarmesJahu.App/libs/` and deployed via `<None Include>` in `.csproj`
- P/Invoke wrappers in [IntelbrasSdk.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Sdk/IntelbrasSdk.cs) expose:
  - `Login()` and `LoginP2P()` for authentication
  - `RealPlay()` for real-time streaming
  - `GetDeviceInfo()` for hardware metadata
  - `Setup()` for device configuration
- **Important:** Error codes are numeric; translate them using the SDK's error mapping (documented in DIAGNOSTICO_P2P.md)
- Requires `x64` platform target (set in `.csproj`)

### P2P Tunnel Library (`libt2u.dll`)
- **Optional but required for P2P Cloud**: Place in `VMS_AlarmesJahu.App/libs/` to enable tunnel mapping
- P/Invoke signatures in [P2PTunnelManager.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Services/P2P/P2PTunnelManager.cs):
  - `t2u_init()`: Initialize P2P subsystem
  - `t2u_add_port(serial, remotePort, localPort)`: Map remote → local port
  - `t2u_del_port(serial, localPort)`: Close tunnel
  - `t2u_query(serial, localPort)`: Check tunnel status
- `.csproj` includes warning if `libt2u.dll` missing (non-blocking; app still runs but P2P won't work)
- Log message at startup indicates DLL found or missing

### NuGet Packages
- **CommunityToolkit.Mvvm**: Provides `ObservableProperty`, `RelayCommand` for MVVM boilerplate
- **Serilog**: Configured at app startup; log files go to `%LOCALAPPDATA%/VMS_AlarmesJahu/Logs/`
- **Microsoft.Extensions.DependencyInjection**: Service container (no factory pattern; straightforward registration)
- **System.Text.Json**: For serialization (not Newtonsoft; keep consistent)

---

## Development Workflows

### Building & Running
```bash
# Visual Studio: F5 (Debug) or Ctrl+Shift+B (Build)
# Or CLI:
cd VMS_AlarmesJahu1/VMS_AlarmesJahu.App
dotnet build -c Debug
dotnet run
```

### Publishing Standalone Executable
```bash
# Publish self-contained for Windows x64
dotnet publish -c Release -r win-x64 --self-contained
# Output: bin\Release\net8.0-windows\win-x64\publish\
```

### Logging & Debugging
- All logs go to `%LOCALAPPDATA%/VMS_AlarmesJahu/Logs/vms-YYYY-MM-DD.log` (daily rolling)
- Access logs via: `Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "VMS_AlarmesJahu", "Logs")`
- Minimum level: **Debug** (set in `App.xaml.cs`)
- Use `Log.Information()`, `Log.Warning()`, `Log.Error()` throughout (not `Console.WriteLine()`)

### Database Schema
[Database.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Data/Database.cs) creates three core tables on first run:
- `devices`: DVR metadata (connection params, channel count, enable/disable)
- `ai_rules`: AI detection rules (lines, polygons, object classes, confidence thresholds)
- `ai_events`: Historical AI alarms (device, channel, timestamp, confidence, snapshot)

No migrations needed—schema is immutable at startup.

---

## Project-Specific Patterns

### MVVM Pattern (WPF)
- ViewModels inherit from [ViewModelBase](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/ViewModels/ViewModelBase.cs)
- Use `[ObservableProperty]` attributes (MVVM Toolkit) instead of manual `INotifyPropertyChanged`
- `RelayCommand` for simple sync commands; `IAsyncRelayCommand` for async
- Data binding via `{Binding PropertyName}` in XAML

### Password Security
[Device.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Models/Device.cs) uses DPAPI:
- `SetPassword(plainPassword)`: Encrypts with `ProtectedData.Protect()`
- `GetPassword()`: Decrypts with `ProtectedData.Unprotect()`
- Falls back to empty string on decryption failure (corrupted data is non-fatal)

### Enum-Based Connection Type
`Device.ConnectionType` determines login flow:
- `ConnectionType.Direct`: Use `Host` + `Port` with `IntelbrasSdk.Login()`
- `ConnectionType.P2PCloud`: Use `SerialNumber` + `Port` with `IntelbrasSdk.LoginP2P()`

Validate the required fields before attempting connection (log.Error if missing).

---

## Common Pitfalls & Conventions

1. **Always check `IntPtr.Zero`** after SDK calls (indicates failure)
2. **Use `GetConnectionInfo()`** in Device model for consistent error messages (shows P2P vs. Direct)
3. **Never call SDK methods without initialization** (`IntelbrasSdk.Initialize()` must run in `App.OnStartup()`)
4. **Prefer `using` statements** for SQLite connections (they auto-close/dispose)
5. **Singletons only**—no transient services; this avoids SDK handle leaks
6. **P2P timeouts are 3x longer** than Direct IP (tune in ConnectionManager health check if needed)
7. **Passwords are encrypted per-user** (DPAPI uses Windows user key; only decryptable by same user on same machine)

---

## Key Files Reference

| File | Purpose |
|------|---------|
| [VMS_AlarmesJahu.App.csproj](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/VMS_AlarmesJahu.App.csproj) | Project config, NuGet refs, SDK DLL deployment |
| [App.xaml.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/App.xaml.cs) | DI setup, Serilog config, P2P initialization, app lifecycle |
| [IntelbrasSdk.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Sdk/IntelbrasSdk.cs) | P/Invoke wrappers for dhnetsdk.dll (Login, RealPlay, Setup) |
| [P2PTunnelManager.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Services/P2P/P2PTunnelManager.cs) | Singleton P2P tunnel management (libt2u.dll), port mapping |
| [ConnectionManager.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Services/ConnectionManager.cs) | Device lifecycle, Direct & P2P login, tunnel integration, health checks |
| [Database.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Data/Database.cs) | SQLite schema, connection pooling |
| [Device.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/Models/Device.cs) | DVR metadata, password encryption, connection type enum |
| [MainViewModel.cs](VMS_AlarmesJahu1/VMS_AlarmesJahu.App/ViewModels/MainViewModel.cs) | Dashboard state, event subscriptions, initial data load |

---

## Testing & Troubleshooting

### P2P Cloud Diagnostics
Refer to [DIAGNOSTICO_P2P.md](VMS_AlarmesJahu1/DIAGNOSTICO_P2P.md) for:
- Error code mappings (numeric error codes from SDK)
- Network parameter tuning
- P2P tunnel validation

### Common Issues
- **Connection fails with error code X**: Check [DIAGNOSTICO_P2P.md](VMS_AlarmesJahu1/DIAGNOSTICO_P2P.md) for error mapping
- **DLL not found**: Ensure `dhnetsdk.dll` is in output bin folder (check `.csproj` `<None Include>` copy directive)
- **Password decryption fails**: User moved to different Windows account or machine; regenerate password
- **Health check kills connections**: Increase timer interval in `ConnectionManager` constructor if devices timeout too quickly

---

## AI Coding Agent Priorities

When implementing features:
1. **Maintain singleton pattern** for services (no DI transient registrations)
2. **Add Serilog logs** at key decision points (especially SDK calls, DB ops, connection state changes)
3. **Validate inputs early** and log.Error() with context before returning error
4. **Use Device.GetConnectionInfo()** in error messages for consistency
5. **Test both Direct IP and P2P Cloud** paths if modifying ConnectionManager
6. **Update Database schema incrementally** if needed (don't break existing tables)
7. **Reference exact file paths** in code comments (e.g., "See IntelbrasSdk.cs line 250")
