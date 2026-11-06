using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(LineRenderer))]
public class GrenadeThrowing : MonoBehaviour
{
    [SerializeField] private InputActionReference _throwGrenadeReference;
    [SerializeField] private InputActionReference _predictTrajectoryReference;


    [SerializeField] private GameObject _grenadePrefab;
    [SerializeField] private Transform _throwingPoint;
    [SerializeField] private float _throwForce = 10f;
    [SerializeField] private float _throwCooldown = 5f;
    private bool _canThrow = true;

    private float _timeStepForPrediction = 0.1f;
    private LineRenderer _lineRenderer;
    private Rigidbody _grenadeRigidbody;
    private SphereCollider _grenadeCollider;
    private int _defaultLayerMask;
    private bool _hasActivatedPreview = false;
    void OnEnable()
    {
        _throwGrenadeReference.action.performed += ThrowGrenade;
        _predictTrajectoryReference.action.started += StartPredictTrajectory;
        _predictTrajectoryReference.action.canceled += StopPredictTrajectory;
    }

    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _grenadeRigidbody = _grenadePrefab.GetComponent<Rigidbody>();
        _grenadeCollider = _grenadePrefab.GetComponent<SphereCollider>();
        _defaultLayerMask = LayerMask.GetMask("Default");
        _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        

        if (!_hasActivatedPreview)
        {
            return;
        }
        Vector3[] positionsArray = new Vector3[30];
        Vector3 position = _throwingPoint.position;
        Vector3 velocity = _throwingPoint.forward * (_throwForce / _grenadeRigidbody.mass);
        Vector3 scale = _grenadePrefab.transform.lossyScale;
        float radius = _grenadeCollider.radius * Mathf.Max(Mathf.Abs(scale.x),Mathf.Abs(scale.y),Mathf.Abs(scale.z));
        int pointCount = 1;
        positionsArray[0] = position;

        for (int i = 1; i < positionsArray.Length; i++)
        {
            velocity += Physics.gravity * _timeStepForPrediction;
            velocity *= Mathf.Exp(-_grenadeRigidbody.linearDamping * _timeStepForPrediction);
            Vector3 nextPosition = position + velocity * _timeStepForPrediction;
            Vector3 displacement = nextPosition - position;

            if (Physics.SphereCast(position, radius, displacement.normalized, out RaycastHit hit, displacement.magnitude, _defaultLayerMask, QueryTriggerInteraction.Ignore))
            {
                positionsArray[pointCount++] = position + displacement.normalized * hit.distance;
                break;
            }

            positionsArray[pointCount++] = nextPosition;
            position = nextPosition;
        }

        _lineRenderer.positionCount = pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            _lineRenderer.SetPosition(i, positionsArray[i]);
        }
        
    }

    public void ThrowGrenade(CallbackContext context)
    {
        if (!_canThrow)
        {
            return;
        }
        GameObject grenade = Instantiate(_grenadePrefab, _throwingPoint.position, Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(_throwingPoint.forward * _throwForce, ForceMode.Impulse);

        _canThrow = false;
        Invoke(nameof(ResetThrow), _throwCooldown);
    }

    private void ResetThrow()
    {
        _canThrow = true;
    }

    private void StartPredictTrajectory(CallbackContext context)
    {
        _lineRenderer.enabled = true;
        _hasActivatedPreview = true;
        _lineRenderer.positionCount = 0;
        
    }

    private void StopPredictTrajectory(CallbackContext context)
    {
        _lineRenderer.enabled = false;
        _hasActivatedPreview = false;
        _lineRenderer.positionCount = 0;
    }

    void OnDisable()
    {
        _throwGrenadeReference.action.performed -= ThrowGrenade;
        _predictTrajectoryReference.action.started -= StartPredictTrajectory;
        _predictTrajectoryReference.action.canceled -= StopPredictTrajectory;
        _hasActivatedPreview = false;
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
            _lineRenderer.positionCount = 0;
        }
    }
}
