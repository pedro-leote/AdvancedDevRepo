using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class GrenadeThrowing : MonoBehaviour
{
    [SerializeField] private InputActionReference _throwGrenadeReference;
    [SerializeField] private InputActionReference _predictTrajectoryReference;


    [SerializeField] private GameObject _grenadePrefab;
    [SerializeField] private Transform _throwingPoint;
    [SerializeField] private float _throwForce = 10f;
    [SerializeField] private float _throwCooldown = 3.5f;

    void OnEnable()
    {
        _throwGrenadeReference.action.performed += ThrowGrenade;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ThrowGrenade(CallbackContext context)
    {
        //TEMP
        GameObject grenade = Instantiate(_grenadePrefab, _throwingPoint.position, Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(_throwingPoint.forward * _throwForce, ForceMode.Impulse);
    }

    void OnDisable()
    {
        _throwGrenadeReference.action.performed -= ThrowGrenade;
    }
}
