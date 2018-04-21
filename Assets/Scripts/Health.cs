using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct DamagePacket
{
    public GameObject instigator;
    public char letter;
    public bool forceLetterMatch;
}


public class Health : MonoBehaviour {

    [Header("Health")]
    public string m_healthLetters = "HEALTH";
    public bool m_forceCaps = true;
    public bool m_ignoreLetters = false;
    int m_healthValue = 0;

    [Header("Death")]

    [Tooltip("Time to wait befor destroying this gameobject after death, if < 0 will not destroy")]
    public float destroyOnDeathDelay = -1;

    [Header("Text")]
    public Text m_healthText = null;
    public Color m_defaultColor = Color.white;
    public Color m_damagedColor = Color.grey;

    public void TakeDamage(DamagePacket packet)
    {
        if (m_healthValue > 0)
        {
            char nextLetter = m_healthLetters[m_healthLetters.Length - m_healthValue];
            if (m_forceCaps)
            {
                packet.letter = packet.letter.ToString().ToUpper()[0];
                nextLetter = nextLetter.ToString().ToUpper()[0];
            }
            if (m_ignoreLetters || packet.forceLetterMatch)
            {
                packet.letter = nextLetter;
            }
            if (packet.letter == nextLetter)
            {
                m_healthValue--;
                OnDamage(packet);
                if (m_healthValue == 0)
                {
                    OnDeath(packet);
                }
            }
        }
    }

    public void SetHealth(string healthLetters)
    {
        m_healthLetters = healthLetters;
        Reset();
    }

    private void Reset()
    {
        if (m_forceCaps) m_healthLetters = m_healthLetters.ToUpper();
        m_healthValue = m_healthLetters.Length;
        OnHealthChanged();
    }

    void OnHealthChanged()
    {
        if(m_healthText)
        {
            string defaultColorHex = ColorUtility.ToHtmlStringRGBA(m_defaultColor);
            string damagedColorHex = ColorUtility.ToHtmlStringRGBA(m_damagedColor);
            string text = "";
            for(int i = 0; i < m_healthLetters.Length; i++)
            {
                if(i < m_healthLetters.Length - m_healthValue)
                {//damaged
                    text += string.Format("<color=#{0}>{1}</color>", damagedColorHex, m_healthLetters[i]);
                }
                else
                {
                    text += string.Format("<color=#{0}>{1}</color>", defaultColorHex, m_healthLetters[i]);
                }
            }
            m_healthText.text = text;
        }
    }

    void OnDamage(DamagePacket packet)
    {
        Debug.Log(gameObject.name + " Took damage: " + packet.letter);
        OnHealthChanged();
    }

    void OnDeath(DamagePacket packet)
    {
        if(destroyOnDeathDelay >= 0)
        {
            Destroy(gameObject, destroyOnDeathDelay);
        }
    }

    private void Start()
    {
        Reset();
    }
}
