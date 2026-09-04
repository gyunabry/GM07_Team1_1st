using UnityEngine;

public class ProductionAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ESFXType operationSfx = ESFXType.Work_Production;

    private ProductionBuilding building;

    private void Awake()
    {
        building = GetComponent<ProductionBuilding>();
    }

    private void OnEnable()
    {
        building.StateChanged += HandleStateChanged;
        HandleStateChanged(building.State);
    }

    private void OnDisable()
    {
        building.StateChanged -= HandleStateChanged;

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void HandleStateChanged(ProductionState state)
    {
        if (state == ProductionState.Producing)
        {
            AudioManager.Instance.PlayLoopSFX(operationSfx, audioSource);
        }
        else
        {
            AudioManager.Instance.StopSFX(audioSource);
        }
    }
}
