using System;
using Terraria;
using Terraria.ModLoader;

namespace PuppyMod.Common.Utils;

public enum AssetCategory
{
    Accessories,
    Weapons,
    Items,
    Projectiles,
    Buffs
}

public static class AssetUtils
{
    public static string GetTexturePath(AssetCategory category, string name) => $"PuppyMod/Assets/{category}/{name}";

    public static string GetAccessoryTexturePath(string name) => GetTexturePath(AssetCategory.Accessories, name);

    public static string GetWeaponTexturePath(string name) => GetTexturePath(AssetCategory.Weapons, name);

    public static string GetItemTexturePath(string name) => GetTexturePath(AssetCategory.Items, name);

    public static string GetProjectileTexturePath(string name) => GetTexturePath(AssetCategory.Projectiles, name);

    public static string GetBuffTexturePath(string name) => GetTexturePath(AssetCategory.Buffs, name);

    public static string GetAccessoryTexturePathWithFallback(string name)
        => HasAsset(AssetCategory.Accessories, name) ? GetAccessoryTexturePath(name) : GetAccessoryTexturePath("DefaultAccessory");

    public static string GetWeaponTexturePathWithFallback(string name)
        => HasAsset(AssetCategory.Weapons, name) ? GetWeaponTexturePath(name) : GetWeaponTexturePath("DefaultWeapon");

    public static string GetItemTexturePathWithFallback(string name)
        => HasAsset(AssetCategory.Items, name) ? GetItemTexturePath(name) : GetItemTexturePath("DefaultItem");

    public static string GetProjectileTexturePathWithFallback(string name)
        => HasAsset(AssetCategory.Projectiles, name) ? GetProjectileTexturePath(name) : GetProjectileTexturePath("DefaultProjectile");

    public static string GetBuffTexturePathWithFallback(string name)
        => HasAsset(AssetCategory.Buffs, name) ? GetBuffTexturePath(name) : GetBuffTexturePath("DefaultBuff");

    private static bool HasAsset(AssetCategory category, string name)
    {
        if (Main.dedServ)
            return false;
        string path = GetTexturePath(category, name);
        try
        {
            return ModContent.HasAsset(path);
        }
        catch (Exception ex)
        {
            try { ModContent.GetInstance<PuppyMod>().Logger.Warn($"HasAsset failed for {path}: {ex.Message}"); } catch { }
            return false;
        }
    }
}
