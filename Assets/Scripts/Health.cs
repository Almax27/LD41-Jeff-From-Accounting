using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    int m_recentlyDamagedCount = 0;

    [Header("Death")]

    [Tooltip("Time to wait befor destroying this gameobject after death, if < 0 will not destroy")]
    public float destroyOnDeathDelay = -1;

    [Header("Text")]
    public TextMeshPro m_healthText = null;
    public Color m_defaultColor = Color.white;
    public Color m_removedColor = Color.grey;
    public Color m_damagedColor = Color.red;
    
    public void TakeDamage(DamagePacket packet)
    {
        if (m_healthValue > 0)
        {
            int letterIndex = m_healthLetters.Length - m_healthValue;
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
                m_recentlyDamagedCount++;
                StartCoroutine(WaitForRecentDamage());
                OnDamage(packet);
                if (m_healthValue == 0)
                {
                    OnDeath(packet);
                }
            }
        }
    }

    public IEnumerator WaitForRecentDamage()
    {
        yield return new WaitForSeconds(0.5f);
        m_recentlyDamagedCount--;
        OnHealthChanged();
    }

    public void SetHealth(string healthLetters)
    {
        m_healthLetters = healthLetters;
        Reset();
    }

    private void Reset()
    {
        StopAllCoroutines();
        if (m_forceCaps) m_healthLetters = m_healthLetters.ToUpper();
        m_healthValue = m_healthLetters.Length;
        OnHealthChanged();
    }

    void OnHealthChanged()
    {
        if(m_healthText)
        {
            string defaultColorHex = ColorUtility.ToHtmlStringRGBA(m_defaultColor);
            string removedColorHex = ColorUtility.ToHtmlStringRGBA(m_removedColor);
            string damagedColorHex = ColorUtility.ToHtmlStringRGBA(m_damagedColor);
            string text = "";
            int nextLetterIndex = m_healthLetters.Length - m_healthValue;
            for (int i = 0; i < m_healthLetters.Length; i++)
            {
                if(i < nextLetterIndex - m_recentlyDamagedCount)
                {
                    text += string.Format("<color=#{0}>{1}</color>", removedColorHex, m_healthLetters[i]);
                }
                else if(i < nextLetterIndex)
                {
                    text += string.Format("<color=#{0}>{1}</color>", damagedColorHex, m_healthLetters[i]);
                }
                else
                {
                    text += string.Format("<color=#{0}>{1}</color>", defaultColorHex, m_healthLetters[i]);
                }
            }
            m_healthText.SetText(text);
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

    private void Update()
    {
        
    }
}
