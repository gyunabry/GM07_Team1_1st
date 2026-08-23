using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPoolManager : MonoBehaviour
{
    public static MonsterPoolManager Instance { get; private set; }

    private Dictionary<Component, Queue<Component>> poolDic = new();
    private Dictionary<Component, Transform> poolParent = new();

    private Dictionary<Type, Component> prefabDic = new Dictionary<Type, Component>();

    // 생성된 인스턴스가 어느 프리팹 풀에 속하는지 기록
    private readonly Dictionary<Component, Component> instanceOrigins = new();

    private readonly HashSet<Component> activeObjects = new();

    //private Dictionary<Type, HashSet<Component>> activeObj = new Dictionary<Type, HashSet<Component>>();
    private Transform poolRoot;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        InitializeRoot();
    }
    private void InitializeRoot()
    {
        GameObject root = new GameObject("PoolRoot");
        root.transform.SetParent(transform);
        poolRoot = root.transform;
    }

    public void PreLoadPool<T>(T prefab, int count) where T : Component
    {
        if (prefab == null || count <= 0) return;

        CreatePool(prefab);

        Type type = typeof(T);

        if(!prefabDic.ContainsKey(type))
        {
            prefabDic.Add(type, prefab);
        }

        for(int i = 0; i < count; i++)
        {
            T obj = Instantiate(prefab);
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(poolParent[prefab]);
            poolDic[prefab].Enqueue(obj);
        }
    }

    //public T GetPool<T>() where T : Component
    //{
    //    Type type = typeof(T);
    //    if (!prefabDic.ContainsKey(type))
    //    {
    //        Debug.LogWarning($"{type.Name} 프리팹이 등록되지 않음");
    //        return null;
    //    }
    //    T obj = null;
    //    if (poolDic[type].Count > 0)
    //    {
    //        obj = poolDic[type].Dequeue() as T;
    //    }
    //    else
    //    {
    //        obj = Instantiate(prefabDic[type] as T);
    //        obj.transform.SetParent(poolParent[type]);
    //    }
    //    obj.gameObject.SetActive(true);
    //    return obj;
    //}

    public T GetPool<T>() where T : Component
    {
        Type type = typeof(T);

        if (!prefabDic.TryGetValue(type, out Component prefab))
        {
            return null;
        }

        return GetPool(prefab as T);
    }

    //public T GetPool<T>(T Prefab) where T : Component
    //{
    //    Type type = typeof(T);
    //    CreatePool(type);
    //    T obj = null;
    //    if (poolDic[type].Count > 0)
    //    {
    //        obj = poolDic[type].Dequeue() as T;
    //    }
    //    else
    //    {
    //        obj = Instantiate(Prefab);
    //        obj.transform.SetParent(poolParent[type]);
    //    }
    //    obj.gameObject.SetActive(true);
    //    activeObjects.Add(obj);
    //    return obj;
    //}

    public T GetPool<T>(T prefab) where T : Component
    {
        if (prefab == null) return null;

        CreatePool(prefab);

        if (!prefabDic.ContainsKey(typeof(T)))
        {
            prefabDic.Add(typeof(T), prefab);
        }

        Queue<Component> pool = poolDic[prefab];
        T instance = null;

        while (pool.Count > 0 && instance == null)
        {
            instance = pool.Dequeue() as T;
        }

        if (instance == null)
        {
            instance = CreateInstance(prefab);
        }

        instanceOrigins[instance] = prefab;

        instance.gameObject.SetActive(true);
        activeObjects.Add(instance);

        return instance;
    }

    private T CreateInstance<T>(T prefab) where T : Component
    {
        // 기존에 poolPrefab에 등록된 해당 오브젝트 풀의 자식으로 인스턴스 생성
        T instance = Instantiate(prefab, poolParent[prefab]);
        instance.name = prefab.name;
        instance.gameObject.SetActive(false);

        instanceOrigins[instance] = prefab;

        return instance;
    }

    //public void ReturnPool<T>(T obj)where T : Component
    //{
    //    Type type = typeof(T);

    //    CreatePool(type);
    //    obj.gameObject.SetActive(false);
    //    if(obj.transform.parent != poolParent[type])
    //    {
    //        obj.transform.SetParent(poolParent[type]);
    //    }
    //    poolDic[type].Enqueue(obj);
    //    if(activeObj.TryGetValue(type, out var activeSet))
    //    {
    //        activeSet.Remove(obj);
    //    }
    //}

    public void ReturnPool<T>(T obj) where T : Component
    {
        if (obj == null) return;

        if (!activeObjects.Remove(obj)) 
        {
            Debug.LogWarning($"{obj.name}은 현재 대여 중인 오브젝트가 아닙니다.");
            return; 
        }

        if (!instanceOrigins.TryGetValue(obj, out Component prefab))
        {
            Debug.LogWarning($"{obj.name}은 원본 프리팹 정보가 없습니다.");
            return;
        }

        activeObjects.Remove(obj);

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(poolParent[prefab], false);
        poolDic[prefab].Enqueue(obj);
    }

    private void CreatePool(Component prefab)
    {
        if (poolDic.ContainsKey(prefab)) return;

        poolDic.Add(prefab, new Queue<Component>());

        // 해당 오브젝트의 부모 오브젝트를 생성
        GameObject parentObject = new GameObject($"{prefab.name}_Pool");
        parentObject.transform.SetParent(poolRoot);

        poolParent.Add(prefab, parentObject.transform);
    }

    //private void CreatePoolParent(Type type)
    //{
    //    GameObject parentObj = new GameObject(type.Name + "_Pool");
    //    parentObj.transform.SetParent(poolRoot);
    //    poolParent.Add(type, parentObj.transform);
    //}

    //public void ReturnAllActiveObj()
    //{
    //    foreach(var pair in activeObj)
    //    {
    //        Type type = pair.Key;
    //        Component[] activeObjs = new Component[pair.Value.Count];
    //        pair.Value.CopyTo(activeObjs);

    //        foreach(Component obj in activeObjs)
    //        {
    //            if (obj == null) continue;
    //            obj.gameObject.SetActive(false);
    //            if(obj.transform.parent != poolParent[type])
    //            {
    //                obj.transform.SetParent(poolParent[type]);
    //            }
    //            poolDic[type].Enqueue(obj);
    //        }
    //        pair.Value.Clear();
    //    }
    //}
}
