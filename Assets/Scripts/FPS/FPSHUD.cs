using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class PromptSetup
{
    public PromptSetup(string _message = "", float _delay = 0, float _duration = 3, int _eventsToTrigger = 0, int _maxTimesToShow = -1)
    {
        message = _message;
        delay = _delay;
        duration = _duration;
        eventsToTrigger = _eventsToTrigger;
        maxTimesToShow = _maxTimesToShow;
    }

    public string message = "";
    public float delay = 0;
    public float duration = 3;
    public int eventsToTrigger = 0;
    public int maxTimesToShow = -1;

    [System.NonSerialized]
    public int eventCount = 0;

    [System.NonSerialized]
    public int triggerCount = 0;
}

public class FPSHUD : MonoBehaviour {

    [Header("Prompts")]
    public float promptFadeDuration = 0.3f;
    public PromptSetup m_reloadPrompt = new PromptSetup("Press R To Reload");
    public PromptSetup m_deathPrompt = new PromptSetup("YOU DIED");
    public PromptSetup m_pausePrompt = new PromptSetup("Paused...");
    public PromptSetup m_sprintPrompt = new PromptSetup("Press SHIFT to sprint");

    [Header("Widgets")]
    public RectTransform m_crosshair = null;
    public TextMeshProUGUI m_promptText = null;

    Coroutine m_pendingPrompt = null;
    PromptSetup m_displayedPrompt = null;

    float m_desiredMoveTick = 0;

    private void Awake()
    {
        if (m_promptText)
        {
            m_promptText.enabled = false;
        }
    }

    private void OnEnable()
    {
        if(m_pendingPrompt != null)
        {
            StopCoroutine(m_pendingPrompt);
        }
        m_pendingPrompt = StartCoroutine(DoShowPrompt(m_displayedPrompt));
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
            prompt.eventCount++;
            if (prompt.eventCount < prompt.eventsToTrigger)
            {
                return;
            }
            if(prompt.maxTimesToShow >= 0 && prompt.triggerCount >= prompt.maxTimesToShow)
            {
                return;
            }
            prompt.eventCount = 0;
            prompt.triggerCount++;
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
            Color c = m_promptText.color;
            c.a = 0;
            m_promptText.color = c;
            yield return new WaitForSecondsRealtime(prompt.delay);
            while (c.a < 1)
            {
                c.a = Mathf.Clamp01(c.a + Time.unscaledDeltaTime / promptFadeDuration);
                m_promptText.color = c;
                yield return null;
            }
            yield return new WaitForSecondsRealtime(prompt.duration);
            while (c.a > 0)
            {
                c.a = Mathf.Clamp01(c.a - Time.unscaledDeltaTime / promptFadeDuration);
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
        m_reloadPrompt.eventCount = 0;
    }

    public void OnDeath()
    {
        TryShowPrompt(m_deathPrompt);
    }

    public void OnPaused()
    {
        TryShowPrompt(m_pausePrompt);
    }

    public void OnUnpaused()
    {
        TryShowPrompt(null);
    }

    public void OnMove(float dt, Vector3 inputVelocity, bool isRunning)
    {
        //disable sprint prompt if player knows how to sprint
        if (isRunning)
        {
            //hide prompt immediately if it's visible
            if (m_displayedPrompt == m_sprintPrompt)
            {
                TryShowPrompt(null);
            }
            m_sprintPrompt.maxTimesToShow = 0;
        }
        //show prompt if player have been moving for a time
        else if (m_displayedPrompt != m_sprintPrompt)
        {
            if (inputVelocity.sqrMagnitude > 0)
            {
                m_desiredMoveTick += dt;
            }
            else
            {
                m_desiredMoveTick = 0;
            }
            if (m_desiredMoveTick > 3.0f)
            {
                TryShowPrompt(m_sprintPrompt);
                m_desiredMoveTick = 0;
            }
        }
    }
}
