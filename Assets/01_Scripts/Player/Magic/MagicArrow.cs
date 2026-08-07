using UnityEngine;
using System.Collections;

public class MagicArrow : MonoBehaviour
{
    private ParticleSystem ps;
    [SerializeField] private MonsterPoolManager monsterPoolManager;

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
        if (ps.isPlaying == true)
        {
            yield return null;
        }
        else
        {
            monsterPoolManager.ReturnPool(this);
        }
    }
}
