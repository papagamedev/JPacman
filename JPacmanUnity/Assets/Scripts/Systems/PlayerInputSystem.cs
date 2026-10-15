#if UNITY_EDITOR || DEBUG
#define CHEATS_ENABLED
#endif

using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(LevelPlayingPhaseSystem))]
[UpdateBefore(typeof(PlayerSystem))]
public partial class PlayerInputSystem : SystemBase
{
    private InputActionMap m_playerActions;
    private InputAction m_moveAction;
    private InputAction m_pauseAction;

    protected override void OnCreate()
    {
        base.OnCreate();
        // Project-wide actions (Assets/InputSystem_Actions.inputactions, set in Project Settings > Input System Package)
        m_playerActions = InputSystem.actions.FindActionMap("Player", throwIfNotFound: true);
        m_moveAction = m_playerActions.FindAction("Move", throwIfNotFound: true);
        m_pauseAction = m_playerActions.FindAction("Pause", throwIfNotFound: true);
        RequireForUpdate<Player>();
        RequireForUpdate<Game>();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        m_playerActions.Enable();
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();
        m_playerActions.Disable();
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

        if (m_pauseAction.WasPressedThisFrame())
        {
            game.ValueRW.PauseRequested = true;
        }

#if CHEATS_ENABLED
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }
        if (keyboard.wKey.isPressed)
        {
            game.ValueRW.CheatLevelCompleted = true;
        }
        if (keyboard.gKey.isPressed)
        {
            game.ValueRW.CheatGameOverWithScore = true;
        }
#endif // CHEATS_ENABLED
    }

}
