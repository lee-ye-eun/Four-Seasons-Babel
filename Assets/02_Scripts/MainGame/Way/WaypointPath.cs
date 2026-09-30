using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    private Transform[] waypoints;

    private void Awake()
    {
        CollectChildren();
    }

    private void CollectChildren()
    {
        waypoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            waypoints[i] = transform.GetChild(i);
    }

    public int Count => waypoints.Length;

    public Vector3 GetSpawnPoint()
    {
        return waypoints[0].position;
    }

    public Transform GetWaypoint(int index)
    {
        return waypoints[index];
    }

    public bool IsLastWaypoint(int index)
    {
        return index >= waypoints.Length - 1;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CollectChildren();
    }

    private void OnDrawGizmos()
    {
        CollectChildren();
        if (waypoints == null || waypoints.Length == 0) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.color = (i == 0) ? Color.green
                         : (i == waypoints.Length - 1) ? Color.red
                         : Color.yellow;

            Gizmos.DrawSphere(waypoints[i].position, 0.25f);
            UnityEditor.Handles.Label(waypoints[i].position + Vector3.up * 0.4f, i.ToString());

            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
#endif
}
