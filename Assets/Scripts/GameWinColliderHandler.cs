using EasyPeasyFirstPersonController;
using UnityEngine;
using UnityEngine.Rendering;

public class GameWinColliderHandler : MonoBehaviour
{
    [SerializeField] private GameObject _gameWinCanvasObject;

    //Used to bring it to greyscale.
    [SerializeField] private Volume _debuffVolume;

    private bool _activateVolume = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!_activateVolume)
        {
            return;
        }

        if(Mathf.Approximately(_debuffVolume.weight, 1))
        {
            return;
        }
        else
        {
            _debuffVolume.weight += Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponentInChildren<FirstPersonController>() != null)
        {
            _gameWinCanvasObject.SetActive(true);
            _activateVolume = true;
        }
    }
}
