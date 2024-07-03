using System.Collections.Generic;
using JWT.Builder;
using ProvingService.Domain.Common.Circuit.Extensions;

namespace ProvingService.Domain.Common.Circuit;

public class ClaimInfo
{
    public ClaimName Name { get; set; }
    public string Claim { get; set; } = null!;
    public int B64Index { get; set; }
    public int B64Length { get; set; }
    public int NameLength { get; set; }
    public int ColonIndex { get; set; }
    public int ValueIndex { get; set; }
    public int ValueLength { get; set; }

    public IEnumerable<KeyValuePair<string, List<string>>> IterArguments()
    {
        var name = Name.GetPublicClaimName();
        yield return new($"{name}_claim", Claim.Pad(Constants.StringLengths[name]));
        yield return new($"{name}_claim_length", new() { Claim.Length.ToString() });
        yield return new($"{name}_index_b64", new() { B64Index.ToString() });
        yield return new($"{name}_length_b64", new() { B64Length.ToString() });
        yield return new($"{name}_name_length", new() { NameLength.ToString() });
        yield return new($"{name}_colon_index", new() { ColonIndex.ToString() });
        yield return new($"{name}_value_index", new() { ValueIndex.ToString() });
        yield return new($"{name}_value_length", new() { ValueLength.ToString() });
    }
}