using UnityEngine.InputSystem;

/// <summary>
/// Provides a universal way to temporarily block all player input actions.
/// </summary>
public static class PlayerInputBlocker
{
    private static int _blockCounter = 0;

    /// <summary>
    /// Disable all currently enabled input actions. Supports nested blocking.
    /// </summary>
    public static void Block()
    {
        _blockCounter++;
        if (_blockCounter == 1)
        {
            InputSystem.DisableAllEnabledActions();
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
            InputSystem.EnableAllActions();
        }
    }

    /// <summary>
    /// Returns true if player input is currently blocked.
    /// </summary>
    public static bool IsBlocked => _blockCounter > 0;
}
