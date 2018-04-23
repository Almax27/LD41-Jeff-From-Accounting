using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

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
    bool m_isTargeted = false;

    [Header("Death")]
    [Tooltip("Time to wait befor destroying this gameobject after death, if < 0 will not destroy")]
    public float destroyOnDeathDelay = 0.1f;

    [Header("Text")]
    public TextMeshPro m_healthText = null;
    public Color m_defaultColor = Color.white;
    public Color m_removedColor = Color.grey;
    public Color m_damagedColor = Color.red;
    Coroutine m_fadeTextCorountine = null;

    public bool TakeDamage(DamagePacket packet)
    {
        if (m_healthValue > 0)
        {
            int letterIndex = m_healthLetters.Length - m_healthValue;
            char nextLetter = m_healthLetters[m_healthLetters.Length - m_healthValue];
            while(!char.IsLetter(nextLetter) && m_healthValue > 0)
            {
                m_healthValue--;
                nextLetter = m_healthLetters[m_healthLetters.Length - m_healthValue];
            }
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
                return true;
            }
        }
        return false;
    }

    public IEnumerator WaitForRecentDamage()
    {
        SetIsTextVisible(true);
        yield return new WaitForSeconds(0.5f);
        m_recentlyDamagedCount--;
        OnHealthChanged();
        SetIsTextVisible(false);
    }

    public void SetHealth(string healthLetters)
    {
        m_healthLetters = healthLetters;
        m_healthLetters = new string(healthLetters.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
        Reset();
    }

    public void SetIsTextVisible(bool isVisible, bool force = false)
    {
        if(m_healthText)
        {
            if (force || (m_healthText.isActiveAndEnabled && m_fadeTextCorountine == null) != isVisible)
            {
                if (m_fadeTextCorountine != null)
                {
                    StopCoroutine(m_fadeTextCorountine);
                    m_fadeTextCorountine = null;
                }
                if (isVisible || force)
                {
                    m_healthText.enabled = isVisible;
                    Color color = m_healthText.color;
                    color.a = 1.0f;
                    m_healthText.color = color;
                }
                else
                {
                    m_fadeTextCorountine = StartCoroutine(FadeOutText(0.5f, 0.2f));
                }
            }
        }
    }

    public IEnumerator FadeOutText(float delay, float fadeTime)
    {
        yield return new WaitForSeconds(delay);
        //wait for recent damage to end
        while(m_recentlyDamagedCount != 0)
        {
            yield return null;
        }
        while (m_healthText && fadeTime > 0)
        {
            Color color = m_healthText.color;
            color.a = Mathf.Clamp01(color.a - Time.deltaTime / fadeTime);
            m_healthText.color = color;
            if(color.a <= 0)
            {
                m_healthText.enabled = false;
                break;
            }
            yield return null;
        }
    }

    private void Reset()
    {
        StopAllCoroutines();
        if (m_forceCaps) m_healthLetters = m_healthLetters.ToUpper();
        m_healthValue = m_healthLetters.Length;
        OnHealthChanged();
        SetIsTextVisible(false, true);
    }

    private void OnValidate()
    {
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
}
