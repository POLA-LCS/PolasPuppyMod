using System.Collections.Generic;
using Terraria.ModLoader;
namespace PuppyMod.Common.Interfaces;
public interface ITooltipProvider
{
    IEnumerable<TooltipLine> GetTooltipLines(Mod mod);
}
