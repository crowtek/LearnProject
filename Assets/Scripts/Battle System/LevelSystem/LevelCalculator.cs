using UnityEngine;

public static class LevelCalculator
{
    // Formula: Basiswert * (Lvl ^ 1.5)
    private const int BaseValue = 100;
    private const float Exponent = 1.5f;

    public static int GetRequiredEXP(int level)
    {
        return Mathf.RoundToInt(BaseValue * Mathf.Pow(level, Exponent));
    }
}