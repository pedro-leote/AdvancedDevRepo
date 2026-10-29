using UnityEditor;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(MeshRenderer))]
public class GreyboxTextureTiling : MonoBehaviour
{
    Material _greyboxMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Renderer objectRenderer = GetComponent<Renderer>();
        _greyboxMaterial = objectRenderer.sharedMaterial;
        Material adjustedMatInstance = new Material(_greyboxMaterial);

        Vector3 initialScale = transform.localScale;
        SetCorrectTiling(initialScale);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetCorrectTiling(Vector3 intendedScale)
    {
        Vector2 correctScale = new Vector2(intendedScale.x, intendedScale.z);
        _greyboxMaterial.mainTextureScale = correctScale;
    }

    #if UNITY_EDITOR
    [MenuItem("Tools/Correct Greybox Tilings")]
    private static void CorrectGreyboxTilings()
    {
        if (Application.isPlaying)
            return;

        GreyboxTextureTiling[] tilings =
            FindObjectsByType<GreyboxTextureTiling>(FindObjectsSortMode.None);

        foreach (GreyboxTextureTiling tiling in tilings)
            tiling.SetCorrectTiling(tiling.transform.localScale);
    }

    [MenuItem("Tools/Correct Greybox Tilings", true)]
    private static bool CanCorrectGreyboxTilings()
    {
        return !Application.isPlaying;
    }
#endif
}

