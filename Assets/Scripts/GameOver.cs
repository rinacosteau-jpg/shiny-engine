using UnityEngine;

public static class GameOver
{
    public static void Trigger()
    {
        LoopResetInputScript.TryLoopReset();
        Debug.Log("Game Over");
    }
}

