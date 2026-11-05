using System.Collections;
using System.Collections.Generic;
using EasyPeasyFirstPersonController;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(RobotFreeAnim))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehavior : MonoBehaviour
{
    private RobotFreeAnim _enemyAnimator;
    private NavMeshAgent _navMeshComponent;

    public List<GameObject> _patrolPoints; 
    private int _patrolPointListIndex = -1;
    public Transform _currentPatrolObjective;
    private readonly List<Vector3> _patrolPointWorldPositions = new List<Vector3>();
    private Vector3 _currentPatrolObjectiveWorldPosition;

    [SerializeField, Min(4)] private float _initialWaitTime = 4f;
    [SerializeField] private float _meanWaitTime = 3.75f;
    private FirstPersonController _player;

    [SerializeField] private float _viewConeRadius = 6f;
    [SerializeField, Range(0f, 360f)] private float _viewConeAngle = 90f;
    [SerializeField] private float _playerDetectedMaxTimer = 2f;
    private float _playerDetectedTimer;
    public bool IsPlayerDetected { get; private set; }
    private Transform[] _stencilledTransforms;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _enemyAnimator = GetComponent<RobotFreeAnim>();
        _navMeshComponent = GetComponent<NavMeshAgent>();

        if (_patrolPoints != null)
        {
            foreach (GameObject patrolPoint in _patrolPoints)
            {
                _patrolPointWorldPositions.Add(patrolPoint != null ? patrolPoint.transform.position : Vector3.zero);
            }
        }

        _player = FindAnyObjectByType<FirstPersonController>();
        _navMeshComponent.isStopped = true;
        StartCoroutine(FirstWaitingState());
    }

    // Update is called once per frame
    void Update()
    {
        RunCheckForPlayerInRadius();
    }

    private bool PickNextPatrolPoint()
    {
        if (_patrolPoints == null || _patrolPoints.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < _patrolPoints.Count; i++)
        {
            _patrolPointListIndex = (_patrolPointListIndex + 1) % _patrolPoints.Count;
            GameObject patrolPoint = _patrolPoints[_patrolPointListIndex];
            if (patrolPoint != null)
            {
                _currentPatrolObjective = patrolPoint.transform;
                _currentPatrolObjectiveWorldPosition = _patrolPointWorldPositions[_patrolPointListIndex];
                return true;
            }
        }

        _currentPatrolObjective = null;
        return false;
    }


    private IEnumerator WaitingState()
    {
        float waitingTime = Random.Range(_meanWaitTime - 0.5f, _meanWaitTime + 1.2f);
        _enemyAnimator.anim.SetBool("Walk_Anim", false);
        yield return new WaitForSeconds(waitingTime);
        PickNextPatrolPoint();
        yield return WalkingState();
    }

    private IEnumerator FirstWaitingState()
    {
        yield return new WaitForSeconds(_initialWaitTime);
        PickNextPatrolPoint();
        yield return WalkingState();
    }

    private IEnumerator WalkingState()
    {
        if (!_navMeshComponent.isOnNavMesh)
        {
            yield break;
        }

        _navMeshComponent.isStopped = false;
        _enemyAnimator.anim.SetBool("Walk_Anim", true);
        if (!_navMeshComponent.SetDestination(_currentPatrolObjectiveWorldPosition))
        {
            _navMeshComponent.isStopped = true;
            Debug.LogWarning("The NavMeshAgent could not set a patrol destination.", this);
            yield break;
        }

        while (_navMeshComponent.pathPending)
        {
            yield return null;
        }

        if (_navMeshComponent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            _navMeshComponent.isStopped = true;
            Debug.LogWarning("The NavMeshAgent could not find a complete path to the patrol point.", this);
            yield break;
        }

        while (_navMeshComponent.hasPath && _navMeshComponent.remainingDistance > _navMeshComponent.stoppingDistance)
        {
            yield return null;
        }

        _navMeshComponent.isStopped = true;
       
        StartCoroutine(WaitingState());
    }

    public void GetHitByStencilGrenade(float duration)
    {
        CancelInvoke(nameof(ResetLayer));
        if (_stencilledTransforms == null)
        {
            _stencilledTransforms = GetComponentsInChildren<Transform>(true);
        }

        foreach (Transform item in _stencilledTransforms)
        {
            if (item != null)
            {
                item.gameObject.layer = LayerMask.NameToLayer("EnemyStencil");
            }
        }

        Invoke(nameof(ResetLayer), duration);
    }

    private void ResetLayer()
    {
        if (_stencilledTransforms == null)
        {
            return;
        }

        for (int i = 0; i < _stencilledTransforms.Length; i++)
        {
            if (_stencilledTransforms[i] != null)
            {
                _stencilledTransforms[i].gameObject.layer = LayerMask.NameToLayer("Enemy");
            }
        }

        _stencilledTransforms = null;
    }

    private void RunCheckForPlayerInRadius()
    {
        if (_player == null)
        {
            return;
        }

        Vector3 toPlayer = _player.transform.position - transform.position;
        Vector3 horizontalToPlayer = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
        Vector3 horizontalForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        bool isInRadius = toPlayer.sqrMagnitude <= _viewConeRadius * _viewConeRadius;
        bool isInViewAngle = horizontalToPlayer.sqrMagnitude == 0f ||
            Vector3.Angle(horizontalForward, horizontalToPlayer) <= _viewConeAngle * 0.5f;

        if (isInRadius && isInViewAngle)
        {
            _playerDetectedTimer += Time.deltaTime;
            if (!IsPlayerDetected && (_playerDetectedMaxTimer <= 0f || _playerDetectedTimer >= _playerDetectedMaxTimer))
            {
                IsPlayerDetected = true;
                Debug.Log("Player detected by enemy after time lapsed!", this);
            }
        }
        else
        {
            _playerDetectedTimer = 0f;
            IsPlayerDetected = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude == 0f)
        {
            return;
        }

        float radius = Mathf.Max(0f, _viewConeRadius);
        float halfAngle = Mathf.Clamp(_viewConeAngle, 0f, 360f) * 0.5f;
        Vector3 previousPoint = origin + Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward * radius;

        Gizmos.color = IsPlayerDetected ? Color.red : Color.yellow;
        Gizmos.DrawLine(origin, previousPoint);

        const int arcSegments = 30;
        for (int i = 1; i <= arcSegments; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)arcSegments);
            Vector3 point = origin + Quaternion.AngleAxis(angle, Vector3.up) * forward * radius;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        Vector3 endPoint = origin + Quaternion.AngleAxis(halfAngle, Vector3.up) * forward * radius;
        Gizmos.DrawLine(origin, endPoint);
    }

}
