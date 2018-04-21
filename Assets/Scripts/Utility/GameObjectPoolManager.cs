using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameObjectPool
{
    public GameObject templateObject = null;

    //Minimum number of objects in pool, this will be used to preallocate and as the Clean(TrimExcess) boundry
    public int minObjects = 0;

    //Maxium number of objects this pool can allocate, create() will return null when this limit is reached. Negative numbers are considered infinte
    public int maxObjects = -1;

    //How often null and inactive references are removed from the pool
    public float trimInterval = 1.0f;

    [System.NonSerialized]
    public List<GameObject> pooledInstances = new List<GameObject>();

    private float lastTrimTime = 0.0f;
    GameObject rootObject;

    public void Initialise(Transform parent)
    {
        if(templateObject == null)
        {
            Debug.LogError("Failed to initialise Pool, no templateObject defined");
            return;
        }

        rootObject = new GameObject();
        rootObject.name = "Pool_" + templateObject.name;
        rootObject.transform.parent = parent;

        //randomise first trim to avoid all pool trims being called on the same frame
        lastTrimTime = Time.time + Random.Range(0, trimInterval);
        for (int i = 0; i < minObjects; i++)
        {
            GameObject gobj = GetOrCreate();
            gobj.SetActive(false);
        }
    }

    public GameObject GetOrCreate()
    {
        GameObject instance = null;
        //First try and return an inactive object in the pool
        for (int i = 0; i < pooledInstances.Count; i++)
        {
            GameObject pooled = pooledInstances[i];
            if(pooled != null && !pooled.activeInHierarchy)
            {
                instance = pooled;
                break;
            }
        }

        //Otherwise spawn a new one if we are allowed
        if(instance == null && (maxObjects < 0 || pooledInstances.Count < maxObjects))
        {
            if(templateObject != null)
            {
                instance = GameObject.Instantiate<GameObject>(templateObject);
                pooledInstances.Add(instance);
            }
        }

        //if we obtained an instance then initialise it
        if(instance)
        {
            instance.SetActive(true);
            instance.transform.parent = rootObject.transform;
        }

        return instance;
    }

    public void Update()
    {
        if (Time.time > lastTrimTime + trimInterval)
        {
            lastTrimTime = Time.time;
            Trim();
        }
    }

    //Remove all null and inactive instances in the pool
    public void Trim()
    {
        //clean up null instances
        pooledInstances.RemoveAll(delegate (GameObject instance)
        {
            if (instance == null) return true;
            if(pooledInstances.Count > minObjects && !instance.activeInHierarchy)
            {
                GameObject.Destroy(instance);
                return true;
            }
            return false;
        });
    }
}

public class GameObjectPoolManager : SingletonBehaviour<GameObjectPoolManager>
{
    public List<GameObjectPool> staticPools = new List<GameObjectPool>();

    [Header("Dynamic Pools")]
    public int dynamicPoolMinObjects = 0;
    public int dynamicPoolMaxObjects = -1;

    Dictionary<int, GameObjectPool> poolsByInstanceID = new Dictionary<int, GameObjectPool>();

    private void Start()
    {
        for(int i = 0; i < staticPools.Count; i++)
        {
            GameObjectPool pool = staticPools[i];
            if (pool.templateObject == null)
            {
                Debug.LogWarningFormat("Static pool {0} has no templateObject defined", i);
            }
            else
            {
                poolsByInstanceID[pool.templateObject.GetInstanceID()] = pool;
            }
        }
        foreach (var pair in poolsByInstanceID)
        {
            pair.Value.Initialise(this.transform);
        }
    }

    private void Update()
    {
        foreach(var pair in poolsByInstanceID)
        {
            pair.Value.Update();
        }
    }

    public GameObject GetOrCreate(GameObject templateObject)
    {
        if (templateObject == null) return null;

        //find the pool with the given gameobject
        int instanceID = templateObject.GetInstanceID();
        GameObjectPool pool = null;
        if (!poolsByInstanceID.TryGetValue(instanceID, out pool))
        {
            //if there was no pool for this object create a new one
            pool = new GameObjectPool();
            pool.templateObject = templateObject;
            pool.minObjects = dynamicPoolMinObjects;
            pool.maxObjects = dynamicPoolMaxObjects;
            pool.Initialise(this.transform);
            poolsByInstanceID[instanceID] = pool;
        }

        return pool.GetOrCreate();
    }
}
