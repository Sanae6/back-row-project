using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TriggerEvent
{
    Automatic,
    SinglePlaced,
    CurvePlaced,
    DominoErased,
    DominoToppled,
    GoldDominoToppled,
    InvalidTopple,
    ResetButtonPressed,
    RestartButtonPressed,
    ActivationPadPressed,
}

public class TutorialPanel : MonoBehaviour
{
    [SerializeField]
    private List<TutorialData> m_TextInstances = new();

    [SerializeField]
    private float m_DelayBetween;

    [SerializeField]
    private RectTransform m_ChildCanvas;

    private GameObject m_WholeChild;

    [SerializeField]
    private TextMeshProUGUI m_TextMesh;

    [SerializeField]
    private PressableButton m_ResetButton;

    [SerializeField]
    private PressableButton m_RestartButton;

    [SerializeField]
    private ActivationPad m_ActivationPad;

    [NonSerialized]
    private int m_CurrentStep = 0;

    void Awake()
    {
        if (m_TextInstances.Count == 0)
        {
            Debug.LogError("Tutorial panel exists but no text instances assigned...");
        }
    }

    private float targetWidth = 1.0f;

    [SerializeField]
    private float animationTime = 1.0f;

    void Start()
    {
        m_TextMesh.color = Color.clear;
        m_ChildCanvas.sizeDelta = new Vector2(0, m_ChildCanvas.sizeDelta.y);
        m_WholeChild = transform.GetChild(0).gameObject;
        m_WholeChild.SetActive(false);

        WaitForNextStartTrigger();
    }

    [ContextMenu("Reset")]
    void Reset()
    {
        m_CurrentStep = 0;

        m_TextMesh.color = Color.clear;
        m_ChildCanvas.sizeDelta = new Vector2(0, m_ChildCanvas.sizeDelta.y);
        m_WholeChild.SetActive(false);

        ClearListeners();

        WaitForNextStartTrigger();
    }

    void WaitForNextStartTrigger()
    {
        if (m_CurrentStep >= m_TextInstances.Count)
            return;

        CalculateTargetWidthForText(m_TextInstances[m_CurrentStep].Text);
        SetText(m_TextInstances[m_CurrentStep].Text);

        StartExpansionListener(m_TextInstances[m_CurrentStep].StartTrigger);
    }

    void StartExpand()
    {
        StartCoroutine(Expand());
        ClearListeners();
    }

    void StartExpandIfToppling(LevelState state)
    {
        if (state == LevelState.Toppling)
        {
            StartExpand();
        }
    }

    void StartExpandIfGoldToppled(LevelState state)
    {
        if (state == LevelState.Completed)
        {
            StartExpand();
        }
    }

    void StartExpandIfInvalidTopple(LevelState state)
    {
        if (state == LevelState.Invalid)
        {
            StartExpand();
        }
    }

    void CalculateTargetWidthForText(string text)
    {
        float width = m_TextMesh.GetPreferredValues(text).x;
        width = Mathf.Min(width, m_TextMesh.GetComponent<RectTransform>().rect.width);
        targetWidth = width + 0.1f;
    }

    void SetText(string text)
    {
        m_TextMesh.text = text;
    }

    IEnumerator Expand()
    {
        yield return new WaitForSeconds(m_DelayBetween);
        float elapsedTime = 0f;
        m_WholeChild.SetActive(true);

        float startWidth = 0;
        Color startColor = Color.clear;
        Color endColor = Color.white;

        while (elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationTime;

            // Lerp width
            float newWidth = Mathf.Lerp(startWidth, targetWidth, t);
            m_ChildCanvas.sizeDelta = new Vector2(newWidth, m_ChildCanvas.sizeDelta.y);

            // Lerp text alpha
            m_TextMesh.color = Color.Lerp(startColor, endColor, Mathf.Pow(t, 4.0f)); // Exp to have the transparency kick in later
            yield return null;
        }

        // Ensure final values are set
        m_ChildCanvas.sizeDelta = new Vector2(targetWidth, m_ChildCanvas.sizeDelta.y);
        m_TextMesh.color = endColor;

        StartShrinkListener();
    }

    [ContextMenu("HideAndNext")]
    IEnumerator HideAndNext()
    {
        yield return new WaitForSeconds(m_DelayBetween);
        m_CurrentStep++;

        float elapsedTime = 0f;

        float startWidth = targetWidth;
        Color startColor = Color.white;
        Color endColor = Color.clear;

        while (elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationTime;

            // Lerp width
            float newWidth = Mathf.Lerp(startWidth, 0, t);
            m_ChildCanvas.sizeDelta = new Vector2(newWidth, m_ChildCanvas.sizeDelta.y);

            // Lerp text alpha
            m_TextMesh.color = Color.Lerp(startColor, endColor, t - Mathf.Pow(1 - t, 4.0f));
            yield return null;
        }

        m_WholeChild.SetActive(false);
        // Ensure final values are set
        m_ChildCanvas.sizeDelta = new Vector2(targetWidth, m_ChildCanvas.sizeDelta.y);
        m_TextMesh.color = endColor;

        WaitForNextStartTrigger();
    }

    void StartExpansionListener(TriggerEvent e)
    {
        switch (e)
        {
            case TriggerEvent.Automatic:
            {
                StartExpand();
                break;
            }
            case TriggerEvent.SinglePlaced:
            {
                DominoManager.Instance.SingleDominoPlaced.AddListener(StartExpand);
                break;
            }
            case TriggerEvent.CurvePlaced:
            {
                DominoManager.Instance.DominoCurvePlaced.AddListener(StartExpand);
                break;
            }
            case TriggerEvent.DominoErased:
            {
                DominoManager.Instance.DominoErased.AddListener(StartExpand);
                break;
            }
            case TriggerEvent.DominoToppled:
            {
                LevelManager.Instance.LevelStateUpdated.AddListener(StartExpandIfToppling);
                break;
            }
            case TriggerEvent.GoldDominoToppled:
            {
                LevelManager.Instance.LevelStateUpdated.AddListener(StartExpandIfGoldToppled);
                break;
            }
            case TriggerEvent.InvalidTopple:
            {
                LevelManager.Instance.LevelStateUpdated.AddListener(StartExpandIfInvalidTopple);
                break;
            }
            case TriggerEvent.ResetButtonPressed:
            {
                m_ResetButton.ButtonPressed.AddListener(StartExpand);
                break;
            }
            case TriggerEvent.RestartButtonPressed:
            {
                m_RestartButton.ButtonPressed.AddListener(StartExpand);
                break;
            }
            case TriggerEvent.ActivationPadPressed:
            {
                m_ActivationPad.PadActivated.AddListener(StartExpand);
                break;
            }
        }
    }

    void StartShrinkListener()
    {
        switch (m_TextInstances[m_CurrentStep].EndTrigger)
        {
            case TriggerEvent.Automatic:
            {
                StartShrink();
                break;
            }
            case TriggerEvent.SinglePlaced:
            {
                DominoManager.Instance.SingleDominoPlaced.AddListener(StartShrink);
                break;
            }
            case TriggerEvent.CurvePlaced:
            {
                DominoManager.Instance.DominoCurvePlaced.AddListener(StartShrink);
                break;
            }
            case TriggerEvent.DominoErased:
            {
                DominoManager.Instance.DominoErased.AddListener(StartShrink);
                break;
            }
            case TriggerEvent.DominoToppled:
            {
                LevelManager.Instance.LevelStateUpdated.AddListener(StartShrinkIfToppling);
                break;
            }
            case TriggerEvent.GoldDominoToppled:
            {
                LevelManager.Instance.LevelStateUpdated.AddListener(StartShrinkIfGoldToppled);
                break;
            }
            case TriggerEvent.InvalidTopple:
            {
                LevelManager.Instance.LevelStateUpdated.AddListener(StartShrinkIfInvalidTopple);
                break;
            }
            case TriggerEvent.ResetButtonPressed:
            {
                m_ResetButton.ButtonPressed.AddListener(StartShrink);
                break;
            }
            case TriggerEvent.RestartButtonPressed:
            {
                m_RestartButton.ButtonPressed.AddListener(StartShrink);
                break;
            }
            case TriggerEvent.ActivationPadPressed:
            {
                m_ActivationPad.PadActivated.AddListener(StartShrink);
                break;
            }
        }
    }



    void StartShrink()
    {
        StartCoroutine(HideAndNext());
        ClearListeners();
    }

    void StartShrinkIfToppling(LevelState state)
    {
        if (state == LevelState.Toppling)
        {
            StartShrink();
        }
    }

    void StartShrinkIfGoldToppled(LevelState state)
    {
        if (state == LevelState.Completed)
        {
            StartShrink();
        }
    }

    void StartShrinkIfInvalidTopple(LevelState state)
    {
        if (state == LevelState.Invalid)
        {
            StartShrink();
        }
    }

    void ClearListeners()
    {
        DominoManager.Instance.DominoErased.RemoveListener(StartShrink);
        DominoManager.Instance.DominoCurvePlaced.RemoveListener(StartShrink);
        DominoManager.Instance.SingleDominoPlaced.RemoveListener(StartShrink);
        LevelManager.Instance.LevelStateUpdated.RemoveListener(StartShrinkIfToppling);
        LevelManager.Instance.LevelStateUpdated.RemoveListener(StartShrinkIfGoldToppled);
        LevelManager.Instance.LevelStateUpdated.RemoveListener(StartShrinkIfInvalidTopple);
        if (m_RestartButton)
            m_RestartButton.ButtonPressed.RemoveListener(StartShrink);
        if (m_ActivationPad)
            m_ActivationPad.PadActivated.RemoveListener(StartShrink);
        if (m_ResetButton)
            m_ResetButton.ButtonPressed.RemoveListener(StartShrink);

        DominoManager.Instance.DominoErased.RemoveListener(StartExpand);
        DominoManager.Instance.DominoCurvePlaced.RemoveListener(StartExpand);
        DominoManager.Instance.SingleDominoPlaced.RemoveListener(StartExpand);
        LevelManager.Instance.LevelStateUpdated.RemoveListener(StartExpandIfToppling);
        LevelManager.Instance.LevelStateUpdated.RemoveListener(StartExpandIfGoldToppled);
        LevelManager.Instance.LevelStateUpdated.RemoveListener(StartExpandIfInvalidTopple);
        if (m_RestartButton)
            m_RestartButton.ButtonPressed.RemoveListener(StartExpand);
        if (m_ActivationPad)
            m_ActivationPad.PadActivated.RemoveListener(StartExpand);
        if (m_ResetButton)
            m_ResetButton.ButtonPressed.RemoveListener(StartExpand);
    }
}
