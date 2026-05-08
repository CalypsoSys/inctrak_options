using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IncTrak.Data
{
    public interface IVestingDefinitionParser
    {
        Task<VestingParseResult> ParseAsync(string input, CancellationToken cancellationToken = default);
    }

    public interface IVestingRuleExtractor
    {
        IReadOnlyList<string> ExtractHints(string input);
    }

    public interface IVestingDefinitionValidator
    {
        IReadOnlyList<string> ValidateForParsing(VestingDefinition definition);
        IReadOnlyList<string> ValidateForScheduleGeneration(VestingDefinition definition);
    }

    public interface IVestingScheduleGenerator
    {
        IReadOnlyList<VestingEvent> Generate(VestingDefinition definition);
    }
}
