using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attackable : MonoBehaviour
{
    protected ClickableObject clickableObject;
    protected ResourceSource resourceSource;

    private bool alive = true;

    public virtual bool isDead
    {
        get
        {
            return !alive;
        }
    }

    protected virtual void Awake()
    {
        resourceSource = GetComponent<ResourceSource>();
        clickableObject = GetComponent<ClickableObject>();
    }

    public virtual bool SufferAttack(int damage, GameObject source)
    {
        if (isDead)
        {
            return false;
        }
        ResourceCollector resourceCollector = source.GetComponent<ResourceCollector>();
        if (resourceCollector != null && resourceSource != null)
        {
            if (resourceCollector.isFull)
            {
                //return false;
            }
            int earnings = resourceSource.GetAmount(resourceCollector.woodPerHitEarnings);
            resourceCollector.AddResource(earnings, resourceSource.resourceType);
            if (resourceSource.isEmpty)
            {
                clickableObject.Die();
                alive = false;
            }
            else
            {
                clickableObject.anim?.SetTrigger("DoHit");
            }
            return true;
        }
        return false;
    }
}
