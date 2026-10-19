---
name: rimworld-logs
description: Find and read RimWorld's Player.log, or disassemble the RimWorld API to inspect vanilla types and method signatures. Use when debugging runtime behavior, chasing an exception, or checking what a vanilla method actually does.
---

# RimWorld Logs and API Inspection

## Reading the log

1. **Enable RimWorld Dev Mode:** Settings → Dev Mode → Logging
2. **Log folder** (the layout depends on the installed game version):
   - **Windows:** `%USERPROFILE%\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\`
   - **WSL:** `/mnt/c/Users/*/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/`
   - **Linux (Steam):** `~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/`
3. **Log file:**
   - **1.7 (Waylith):** `Logs/Player.log` in that folder (`Logs/Player-prev.log` is the boot
     before). Every entry is `YYYY-MM-DD HH:MM:SS.mmm  LEVEL [Category] message` followed by
     `    at ...` stack frames, so levels can be grepped directly:
     ```bash
     grep -hE '^\S+ \S+ +(ERROR|WARN)' "$LOGDIR/Logs/Player.log" | grep -v '\[Vulkan\]'
     ```
   - **1.6 (Unity):** `Player.log` directly in that folder, Unity's unstructured format. A 1.7
     install leaves this file in place, stale; check the timestamp before reading it.

Our own messages are prefixed `[Medic Warcasket]`, so grep for that to isolate them from
vanilla and other mods' output.

## Inspecting the RimWorld API

The local RimWorld installation is the source of truth for API shapes; prefer it over the
`Krafs.Rimworld.Ref` NuGet package the build compiles against. `monodis` gives signatures,
`ilspycmd -t "Namespace.ClassName"` method bodies.

- **1.7 (Waylith):** the assemblies sit flat in the install root. `Assembly-CSharp.dll` is the
  game; `UnityStubs.dll` beside it holds every `UnityEngine.*` type the game and mods bind to,
  so inspect it for Unity-facing signatures (`Rect`, `Texture2D`, `GUI`, ...):
  ```bash
  monodis "$RIMWORLD_PATH/Assembly-CSharp.dll"
  ilspycmd "$RIMWORLD_PATH/Assembly-CSharp.dll" -t Verse.ModContentPack
  monodis --typedef "$RIMWORLD_PATH/UnityStubs.dll" | grep 'UnityEngine.Rect '
  ```
- **1.6 (Unity):** `"$RIMWORLD_PATH/RimWorldWin64_Data/Managed/Assembly-CSharp.dll"` with the
  `UnityEngine.*Module.dll` assemblies beside it.

The install is whichever Steam branch is selected, so a call path verified against it is
verified on that version only; say which in the def or code header when it matters.
