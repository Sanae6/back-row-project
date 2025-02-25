using UnityEngine;

public class DominoCurve : MonoBehaviour
{
    void Start()
    {
        DominoManager.Instance.RegisterCurve(this);
    }
}
