using UnityEngine;
using UnityEngine.InputSystem;

public class SkillTreeESC : MonoBehaviour
{
    InputAction ia;

    private void Awake()
    {
        ia = InputSystem.actions.FindAction("Cancel");
    }
    void Update()
    {
        if (ia.WasPressedThisFrame())
        {
            this.gameObject.SetActive(false);
        }
    }
}
