using UnityEngine;

public class ChangeColorWithLevelState : MonoBehaviour
{
    [SerializeField] private Color m_NormalColor;

    [SerializeField] private Color m_CompletedColor;

    [SerializeField] private Color m_InvalidColor;

    private Renderer m_Renderer;

    void Awake()
    {
        m_Renderer = GetComponent<Renderer>();
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
                m_Renderer.material.color = m_InvalidColor;
                break;
            case LevelState.Valid:
                m_Renderer.material.color = m_NormalColor;
                break;
            case LevelState.Completed:
                m_Renderer.material.color = m_CompletedColor;
                break;
        }
    }
}