using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField]
    private ActivationPad m_ActivationPad;

    [SerializeField]
    private GameObject m_CannonPrefab;

    [SerializeField]
    private Transform m_CannonLaunchTransform;

    [SerializeField]
    private float m_CannonVelocity = 1;

    [SerializeField]
    private float m_Gravity = -1.0f;

    void Awake()
    {
        if (m_ActivationPad == null)
        {
            Debug.LogError("Cannon with no activation pad...");
        }
    }

    void Start()
    {
        m_ActivationPad.PadActivated.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        GameObject go = Instantiate(m_CannonPrefab, m_CannonLaunchTransform.position, Quaternion.identity);
        go.GetComponent<Rigidbody>().linearVelocity = m_CannonLaunchTransform.forward * m_CannonVelocity;
        go.GetComponent<CannonGravity>().GravityScale = m_Gravity;
        go.GetComponent<Domino>().isValid = true;
    }


    private int steps = 1000;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 startPos = m_CannonLaunchTransform.position;
        Vector3 velocity = m_CannonLaunchTransform.forward * m_CannonVelocity;
        Vector3 prevPos = startPos;

        for (int i = 1; i <= steps; i++)
        {
            float t = i * 0.05f;
            Vector3 nextPos = startPos + velocity * t + 0.5f * t * t * new Vector3(0, m_Gravity, 0);
            Gizmos.DrawLine(prevPos, nextPos);
            prevPos = nextPos;
        }
    }
}
