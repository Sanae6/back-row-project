using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerLineDraw : MonoBehaviour
{
    [SerializeField]
    private GameObject m_DominoPrefab;

    [SerializeField]
    private float m_PointDistanceThreshold;

    [SerializeField]
    private float m_DominoDistance;

    [SerializeField]
    private float m_ColorGradientStep;

    [SerializeField]
    private Transform m_RightHandTransform;



    [SerializeField]
    private LineRenderer m_RayLineRenderer;

    [SerializeField]
    private LineRenderer m_DominoPreviewLineRenderer;

    void Start()
    {
        m_RayLineRenderer.positionCount = 2;
        m_RayLineRenderer.SetPositions(new Vector3[0]);
    }

    private bool m_TriggerPressedLastFrame = false;
    private List<Vector3> m_CurrentLinePositions = new();
    void Update() // todo: hella cleanup
    {
        SetDominoPreviewLineRendererPoints();

        Vector3? currentPoint = null;

        if (Physics.Raycast(m_RightHandTransform.position, m_RightHandTransform.forward, out RaycastHit hitInfo ,10.0f))
        {
            SetRayCastLineRendererPoints(hitInfo.point);
            currentPoint = hitInfo.point;
        }
        else 
        {
            ClearLineRendererPoints();
        }

        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            m_TriggerPressedLastFrame = true;

            if (currentPoint != null)
                TryAddPoint((Vector3)currentPoint);

        } 
        else if (m_TriggerPressedLastFrame)
        {
            m_TriggerPressedLastFrame = false;
            ResetDominoPreview();

            List<Vector3> curvePoints = GenerateCurveFromPoints();
            StartCoroutine(SpawnDominosAlongCurve(curvePoints));
            m_CurrentLinePositions.Clear();
        }
    }

    void TryAddPoint(Vector3 point)
    {
        if (m_CurrentLinePositions.Count == 0)
        {
            m_CurrentLinePositions.Add(point);
        }
        else
        {
            Vector3 lastPosition = m_CurrentLinePositions[m_CurrentLinePositions.Count - 1];
            float dist = Vector3.Distance(lastPosition, point);

            if (dist > m_PointDistanceThreshold)
            {
                m_CurrentLinePositions.Add(point);
            }
        }
    }

    private float m_CurrentColorHue = 0.0f;

    IEnumerator SpawnDominosAlongCurve(List<Vector3> curvePoints)
    {
        if (curvePoints == null || curvePoints.Count < 2)
            yield break;

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

                Instantiate(m_DominoPrefab, spawnPos, rotation).GetComponentInChildren<Domino>().hue =
                    m_CurrentColorHue;
                m_CurrentColorHue += m_ColorGradientStep;
                m_CurrentColorHue %= 1;

                targetDistance += m_DominoDistance;
                yield return new WaitForSeconds(0.01f); // adjust as needed
            }
            accumulatedDistance += segment;
            i++;
        }
    }

    List<Vector3> GenerateCurveFromPoints()
    {
        List<Vector3> curvePoints = new();

        if (m_CurrentLinePositions.Count < 4)
        {
            Debug.LogWarning("Unable to generate curve: Too few points");
            return curvePoints;
        }

        // along curve, we want our ideal distance to be m_DominoDistance

        for (int i = 1; i < m_CurrentLinePositions.Count - 2; i++)
        {
            Vector3 p0 = m_CurrentLinePositions[i - 1];
            Vector3 p1 = m_CurrentLinePositions[i];
            Vector3 p2 = m_CurrentLinePositions[i + 1];
            Vector3 p3 = m_CurrentLinePositions[i + 2];
            float calculatedDomDist = Vector3.Distance(p1, p2) / m_DominoDistance;

            // Sample points along the spline and add them to the list
            for (float t = 0; t < Vector3.Distance(p1, p2); t += calculatedDomDist) // 0.1f can be adjusted for smoothness
            {
                Vector3 splinePoint = GetCatmullRomPosition(t, p0, p1, p2, p3);
                curvePoints.Add(splinePoint);
            }
        }
        return curvePoints;
    }

    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Catmull-Rom spline formula
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 result =
            0.5f
            * (
                (2f * p1)
                + (-p0 + p2) * t
                + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
                + (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        return result;
    }

    void SetRayCastLineRendererPoints(Vector3 pointHit)
    {
        m_RayLineRenderer.positionCount = 2;
        m_RayLineRenderer.SetPositions(new Vector3[]{ m_RightHandTransform.position, pointHit });
    }

    void SetDominoPreviewLineRendererPoints()
    {
        m_DominoPreviewLineRenderer.positionCount = m_CurrentLinePositions.Count;
        m_DominoPreviewLineRenderer.SetPositions(m_CurrentLinePositions.ToArray());
    }

    void ClearLineRendererPoints()
    {
        m_RayLineRenderer.positionCount = 0;
        m_RayLineRenderer.SetPositions(new Vector3[]{});
    }

    void ResetDominoPreview()
    {
        m_DominoPreviewLineRenderer.positionCount = 0;
        m_DominoPreviewLineRenderer.SetPositions(new Vector3[0]);
    }
}
