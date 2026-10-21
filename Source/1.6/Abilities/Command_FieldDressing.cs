using System.Collections.Generic;
using UnityEngine;
using Verse;
using VEF.Abilities;
using ThingDefOf = RimWorld.ThingDefOf;

namespace MedicWarcasket;

// The field dressing's gizmo: VEF's ability Command showing the selected medicine's item icon
// in place of the def's, the selected type's "loaded / max" count top-right where vanilla's
// reloadables show theirs, and a right-click menu that switches the suite's selected type.
// Named as the def's gizmoClass.
//
// VEF builds a new Command on every gizmo request and sets the icon in the constructor, so
// the icon is refreshed before each draw as well, for the frame after a selection change. The
// right-click menu lists every medicine with its item icon and count, the current selection
// disabled so the menu shows the state; a right-click with options present never reaches
// VEF's auto-cast toggle (GizmoGridDrawer opens the menu instead). The menu must stay
// reachable while the button is disabled (the selected type empty, or the cooldown running),
// since switching is the way out of an empty type, and vanilla's Command answers any click on
// a disabled button with the disabled message instead; so the button reads as enabled for the
// span of a right-click. With several medics selected VEF groups the gizmos by def and vanilla
// merges the grouped menus by label, so rows with matching counts act on every selected suite
// and rows with differing counts appear once per medic.
public class Command_FieldDressing : Command_Ability
{
    public Command_FieldDressing(Pawn pawn, Ability ability) : base(pawn, ability)
    {
        RefreshIcon();
    }

    private Ability_FieldDressing Dressing => (Ability_FieldDressing)ability;

    private CompFieldDressingSuite Suite => Dressing.Suite;

    public override string TopRightLabel => Suite?.LabelRemaining;

    public override Color IconDrawColor => Suite?.Selected?.uiIconColor ?? defaultIconColor;

    public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
    {
        get
        {
            CompFieldDressingSuite suite = Suite;
            if (suite == null)
                yield break;
            ThingDef selected = suite.Selected;
            foreach (ThingDef type in CompFieldDressingSuite.Types)
            {
                string label = "MDWC_MedicineOption".Translate(type.LabelCap, suite.Loaded(type), suite.MaxCharges);
                yield return new FloatMenuOption(label, type == selected ? null : () => suite.Select(type), type);
            }
        }
    }

    protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
    {
        RefreshIcon();
        bool wasDisabled = Disabled;
        if (wasDisabled && Event.current.isMouse && Event.current.button == 1 && Mouse.IsOver(butRect))
            Disabled = false;
        GizmoResult result = base.GizmoOnGUIInt(butRect, parms);
        Disabled = wasDisabled;
        return result;
    }

    // The def has no icon of its own; without a suite (never in practice, the comps share the
    // def) the industrial medicine stands in.
    private void RefreshIcon()
    {
        Texture2D medicineIcon = (Suite?.Selected ?? ThingDefOf.MedicineIndustrial).uiIcon;
        icon = medicineIcon != null ? medicineIcon : BaseContent.BadTex;
    }
}
