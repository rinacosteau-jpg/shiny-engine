using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Provides a universal way to temporarily block all player input actions.
/// </summary>
public static class PlayerInputBlocker
{
    private static int _blockCounter = 0;
    private static readonly HashSet<InputActionMap> _disabledMaps = new();
    private static readonly List<InputAction> _scratchActions = new();

    /// <summary>
    /// Disable all currently enabled input actions. Supports nested blocking.
    /// </summary>
    public static void Block()
    {
        _blockCounter++;
        if (_blockCounter == 1)
        {
            _disabledMaps.Clear();
            _scratchActions.Clear();
            InputSystem.ListEnabledActions(_scratchActions);

            foreach (var action in _scratchActions)
            {
                var map = action.actionMap;
                if (map == null || map.name == "UI")
                    continue;

                if (_disabledMaps.Add(map))
                    map.Disable();
            }
            _scratchActions.Clear();
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
            foreach (var map in _disabledMaps)
            {
                map.Enable();
            }
            _disabledMaps.Clear();
        }
    }

    /// <summary>
    /// Returns true if player input is currently blocked.
    /// </summary>
    public static bool IsBlocked => _blockCounter > 0;
}

