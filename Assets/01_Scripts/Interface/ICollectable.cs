
// 수집 대상이 되는 클래스가 구현할 인터페이스
public interface ICollectable
{
    bool TryCollect(ItemInventory target);
}
