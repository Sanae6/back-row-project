using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DominoManager : MonoBehaviour
{
    [HideInInspector]
    public static DominoManager Instance;

    [SerializeField]
    private GameObject m_DominoPrefab;

    [SerializeField]
    private float m_DominoDistance;

    [SerializeField]
    private float m_ColorGradientStep;

    [Space(5)]
    [SerializeField]
    private GameObject m_ClearDominosButton;

    private List<DominoCurve> curves = new();
    private List<GameObject> singles = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        m_ClearDominosButton
            .GetComponentInChildren<PressableButton>()
            .ButtonPressed.AddListener(OnClearDominosButtonPressed);
    }

    public void SpawnCurve(List<Vector3> curve)
    {
        StartCoroutine(SpawnDominosAlongCurve(curve));
    }


    IEnumerator SpawnDominosAlongCurve(List<Vector3> curvePoints)
    {
        if (curvePoints == null || curvePoints.Count < 2)
            yield break;

        GameObject curveObj = new GameObject("DominoCurve");
        curveObj.AddComponent<DominoCurve>();

        float accumulatedDistance = 0f;
        float targetDistance = m_DominoDistance;
        int i = 1;
        while (i < curvePoints.Count)
        {
            float segment = Vector3.Distance(curvePoints[i - 1], curvePoints[i]);
            while (accumulatedDistance + segment >= targetDistance)
            {
                float t = (targetDistance - accumulatedDistance) / segment;
                Vector3 spawnPos = Vector3.Lerp(curvePoints[i - 1], curvePoints[i], t);

                // use the segment's direction as an approximation for the tangent

                Vector3 tangent;

                if (i == 0)
                    tangent = (curvePoints[1] - curvePoints[0]).normalized;
                else if (i == curvePoints.Count - 1)
                    tangent = (curvePoints[i] - curvePoints[i - 1]).normalized;
                else
                    tangent = (curvePoints[i + 1] - curvePoints[i - 1]).normalized;

                Quaternion rotation = Quaternion.LookRotation(tangent);

                Instantiate(m_DominoPrefab, spawnPos, rotation, curveObj.transform);

                targetDistance += m_DominoDistance;
                yield return new WaitForSeconds(0.01f); // adjust as needed
            }
            accumulatedDistance += segment;
            i++;
        }
    }

    public void RegisterCurve(DominoCurve curve)
    {
        curves.Add(curve);
    }

    private void OnClearDominosButtonPressed()
    {
        ClearDominos();
    }

    public void ClearDominos()
    {
        Debug.Log($"Domino manager clearing {curves.Count} curves and {singles.Count} singles.");

        for (int i = 0; i < curves.Count; i++)
        {
            Destroy(curves[i].gameObject);
        }

        for (int i = 0; i < singles.Count; i++)
        {
            Destroy(singles[i]);
        }

        curves.Clear();
        singles.Clear();
    }

    public void InstantiateSingle(Vector3 pos, Quaternion rot)
    {
        GameObject dom = Instantiate(m_DominoPrefab, pos, rot);
        singles.Add(dom);
    }

    private float m_CurrentColorHue = 0;

    public float GetNextHue()
    {
        m_CurrentColorHue += m_ColorGradientStep;
        m_CurrentColorHue %= 1;
        return m_CurrentColorHue;
    }
}
