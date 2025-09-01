using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Provides a universal way to temporarily block all player input actions.
/// </summary>
public static class PlayerInputBlocker
{
    private static int _blockCounter = 0;
    private static readonly List<InputAction> _disabledActions = new();
    private static readonly List<InputAction> _allEnabledActions = new();

    /// <summary>
    /// Disable all currently enabled input actions. Supports nested blocking.
    /// </summary>
    public static void Block()
    {
        _blockCounter++;
        if (_blockCounter == 1)
        {
            _disabledActions.Clear();
            _allEnabledActions.Clear();
            InputSystem.ListEnabledActions(_allEnabledActions);

            foreach (var action in _allEnabledActions)
            {
                if (action.actionMap != null && action.actionMap.name == "UI")
                    continue;

                action.Disable();
                _disabledActions.Add(action);
            }
            _allEnabledActions.Clear();
        }
    }

    /// <summary>
    /// Re-enable input actions when no blockers remain.
    /// </summary>
    public static void Unblock()
    {
        if (_blockCounter == 0)
            return;

        _blockCounter--;
        if (_blockCounter == 0)
        {
            foreach (var action in _disabledActions)
            {
                action.Enable();
            }
            _disabledActions.Clear();
        }
    }

    /// <summary>
    /// Returns true if player input is currently blocked.
    /// </summary>
    public static bool IsBlocked => _blockCounter > 0;
}

