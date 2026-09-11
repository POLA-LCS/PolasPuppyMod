using System.Collections.Generic;
using Terraria.ModLoader;

namespace PuppyMod.Common.Tooltip;

public interface ITooltipProvider
{
    IEnumerable<TooltipLine> GetTooltipLines(Mod mod);
}
