namespace IncTrak.Data
{
    public interface IVestingPromptInterpreterProvider
    {
        string Name { get; }

        int Priority { get; }

        bool IsAiProvider { get; }

        bool IsConfigured();

        QuickVestingInterpretResult TryInterpret(QuickVestingInterpretRequest request);
    }
}
