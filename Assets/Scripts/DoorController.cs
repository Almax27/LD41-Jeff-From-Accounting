using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public enum DoorState
{
    None,
    Opening,
    Open,
    Closing,
    Closed
}

[RequireComponent(typeof(PlayableDirector))]
public class DoorController : MonoBehaviour
{
    public bool m_openOnAwake = false;
    public PlayableAsset m_openPlayable = null;
    public PlayableAsset m_closePlayable = null;

    protected DoorState m_state = DoorState.None;
    protected DoorState m_pendingState = DoorState.None;
    protected PlayableDirector m_director = null;

    private void Start()
    {
        m_director = GetComponent<PlayableDirector>();
        Debug.Assert(m_director);

        if (m_openOnAwake)
        {
            m_director.playableAsset = m_openPlayable;
            m_state = DoorState.Open;
        }
        else
        {
            m_director.playableAsset = m_closePlayable;
            m_state = DoorState.Closed;
        }
        m_director.time = m_director.playableAsset.duration;
        m_director.Evaluate();
    }

    [ContextMenu("OpenDoor")]
    public void OpenDoor()
    {
        if (m_state == DoorState.Closed)
        {
            m_state = DoorState.Opening;
            m_director.time = 0;
            m_director.Play(m_openPlayable);
        }
        else if(m_state == DoorState.Closing)
        {
            m_pendingState = DoorState.Opening;
        }
    }

    [ContextMenu("CloseDoor")]
    public void CloseDoor()
    {
        if (m_state == DoorState.Open)
        {
            m_state = DoorState.Closing;
            m_director.time = 0;
            m_director.Play(m_closePlayable);
        }
        else if (m_state == DoorState.Open)
        {
            m_pendingState = DoorState.Closing;
        }
    }

    protected void HandleDoorOpened()
    {
        m_state = DoorState.Open;
        if(m_pendingState == DoorState.Closing)
        {
            m_pendingState = DoorState.None;
            CloseDoor();
        }
    }

    protected void HandleDoorClosed()
    {
        m_state = DoorState.Closed;
        if (m_pendingState == DoorState.Opening)
        {
            m_pendingState = DoorState.None;
            OpenDoor();
        }
    }
}