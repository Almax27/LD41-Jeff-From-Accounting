using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    static T s_instance = null;

	public static T Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = (T)FindObjectOfType(typeof(T));
				if (s_instance == null)
				{
					GameObject gobj = new GameObject(typeof(T).ToString());
					s_instance = gobj.AddComponent<T>();
				}
			}
			return s_instance;
		}
	}
	public static void DestroyInstance()
	{
		Destroy(s_instance.gameObject);
		s_instance = null;
	}
	
	virtual protected void Start()
	{
		//there can only be one!
		if (s_instance && s_instance != this)
		{
            Debug.LogError("There can only be one " + typeof(T).ToString() + ", deleting...");
			Destroy(this.gameObject);
		}
        else
        {
            s_instance = this as T;
        }
	}

	virtual protected void OnDestroy()
	{
		if(s_instance == this)
		{
			s_instance = null;
		}
	}
}
