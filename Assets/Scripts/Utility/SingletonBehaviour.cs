using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	static T instance = null;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (T)FindObjectOfType(typeof(T));
				if (instance == null)
				{
					GameObject gobj = new GameObject(typeof(T).ToString());
					instance = gobj.AddComponent<T>();
					//DontDestroyOnLoad(gobj);
				}
			}
			return instance;
		}
	}
	public static void DestroyInstance()
	{
		Destroy(instance.gameObject);
		instance = null;
	}
	
	virtual protected void Awake()
	{
		//there can only be one!
		if (instance)
		{
			Debug.Break();
			Destroy(this.gameObject);
		}
	}

	virtual protected void OnDestroy()
	{
		if(instance == this)
		{
			instance = null;
		}
	}
}
