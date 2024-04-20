using Spiderman;
using UnityEngine;

public class SwingState : BaseState
{
    public override void EnterState(SpidermanCharacterController manager)
    {
        base.EnterState(manager);
        Debugger.Instance.UpdateCurrentStateDebugger(MainStates.InAir, SubStates.Swing);
        
    }

    protected override void ExitState()
    {
        base.ExitState();
    }

    protected override void CheckSwitchState()
    {
    }
}
