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

    private Transform modelHolder, modelHolder2, treeHolder;

    void Awake()
    {
        modelHolder = transform.Find("Model");
        treeHolder = modelHolder.GetChild(0);
        modelHolder2 = transform.Find("Model2");
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
        if (modelHolder == null || treeHolder == null)
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

        if (modelHolder2 != null)
        {
            GameObject trunkPrefab = SelectRandomElement(trunkPrefabs);
            trunk = Instantiate(trunkPrefab, modelHolder2.position, GetRandomYAxisRotation(), modelHolder2);
            trunk.transform.localScale = Vector3.one * scale;
            trunk.layer = modelHolder2.gameObject.layer;
        }

        GameObject treePrefab = SelectRandomElement(treePrefabs);
        tree = Instantiate(treePrefab, modelHolder.position, GetRandomYAxisRotation(), treeHolder);
        tree.transform.localScale = Vector3.one * scale;
        tree.layer = modelHolder.gameObject.layer;
    }
}
