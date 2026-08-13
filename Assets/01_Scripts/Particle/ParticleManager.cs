using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private MonsterPoolManager monsterPoolManager;
    [SerializeField] private List<ParticleData> particleList;
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
    public void GetParticle(int code, Vector3 position, Quaternion rotation)
    {
        particleDic.TryGetValue(code, out nowParticle);

        foreach (var data in particleList) 
        { 
            if(nowParticle.particleName == "ChasingSickle")
            {
                ChasingSickle cs = monsterPoolManager.GetPool<ChasingSickle>();
                if (cs == null) return;
                cs.transform.position = position;
                cs.transform.rotation = rotation;
                ParticleSystem ps = cs.GetComponent<ParticleSystem>();
                ps.Play();
            }
            if(nowParticle.particleName == "FireCircle")
            {
                FireCircle cs = monsterPoolManager.GetPool<FireCircle>();
                if (cs == null) return;
                cs.transform.position = position;
                cs.transform.rotation = rotation;
                ParticleSystem ps = cs.GetComponent<ParticleSystem>();
                ps.Play();
            }
            if(nowParticle.particleName == "FlowerThorns")
            {
                FlowerThorns cs = monsterPoolManager.GetPool<FlowerThorns>();
                if (cs == null) return;
                cs.transform.position = position;
                cs.transform.rotation = rotation;
                ParticleSystem ps = cs.GetComponent<ParticleSystem>();
                ps.Play();
            }
            if(nowParticle.particleName == "LightningRay")
            {
                LightningRay cs = monsterPoolManager.GetPool<LightningRay>();
                if (cs == null) return;
                cs.transform.position = position;
                cs.transform.rotation = rotation;
                ParticleSystem ps = cs.GetComponent<ParticleSystem>();
                ps.Play();
            }
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