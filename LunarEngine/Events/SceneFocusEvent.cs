namespace LunarEngine.Events;

public readonly struct SceneFocusEvent
{
    public readonly bool IsFocused;

    public SceneFocusEvent(bool isFocused)
    {
        IsFocused = isFocused;
    }
}
