using UnityEngine;

// 아이템을 자동으로 수납하는 오브젝트들이 구현할 인터페이스

public interface IItemReceiver
{
    bool CanReceive(int amount);

    // 실제로 들어간 수량을 반환
    int TryReceive(int amount);
}
