using UnityEngine;

public abstract class BaseState : MonoBehaviour
{
    protected SpidermanCharacterController _manager;
    public virtual void EnterState(SpidermanCharacterController manager)
    {
        gameObject.SetActive(true);
        _manager = manager;
        _manager.currentState = this;
#if UNITY_EDITOR
        //Debug.LogError($"Update state {this.gameObject.name}");
#endif
    }
    public virtual void UpdateState()
    {
        CheckSwitchState();
    }

    public virtual void SwitchState(BaseState nextState)
    {
        ExitState();
        nextState.EnterState(_manager);
    }

    protected virtual void ExitState()
    {
        gameObject.SetActive(false);
    }
    protected abstract void CheckSwitchState();
}
