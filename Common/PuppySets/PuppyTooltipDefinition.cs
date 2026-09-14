using System;
using System.Collections.Generic;

namespace PuppyMod.Common.PuppySets;

public sealed class PuppyTooltipDefinition(params PuppyTooltipLineDefinition[] lines)
{

    public IReadOnlyList<PuppyTooltipLineDefinition> Lines { get; } = Array.AsReadOnly(lines ?? []);
}
