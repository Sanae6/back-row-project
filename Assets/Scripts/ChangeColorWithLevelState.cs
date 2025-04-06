using UnityEngine;

public class ChangeColorWithLevelState : MonoBehaviour
{
    private Color m_NormalColor;

    [SerializeField] private Color m_CompletedColor;

    [SerializeField] private Color m_InvalidColor;

    [SerializeField] private Light m_DirectionalLight;

    void Awake()
    {
        m_NormalColor = m_DirectionalLight.color;
    }

    void Start()
    {
        LevelSystem.Instance.LevelStarted.AddListener(level => level.LevelStateUpdated.AddListener(LevelStateUpdated));
    }

    void LevelStateUpdated(LevelState state)
    {
        switch (state)
        {
            case LevelState.Invalid:
                m_DirectionalLight.color = m_InvalidColor;
                break;
            case LevelState.Valid:
                m_DirectionalLight.color = m_NormalColor;
                break;
            case LevelState.Completed:
                m_DirectionalLight.color = m_CompletedColor;
                break;
        }
    }
}