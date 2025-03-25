using UnityEngine;

// Cannon should be the one to set the gravity variables for the ball. do not touch
public class CannonGravity : MonoBehaviour
{
    [HideInInspector]
    public float GravityScale = 0;

    private Rigidbody m_Rigidbody;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        LevelManager.Instance.RegisterCannonBall(this);

        // ParticleSystem ps = GetComponent<ParticleSystem>();

        // ParticleSystem.TrailModule ts = ps.trails;
        // ts.minVertexDistance = 0.01f;

    }

    void FixedUpdate()
    {
        m_Rigidbody.AddForce(new Vector3(0, GravityScale, 0), ForceMode.Acceleration);
    }
}
