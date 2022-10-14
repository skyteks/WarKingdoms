using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubParticleSpawner : MonoBehaviour
{
    public enum SubParticleSpawnRotation
    {
        PrefabAsIs,
        FirePoint,
        RotationOffset,
    }

    public float hitOffset = 0f;
    public SubParticleSpawnRotation useFirePointRotation;
    public Vector3 rotationOffset = new Vector3(0, 0, 0);
    public GameObject hitEffectPrefab;
    public GameObject spawnEffectPrefab;
    private Rigidbody rb;
    public GameObject[] detachedPrefabs;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (spawnEffectPrefab != null)
        {
            GameObject flashInstance = Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);
            flashInstance.SetLayerRecursivly(gameObject.layer);
            flashInstance.transform.forward = gameObject.transform.forward;
            ParticleSystem flashPs = flashInstance.GetComponent<ParticleSystem>();
            if (flashPs != null)
            {
                Destroy(flashInstance, flashPs.main.duration);
            }
            else
            {
                ParticleSystem flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(flashInstance, flashPsParts.main.duration);
            }
        }
        Destroy(gameObject, 5);
    }

    public void TriggerEnter()
    {
        //Lock all axes movement and rotation
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (hitEffectPrefab != null)
        {
            GameObject hitInstance = Instantiate(hitEffectPrefab, transform.position, transform.rotation);
            hitInstance.SetLayerRecursivly(gameObject.layer);
            switch (useFirePointRotation)
            {
                case SubParticleSpawnRotation.PrefabAsIs:
                    break;
                case SubParticleSpawnRotation.FirePoint:
                    hitInstance.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
                    break;
                case SubParticleSpawnRotation.RotationOffset:
                    hitInstance.transform.rotation = Quaternion.Euler(rotationOffset);
                    break;
            }

            ParticleSystem hitPs = hitInstance.GetComponent<ParticleSystem>();
            if (hitPs != null)
            {
                Destroy(hitInstance, hitPs.main.duration);
            }
            else
            {
                ParticleSystem hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitInstance, hitPsParts.main.duration);
            }
        }
        foreach (var detachedPrefab in detachedPrefabs)
        {
            if (detachedPrefab != null)
            {
                detachedPrefab.transform.SetParent(null);
            }
        }
    }
}
