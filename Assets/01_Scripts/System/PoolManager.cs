//using System;
//using System.Collections.Generic;
//using UnityEngine;

///// Component 타입별 오브젝트 풀을 생성·대여·반환한다.
///// 손님, VIP 손님, 드랍 아이템, 몬스터 담당 시스템은 이 클래스의 제네릭 API만 호출한다.
//public sealed class PoolManager : MonoBehaviour
//{
//    public static PoolManager Instance { get; private set; }

//    private readonly Dictionary<Type, Queue<Component>> poolDictionary = new Dictionary<Type, Queue<Component>>();

//    private readonly Dictionary<Type, Transform> poolParents = new Dictionary<Type, Transform>();

//    private readonly Dictionary<Type, Component> prefabs = new Dictionary<Type, Component>();

//    // 대여 중인 오브젝트만 추적해 중복 반환을 방지하고 씬 전환 시 일괄 반환한다.
//    private readonly Dictionary<Type, HashSet<Component>> activeObjects = new Dictionary<Type, HashSet<Component>>();

//    private Transform poolRoot;

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        InitializeRoot();
//    }

//    private void OnDestroy()
//    {
//        if (Instance == this)
//        {
//            Instance = null;
//        }
//    }

//    /// 지정한 프리팹을 미리 생성해 해당 Component 타입의 풀에 보관한다.
//    /// 예: PreLoadPool(monsterPrefab, 10)
//    public void PreLoadPool<T>(T prefab, int count) where T : Component
//    {
//        if (prefab == null || count <= 0)
//        {
//            return;
//        }

//        Type type = typeof(T);
//        CreatePool(type);
//        RegisterPrefab(type, prefab);

//        for (int i = 0; i < count; i++)
//        {
//            T instance = CreateInstance(prefab, type);
//            poolDictionary[type].Enqueue(instance);
//        }
//    }

//    /// 미리 등록한 프리팹 타입의 오브젝트를 대여한다.
//    /// 등록되지 않은 타입이면 null을 반환한다.
//    public T GetPool<T>() where T : Component
//    {
//        Type type = typeof(T);

//        if (!prefabs.TryGetValue(type, out Component prefab))
//        {
//            Debug.LogWarning($"{type.Name} 프리팹이 PoolManager에 등록되지 않았습니다.", this);
//            return null;
//        }

//        return GetOrCreate(type, prefab as T);
//    }

//    /// 프리팹을 등록한 뒤 오브젝트를 대여한다.
//    /// 첫 호출만 프리팹을 등록하며, 같은 타입에는 같은 프리팹을 사용해야 한다.
//    public T GetPool<T>(T prefab) where T : Component
//    {
//        if (prefab == null)
//        {
//            return null;
//        }

//        Type type = typeof(T);
//        CreatePool(type);
//        RegisterPrefab(type, prefab);

//        return GetOrCreate(type, prefabs[type] as T);
//    }

//    /// 대여한 오브젝트를 해당 타입의 풀로 반환한다.
//    /// 각 담당 시스템은 자체 상태를 초기화한 뒤 이 함수를 호출한다.
//    public void ReturnPool<T>(T instance) where T : Component
//    {
//        if (instance == null)
//        {
//            return;
//        }

//        Type type = typeof(T);
//        if (!activeObjects.TryGetValue(type, out HashSet<Component> activeSet) || !activeSet.Remove(instance))
//        {
//            Debug.LogWarning($"{instance.name}은(는) 대여 중인 {type.Name} 오브젝트가 아니므로 반환하지 않습니다.", instance);
//            return;
//        }

//        ReturnInternal(type, instance);
//    }

//    /// 현재 대여 중인 모든 오브젝트를 각자의 풀로 반환한다.
//    /// 게임 재시작, 씬 정리 등의 시점에 호출한다.
//    public void ReturnAllActiveObjects()
//    {
//        foreach (KeyValuePair<Type, HashSet<Component>> pair in activeObjects)
//        {
//            Type type = pair.Key;
//            Component[] rentedInstances = new Component[pair.Value.Count];
//            pair.Value.CopyTo(rentedInstances);

//            foreach (Component instance in rentedInstances)
//            {
//                if (instance != null)
//                {
//                    ReturnInternal(type, instance);
//                }
//            }

//            pair.Value.Clear();
//        }
//    }

//    private void InitializeRoot()
//    {
//        GameObject rootObject = new GameObject("PoolRoot");
//        rootObject.transform.SetParent(transform);
//        poolRoot = rootObject.transform;
//    }

//    private T GetOrCreate<T>(Type type, T prefab) where T : Component
//    {
//        Queue<Component> pool = poolDictionary[type];
//        T instance = null;

//        while (pool.Count > 0 && instance == null)
//        {
//            instance = pool.Dequeue() as T;
//        }

//        if (instance == null)
//        {
//            instance = CreateInstance(prefab, type);
//        }

//        instance.gameObject.SetActive(true);
//        activeObjects[type].Add(instance);
//        return instance;
//    }

//    private T CreateInstance<T>(T prefab, Type type) where T : Component
//    {
//        T instance = Instantiate(prefab, poolParents[type]);
//        instance.name = prefab.name;
//        instance.gameObject.SetActive(false);
//        return instance;
//    }

//    private void ReturnInternal(Type type, Component instance)
//    {
//        instance.gameObject.SetActive(false);
//        instance.transform.SetParent(poolParents[type], false);
//        poolDictionary[type].Enqueue(instance);
//    }

//    private void RegisterPrefab(Type type, Component prefab)
//    {
//        if (!prefabs.TryGetValue(type, out Component registeredPrefab))
//        {
//            prefabs.Add(type, prefab);
//            return;
//        }

//        if (registeredPrefab != prefab)
//        {
//            Debug.LogWarning($"{type.Name} 풀에는 이미 {registeredPrefab.name} 프리팹이 등록되어 있습니다. 기존 프리팹을 사용합니다.", this);
//        }
//    }

//    private void CreatePool(Type type)
//    {
//        if (poolDictionary.ContainsKey(type))
//        {
//            return;
//        }

//        poolDictionary.Add(type, new Queue<Component>());
//        activeObjects.Add(type, new HashSet<Component>());

//        GameObject parentObject = new GameObject($"{type.Name}_Pool");
//        parentObject.transform.SetParent(poolRoot);
//        poolParents.Add(type, parentObject.transform);
//    }
//}
