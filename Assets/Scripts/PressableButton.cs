using System;
using UnityEngine;
using UnityEngine.Events;

public class PressableButton : MonoBehaviour
{
    [SerializeField] private float m_PushActivationThreshold = 0.04f;

    private ConfigurableJoint m_ConfigurableJoint;

    private Vector3 m_OriginalPosition;

    [HideInInspector] public UnityEvent ButtonPressed;

    private void Awake()
    {
        OnEnable();
    }

    void OnEnable()
    {
        m_OriginalPosition = transform.localPosition;

        m_ConfigurableJoint = GetComponent<ConfigurableJoint>();
        if (m_ConfigurableJoint == null)
            Debug.LogError("Pressable button instantiated without configurable joint.");
    }

    void Update()
    {
        float dist = Vector3.Distance(m_OriginalPosition, transform.localPosition);

        if (dist > m_PushActivationThreshold)
        {
            ButtonPressed.Invoke();
        }

        if (transform.localPosition.y > m_OriginalPosition.y)
        {
            transform.localPosition = m_OriginalPosition;
        }
    }
}