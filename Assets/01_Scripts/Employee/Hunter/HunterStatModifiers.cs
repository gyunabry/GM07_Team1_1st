using System;

/// <summary>
/// 스킬과 건물 업그레이드가 사냥 직원에게 전달하는 능력치 보정값이다.
/// 이 구조체는 보정값만 표현하며, 스킬 시스템을 직접 참조하지 않는다.
/// </summary>
[Serializable]
public struct HunterStatModifiers
{
    public float MovementSpeedBonus;
    public int CarryingCapacityBonus;
    public float AttackDamageBonus;
    public float AttackRangeBonus;

    public static HunterStatModifiers None => default;
}
