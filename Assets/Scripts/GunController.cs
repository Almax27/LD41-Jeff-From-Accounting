using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq.Expressions;
using System;

[System.Serializable]
public class GunKeyBinding
{
    public Transform transform;
    public KeyCode keyCode;

    [System.NonSerialized]
    public float originalZ;

    [System.NonSerialized]
    public float tVal;
}

public enum GunState
{
    Down,
    Raising,
    Up,
    Lowering
}

public class GunController : MonoBehaviour {

    public List<char> Ammo = new List<char>("PUNCTUATOR".ToCharArray());
    public ProjectileController ProjectileControllerPrefab = null;
    public Transform muzzleTranform = null;
    public Animator gunControllerAnimator = null;
    public Animator gunAnimator = null;
    public GunAudioController gunAudioController = null;

    [Header("Keys")]
    public List<GunKeyBinding> gunKeys = new List<GunKeyBinding>();
    public float KeyAnimMoveZ = 0.012f;
    public float KeyDownAnimDuration = 0.1f;
    public float KeyUpAnimDuration = 0.2f;
    public float DeleteRepeatDelay = 0.5f;
    public float DeleteRepeatRate = 0.3f;

    [Header("AmmoDisplay")]
    public List<TextMeshPro> ammoDisplayPanels = new List<TextMeshPro>();
    public char emptyAmmoChar = ' ';

    RectTransform m_crosshair = null;
    bool m_isReloading;
    GunState m_gunState = GunState.Down;
    float m_timeToNextDelete = 0;
    Health m_targetUnderReticule = null;

    public bool GetIsReloading() { return m_isReloading; }
    public bool GetIsGunUp() { return m_gunState == GunState.Up; }
    public int GetMaxAmmoCount() { return ammoDisplayPanels.Count; }

    public void SetGunUp(bool gunUp)
    {
        if(gunUp && m_gunState != GunState.Up)
        {
            gunControllerAnimator.SetBool("IsGunUp", true);
            m_gunState = GunState.Raising;
        }
        else if(!gunUp && m_gunState != GunState.Down)
        {
            gunControllerAnimator.SetBool("IsGunUp", false);
            m_gunState = GunState.Lowering;
        }
    }

    public void OnGunUp()
    {
        m_gunState = GunState.Up;
        FPSPlayerController player = GameManager.Instance.Player;
        if (player && player.m_fpsHUD)
        {
            player.m_fpsHUD.SetCrosshairVisible(true);
        }
    }

    public void OnGunDown()
    {
        m_gunState = GunState.Down;
        FPSPlayerController player = GameManager.Instance.Player;
        if (player && player.m_fpsHUD)
        {
            player.m_fpsHUD.SetCrosshairVisible(false);
        }
    }

    public void TryFire()
    {
        if(CanFire())
        {
            if (Ammo.Count > 0)
            {
                OnFire();
            }
            else
            {
                OnDryFire();
            }
        }
    }

    bool CanFire()
    {
        return !m_isReloading && GetIsGunUp();
    }

    bool CanReload()
    {
        return !m_isReloading && GetIsGunUp();
    }

    void OnFire()
    {
        if(Ammo.Count > 0)
        {
            char letterToFire = Ammo[0];
            Ammo.RemoveAt(0);
            UpdateAmmoDisplay();
            SpawnProjectile(letterToFire);

            if (gunAnimator)
            {
                if (Ammo.Count == 0)
                {
                    gunAnimator.SetTrigger("OnFireLast");
                }
                else
                {
                    gunAnimator.SetTrigger("OnFire");
                }
            }

            if (gunAudioController)
            {
                gunAudioController.OnFire();
            }
        }
    }

    void OnDryFire()
    {
        if (gunAudioController)
        {
            gunAudioController.OnDryFire();
        }
        FPSPlayerController player = GameManager.Instance.Player;
        if (player && player.m_fpsHUD)
        {
            player.m_fpsHUD.OnOutOfAmmo();
        }
    }

    ProjectileController SpawnProjectile(char letter)
    {
        ProjectileController projectile = null;
        if (ProjectileControllerPrefab && muzzleTranform)
        {
            GameObject gobj = Instantiate<GameObject>(ProjectileControllerPrefab.gameObject);
            gobj.transform.position = muzzleTranform.position;
            projectile = gobj.GetComponent<ProjectileController>();
            projectile.letter = letter;
            projectile.direction = muzzleTranform.forward;

            bool usedRayCast = false;
            if (m_crosshair)
            {
                Ray ray = Camera.main.ScreenPointToRay(m_crosshair.position);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, projectile.maxDistance))
                {
                    Vector3 fireDir = (hitInfo.point - muzzleTranform.position).normalized;
                    if (Vector3.Angle(fireDir, ray.direction) < 90.0f)
                    {
                        projectile.direction = fireDir;
                        Debug.DrawLine(muzzleTranform.position, hitInfo.point, Color.green, 1.0f);
                    }
                    else
                    {
                        //force projectile hit immediately with camera cast as it's behind the muzzle
                        projectile.OnHit(hitInfo);
                        Debug.DrawLine(muzzleTranform.position, hitInfo.point, Color.red, 1.0f);
                    }
                    usedRayCast = true;
                }
            }
            if (!usedRayCast)
            {
                Debug.DrawLine(muzzleTranform.position, muzzleTranform.position + projectile.direction * projectile.maxDistance, Color.grey, 1.0f);
            }
        }
        return projectile;
    }

	// Use this for initialization
	void Start () {

        FPSPlayerController player = GameManager.Instance.Player;
        if (player && player.m_fpsHUD)
        {
            m_crosshair = player.m_fpsHUD.m_crosshair;
        }

        GenerateGunKeyBindings();
        FindAmmoText();
        UpdateAmmoDisplay();

        if(gunControllerAnimator == null)
        {
            gunControllerAnimator = GetComponent<Animator>();
        }

        OnGunDown();
    }

    private void LateUpdate()
    {
        this.transform.rotation = Camera.main.transform.rotation;

        if (Input.GetMouseButtonDown(0))
        {
            TryFire();
        }

        //Update keys before reload input to make sure we don't capture "r" on the same frame
        UpdateGunKeys();

        if (m_isReloading)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(Ammo.Count == 0)
                {
                    SetReloadState(false);
                }
                else
                {
                    RemoveAllAmmo();
                }
            }
            else if(Input.GetKeyDown(KeyCode.Return))
            {
                SetReloadState(false);
            }
            else if (Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Delete))
            {
                bool pressedThisFrame = Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete);
                if(pressedThisFrame)
                {
                    RemoveAmmo();
                    m_timeToNextDelete = DeleteRepeatDelay;
                }
                else
                {
                    m_timeToNextDelete -= Time.deltaTime;
                    if(m_timeToNextDelete <= 0)
                    {
                        RemoveAmmo();
                        m_timeToNextDelete = DeleteRepeatRate;
                    }
                }
            }
        }
        else //not reloading
        {
            if(Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return))
            {
                if (CanReload())
                {
                    SetReloadState(true);
                }
            }
            /*
            else if (Input.GetKeyDown(KeyCode.E))
            {
                bool IsUpOrRaising = m_gunState == GunState.Up || m_gunState == GunState.Raising;
                SetGunUp(!IsUpOrRaising);
            }
            */
        }

        if (Camera.main && ProjectileControllerPrefab)
        {
            if (m_crosshair)
            {
                Ray ray = Camera.main.ScreenPointToRay(m_crosshair.position);
                RaycastHit damageHitInfo;
                Health health = null;
                if (Physics.SphereCast(ray, ProjectileControllerPrefab.damageRadius, out damageHitInfo, ProjectileControllerPrefab.maxDistance, ProjectileControllerPrefab.damageMask))
                {
                    //make sure no geometry is in the way
                    float distanceToTarget = Vector3.Distance(ray.origin, damageHitInfo.transform.position);
                    if(!Physics.Raycast(ray, distanceToTarget, ProjectileControllerPrefab.hitMask))
                    {
                        health = damageHitInfo.collider.GetComponentInParent<Health>();
                    }
                }
                if(health != m_targetUnderReticule)
                {
                    if (m_targetUnderReticule) m_targetUnderReticule.SetIsTextVisible(false);
                    m_targetUnderReticule = health;
                }
                if(m_targetUnderReticule)
                {
                    m_targetUnderReticule.SetIsTextVisible(true);
                }
            }
        }
    }

    void SetReloadState(bool isReloading)
    {
        if (isReloading != m_isReloading)
        {
            m_isReloading = isReloading;
            if (gunAnimator)
            {
                gunAnimator.SetBool("IsReloading", isReloading);
            }
            if (gunControllerAnimator)
            {
                gunControllerAnimator.SetBool("IsReloading", isReloading);
            }
            if (gunAudioController)
            {
                if (isReloading)
                {
                    gunAudioController.OnReloadStart();
                }
                else
                {
                    gunAudioController.OnReloadEnd();
                }
            }
            FPSPlayerController player = GameManager.Instance.Player;
            if(player && player.m_fpsHUD)
            {
                if (isReloading)
                {
                    player.m_fpsHUD.OnReloadStarted();
                }
            }
        }
    }

    void GenerateGunKeyBindings()
    {
        List<Transform> children = new List<Transform>(gameObject.GetComponentsInChildren<Transform>());
        children.RemoveAll(t => !t.name.StartsWith("Key_"));

        KeyCode alphaStart = KeyCode.A;
        for (KeyCode alphaKey = alphaStart; alphaKey < alphaStart + 26; alphaKey++)
        {
            Transform keyTransform = children.Find(t => t.name.StartsWith("Key_" + alphaKey));
            if (keyTransform)
            {
                GunKeyBinding newBinding = new GunKeyBinding();
                newBinding.keyCode = alphaKey;
                newBinding.transform = keyTransform;
                newBinding.originalZ = keyTransform.localPosition.z;
                gunKeys.Add(newBinding);
                Debug.Log("Found key: Key_" + alphaKey);
            }
        }
        UpdateGunKeys(true);
    }

    void UpdateGunKeys(bool forceUpdate = false)
    {
        if(!m_isReloading && !forceUpdate)
        {
            return;
        }
        foreach(GunKeyBinding binding in gunKeys)
        {
            if(m_isReloading && Input.GetKeyDown(binding.keyCode))
            {
                AddAmmo(binding.keyCode);
            }
            bool isKeyHeld = Input.GetKey(binding.keyCode);
            if(forceUpdate || (isKeyHeld && binding.tVal < 1))
            {
                if (KeyDownAnimDuration > 0)
                {
                    binding.tVal = Mathf.Clamp01(binding.tVal + Time.deltaTime / KeyDownAnimDuration);
                }
                else binding.tVal = 1;
                UpdateKeyBindingTransform(binding);
            }
            else if (forceUpdate || (!isKeyHeld && binding.tVal > 0))
            {
                if (KeyDownAnimDuration > 0)
                {
                    binding.tVal = Mathf.Clamp01(binding.tVal - Time.deltaTime / KeyUpAnimDuration);
                }
                else binding.tVal = 0;
                UpdateKeyBindingTransform(binding);
            }
        }
    }

    void UpdateKeyBindingTransform(GunKeyBinding binding)
    {
        if(binding.transform)
        {
            Vector3 pos = binding.transform.localPosition;
            pos.z = binding.originalZ + KeyAnimMoveZ * binding.tVal;
            binding.transform.localPosition = pos;
        }
    }

    void AddAmmo(KeyCode keyCode)
    {
        if (Ammo.Count < GetMaxAmmoCount())
        {
            char upperChar = keyCode.ToString().ToUpper()[0];
            Ammo.Add(upperChar);
            OnAmmoChanged();
        }
    }

    void RemoveAmmo()
    {
        if(Ammo.Count > 0)
        {
            Ammo.RemoveAt(Ammo.Count - 1);
            OnAmmoChanged();
        }
    }

    void RemoveAllAmmo()
    {
        if (Ammo.Count > 0)
        {
            Ammo.Clear();
            OnAmmoChanged();
        }
    }

    void OnAmmoChanged()
    {
        UpdateAmmoDisplay();
        if (gunAudioController)
        {
            gunAudioController.OnReloadKeyPress();
        }
    }

    void FindAmmoText()
    {
        string prefix = "AmmoText_";
        List<Transform> AmmoTextTransforms = new List<Transform>();
        foreach(Transform child in gameObject.GetComponentsInChildren<Transform>())
        {
            string name = child.name;
            if(name.StartsWith(prefix))
            {
                AmmoTextTransforms.Add(child);
            }
        }
        AmmoTextTransforms.Sort((t1, t2) => int.Parse(t1.name.Remove(0, prefix.Length)) < int.Parse(t2.name.Remove(0, prefix.Length)) ? -1 : 1);

        foreach(Transform t in AmmoTextTransforms)
        {
            TextMeshPro textMeshPro = t.GetComponent<TextMeshPro>();
            Debug.Assert(textMeshPro != null);
            if(textMeshPro)
            {
                ammoDisplayPanels.Add(textMeshPro);
            }
        }
    }

    void UpdateAmmoDisplay()
    {
        for(int i = 0; i < ammoDisplayPanels.Count; i++)
        {
            if(i < Ammo.Count)
            {
                ammoDisplayPanels[i].SetText(Ammo[i].ToString().ToUpper());
            }
            else
            {
                ammoDisplayPanels[i].SetText(emptyAmmoChar.ToString().ToUpper());
            }
        }
    }
}
