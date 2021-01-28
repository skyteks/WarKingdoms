using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeTree : MonoBehaviour
{
    public GameObject[] trunkPrefabs;
    public GameObject[] treePrefabs;

    public float scale = 1f;
    public GameObject tree;
    public GameObject trunk;

    private Transform modelHolder;
    private Transform treeHolder;

    void Awake()
    {
        modelHolder = transform.Find("Model");
        treeHolder = modelHolder.Find("TreeHolder");
    }

    void Start()
    {
        Randomize();
    }

    private static GameObject SelectRandomElement(IList<GameObject> prefabs)
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            throw new System.ArgumentOutOfRangeException();
        }

        int index = Random.Range(0, prefabs.Count);
        GameObject tmp = prefabs[index];
        return tmp;
    }

    private static Quaternion GetRandomYAxisRotation()
    {
        return Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }

    [ContextMenu("Randomize")]
    private void Randomize()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Cannot randomize Prefab during edit mode");
            return;
        }
        if (modelHolder == null)
        {
            throw new System.NullReferenceException();
        }
        if (trunk != null)
        {
            trunk.SetActive(false);
            trunk.transform.parent = null;
            Destroy(trunk);
        }
        if (tree != null)
        {
            tree.SetActive(false);
            tree.transform.parent = null;
            Destroy(tree);
        }

        GameObject trunkPrefab = SelectRandomElement(trunkPrefabs);
        trunk = Instantiate(trunkPrefab, modelHolder.position, GetRandomYAxisRotation(), modelHolder);
        trunk.transform.localScale = Vector3.one * scale;
        trunk.layer = modelHolder.gameObject.layer;

        GameObject treePrefab = SelectRandomElement(treePrefabs);
        tree = Instantiate(treePrefab, treeHolder.position, GetRandomYAxisRotation(), treeHolder);
        tree.transform.localScale = Vector3.one * scale;
        tree.layer = modelHolder.gameObject.layer;
    }
}
