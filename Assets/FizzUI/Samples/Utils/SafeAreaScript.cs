using UnityEngine;

public class SafeAreaScript : MonoBehaviour
{
    private Rect safeRect = new Rect(0, 0, Screen.width, Screen.height);
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();

        UpdateSafeAreaRect();
    }

    void Start()
    {
        ApplySafeArea();
    }

    void UpdateSafeAreaRect()
    {
#if UNITY_2017_2_OR_NEWER
        safeRect = Screen.safeArea;
#endif
    }

    void ApplySafeArea()
    {
        var anchorMin = safeRect.position;
        var anchorMax = safeRect.position + safeRect.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}
