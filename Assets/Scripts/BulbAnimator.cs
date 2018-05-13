using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulbAnimator : MonoBehaviour {

    public List<GameObject> bulbGroups = new List<GameObject>();

    public float speed = 1.0f;
    public float frequency = 1.0f;
    public int thickness = 1;
    public Color emissionColor = Color.white;
    public float emissionScale = 1.0f;

    float m_tick = 0.0f;

    private void Start()
    {
        if(bulbGroups.Count == 0)
        {
            foreach(Transform child in transform)
            {
                bulbGroups.Add(child.gameObject);
            }
        }
    }

    private void OnEnable()
    {
        m_tick = 0.0f;
        foreach (GameObject bulb in bulbGroups)
        {
            SetBulbEmission(bulb, 0);
        }
    }

    private void OnDisable()
    {
        foreach(GameObject bulb in bulbGroups)
        {
            SetBulbEmission(bulb, 0);
        }
    }


    // Update is called once per frame
    void Update ()
    {
        if (frequency <= 0) return;

        m_tick += Time.deltaTime * speed * frequency;
        float tval = m_tick % 1.0f;

        float step = 1.0f / (bulbGroups.Count + (thickness-1));
        for (int i = 0; i < bulbGroups.Count; i++)
        {
            float bulbStart = Mathf.Repeat(step * i * frequency, 1.0f);
            float bulbEnd = bulbStart + step * thickness * frequency;
            SetBulbEmission(bulbGroups[i], Mathf.PingPong(Mathf.InverseLerp(bulbStart, bulbEnd, tval) * 2.0f, 1.0f));
        }
    }

    void SetBulbEmission(GameObject bulb, float emission)
    {
        if (bulb)
        {
            foreach (MeshRenderer r in bulb.GetComponentsInChildren<MeshRenderer>())
            {
                r.material.SetColor("_EmissionColor", emissionColor * Mathf.LinearToGammaSpace(emission * emissionScale));
            }
        }
    }
}
