using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(LevelPlayingPhaseSystem))]
[UpdateBefore(typeof(PlayerSystem))]
public partial class PlayerInputSystem : SystemBase
{
    private InputAction m_moveAction;
    private InputAction m_cancelAction;
    private InputActionMap m_playerActions;
    private InputActionMap m_uiActions;

    protected override void OnCreate()
    {
        base.OnCreate();
        var actions = new DefaultInputActions();
        m_playerActions = actions.Player;
        m_moveAction = actions.Player.Move;
        m_uiActions = actions.UI;
        m_cancelAction = actions.UI.Cancel;
        RequireForUpdate<Player>();
        RequireForUpdate<Game>();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        m_playerActions.Enable();
        m_uiActions.Enable();
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();
        m_playerActions.Disable();
        m_uiActions.Disable();
    }

    protected override void OnUpdate()
    {
        CheckPlayerMovement();
        CheckPauseAndCheats();
    }

    private void CheckPlayerMovement()
    {
        if (!SystemAPI.TryGetSingletonRW<Player>(out var player))
        {
            return;
        }

        var moveVector = m_moveAction.ReadValue<Vector2>();
        player.ValueRW.MoveVector = moveVector;
    }

    private void CheckPauseAndCheats()
    {
        if (!SystemAPI.TryGetSingletonRW<Game>(out var game))
        {
            return;
        }

        if (game.ValueRO.Paused > 0)
        {
            return;
        }

        if (m_cancelAction.triggered)
        {
            game.ValueRW.PauseRequested = true;
        }
        if (Keyboard.current.wKey.ReadValue() > 0)
        {
            game.ValueRW.CheatLevelCompleted = true;
        }
        if (Keyboard.current.gKey.ReadValue() > 0)
        {
            game.ValueRW.CheatGameOverWithScore = true;
        }
    }

}
