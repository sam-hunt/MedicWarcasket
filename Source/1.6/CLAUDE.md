# Source/1.6/: C# guidance

Loaded when work touches the code. Each file's header carries its decompile-verified call paths
and the rationale for its numbers; this file holds only the rules that cross files. The csproj's
comments cover install-path detection, the compile-only VFEP/VEF references (`VFEP_PATH` and
`VEF_PATH` override the Workshop copy) and the deploy manifest.

- **Patch classes live in `Patches/`** under the `.Patches` namespace suffix, to avoid RimWorld
  type-name conflicts. Log with the `[Medic Warcasket]` prefix.
- **Patch timing is the load-bearing hazard.** `PatchAll()` runs from the
  `[StaticConstructorOnStartup]` in `ModInit.cs`, never from the `Mod` subclass constructor: a
  detour on a VFEP method applied before defs load runs that type's static ctor early and breaks
  it for good (the BetterTradersGuild v1.1.0 CWTL incident). `ModInit`'s header has the
  rationale; `Settings/MedicWarcasketMod.cs` owns settings only.
- **Startup def-derived state must survive an in-process play-data reload** (a language switch
  reloads every def). It goes in the idempotent `Startup.Run`, called once from `ModInit` and
  again from the `StaticConstructorOnStartupUtility.CallAll` postfix in `Patches/`. Mod-shipped
  textures are destroyed on the same reload: resolve them through a getter that re-resolves on a
  Unity-null `== null` check, never `??` or `??=`
  (`~/dev/PersonaWeaponsUnbound/Source/1.6/Defs/PWU_Textures.cs` is the pattern). Vanilla-path
  textures are safe as plain statics.
- **Settings override def fields, never code paths.** `MedicWarcasketSettings.ApplyToDefs` writes
  each tunable onto its def at startup and on window close (its header lists the fields), so
  vanilla, VEF and our code read the setting through the def. Live state a write leaves behind (a
  running cooldown, kits above a lowered maximum, a mote's cached graphic) is reconciled by the
  instance on its next use, keyed on `MedicWarcasketSettings.Generation`, or by the write itself.
- **The two actives share their bases.** `Ability_MedicTargeted` (vanilla order semantics,
  Shift-click chaining, the cursor rule, the stun) and `JobDriver_MedicCast` (the walk, the
  cooldown wait, the check, the start-only binding of VEF's cast state; it finds its ability as
  the worn one whose def names its JobDef). A new targeted ability derives from both and gets its
  own JobDef.
- **VEF never fires an apparel ability's autocast** (its one consumer reads learned abilities
  only) and resets the flag on every re-bind. An apparel autocast is read, fired and kept across
  `Init` by our own code, as the shot's is by the dressing's shot toil.
- **The medic trait is mirrored from VFEP's at startup** by `WorkProfile.Mirror`, first in
  `Startup.Run`, so editing or patching its work tags, work types, degree effects or VEF
  extension on our def does not stick, and VFEP's 1.6/1.7 differences need no version check.
  Only the degree's text is ours. The trait def's header carries the contract.
- **Vanilla's reload queries are non-virtual** (`CompApparelReloadable.NeedsReload` and kin). A
  reloadable that bends one does it with a Harmony prefix gated on its own comp type, as
  `Patches/CompApparelReloadable_Reload.cs` does for the medicine suite.
- **Keyed strings a DLC owns are not available here.** Before using a vanilla key from code, grep
  `Data/*/Languages/English/Keyed/`; if Core does not own it (Biotech's `NumWoundsTended` was
  one), add an `MDWC_` key to the one Keyed file instead.
- **Verification notes name their game version.** The local install's assembly is now the 1.7
  one; a header call path verified on 1.6 says so if re-verified on 1.7 and differs.
