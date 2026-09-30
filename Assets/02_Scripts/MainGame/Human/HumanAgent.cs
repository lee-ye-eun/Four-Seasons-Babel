using System;
using UnityEngine;

public enum AgentState { Walking, Waiting, UsingFacility, Despawned }

public class HumanAgent : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arrivalThreshold = 0.1f;
    [SerializeField] private int babelDamage = 1;

    public event Action OnReachedDestination;
    public event Action OnEnteredBuilding;

    private WaypointPath path;
    private AgentState currentState;
    private int targetWaypointIndex;

    private BuildSpot currentSpot;
    private bool arrivedAtSpot;

    public AgentState CurrentState => currentState;
    public bool IsInsideBuilding => currentState == AgentState.UsingFacility && arrivedAtSpot;
    public Vector2 MoveDirection { get; private set; }

    public void Initialize(WaypointPath waypointPath)
    {
        path = waypointPath;
        targetWaypointIndex = 0;
        currentState = AgentState.Walking;
    }

    private void Update()
    {
        switch (currentState)
        {
            case AgentState.Walking:       UpdateWalking();       break;
            case AgentState.UsingFacility: UpdateUsingFacility(); break;
        }
    }

    // ---- Walking ----

    private void UpdateWalking()
    {
        if (path == null || path.Count == 0) return;

        Transform target = path.GetWaypoint(targetWaypointIndex);
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= arrivalThreshold)
        {
            OnWaypointReached();
            return;
        }

        float stepDist = moveSpeed * Time.deltaTime;
        MoveDirection = direction.normalized;

        if (stepDist >= distance)
        {
            transform.position = target.position;
            OnWaypointReached();
            return;
        }

        transform.position += direction.normalized * stepDist;
    }

    private void OnWaypointReached()
    {
        if (path.IsLastWaypoint(targetWaypointIndex))
        {
            Debug.Log($"[HumanAgent] {gameObject.name} reached destination.");
            currentState = AgentState.Despawned;
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.TakeDamage(babelDamage);
            else
                Debug.LogWarning("[HumanAgent] GameStateManager.Instance is NULL");
            OnReachedDestination?.Invoke();
            Destroy(gameObject);
            return;
        }

        targetWaypointIndex++;
    }

    // ---- UsingFacility ----

    public void EnterFacility(BuildSpot spot)
    {
        currentSpot = spot;
        arrivedAtSpot = false;
        currentState = AgentState.UsingFacility;
    }

    // BuildSpot 타이머로 이관됨 — SimpleSpawner 리팩터 시 처리
    public void SkipUsage(float delay = 0f) { }

    private void UpdateUsingFacility()
    {
        if (arrivedAtSpot) return;

        Vector3 target = currentSpot.transform.position;
        Vector3 dir = target - transform.position;

        if (dir.magnitude <= arrivalThreshold)
        {
            transform.position = target;
            arrivedAtSpot = true;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            currentSpot.EnqueueForUsage(this);
            OnEnteredBuilding?.Invoke();
        }
        else
        {
            float stepDist = moveSpeed * Time.deltaTime;
            MoveDirection = dir.normalized;
            transform.position += dir.normalized * Mathf.Min(stepDist, dir.magnitude);
        }
    }

    public void CompleteUsage()
    {
        currentState = AgentState.Despawned;
        OnReachedDestination?.Invoke();
        Destroy(gameObject);
    }

    // ---- 미구현 골격 ----

    public void ResumeWalking()
    {
        // TODO: Waiting → Walking 복귀 시 정리 로직 추가
        currentState = AgentState.Walking;
    }
}
