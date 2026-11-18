using UnityEngine;

public class BJAbilityManager : MonoBehaviour
{
    // プレイヤーごとの能力強化段階
    public int[] winMoneyLevel = new int[3];
    public int[] betLimitLevel = new int[3];

    // プレイヤーごとの能力所持状況

    // プレイヤーごとのバースト無効所持数
    public int[] bustGuardCount = new int[3];
    public bool[] hasRedraw = new bool[3];
    public bool[] hasForceRedraw = new bool[3];
    public bool[] hasDealerInfo = new bool[3];
}