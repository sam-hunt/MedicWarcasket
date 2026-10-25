using UnityEngine;
using Verse;
using Ability = VEF.Abilities.Ability;
using Command_Ability = VEF.Abilities.Command_Ability;

namespace MedicWarcasket;

// The adrenaline shot's gizmo: VEF's ability command plus the top-right "shots / max" count
// that vanilla draws on a reloadable's verb command (Shipcracker's Command_BreachJump idiom),
// named as the def's gizmoClass. VEF draws its autocast check in that same corner, so the
// ability's AutoCast reads false while VEF draws (Drawing) and the check goes top-left here
// instead, at VEF's size; the command has no hotkey, whose label vanilla would put there. The
// count is read live on every draw, so the cached command in Ability_AdrenalineShot.GetGizmo
// needs no refresh for it.
public class Command_AdrenalineShot : Command_Ability
{
    private Ability_AdrenalineShot Shot => ability as Ability_AdrenalineShot;

    public Command_AdrenalineShot(Pawn pawn, Ability ability) : base(pawn, ability)
    {
    }

    public override string TopRightLabel => Shot?.ShotsLabel;

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        Ability_AdrenalineShot shot = Shot;
        GizmoResult result;
        if (shot != null)
            shot.Drawing = true;
        try
        {
            result = base.GizmoOnGUI(topLeft, maxWidth, parms);
        }
        finally
        {
            if (shot != null)
                shot.Drawing = false;
        }
        if (shot != null && shot.AutoCast)
        {
            float size = parms.shrunk ? 12f : 24f;
            GUI.DrawTexture(new Rect(topLeft.x, topLeft.y, size, size), AutoCastTex);
        }
        return result;
    }
}
