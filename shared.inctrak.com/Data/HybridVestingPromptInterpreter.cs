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
            string preferredProvider = request?.PreferredProvider?.Trim();
            QuickVestingInterpretResult lastAiFailure = null;
            bool sawAiProvider = false;

            if (string.IsNullOrWhiteSpace(preferredProvider) == false)
            {
                return InterpretPreferredProvider(request, preferredProvider, strictAi, ref sawAiProvider, ref lastAiFailure);
            }

            if (strictAi)
            {
                return InterpretStrictAi(request, ref sawAiProvider, ref lastAiFailure);
            }

            List<QuickVestingInterpretResult> builtInResults = CollectBuiltInResults(request);
            QuickVestingInterpretResult bestBuiltIn = builtInResults.Aggregate<QuickVestingInterpretResult, QuickVestingInterpretResult>(null, ChooseBetterBuiltIn);
            QuickVestingInterpretResult alternateBuiltIn = SelectAlternateBuiltIn(bestBuiltIn, builtInResults);

            if (bestBuiltIn != null)
            {
                bestBuiltIn.AlternateProvider = alternateBuiltIn?.Provider;
                if (bestBuiltIn.Success && bestBuiltIn.RequiresAi == false)
                {
                    return bestBuiltIn;
                }
            }

            foreach (IVestingPromptInterpreterProvider provider in _providers.Where(provider => provider.IsAiProvider))
            {
                sawAiProvider = true;
                if (provider.IsConfigured() == false)
                {
                    continue;
                }

                QuickVestingInterpretResult aiResult = provider.TryInterpret(request);
                if (string.IsNullOrWhiteSpace(aiResult.Provider))
                {
                    aiResult.Provider = provider.Name;
                }

                if (aiResult.Success)
                {
                    return aiResult;
                }

                lastAiFailure = aiResult;
            }

            if (bestBuiltIn != null)
            {
                return bestBuiltIn;
            }

            return lastAiFailure ?? new QuickVestingInterpretResult
            {
                Success = false,
                Message = "I could not build a vesting schedule from that description yet.",
                Periods = Array.Empty<PERIOD_UI>()
            };
        }

        private QuickVestingInterpretResult InterpretPreferredProvider(
            QuickVestingInterpretRequest request,
            string preferredProvider,
            bool strictAi,
            ref bool sawAiProvider,
            ref QuickVestingInterpretResult lastAiFailure)
        {
            IVestingPromptInterpreterProvider provider = _providers.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, preferredProvider, StringComparison.OrdinalIgnoreCase));

            if (provider == null || provider.IsConfigured() == false)
            {
                return new QuickVestingInterpretResult
                {
                    Success = false,
                    Provider = preferredProvider,
                    Message = $"The {preferredProvider} interpreter is not available right now.",
                    Periods = Array.Empty<PERIOD_UI>()
                };
            }

            if (provider.IsAiProvider)
            {
                sawAiProvider = true;
            }

            if (strictAi && provider.IsAiProvider == false)
            {
                return new QuickVestingInterpretResult
                {
                    Success = false,
                    Provider = "strict-ai",
                    Message = "Strict AI mode only allows AI interpreters.",
                    Periods = Array.Empty<PERIOD_UI>()
                };
            }

            QuickVestingInterpretResult preferredResult = provider.TryInterpret(request);
            if (string.IsNullOrWhiteSpace(preferredResult.Provider))
            {
                preferredResult.Provider = provider.Name;
            }

            if (provider.IsAiProvider && preferredResult.Success == false)
            {
                lastAiFailure = preferredResult;
            }

            return preferredResult;
        }

        private QuickVestingInterpretResult InterpretStrictAi(
            QuickVestingInterpretRequest request,
            ref bool sawAiProvider,
            ref QuickVestingInterpretResult lastAiFailure)
        {
            foreach (IVestingPromptInterpreterProvider provider in _providers.Where(provider => provider.IsAiProvider))
            {
                sawAiProvider = true;
                if (provider.IsConfigured() == false)
                {
                    continue;
                }

                QuickVestingInterpretResult aiResult = provider.TryInterpret(request);
                if (string.IsNullOrWhiteSpace(aiResult.Provider))
                {
                    aiResult.Provider = provider.Name;
                }

                if (aiResult.Success)
                {
                    return aiResult;
                }

                lastAiFailure = aiResult;
            }

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

            return new QuickVestingInterpretResult
            {
                Success = false,
                Provider = "strict-ai",
                Message = "The AI interpreters could not build a vesting schedule from that description yet.",
                Periods = Array.Empty<PERIOD_UI>()
            };
        }

        private List<QuickVestingInterpretResult> CollectBuiltInResults(QuickVestingInterpretRequest request)
        {
            var results = new List<QuickVestingInterpretResult>();
            foreach (IVestingPromptInterpreterProvider provider in _providers.Where(provider => provider.IsAiProvider == false))
            {
                if (provider.IsConfigured() == false)
                {
                    continue;
                }

                QuickVestingInterpretResult builtInResult = provider.TryInterpret(request);
                if (string.IsNullOrWhiteSpace(builtInResult.Provider))
                {
                    builtInResult.Provider = provider.Name;
                }

                results.Add(builtInResult);
            }

            return results;
        }

        private static QuickVestingInterpretResult SelectAlternateBuiltIn(
            QuickVestingInterpretResult bestBuiltIn,
            IReadOnlyList<QuickVestingInterpretResult> builtInResults)
        {
            if (bestBuiltIn == null)
            {
                return null;
            }

            foreach (QuickVestingInterpretResult candidate in builtInResults)
            {
                if (candidate == null || candidate.Success == false)
                {
                    continue;
                }

                if (string.Equals(candidate.Provider, bestBuiltIn.Provider, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (AreEquivalent(bestBuiltIn, candidate) == false)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static bool AreEquivalent(QuickVestingInterpretResult left, QuickVestingInterpretResult right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (left.SharesGranted != right.SharesGranted ||
                string.Equals(left.VestingStart, right.VestingStart, StringComparison.Ordinal) == false ||
                string.Equals(left.Kind, right.Kind, StringComparison.Ordinal) == false)
            {
                return false;
            }

            PERIOD_UI[] leftPeriods = left.Periods ?? Array.Empty<PERIOD_UI>();
            PERIOD_UI[] rightPeriods = right.Periods ?? Array.Empty<PERIOD_UI>();
            if (leftPeriods.Length != rightPeriods.Length)
            {
                return false;
            }

            for (int i = 0; i < leftPeriods.Length; i++)
            {
                PERIOD_UI leftPeriod = leftPeriods[i];
                PERIOD_UI rightPeriod = rightPeriods[i];
                if (leftPeriod.PERIOD_AMOUNT != rightPeriod.PERIOD_AMOUNT ||
                    leftPeriod.PERIOD_TYPE_FK != rightPeriod.PERIOD_TYPE_FK ||
                    leftPeriod.AMOUNT_TYPE_FK != rightPeriod.AMOUNT_TYPE_FK ||
                    leftPeriod.AMOUNT != rightPeriod.AMOUNT ||
                    leftPeriod.INCREMENTS != rightPeriod.INCREMENTS)
                {
                    return false;
                }
            }

            return true;
        }

        private static QuickVestingInterpretResult ChooseBetterBuiltIn(QuickVestingInterpretResult currentBest, QuickVestingInterpretResult candidate)
        {
            if (candidate == null)
            {
                return currentBest;
            }

            if (currentBest == null)
            {
                return candidate;
            }

            if (candidate.Success != currentBest.Success)
            {
                return candidate.Success ? candidate : currentBest;
            }

            if (candidate.RequiresAi != currentBest.RequiresAi)
            {
                return candidate.RequiresAi ? currentBest : candidate;
            }

            if (candidate.Confidence != currentBest.Confidence)
            {
                return candidate.Confidence > currentBest.Confidence ? candidate : currentBest;
            }

            int candidateCompleteness = ScoreCompleteness(candidate);
            int currentCompleteness = ScoreCompleteness(currentBest);
            if (candidateCompleteness != currentCompleteness)
            {
                return candidateCompleteness > currentCompleteness ? candidate : currentBest;
            }

            return currentBest;
        }

        private static int ScoreCompleteness(QuickVestingInterpretResult result)
        {
            int score = 0;
            if (result.SharesGranted.HasValue)
            {
                score++;
            }

            if (string.IsNullOrWhiteSpace(result.VestingStart) == false)
            {
                score++;
            }

            score += result.Periods?.Length ?? 0;
            return score;
        }
    }
}
