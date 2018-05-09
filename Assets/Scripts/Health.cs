using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Audio;

public struct DamagePacket
{
    public GameObject instigator;

    public Vector3 hitNormal;

    public char letter;
    public bool forceLetterMatch;

    public DamagePacket(GameObject _instigator = null, Vector3 _hitNormal = new Vector3(), bool _forceMatch = false, char _letter = (char)0)
    {
        instigator = _instigator;
        hitNormal = _hitNormal;
        letter = _letter;
        forceLetterMatch = _forceMatch;
    }
}


public class Health : MonoBehaviour {

    [Header("Health")]
    public string m_healthLetters = "HEALTH";
    public bool m_forceCaps = true;
    public bool m_ignoreLetters = false;
    public bool m_alwaysVisible = false;
    public bool m_ignoreRaycat = false;
    int m_healthValue = 0;
    int m_recentlyDamagedCount = 0;
    bool m_isTargeted = false;

    [Header("Audio")]
    public FAFAudioSFXSetup m_SFXOnHitNoDamage = null;
    public FAFAudioSFXSetup m_SFXOnDamaged = null;
    public FAFAudioSFXSetup m_SFXOnDeath = null;

    [Header("Death")]
    [Tooltip("Time to wait befor destroying this gameobject after death, if < 0 will not destroy")]
    public float destroyOnDeathDelay = 0.1f;

    [Header("Text")]
    public TMP_Text m_healthText = null;
    public Color m_defaultColor = Color.white;
    public Color m_removedColor = Color.grey;
    public Color m_damagedColor = Color.red;
    Coroutine m_fadeTextCorountine = null;

    public bool IsAlive() { return isActiveAndEnabled && m_healthValue > 0; }

    public bool TakeDamage(DamagePacket packet)
    {
        if (m_healthValue > 0)
        {
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
                HandleDamage(packet);
                if (m_healthValue == 0)
                {
                    HandleDeath(packet);
                }
                return true;
            }
            else
            {
                HandleHitNoDamage(packet);
            }
        }
        return false;
    }

    public void Heal()
    {
        m_recentlyDamagedCount = Mathf.Max(m_recentlyDamagedCount - 1, 0);
        m_healthValue = Mathf.Min(m_healthValue + 1, m_healthLetters.Length);
        OnHealthChanged();
    }

    public IEnumerator WaitForRecentDamage()
    {
        SetIsTextVisible(true);
        yield return new WaitForSeconds(0.5f);
        m_recentlyDamagedCount = Mathf.Max(m_recentlyDamagedCount - 1, 0);
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
        if(m_healthText && !m_alwaysVisible)
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

    public void Reset()
    {
        StopAllCoroutines();
        if (m_forceCaps) m_healthLetters = m_healthLetters.ToUpper();
        m_healthValue = m_healthLetters.Length;
        m_recentlyDamagedCount = 0;
        OnHealthChanged();
        SetIsTextVisible(m_alwaysVisible, true);
    }

    private void OnValidate()
    {
        OnHealthChanged();
        if (tag != "Player")
        {
            gameObject.name = m_healthLetters;
        }
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

    void HandleHitNoDamage(DamagePacket packet)
    {
        if(m_SFXOnHitNoDamage)
        {
            m_SFXOnHitNoDamage.Play(transform.position);
        }
        SendMessage("OnHitNoDamage", packet, SendMessageOptions.DontRequireReceiver);
    }

    void HandleDamage(DamagePacket packet)
    {
        Debug.Log(gameObject.name + " Took damage: " + packet.letter);
        OnHealthChanged();
        if (m_SFXOnDamaged)
        {
            float SFXPitchOffset = 1.0f - m_SFXOnDamaged.Pitch;
            SFXPitchOffset *= m_healthLetters.Length > 0 ? 1.0f - ((float)m_healthValue / m_healthLetters.Length) : 1.0f;
            m_SFXOnDamaged.Play(transform.position, 1.0f, 1.0f, SFXPitchOffset);
        }
        if (tag != "Player")
        {
            MeleeEnemyController melee = GetComponent<MeleeEnemyController>();
            if(melee)
            {
                melee.SetTarget(GameManager.Instance.Player.transform);
            }
        }
        SendMessage("OnDamage", packet, SendMessageOptions.DontRequireReceiver);
    }

    void HandleDeath(DamagePacket packet)
    {
        if(destroyOnDeathDelay >= 0)
        {
            Destroy(gameObject, destroyOnDeathDelay);
        }

        //Mega end game hack
        if (tag == "Player")
        {
            GameManager.Instance.OnPlayerKilled();
        }
        else
        {
            GameManager.Instance.OnEnemyKilled(this);
        }
        if(m_SFXOnDeath)
        {
            m_SFXOnDeath.Play(transform.position);
        }
        SendMessage("OnDeath", packet, SendMessageOptions.DontRequireReceiver);
    }

    private void Start()
    {
        Reset();
    }
}
