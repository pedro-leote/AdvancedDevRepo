using System.Collections.Generic;
using System.Linq;
using EasyPeasyFirstPersonController;
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
            Debug.Log("Destroying grenade after timer");
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
        Collider[] hits = Physics.OverlapSphere(transform.position, _stencilRadius, LayerMask.GetMask("Enemy"));
        if(hits.Length == 0)
        {
            GameObject player = FindAnyObjectByType<FirstPersonController>().gameObject;
            player.GetComponent<FirstPersonController>().DisableMovement();
            Destroy(gameObject);
            return;
        }

        hits.ToList().ForEach(enemy =>
        {

            Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            Physics.Raycast(transform.position, directionToEnemy, out RaycastHit hit, distanceToEnemy, LayerMask.GetMask("Default"));
            if(hit.collider != null && hit.collider.gameObject != enemy)
            {
                GameObject player = FindAnyObjectByType<FirstPersonController>().gameObject;
                player.GetComponent<FirstPersonController>().DisableMovement();
                return;
            }

            EnemyBehavior enemyBehavior = enemy.GetComponentInParent<EnemyBehavior>();
            if (enemyBehavior == null)
            {
                GameObject player = FindAnyObjectByType<FirstPersonController>().gameObject;
                player.GetComponent<FirstPersonController>().DisableMovement();
                return;
            }

            if (affectedEnemies.Add(enemyBehavior))
            {
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
