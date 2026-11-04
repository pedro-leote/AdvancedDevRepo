using UnityEngine;

public class StencilGrenade : MonoBehaviour
{
    [SerializeField] private float _countTimer = 0f;
    [SerializeField] private float _throwTimer = 3.5f;
    [SerializeField] private float _throwTimerAfterHittingGround = 1f;

    [SerializeField] private bool _hasHitSurface = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _countTimer += Time.deltaTime;
        if(_countTimer >= _throwTimer && !_hasHitSurface)
        {
            DestroyGrenade();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Default") && !_hasHitSurface)
        {
            _hasHitSurface = true;
            Invoke(nameof(DestroyGrenade), _throwTimerAfterHittingGround);
        }
    }

    private void DestroyGrenade()
    {
        Debug.Log("Destroying grenade");
        Destroy(gameObject);
    }
}
