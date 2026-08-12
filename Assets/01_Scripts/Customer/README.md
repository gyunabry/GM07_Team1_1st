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
| `CustomerCheckoutStation.cs` | 프리팹에 명시적으로 배치한 대기열·결제 영역을 검증하고 자동 등록·폐쇄·해제한다. |
| `CustomerContracts.cs` | 주문 데이터와 재고·재화·계산 담당자 인터페이스를 정의한다. |
| `../Data/CustomerDataSO.cs` | 손님 유형이 공유하는 이동 속도, 결제 시간, 퇴장 제한 시간, 기본 주문을 보관한다. 최대 대기시간은 아직 정의하지 않는다. |
| `CustomerRuntimeData.cs` | 풀에서 대여된 손님 한 명의 현재 상태, 선택 대기열·계산대, 확정 주문, 결제 완료 여부를 보관한다. |
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

계산대 프리팹은 `CheckoutFront`, `OperatorArea`, `ShopCustomerQueue`, `ShopCheckout`을 명시적으로 포함해야 한다. `CustomerCheckoutStation`은 Inspector에 연결된 참조를 검증하고 스폰 매니저에 등록할 뿐, 자식 오브젝트·컴포넌트·Trigger를 런타임에 생성하거나 위치와 크기를 덮어쓰지 않는다. 비활성화되면 자동으로 폐쇄·해제된다. 손님 생성 시 정원이 남은 활성 계산대 중 손님 수가 가장 적은 대기열을 선택한다.

각 스테이션은 독립된 대기열과 계산 담당자 감지 영역을 사용하므로 여러 계산대에서 동시에 결제할 수 있다. 활성 계산대가 하나도 없으면 새 손님 스폰은 중단되고, 계산대가 다시 열리면 활성 계산대 총 정원까지 `spawnInterval` 간격으로 보충한다.

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

## 손님 기본·런타임 데이터

`CustomerDataSO`는 여러 손님이 공유하는 기본값이다. `movementSpeed`, `paymentDuration`, `exitTimeout`, `defaultOrder`를 Inspector에서 설정한다. `CustomerController`에 이 SO를 연결하면 NavMeshAgent 이동 속도와 결제·퇴장 시간이 해당 값으로 적용된다. 연결하지 않은 기존 프리팹과 테스트 씬은 컨트롤러의 기존 기본값으로 동작한다.

`CustomerRuntimeData`는 풀에서 손님이 대여될 때 초기화되고 반납 전 초기화된다. 현재 상태 이름, 가장 짧은 줄 선택 결과(`SelectedQueue`, `SelectedCheckout`), 실제 확정 주문, 결제 완료 여부를 한 손님 단위로 관리한다. 최대 대기시간은 현재 게임 규칙에 포함하지 않는다.

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

퇴장 경로가 생성되지 않거나 퇴장 시작 후 `exitTimeout`(기본 30초)을 넘기면, 손님은 경고를 남기고 자동으로 풀에 반납된다. 이 경우에도 스폰 매니저가 즉시 다음 손님을 보충하므로 대기열 정원이 줄어들지 않는다.

## 씬 연결

## 건물·인벤토리·재화 연동 가이드

계산대는 플레이 중에 배치·이동·철거될 수 있으므로, **건물 시스템이 계산대 인스턴스와 위치를 소유**하고 Customer 시스템은 `CustomerCheckoutStation` 컴포넌트를 통해 자동 등록·해제한다. Customer는 건물 시스템을 직접 찾거나 수정하지 않는다.

### 책임 분리

| 영역 | 책임 |
| --- | --- |
| 건물 시스템 | 계산대 배치 완료 여부, 이동·회전·철거, NavMesh 갱신, 계산대 프리팹과 직원 위치 제공 |
| Customer 시스템 | 사용 가능한 계산대 목록 유지, 가장 짧은 줄 선택, 대기열·결제·퇴장 처리 |
| 인벤토리 시스템 | 주문 재료의 보유량 확인과 **전량 동시 차감** |
| 재화 시스템 | 결제 성공 뒤 돈·경험치 지급 및 변경 알림 |

### 계산대 건물 연동 순서

1. 계산대 프리팹 루트에 `ShopCustomerQueue`와 `CustomerCheckoutStation`을 붙인다.
2. 루트의 자식으로 `CheckoutFront`를 만들고, 손님이 계산할 정확한 위치에 배치한다. `ShopCustomerQueue`의 `Checkout Front` 필드에 이 Transform을 연결한다.
3. 루트의 자식으로 `OperatorArea`를 만들고, 직원이 설 위치에 배치한다. 여기에 `ShopCheckout`을 붙인다. `ShopCheckout`이 자동으로 보장하는 `BoxCollider`는 **Is Trigger**로, `Rigidbody`는 **Is Kinematic**·**Use Gravity 해제**로 설정한다. Collider의 중심과 크기는 프리팹에서 직접 조정한다.
4. 루트의 `CustomerCheckoutStation` Inspector에서 `Queue`에 `ShopCustomerQueue`, `Checkout`에 `OperatorArea`의 `ShopCheckout`을 명시적으로 연결한다. 컴포넌트를 처음 붙일 때는 `Reset`이 같은 프리팹 안의 후보를 한 번 채워주지만, 저장 전 참조와 위치를 확인한다.
5. 직원 또는 유저 프리팹에는 기존처럼 `CheckoutOperatorPresence`를 붙인다. `OperatorArea` Trigger 안에 들어오면 해당 계산대 결제가 시작된다.
6. 계산대 오브젝트가 활성화되면 `CustomerCheckoutStation`이 자동 등록한다. 건설 중 손님을 받지 않아야 하면 오브젝트를 비활성화한 뒤 완공 시 활성화한다.
7. 이동 또는 회전 전에는 `CloseStation()`을 호출한다. 새 배정은 즉시 닫히고, 대기·접근·결제 대기 손님은 **결제와 보상 없이 전원 퇴장**한다.
8. 위치·회전 적용과 NavMesh 갱신 후 `OpenStation()`을 호출한다. 오브젝트 비활성화 → 위치 변경 → 활성화 방식은 이 호출을 자동으로 처리한다.
9. 철거는 `CloseStation()` 후 오브젝트를 비활성화하거나 삭제한다. `OnDisable`이 등록 해제를 수행한다.

건물 시스템이 사용하는 공개 API는 `CustomerCheckoutStation.CloseStation()`과 `OpenStation()`이다. 외부 시스템은 이 두 메서드 또는 오브젝트 활성화만 사용하면 되며, `CustomerSpawnManager` 내부 목록을 직접 수정하지 않는다. `ConfigureReferences()`는 `CustomerVisualTest` 같은 런타임 Factory 전용이며, 건물 프리팹 장착에는 사용하지 않는다.

### 주문 재료·재화 연동

`CurrencySystem`은 이미 `ICustomerCurrency`를 구현하므로 `CustomerSpawnManager.BindServices()`에 그대로 전달할 수 있다. `ItemInventory`는 `InventoryChanged` 이벤트와 아이템 수량 조회·차감 기능을 제공하지만 아직 `ICustomerInventory`는 구현하지 않는다. 인벤토리 담당자는 `ICustomerInventory` 어댑터를 만들어 다음을 보장해야 한다.

1. `CustomerOrder.Items` 전체 보유량을 먼저 검사한다.
2. 하나라도 부족하면 아무 재료도 차감하지 않고 `false`를 반환한다.
3. 모두 충분할 때만 전량을 차감하고, 한 번의 변경 알림을 보낸 뒤 `true`를 반환한다.

이 계약을 지키면 Customer 시스템은 구체적인 인벤토리·건물 구현을 몰라도 안전하게 결제를 처리할 수 있다.

필수 구성 요소는 다음과 같다.

- `PoolManager` 1개
- 손님 프리팹: `NavMeshAgent`, `CustomerStateMachine`, `CustomerController`
- 베이크된 NavMesh와 입구·판매대 앞·퇴장 통로·출구 Transform
- 계산대 프리팹마다 `CustomerCheckoutStation`, `ShopCustomerQueue`, `CheckoutFront`, `OperatorArea/ShopCheckout` 하나씩
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
- 계산대: 4개, 각 대기열 최대 5명
- 손님 군집 위치: 각 카운터 앞 `z = 1.5`
- 계산 담당자 감지 영역: 각 카운터 뒤 `z = 3.5`
- 퇴장 경로: 계산대 오른쪽 바깥 통로를 거쳐 출구로 이동

테스트용 계산 담당자 캡슐은 기본적으로 감지 영역 밖에 있다. 마우스를 각 계산대 뒤의 감지 영역 위에 놓으면 캡슐이 그 위치로 이동하고, 해당 줄의 앞 손님만 1.5초 후 결제한다. 마우스를 감지 영역 밖에 두면 결제가 시작되지 않아 네 줄에 손님이 쌓이는 모습을 확인할 수 있다. 테스트용 `CustomerVisualTestServices`는 주문이 항상 처리 가능하다고 가정하는 간이 재고 구현이다.

각 테스트 계산대도 실제 프리팹과 동일한 구조를 런타임에 명시적으로 만든 뒤 `CustomerCheckoutStation`으로 등록한다. 플레이 중 해당 스테이션 오브젝트를 비활성화하면 그 줄 손님이 무보상 퇴장하고, 다시 활성화하면 손님 보충이 재개된다.
