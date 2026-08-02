using UnityEngine;

/// <summary>
/// 오브젝트 풀링 시스템 구현 전 계획 스크립트
public class ObjectPoolManager : MonoBehaviour
{
    // PoolManager는 손님, VIP 손님, 드랍 아이템, 몬스터의 제네릭 풀을 각각 관리
    // 공통 풀 기능은 GenericPool<T>로 분리

    // 일반 손님과 VIP 손님은 행동 흐름이 달라서 별도 프리팹, 풀로 관리

    // SpawnManager는 Instantiate 대신 monsterPool.Get()을 호출

    // 드랍 아이템은 몬스터 사망 위치에서 dropItemPool.Get() 호출
    // 플레이어가 아이템을 회수하면 dropItemPool.Return(item) 호출

    // 변경될 여지 있음.
}
