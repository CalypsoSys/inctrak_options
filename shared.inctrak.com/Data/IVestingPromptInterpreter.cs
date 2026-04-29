namespace IncTrak.Data
{
    public interface IVestingPromptInterpreter
    {
        QuickVestingInterpretResult Interpret(QuickVestingInterpretRequest request);
    }
}
