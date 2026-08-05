using UnityEngine;

public class PoolPreLoader : MonoBehaviour
{
    [Header("积己且 橇府普")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Dropitem dropItemPrefab;

    [Header("积己 俺荐")]
    [SerializeField] private int enemyCount;
    [SerializeField] private int dropItemCount;

    private void Start()
    {
        MonsterPoolManager.Instance.PreLoadPool(enemyPrefab, enemyCount);
        MonsterPoolManager.Instance.PreLoadPool(dropItemPrefab, dropItemCount);
    }
}
