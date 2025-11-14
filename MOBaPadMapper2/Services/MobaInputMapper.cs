namespace MOBaPadMapper2;

public class MobaInputMapper
{
    private readonly ITouchInjector _touch;

    public IList<ActionMapping> Mappings { get; } = new List<ActionMapping>();

    public MobaInputMapper(ITouchInjector touch)
    {
        _touch = touch;

        // Prosty domyœlny mapping – A -> tap w dolnym œrodku
        Mappings.Add(new ActionMapping
        {
            TriggerButton = GamepadButton.A,
            ActionType = ActionType.Tap,
            TargetX = 0.5,
            TargetY = 0.7,
            Size = 60
        });
    }

    // PóŸniej do³o¿ymy tu logikê reagowania na GamepadState
}
