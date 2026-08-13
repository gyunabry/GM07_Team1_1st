using UnityEngine;

// 손님 유형이 공유하는 변경 불가능한 기본 설정이다.
[CreateAssetMenu(fileName = "CustomerData", menuName = "Tycoon/Customer Data")]
public sealed class CustomerDataSO : ScriptableObject
{
    [SerializeField, Min(0.1f)] private float movementSpeed = 1.5f;
    [SerializeField, Min(0f)] private float paymentDuration = 1.5f;
    [SerializeField, Min(0.1f)] private float exitTimeout = 30f;
    [SerializeField] private CustomerOrder defaultOrder;

    public float MovementSpeed => movementSpeed;
    public float PaymentDuration => paymentDuration;
    public float ExitTimeout => exitTimeout;
    public CustomerOrder DefaultOrder => defaultOrder;
}
