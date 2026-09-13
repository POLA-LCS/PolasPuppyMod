using Terraria.ModLoader;

namespace PuppyMod.Content.GlobalItems;

/// <summary>
/// Per-Item storage for BaseWeaponLeashItem original use values to avoid cross-contamination
/// from ModItem singleton fields (M11). Each Item entity gets its own copy via InstancePerEntity.
/// </summary>
public class WeaponLeashGlobalItem : GlobalItem
{
    public int OriginStyle;
    public int OrigTime;
    public int OrigAnim;
    public bool HasOriginal;

    public override bool InstancePerEntity => true;
}
