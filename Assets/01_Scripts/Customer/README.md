# 손님 시스템 안내

## 파일 안내

| 파일 | 역할 |
| --- | --- |
| `CustomerSpawnManager.cs` | 손님을 생성하고 재사용합니다. |
| `CustomerController.cs` | 손님의 이동, 결제, 퇴장을 처리합니다. |
| `CustomerStateMachine.cs` | 손님의 현재 상태를 전환합니다. |
| `CustomerStates.cs` | 방문, 대기, 결제, 퇴장 상태의 동작을 정의합니다. |
| `ShopCustomerQueue.cs` | 계산대 앞 손님 줄과 대기 위치, 맨 앞 손님의 이동을 관리합니다. |
| `ShopCheckout.cs` | 계산 담당자가 계산 영역에 있는지 확인합니다. |
| `CustomerCheckoutStation.cs` | 계산대 전체를 등록·관리하며, 대기열과 계산 영역을 연결합니다. |
| `CheckoutOperatorPresence.cs` | 플레이어 또는 직원이 계산을 담당함을 알립니다. |
| `CustomerContracts.cs` | 주문, 인벤토리, 재화에 사용하는 공통 인터페이스를 정의합니다. |
| `CustomerRuntimeData.cs` | 손님별 진행 상태와 주문 정보를 보관합니다. |
| `CustomerVisualTestBootstrap.cs` | 손님 시스템 테스트 씬을 준비합니다. |

## 계산대 구성

계산대 건물 프리팹은 아래 구조로 구성합니다.

```text
계산대 건물 루트
├─ CustomerCheckoutStation        ← 계산대를 손님 시스템에 등록
├─ ShopCustomerQueue              ← 손님 줄과 대기 위치 관리
├─ CheckoutFront                  ← 손님의 정확한 계산 위치
└─ OperatorArea                   ← 직원이 설 빈 자식 오브젝트
   └─ ShopCheckout                ← 계산 담당자 감지
      ├─ BoxCollider              ← 자동 추가
      └─ Rigidbody                ← 자동 추가
```

1. 계산대 건물 루트에 `CustomerCheckoutStation`, `ShopCustomerQueue`를 붙입니다.
2. 루트 아래에 `CheckoutFront`를 만들고, 손님이 계산할 정확한 위치에 배치합니다. `ShopCustomerQueue`의 `Checkout Front` 참조에 연결합니다.
3. 루트 아래에 빈 자식 `OperatorArea`를 만들고, 직원이 설 위치에 배치한 뒤 `ShopCheckout`을 붙입니다.
4. `ShopCheckout`의 `BoxCollider` Size로 계산 담당자 감지 영역 크기를 조절합니다.
5. `CustomerCheckoutStation`의 `Queue`와 `Checkout` 참조에 각각 `ShopCustomerQueue`, `ShopCheckout`을 연결합니다.

## 이동·철거 연동

- 계산대 이동·철거 기능에서는 이동 또는 철거 전에 `CloseStation()`, 새 위치의 NavMesh 갱신 후에는 `OpenStation()`을 호출합니다.

## 확인 체크리스트

- `CheckoutFront`가 손님 쪽을 향하고, 해당 위치가 NavMesh 위에 있습니다.
- 프리팹 구성을 마친 뒤 `CustomerCheckoutStation`의 `Queue`와 `Checkout` 참조가 각각 `ShopCustomerQueue`, `ShopCheckout`으로 연결됐는지 확인했습니다.

손님은 계산대 앞에서 대기하고, 계산 담당자가 `OperatorArea`에 들어오면 결제를 진행합니다. 결제가 완료되면 손님은 퇴장 경로를 따라 나갑니다.

## 시스템 연동

계산대 프리팹과 별개로 씬에 아래 요소를 구성합니다.

```text
씬
├─ CustomerSpawnManager           ← 손님 생성, 입구·퇴장·주문 설정
├─ PoolManager                    ← 손님 오브젝트 재사용
└─ 계산 담당자(플레이어 또는 직원)
   ├─ Collider
   └─ CheckoutOperatorPresence    ← 계산 담당자로 인식
```

- `CustomerSpawnManager`에 손님 프리팹, 입구 위치, 퇴장 경로, 인벤토리와 재화 서비스를 연결합니다.
- 이동 구역의 NavMesh를 베이크합니다.
