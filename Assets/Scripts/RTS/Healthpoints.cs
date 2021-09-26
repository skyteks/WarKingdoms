using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class Healthpoints : Attackable
{
    public override bool isDead
    {
        get
        {
            if (clickableObject == null)
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
    }

    public override bool SufferAttack(int damage, GameObject source)
    {
        if (isDead)
        {
            return false;
        }

        damage = Mathf.RoundToInt(damage * damageReductionMuliplier);
        clickableObject.template.health -= damage;

        if (clickableObject is Building)
        {
            (clickableObject as Building).TriggerBurnEffects();
        }

        if (clickableObject.template.health <= 0)
        {
            clickableObject.Die();
        }
        return true;
    }
}
