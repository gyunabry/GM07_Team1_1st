using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AttackEquButtonList : MonoBehaviour
{
    [SerializeField] List<AttackEquHud> equHud = new List<AttackEquHud>();
    [SerializeField] PlayerAttack playerAttack;

    private void Awake()
    {
        equHud.Clear();
        AttackEquHud[] attackEquHud = transform.GetComponentsInChildren<AttackEquHud>();
        foreach(var equ in attackEquHud)
        {
            equHud.Add(equ);
        }
        
        int i = 0;
        foreach(var slot in equHud)
        {
            slot.slotIndex = i;
            i++;
        }
    }
}
