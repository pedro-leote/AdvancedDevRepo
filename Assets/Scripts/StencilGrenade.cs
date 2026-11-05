using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StencilGrenade : MonoBehaviour
{

    [SerializeField] private float _stencilRadius = 3.75f;
    [SerializeField] private float _stencilDuration = 8f;
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
        HashSet<EnemyBehavior> affectedEnemies = new HashSet<EnemyBehavior>();
        Physics.OverlapSphere(transform.position, _stencilRadius, LayerMask.GetMask("Enemy")).ToList().ForEach(enemy =>
        {
            Debug.Log($"Enemy {enemy.name} is within stencil radius, checking for obstacles.");
            Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            Physics.Raycast(transform.position, directionToEnemy, out RaycastHit hit, distanceToEnemy, LayerMask.GetMask("Default"));
            if(hit.collider != null && hit.collider.gameObject != enemy)
            {
                Debug.Log($"Enemy is blocked by an obstacle, not applying stencil effect. {hit.collider.gameObject.name}");
                return;
            }

            EnemyBehavior enemyBehavior = enemy.GetComponentInParent<EnemyBehavior>();
            if (enemyBehavior == null)
            {
                Debug.LogWarning($"Collider {enemy.name} is on the Enemy layer but has no EnemyBehavior in its parent hierarchy.", enemy);
                return;
            }

            if (affectedEnemies.Add(enemyBehavior))
            {
                Debug.Log($"Enemy {enemyBehavior.name} is within stencil radius, applying stencil effect.");
                enemyBehavior.GetHitByStencilGrenade(_stencilDuration);
            }
        });
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stencilRadius);
    }
}
