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
            var flashInstance = Instantiate(spawnEffectPrefab, transform.position, Quaternion.identity);
            flashInstance.transform.forward = gameObject.transform.forward;
            var flashPs = flashInstance.GetComponent<ParticleSystem>();
            if (flashPs != null)
            {
                Destroy(flashInstance, flashPs.main.duration);
            }
            else
            {
                var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
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
            var hitInstance = Instantiate(hitEffectPrefab, transform.position, transform.rotation);
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

            var hitPs = hitInstance.GetComponent<ParticleSystem>();
            if (hitPs != null)
            {
                Destroy(hitInstance, hitPs.main.duration);
            }
            else
            {
                var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitInstance, hitPsParts.main.duration);
            }
        }
        foreach (var detachedPrefab in detachedPrefabs)
        {
            if (detachedPrefab != null)
            {
                detachedPrefab.transform.parent = null;
            }
        }
    }
}
