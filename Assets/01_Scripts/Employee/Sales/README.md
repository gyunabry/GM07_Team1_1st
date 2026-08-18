# 판매 직원

## 설정

`Assets/02_Prefabs/Building/Stand_Building.prefab` 루트에는 `SalesEmployeeCheckoutOperator`가 설정되어 있으며
`Sales Employee Prefab` 필드는 `Assets/02_Prefabs/Employee/SalesEmployee.prefab`을 참조한다.

기존 판매대 구성은 다음과 같다.

- 루트: `PlacedBuilding`, `SalesCounter`, `SalesEmployeeCheckoutOperator`
- 하위 계산 구역: `ShopCheckout`

위 구성요소는 판매 직원 기능에서 별도로 추가하지 않는다. 씬에는 `EmployeeManager`가 하나 있어야 한다.

`Building Profiles` 설정과 판매 직원 데이터 에셋은 필요하지 않다. 판매 직원 1명은 판매대 등록 시 자동 고용된다.

## 동작

- 판매대 건설이 완료되면 `SalesEmployeeCheckoutOperator`가 판매 직원 1명을 자동 고용·배정한다.
- 판매 직원이 배정되면 계산 구역에 판매 직원 프리팹을 생성한다.
- 생성된 판매 직원은 `CheckoutOperatorPresence`와 `BoxCollider`를 통해 계산대의 직원 배정 상태를 알린다.
- `ShopCheckout`은 직원 배정 상태와 재고를 확인한 뒤 판매·결제·다음 고객 처리를 진행한다.
- 판매대가 제거되면 등록된 판매 직원과 `CheckoutOperatorPresence` Collider도 함께 제거한다.
