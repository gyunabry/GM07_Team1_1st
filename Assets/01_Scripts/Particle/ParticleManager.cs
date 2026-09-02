using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private PoolManager monsterPoolManager;
    [SerializeField] private List<ParticleData> particleList;
    [SerializeField] private Player player;
    private Dictionary<int, ParticleData> particleDic = new Dictionary<int, ParticleData>();
    private ParticleData nowParticle;

    private void Awake()
    {
        foreach(var data in particleList)
        {
            if (!particleDic.ContainsKey(data.particleID))
            {
                particleDic.Add(data.particleID, data);
            }
        }
    }
    public void GetParticle(int code, Vector3 position, Quaternion rotation, float damage, float distance, float attackSpeed)
    {
        particleDic.TryGetValue(code, out nowParticle);

        
            if(nowParticle.particleName == "ChasingSickle")
            {
                ChasingSickle cs = monsterPoolManager.GetPool<ChasingSickle>();
                if (cs == null) return;
                cs.transform.position = position;
                cs.transform.rotation = rotation;
                ParticleSystem[] ps = cs.GetComponentsInChildren<ParticleSystem>();
                ps[0].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                float dis = distance * 0.75f;
                ps[0].transform.localScale = new Vector3(dis, dis, dis);
                foreach (var p in ps)
                {
                    var pm = p.main;
                }

                ps[0].Play();
            }
            if(nowParticle.particleName == "FireCircle")
            {
                FireCircle cs = monsterPoolManager.GetPool<FireCircle>();
                if (cs == null) return;
                cs.transform.position = position;
                cs.transform.rotation = rotation;
                cs.transform.SetParent(player.transform);
                ParticleSystem[] ps = cs.GetComponentsInChildren<ParticleSystem>();
                ps[0].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                float dis = distance * 0.8f;
                ps[0].transform.localScale = new Vector3(dis, dis, dis);
                foreach (var p in ps)
                {
                    var pm = p.main;
                }

                ps[0].Play();
            }
            if(nowParticle.particleName == "FlowerThorns")
            {
                FlowerThorns cs = monsterPoolManager.GetPool<FlowerThorns>();
                if (cs == null) return;
                cs.transform.position = position;
                cs.transform.rotation = rotation;
                ParticleSystem[] ps = cs.GetComponentsInChildren<ParticleSystem>();
                ps[0].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                float dis = distance * 0.7f;
                ps[0].transform.localScale = new Vector3(dis, dis, dis);
                foreach (var p in ps)
                {
                    var pm = p.main;
                }

                ps[0].Play();
            }
            if(nowParticle.particleName == "LightningRay")
            {
                LightningRay cs = monsterPoolManager.GetPool<LightningRay>();
                if (cs == null) return;
                cs.damage = damage;
                cs.transform.position = position;
                cs.transform.localPosition = new Vector3(cs.transform.position.x, -3f, cs.transform.position.z);
                cs.transform.rotation = rotation;
                ParticleSystem[] ps = cs.GetComponentsInChildren<ParticleSystem>();
                ps[0].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                float dis = distance * 0.2f;
                ps[0].transform.localScale = new Vector3(dis, dis, dis);
                foreach (var p in ps)
                {
                    var pm = p.main;
                }

                ps[0].Play();
            }
        
    }

}
[System.Serializable]
public class ParticleData
{
    public int particleID;
    public string particleName;
    public ParticleSystem particlePrefab;
}