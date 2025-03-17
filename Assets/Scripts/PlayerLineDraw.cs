using System.Collections.Generic;
using UnityEngine;

// This script is responsible for
//  - Managing the line renderers for the controller raycast and the domino line preview
//  - Taking points on the surface that the player "draws" on
//  - Generating a curve out of the player-drawn points, which is sent to DominoManager
//      to instantiate the dominos

public class PlayerLineDraw : MonoBehaviour
{
    [SerializeField]
    private float m_PointDistanceThreshold;

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

    void Update()
    {
        SetDominoPreviewLineRendererPoints();

        Vector3? currentPoint = null;

        // Check if player is pointing at valid play area
        if (Physics.Raycast(m_RightHandTransform.position, m_RightHandTransform.forward, out RaycastHit hitInfo, 10.0f) 
            && hitInfo.collider.gameObject.CompareTag("PlayArea"))
        {
            SetRayCastLineRendererPoints(hitInfo.point);
            currentPoint = hitInfo.point;
        }
        else
        {
            ClearLineRendererPoints();
        }

        // Right hand trigger pressed
        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            m_TriggerPressedLastFrame = true;

            if (currentPoint != null)
                TryAddPoint((Vector3)currentPoint);
        }
        // Trigger Released
        else if (m_TriggerPressedLastFrame)
        {
            m_TriggerPressedLastFrame = false;
            ResetDominoPreview();

            List<Vector3> curvePoints = GenerateCurveFromPoints();
            DominoManager.Instance.SpawnCurve(curvePoints);
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

    List<Vector3> GenerateCurveFromPoints()
    {
        List<Vector3> curvePoints = new();

        if (m_CurrentLinePositions.Count < 4)
        {
            Debug.LogWarning("Unable to generate curve: Too few points");
            return curvePoints;
        }

        // the first and last points are manually added, since they don't have neighbours on both sides
        // to be used for curve interpolation
        curvePoints.Add(m_CurrentLinePositions[0]);

        for (int i = 1; i < m_CurrentLinePositions.Count - 2; i++)
        {
            Vector3 p0 = m_CurrentLinePositions[i - 1];
            Vector3 p1 = m_CurrentLinePositions[i];
            Vector3 p2 = m_CurrentLinePositions[i + 1];
            Vector3 p3 = m_CurrentLinePositions[i + 2];

            // Sample points along the spline and add them to the list
            for (float t = 0; t < Vector3.Distance(p1, p2); t += 0.001f)
            {
                Vector3 splinePoint = GetCatmullRomPosition(t, p0, p1, p2, p3);
                curvePoints.Add(splinePoint);
            }
        }

        curvePoints.Add(m_CurrentLinePositions[m_CurrentLinePositions.Count - 1]);

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
        m_RayLineRenderer.SetPositions(new Vector3[] { m_RightHandTransform.position, pointHit });
    }

    void SetDominoPreviewLineRendererPoints()
    {
        m_DominoPreviewLineRenderer.positionCount = m_CurrentLinePositions.Count;
        m_DominoPreviewLineRenderer.SetPositions(m_CurrentLinePositions.ToArray());
    }

    void ClearLineRendererPoints()
    {
        m_RayLineRenderer.positionCount = 0;
        m_RayLineRenderer.SetPositions(new Vector3[] { });
    }

    void ResetDominoPreview()
    {
        m_DominoPreviewLineRenderer.positionCount = 0;
        m_DominoPreviewLineRenderer.SetPositions(new Vector3[0]);
    }
}
