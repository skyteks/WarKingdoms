using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class Healthpoints : Attackable
{
    private ClickableObject clickableObject;

    public override bool isDead
    {
        get
        {
            if (interactableObject == null)
            {
                Awake();
            }
            return clickableObject.template.health <= 0;
        }
    }

    private float damageReductionMuliplier
    {
        get
        {
            return clickableObject.template.armor >= 0 ? 100f / (100f + clickableObject.template.armor) : 2f - (100f / (100f - clickableObject.template.armor));
        }
    }

    protected override void Awake()
    {
        base.Awake();
        clickableObject = GetComponent<ClickableObject>();
    }

    public override bool SufferAttack(int damage, GameObject source)
    {
        if (isDead)
        {
            return false;
        }

        damage = Mathf.RoundToInt(damage);
        clickableObject.template.health -= damage;

        if (interactableObject is Building)
        {
            (interactableObject as Building).TriggerBurnEffects();
        }

        if (clickableObject.template.health <= 0)
        {
            interactableObject.Die();
        }
        return true;
    }

    [ContextMenu("Debug Kill")]
    private void DebugKill()
    {
        SufferAttack(clickableObject.template.health, null);
    }
}
