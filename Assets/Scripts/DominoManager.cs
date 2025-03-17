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

    private float m_CurrentColorHue = 0;

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

                Instantiate(m_DominoPrefab, spawnPos, rotation, curveObj.transform)
                    .GetComponentInChildren<Domino>()
                    .hue = m_CurrentColorHue;
                m_CurrentColorHue += m_ColorGradientStep;
                m_CurrentColorHue %= 1;

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
        foreach (DominoCurve curve in curves)
        {
            Destroy(curve.gameObject);
        }
        curves.Clear();
    }
}
