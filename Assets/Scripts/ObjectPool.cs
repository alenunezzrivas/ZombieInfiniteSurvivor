using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolConfig
    {
        public GameObject prefab;
        public int preAllocate = 10;
    }

    public PoolConfig[] configs;

    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    void Awake()
    {
        if (configs == null) return;

        foreach (PoolConfig cfg in configs)
        {
            if (cfg.prefab == null) continue;

            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < cfg.preAllocate; i++)
            {
                GameObject obj = CrearNuevo(cfg.prefab);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            pools[cfg.prefab] = queue;
        }
    }

    GameObject CrearNuevo(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.name = prefab.name;
        return obj;
    }

    public GameObject Get(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            if (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                obj.SetActive(true);
                return obj;
            }
        }

        GameObject nuevo = CrearNuevo(prefab);
        nuevo.SetActive(true);
        return nuevo;
    }

    public void ReturnToPool(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);

        if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        queue.Enqueue(obj);
    }
}
