using EasyPeasyFirstPersonController;
using UnityEngine;
using UnityEngine.Rendering;

public class GameWinColliderHandler : MonoBehaviour
{
    [SerializeField] private GameObject _gameWinCanvasObject;
    private DebuffVolumeHandler _debuffVolume;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _debuffVolume = FindAnyObjectByType<DebuffVolumeHandler>();
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
            _debuffVolume.ActivateDebuffVisuals();
        }
    }
}
