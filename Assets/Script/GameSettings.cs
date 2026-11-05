public static class GameSettings
{
    public static int HumanPlayerCount = 1;
    public static int CPUCount = 0;
    public static int CPULevel = 1;

    // 各プレイヤー枠がCPUかどうか
    public static bool[] IsCPU = new bool[4];

    public static int TotalPlayerCount
    {
        get { return HumanPlayerCount + CPUCount; }
    }
}