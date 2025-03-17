using UnityEngine;

public class LevelUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_ResetLevelButton;

    void Awake()
    {
        m_ResetLevelButton
            .GetComponentInChildren<PressableButton>()
            .ButtonPressed.AddListener(OnResetButtonPressed);
    }

    private void OnResetButtonPressed()
    {
        LevelManager.Instance.ResetLevel();
    }
}
