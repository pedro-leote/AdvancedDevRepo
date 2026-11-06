using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class DebuffVolumeHandler : MonoBehaviour
{
    private Volume _debuffVolume;
    private bool _doIncrease = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _debuffVolume = GetComponent<Volume>();
    }

    // Update is called once per frame
    void Update()
    {
        float targetWeight = _doIncrease ? 1f : 0f;
        _debuffVolume.weight = Mathf.MoveTowards(_debuffVolume.weight, targetWeight, Time.deltaTime * 3f);
        _debuffVolume.weight = Mathf.Clamp01(_debuffVolume.weight);
    }

    public void ActivateDebuffVisuals()
    {
        _doIncrease = true;
    }

    public void DeactivateDebuffVisuals()
    {
        _doIncrease = false;
    }
}
