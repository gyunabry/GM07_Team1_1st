using System;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPoolManager : MonoBehaviour
{
    public static MonsterPoolManager Instance { get; private set; }

    private Dictionary<Type, Queue<Component>> poolDic = new Dictionary<Type, Queue<Component>>();
    private Dictionary<Type, Transform> poolParent = new Dictionary<Type, Transform>();

    private Dictionary<Type, Component> prefabDic = new Dictionary<Type, Component>();
    private Dictionary<Type, HashSet<Component>> activeObj = new Dictionary<Type, HashSet<Component>>();
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
        Type type = typeof(T);
        CreatePool(type);

        if(!prefabDic.ContainsKey(type))
        {
            prefabDic.Add(type, prefab);
        }
        for(int i = 0; i < count; i++)
        {
            T obj = Instantiate(prefab);
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(poolParent[type]);
            poolDic[type].Enqueue(obj);
        }
    }
    public T GetPool<T>() where T : Component
    {
        Type type = typeof(T);
        if (!prefabDic.ContainsKey(type))
        {
            Debug.LogWarning($"{type.Name} 프리팹이 등록되지 않음");
            return null;
        }
        T obj = null;
        if (poolDic[type].Count > 0)
        {
            obj = poolDic[type].Dequeue() as T;
        }
        else
        {
            obj = Instantiate(prefabDic[type] as T);
            obj.transform.SetParent(poolParent[type]);
        }
        obj.gameObject.SetActive(true);
        return obj;
    }
    public T GetPool<T>(T Prefab) where T : Component
    {
        Type type = typeof(T);
        CreatePool(type);
        T obj = null;
        if (poolDic[type].Count > 0)
        {
            obj = poolDic[type].Dequeue() as T;
        }
        else
        {
            obj = Instantiate(Prefab);
            obj.transform.SetParent(poolParent[type]);
        }
        obj.gameObject.SetActive(true);
        activeObj[type].Add(obj);
        return obj;
    }
    public void ReturnPool<T>(T obj)where T : Component
    {
        Type type = typeof(T);
        CreatePool(type);
        obj.gameObject.SetActive(false);
        if(obj.transform.parent != poolParent[type])
        {
            obj.transform.SetParent(poolParent[type]);
        }
        poolDic[type].Enqueue(obj);
        if(activeObj.TryGetValue(type, out var activeSet))
        {
            activeSet.Remove(obj);
        }
    }
 
    private void CreatePool(Type type)
    {
        if(poolDic.ContainsKey(type)) return;
        poolDic.Add(type, new Queue<Component>());
        activeObj.Add(type, new HashSet<Component>());
        CreatePoolParent(type);
    }
    private void CreatePoolParent(Type type)
    {
        GameObject parentObj = new GameObject(type.Name + "_Pool");
        parentObj.transform.SetParent(poolRoot);
        poolParent.Add(type, parentObj.transform);
    }
    public void ReturnAllActiveObj()
    {
        foreach(var pair in activeObj)
        {
            Type type = pair.Key;
            Component[] activeObjs = new Component[pair.Value.Count];
            pair.Value.CopyTo(activeObjs);

            foreach(Component obj in activeObjs)
            {
                if (obj == null) continue;
                obj.gameObject.SetActive(false);
                if(obj.transform.parent != poolParent[type])
                {
                    obj.transform.SetParent(poolParent[type]);
                }
                poolDic[type].Enqueue(obj);
            }
            pair.Value.Clear();
        }
    }
}
