using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroStage : GameStage {

    public string m_playerIntroAnim = "PlayerIntro";

    private Coroutine m_waitForIntroRoutine = null;

    public override void OnStageBegan()
    {
        base.OnStageBegan();

        if (m_waitForIntroRoutine != null)
            StopCoroutine(m_waitForIntroRoutine);

        FPSPlayerController player = GameManager.Instance.Player;
        if (player)
        {
            Animation animation = player.GetComponent<Animation>();
            bool canPlayIntroCutscene = !Application.isEditor || !GameManager.Instance.spawnAtSceneViewCamera || !GameManager.Instance.skipCutscenes;
            if (animation && canPlayIntroCutscene)
            {
                player.m_isInputEnabled = false;
                player.m_gunController.gameObject.SetActive(false);
                animation.Play(m_playerIntroAnim);
                m_waitForIntroRoutine = StartCoroutine(WaitForIntro(animation));
            }
            else
            {
                OnIntroFinished();
            }

            //START MESSAGE HACK
            if (player.m_fpsHUD)
            {
                player.m_fpsHUD.TryShowPrompt(new PromptSetup("Jeff from Accounting\nMade for LudumDare 41\nBy Greg Lee, Dale Smith and Aaron Baumbach", 0, 10.0f));
            }
        }
    }

    public override bool IsStageFinished()
    {
        return base.IsStageFinished() && m_waitForIntroRoutine == null;
    }

    IEnumerator WaitForIntro(Animation animation)
    {
        while (animation && animation.IsPlaying(m_playerIntroAnim))
        {
            yield return null;
        }
        OnIntroFinished();
    }

    public override void OnStageEnded()
    {
        base.OnStageEnded();

        if (m_waitForIntroRoutine != null)
            StopCoroutine(m_waitForIntroRoutine);
    }

    public void OnIntroFinished()
    {
        FPSPlayerController player = GameManager.Instance.Player;
        if (player)
        {
            player.m_isInputEnabled = true;
            player.m_gunController.gameObject.SetActive(true);
            player.m_gunController.SetGunUp(true);
        }
        m_waitForIntroRoutine = null;
    }
}
