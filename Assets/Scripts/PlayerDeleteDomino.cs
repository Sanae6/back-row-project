using UnityEngine;

public class PlayerDeleteDomino : MonoBehaviour
{
    private bool m_Deleting;


    [SerializeField]
    private Color m_DeleteColor = Color.red;
    private Color m_OriginalColor;

    private Renderer m_Renderer;

    void Awake()
    {
        m_Renderer = GetComponentInParent<Renderer>();
        m_OriginalColor = m_Renderer.material.color;
    }

    private bool m_DeletingLastFrame = false;

    void Update()
    {
        m_DeletingLastFrame = m_Deleting;

        if (
            OVRInput.Get(OVRInput.RawButton.RHandTrigger)
            || OVRInput.Get(OVRInput.RawButton.LHandTrigger)
        )
        {
            m_Deleting = true;
        }
        else
        {
            m_Deleting = false;
        }

        if (m_DeletingLastFrame != m_Deleting)
        {
            if (m_Deleting)
                m_Renderer.material.color = m_DeleteColor;
            else 
                m_Renderer.material.color = m_OriginalColor;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!m_Deleting)
            return;

        if (other.gameObject.TryGetComponent(out Domino dom))
        {
            DominoManager.Instance.DeleteSingle(dom);
        }
    }
}
