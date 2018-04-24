using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class PromptSetup
{
    public string message = "";
    public int eventsToTrigger = 1;
    public float delay = 0;
    public float duration = 3;

    [System.NonSerialized]
    public int triggerCount = 0;
}

public class FPSHUD : MonoBehaviour {

    [Header("Prompts")]
    public float promptFadeDuration = 0.3f;
    public PromptSetup m_reloadPrompt;
    public PromptSetup m_deathPrompt;

    [Header("Widgets")]
    public RectTransform m_crosshair = null;
    public TextMeshProUGUI m_promptText = null;

    Coroutine m_pendingPrompt = null;
    PromptSetup m_displayedPrompt = null;

    private void Start()
    {
        if (m_promptText)
        {
            m_promptText.enabled = false;
        }
    }

    public void SetCrosshairVisible(bool isVisible)
    {
        if(m_crosshair)
        {
            m_crosshair.gameObject.SetActive(isVisible);
        }
    }

    public void TryShowPrompt(PromptSetup prompt)
    {
        if (prompt == m_displayedPrompt) return;

        if (prompt != null)
        {
            if (prompt.triggerCount < prompt.eventsToTrigger)
            {
                prompt.triggerCount++;
                return;
            }
            prompt.triggerCount = 0;
        }

        if (m_pendingPrompt != null) StopCoroutine(m_pendingPrompt);
        m_pendingPrompt = StartCoroutine(DoShowPrompt(prompt));
    }

    IEnumerator DoShowPrompt(PromptSetup prompt)
    {
        if (m_promptText == null) yield break;
        if(prompt != null && prompt.duration > 0)
        {
            m_displayedPrompt = prompt;
            m_promptText.SetText(prompt.message);
            m_promptText.enabled = true;
            yield return new WaitForSeconds(prompt.delay);
            Color c = m_promptText.color;
            c.a = 0;
            m_promptText.color = c;
            while (c.a < 1)
            {
                c.a = Mathf.Clamp01(c.a + Time.deltaTime / promptFadeDuration);
                m_promptText.color = c;
                yield return null;
            }
            yield return new WaitForSeconds(prompt.duration);
            while (c.a > 0)
            {
                c.a = Mathf.Clamp01(c.a - Time.deltaTime / promptFadeDuration);
                m_promptText.color = c;
                yield return null;
            }
        }
        m_promptText.SetText("");
        m_promptText.enabled = false;
        m_displayedPrompt = null;
    }

    public void OnOutOfAmmo()
    {
        TryShowPrompt(m_reloadPrompt);
    }

    public void OnReloadStarted()
    {
        TryShowPrompt(null);
        m_reloadPrompt.triggerCount = 0;
    }

    public void OnDeath()
    {
        TryShowPrompt(m_deathPrompt);
    }
}
