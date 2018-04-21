using UnityEngine;

[System.Serializable]
public class SceneProperty
{
    [SerializeField]
    private Object m_SceneAsset;
    [SerializeField]
    private string m_SceneName = "";
    public string SceneName
    {
        get { return m_SceneName; }
    }
    // makes it work with the existing Unity methods (LoadLevel/LoadScene)
    public static implicit operator string(SceneProperty sceneField)
    {
        return sceneField.SceneName;
    }
}