using UnityEngine;

[System.Serializable]
public class MinMax
{
    public MinMax() : this(0, 0) { }
    public MinMax(float _min, float _max)
    {
        min = _min;
        max = _max;
    }

    public float min = 0;
	public float max = 0;

    public float size { get { return Mathf.Abs(max - min); } }

	public float RandomInRange()
	{
		return Random.Range(min, max);
	}

    public float Clamp(float value)
    {
        return Mathf.Clamp(value, min, max);
    }

    public float Lerp(float t, bool clamp = true)
    {
        float value = min + (max - min) * t;
        if(clamp)
        {
            value = Mathf.Clamp(value, min, max);
        }
        return value;
    }

    public float InverseLerp(float value, bool clamp = true)
    {
        value -= min;
        value /= max - min;
        if(clamp)
        {
            value = Mathf.Clamp01(value);
        }
        return value;
    }
}
