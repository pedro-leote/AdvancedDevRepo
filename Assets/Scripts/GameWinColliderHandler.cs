using EasyPeasyFirstPersonController;
using UnityEngine;

public class GameWinColliderHandler : MonoBehaviour
{
    [SerializeField] private GameObject _gameWinCanvasObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponentInChildren<FirstPersonController>() != null)
        {
            _gameWinCanvasObject.SetActive(true);
        }
    }
}
