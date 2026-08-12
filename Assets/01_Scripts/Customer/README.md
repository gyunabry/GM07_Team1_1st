# 손님(Customer) 시스템 안내

`Assets/01_Scripts/Customer`는 판매대에 손님이 모이고, 계산 담당자가 있을 때 주문 재료를 차감한 뒤 보상을 지급하는 시스템이다.

## 전체 흐름

```text
초기 손님 생성
  → 판매대 앞 군중에 합류
  → 대기열 맨 앞 손님만 계산대 정면으로 진입
  → 계산 담당자 감지 후 1.5초 계산
  → 재료 차감 · 돈/경험치 지급
  → 옆 통로를 따라 퇴장
  → 출구 도착 · 풀 반납
  → 그 뒤 새 손님 한 명 보충
```

처음에는 최대 인원까지 `spawnInterval` 간격으로 손님을 채운다. 이후에는 결제가 완료된 시점이 아니라, 손님이 출구까지 이동해 풀에 반납된 뒤에만 새 손님 한 명이 들어온다.

## 주요 파일

| 파일 | 역할 |
| --- | --- |
| `CustomerSpawnManager.cs` | 초기 인원을 채우고, 손님의 퇴장 완료·실패 이벤트를 받아 풀에 반납한 뒤 다음 손님을 보충한다. 입구 위치는 `NavMesh.SamplePosition()`으로 NavMesh 위에 보정한다. |
| `CustomerController.cs` | NavMeshAgent 이동, 주문·결제 서비스 연결, 결제·퇴장 상태용 Agent 설정과 퇴장 결과 알림을 담당한다. |
| `CustomerStateMachine.cs` | 현재 손님 상태를 보관하고 `Exit → Enter` 순서로 상태를 전환한다. |
| `CustomerStates.cs` | `Visit`, `Order`, `Idle`, `Exit` 상태의 행동을 구현한다. |
| `ShopCustomerQueue.cs` | 결제 순서를 관리하고, 군중 속 다음 손님을 계산대 정면으로 진입시킨다. |
| `ShopCheckout.cs` | 계산 담당자가 계산대 뒤쪽 감지 영역에 있는지 확인한다. |
| `CustomerContracts.cs` | 주문 데이터와 재고·재화·계산 담당자 인터페이스를 정의한다. |
| `CheckoutOperatorPresence.cs` | 플레이어 또는 자동 판매 직원에게 붙여 계산 담당자로 인식시키는 마커다. |
| `CustomerVisualTestBootstrap.cs` | `CustomerVisualTest` 씬을 런타임에 구성하는 테스트 전용 코드다. |

## 상태 전환

| 상태 | 동작 | 다음 상태 |
| --- | --- | --- |
| `Visit` | 모든 손님이 판매대 정면을 향해 이동한다. 앞사람 때문에 막히며 자연스러운 군중을 만든다. | 계산 가능 범위 진입 시 `Order` |
| `Order` | 대기열 순서를 기다린다. 맨 앞이 되면 해당 손님만 높은 통행 우선순위와 작은 반경으로 계산대 정면에 접근한다. | 계산 가능 범위 안의 맨 앞 손님이면 `Idle` |
| `Idle` | 계산대 앞에서 멈춘다. 계산 담당자가 감지 영역에 들어온 시점부터 계산 시간을 잰다. | 1.5초 후 결제 성공 시 `Exit` |
| `Exit` | 대기열에서 제거한 뒤 바깥쪽 통로를 따라 출구로 간다. | 출구 도착 또는 실패 시 풀 반납 및 다음 손님 보충 |

## 군중과 계산 순서

대기열은 손님의 위치를 줄 단위로 배정하지 않는다. 모든 손님이 동일한 판매대 정면(`checkoutFront`)을 향하기 때문에 NavMeshAgent 회피에 따라 판매대 앞에 군집을 이룬다.

대기열의 첫 손님만 `frontCustomerRadius`를 적용하고 낮은 회피 우선순위 값으로 이동한다. 따라서 차례가 되면 군중 사이를 지나 실제 계산 위치 가까이 들어갈 수 있다. `checkoutAcceptanceRadius` 안에 들어온 맨 앞 손님만 계산을 시작한다.

`ShopCustomerQueue`에서 조절할 값:

- `maxCustomers`: 최대 손님 수. 기본값은 20명이다.
- `crowdAgentRadius`: 군중 속 일반 손님의 Agent 반경. 클수록 서로 간격이 넓다.
- `frontCustomerRadius`: 계산 차례 손님의 Agent 반경.
- `checkoutAcceptanceRadius`: 계산을 시작할 수 있는 판매대 앞 범위.

## 결제 규칙

결제는 아래 조건을 모두 만족해야 성공한다.

1. 아직 결제되지 않은 손님일 것
2. 계산 담당자가 `ShopCheckout` 감지 영역 안에 있을 것
3. `ICustomerInventory`와 `ICustomerCurrency`가 연결되어 있을 것
4. `CustomerOrder`가 유효할 것
5. `ICustomerInventory.TryConsumeAll()`이 주문 재료 전량 차감에 성공할 것

손님이 `Idle` 상태가 된 뒤 계산 담당자가 감지 영역에 들어오면 `paymentDuration`(기본 1.5초) 동안 계산한다. 담당자가 중간에 영역을 벗어나면 계산 타이머는 0으로 초기화된다.

계산이 끝나면 주문 재료를 전량 차감하고, `ICustomerCurrency.GrantReward()`로 돈과 경험치를 지급한다. `paymentCompleted`로 중복 지급을 막는다.

## 주문 데이터

`CustomerOrder`는 여러 `CustomerOrderItem`과 보상으로 구성된다.

- `CustomerOrderItem.ItemId` 필드는 이름과 달리 `ItemDataSO` 참조 타입이다.
- 아이템 참조는 비어 있으면 안 되며, 수량은 1 이상이어야 한다.
- 같은 `ItemDataSO`를 주문 안에 중복으로 넣을 수 없다.
- 돈·경험치 보상은 0 이상이어야 한다.

실제 인벤토리는 `ICustomerInventory.TryConsumeAll()`에서 주문 재료를 **모두 보유했을 때만 한 번에 차감**해야 한다. 일부만 차감한 뒤 실패를 반환하면 재고와 주문이 어긋난다.

## 퇴장 처리

군중에 퇴장 손님이 막히지 않도록 `Exit` 상태에서는 다음 설정을 적용한다.

- NavMeshAgent 회피 비활성화
- Agent 반경 최소화
- 비트리거 Collider 비활성화
- 퇴장 이동 우선순위 적용

손님이 풀에 반납되어 다음 스폰에 재사용될 때는 기본 Agent 반경, 회피 설정, Collider 상태를 복원한다.

퇴장 경로가 생성되지 않거나 퇴장 시작 후 `exitTimeout`(기본 10초)을 넘기면, 손님은 경고를 남기고 자동으로 풀에 반납된다. 이 경우에도 스폰 매니저가 즉시 다음 손님을 보충하므로 대기열 정원이 줄어들지 않는다.

## 씬 연결

필수 구성 요소는 다음과 같다.

- `PoolManager` 1개
- 손님 프리팹: `NavMeshAgent`, `CustomerStateMachine`, `CustomerController`
- 베이크된 NavMesh와 입구·판매대 앞·퇴장 통로·출구 Transform
- `ShopCustomerQueue`와 `ShopCheckout`
- 계산 담당자 역할의 오브젝트에 붙일 `CheckoutOperatorPresence`
- `ICustomerInventory`, `ICustomerCurrency` 구현체

스폰 매니저에는 다음을 연결한다.

```csharp
spawnManager.BindServices(inventoryService, currencySystem);
```

입구 위치가 NavMesh에서 2m 안에 발견되지 않으면 손님을 생성하지 않고 경고를 남긴다.

## CustomerVisualTest

`Assets/00_Scenes/CustomerVisualTest.unity`는 `CustomerVisualTestBootstrap`으로 테스트 환경을 런타임에 만든다.

- 손님 최대 인원: 20명
- 손님 군집 위치: 카운터 앞 `z = 1.5`
- 계산 담당자 감지 영역: 카운터 뒤 `z = 3.5`
- 퇴장 경로: 계산대 오른쪽 바깥 통로를 거쳐 출구로 이동

마우스로 계산 담당자 캡슐을 감지 영역에 올리면, 계산대 앞 손님이 1.5초 뒤 결제하고 퇴장하는지 확인할 수 있다. 테스트용 `CustomerVisualTestServices`는 주문이 항상 처리 가능하다고 가정하는 간이 재고 구현이다.
