using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using PuppyMod.Players;

namespace PuppyMod.Services.Leash;

public static class LeashAttachService
{
    public static bool TryToggleLeash(Player owner, int leashItemType, int rangeTiles)
    {
        var target = LeashService.FindPuppyUnderCursor(owner, rangeTiles);
        if (target == null)
            return false;

        var chain = target.GetModPlayer<ChainedPlayer>();
        bool ownedByMe = chain.GrabberIndex == owner.whoAmI;

        if (ownedByMe)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                ModContent.GetInstance<PuppyMod>().RequestLeashDetach(target.whoAmI);
            else
                chain.SetGrabberAuthority(-1, 0);
        }
        else
        {
            if (chain.GrabberIndex.HasValue && chain.GrabberIndex != owner.whoAmI)
                return false;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                ModContent.GetInstance<PuppyMod>().RequestLeashAttach(target.whoAmI, leashItemType);
            else
                chain.SetGrabberAuthority(owner.whoAmI, leashItemType);
        }

        return true;
    }

    public static bool TryAttach(Player owner, Player target, int leashItemType)
    {
        if (!LeashService.CanAttach(owner, target, leashItemType))
            return false;

        var chain = target.GetModPlayer<ChainedPlayer>();
        if (Main.netMode == NetmodeID.MultiplayerClient)
            ModContent.GetInstance<PuppyMod>().RequestLeashAttach(target.whoAmI, leashItemType);
        else
            chain.SetGrabberAuthority(owner.whoAmI, leashItemType);

        return true;
    }

    public static bool TryDetach(Player owner, Player target)
    {
        var chain = target.GetModPlayer<ChainedPlayer>();
        if (chain.GrabberIndex != owner.whoAmI)
            return false;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            ModContent.GetInstance<PuppyMod>().RequestLeashDetach(target.whoAmI);
        else
            chain.SetGrabberAuthority(-1, 0);

        return true;
    }
}
