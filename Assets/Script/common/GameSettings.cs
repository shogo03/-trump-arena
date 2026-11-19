public static class GameSettings
{
    public static int HumanPlayerCount = 1;
    public static int CPUCount = 0;

    // CPUごとの難易度
    // 1 = Easy、2 = Normal、3 = Hard
    public static int[] CPULevels = new int[4]
    {
        2, 2, 2, 2
    };

    public static bool[] IsCPU = new bool[4];
    public static bool[] IsActive = new bool[4];

    public static int TotalPlayerCount
    {
        get
        {
            int count = 0;

            for (int i = 0; i < 4; i++)
            {
                if (IsActive[i])
                    count++;
            }

            return count;
        }
    }
}