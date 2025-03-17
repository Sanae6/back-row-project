using System;
using UnityEngine;
using UnityEngine.Events;

public class PressableButton : MonoBehaviour
{
    [SerializeField]
    private float m_PushActivationThreshold = 0.04f;

    private ConfigurableJoint m_ConfigurableJoint;

    private Vector3 m_OriginalPosition;

    [HideInInspector]
    public UnityEvent ButtonPressed;

    void Awake()
    {
        m_OriginalPosition = transform.position;

        m_ConfigurableJoint = GetComponent<ConfigurableJoint>();
        if (m_ConfigurableJoint == null)
            Debug.LogError("Pressable button instantiated without configurable joint.");
    }

    void Update()
    {
        float dist = Vector3.Distance(m_OriginalPosition, transform.position);
        if (dist > m_PushActivationThreshold)
        {
            ButtonPressed.Invoke();
        }
    }
}
