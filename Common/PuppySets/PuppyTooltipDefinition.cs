using System;
using System.Collections.Generic;

namespace PuppyMod.Common.PuppySets;

public sealed class PuppyTooltipDefinition
{
    public PuppyTooltipDefinition(params PuppyTooltipLineDefinition[] lines)
    {
        Lines = Array.AsReadOnly(lines ?? Array.Empty<PuppyTooltipLineDefinition>());
    }

    public IReadOnlyList<PuppyTooltipLineDefinition> Lines { get; }
}
