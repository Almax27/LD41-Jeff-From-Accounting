using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Spawner
{
    [System.Serializable]
    public class SpawnDefinition
    {
        public GameObject gameobject = null;
        public bool addAsChild = false;
        public bool copyPosition = true;
        public bool copyRotation = false;
        public bool copyLocalScale = false;
    }

    public SpawnDefinition[] objectsToSpawn = new SpawnDefinition[0];

    public void ProcessSpawns(Transform parent)
    {
        if (parent)
        {
            ProcessSpawns(parent, parent.position, parent.rotation, parent.localScale);
        }
        else
        {
            ProcessSpawns(parent, Vector3.zero, Quaternion.identity, Vector3.zero);
        }
    }

    public void ProcessSpawns(Transform parent, Vector3 position, Quaternion rotation, Vector3 localScale)
    {
        foreach (SpawnDefinition def in objectsToSpawn)
        {
            if (def.gameobject)
            {
                position = def.copyPosition ? position : Vector3.zero;
                rotation = def.copyRotation ? rotation : Quaternion.identity;
                var gobj = GameObjectPoolManager.Instance.GetOrCreate(def.gameobject, position, rotation);
                if (def.addAsChild)
                {
                    gobj.transform.parent = parent;
                }
                if (def.copyLocalScale)
                {
                    gobj.transform.localScale = localScale;
                }
            }
        }
    }
}
