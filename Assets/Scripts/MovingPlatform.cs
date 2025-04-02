using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private Vector3 movementDirection = Vector3.up; // Normalized ideally

    [SerializeField]
    private float movementAmount = 1f; // Amplitude (up and down)

    [SerializeField]
    private float offset = 0f; // Phase offset for the sine function

    [SerializeField]
    private float speed = 1f;

    [SerializeField]
    private Vector3 m_PlatformPreviewScale;

    [SerializeField]
    private GameObject m_PlatformContainer;

    [SerializeField]
    private Rigidbody m_PlatformRigidbody;

    void Start()
    {
        LevelManager.Instance.LevelStateUpdated.AddListener(OnLevelStateUpdated);
    }

    private bool m_ShouldMove = false;

    float m_MoveTime = 0;

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_ShouldMove = true;
        }

        if (!m_ShouldMove)
            return;

        Vector3 movement =
            Mathf.Sin((offset + m_MoveTime) * speed)
            * movementAmount
            * movementDirection.normalized;

        m_PlatformContainer.transform.position = transform.position + movement;

        m_MoveTime += Time.fixedDeltaTime;
    }

    void OnLevelStateUpdated(LevelState state)
    {
        if (state == LevelState.Valid) // platform is reset when transitioning to valid state
        {
            m_PlatformContainer.transform.position =
                transform.position
                + movementDirection.normalized * movementAmount * Mathf.Sin(offset * speed);

            m_ShouldMove = false;
            m_MoveTime = 0;
        }
        else if (state == LevelState.Toppling || state == LevelState.Completed)
        {
            m_ShouldMove = true;
        }
    }

#if UNITY_EDITOR
    private double initialEditorTime = -1;
#endif

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

#if UNITY_EDITOR
        if (initialEditorTime < 0)
            initialEditorTime = EditorApplication.timeSinceStartup;

        float elapsed = (float)(EditorApplication.timeSinceStartup - initialEditorTime);
#else
        float elapsed = Time.time;
#endif

        if (!Application.isPlaying)
            m_PlatformContainer.transform.position =
                transform.position + Mathf.Sin(offset) * movementAmount * movementDirection;

        Vector3 posMax = transform.position + movementDirection.normalized * movementAmount;
        Vector3 posMin = transform.position - movementDirection.normalized * movementAmount;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(posMax, m_PlatformPreviewScale);
        Gizmos.DrawWireCube(posMin, m_PlatformPreviewScale);

        Vector3 offsetPos =
            transform.position
            + movementDirection.normalized * movementAmount * Mathf.Sin((offset + elapsed) * speed);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(offsetPos, m_PlatformPreviewScale);
        Gizmos.DrawLine(posMin, posMax);
    }
}
