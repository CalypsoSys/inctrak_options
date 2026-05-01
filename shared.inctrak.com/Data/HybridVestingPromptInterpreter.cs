using System;
using System.Collections.Generic;
using System.Linq;
using IncTrak.data;

namespace IncTrak.Data
{
    public class HybridVestingPromptInterpreter : IVestingPromptInterpreter
    {
        private readonly IReadOnlyList<IVestingPromptInterpreterProvider> _providers;

        public HybridVestingPromptInterpreter(IEnumerable<IVestingPromptInterpreterProvider> providers)
        {
            _providers = providers?
                .OrderBy(provider => provider.Priority)
                .ToArray() ?? Array.Empty<IVestingPromptInterpreterProvider>();
        }

        public QuickVestingInterpretResult Interpret(QuickVestingInterpretRequest request)
        {
            bool strictAi = request?.StrictAi == true;
            QuickVestingInterpretResult lastAiFailure = null;
            bool sawAiProvider = false;

            foreach (IVestingPromptInterpreterProvider provider in _providers)
            {
                if (strictAi && string.Equals(provider.Name, "rules", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (provider.IsConfigured() == false)
                {
                    continue;
                }

                if (string.Equals(provider.Name, "rules", StringComparison.OrdinalIgnoreCase) == false)
                {
                    sawAiProvider = true;
                }

                QuickVestingInterpretResult result = provider.TryInterpret(request);
                if (string.IsNullOrWhiteSpace(result.Provider))
                {
                    result.Provider = provider.Name;
                }

                if (result.Success)
                {
                    return result;
                }

                if (strictAi && string.Equals(provider.Name, "rules", StringComparison.OrdinalIgnoreCase) == false)
                {
                    lastAiFailure = result;
                }
            }

            if (strictAi)
            {
                if (lastAiFailure != null)
                {
                    return lastAiFailure;
                }

                if (sawAiProvider == false)
                {
                    return new QuickVestingInterpretResult
                    {
                        Success = false,
                        Provider = "strict-ai",
                        Message = "Strict AI mode is enabled, but no AI interpreter is configured.",
                        Periods = Array.Empty<PERIOD_UI>()
                    };
                }
            }

            return new QuickVestingInterpretResult
            {
                Success = false,
                Provider = strictAi ? "strict-ai" : null,
                Message = "I could not build a vesting schedule from that description yet.",
                Periods = Array.Empty<PERIOD_UI>()
            };
        }
    }
}
