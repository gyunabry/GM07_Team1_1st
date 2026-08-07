using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ParticleMain : MonoBehaviour
{
    private ParticleSystem ps;
    [SerializeField] private MonsterPoolManager monsterPoolManager;
    [SerializeField] private AttackSO attackSo;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }
    private void OnEnable()
    {
        StartCoroutine(PlayCo());
    }
    IEnumerator PlayCo()
    {
        if(ps.isPlaying == true)
        {
            yield return null;
        }
        else
        {
            monsterPoolManager.ReturnPool(this);
        }
    }
}
