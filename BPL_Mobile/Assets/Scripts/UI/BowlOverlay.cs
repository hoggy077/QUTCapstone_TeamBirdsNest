using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BowlOverlay : MonoBehaviour
{
    [HideInInspector] public static BowlOverlay instance;
    private float targetAlpha = 0f;
    private Image sr;
    private RectTransform rect;
    private Camera cam;

    [Header("Pullback Line")]
    public RectTransform distanceToOuterCheck;
    private float radius;
    public Gradient activeGradient;
    public Gradient inactiveGradient;
    public LineRenderer line;
    private Vector3 lineOffset1;
    private Vector3 lineOffset2;
    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        sr = GetComponent<Image>();
        sr.color = new Color(1f, 1f, 1f, targetAlpha);
        rect = GetComponent<RectTransform>();
        line.colorGradient = inactiveGradient;
        cam = Camera.main;
        radius = Vector3.Distance(GetCurrentScreenPosition(), cam.ScreenToWorldPoint(new Vector3(distanceToOuterCheck.anchoredPosition.x, distanceToOuterCheck.anchoredPosition.y, cam.nearClipPlane)));
    }

    private void Update()
    {
        sr.color = Color.Lerp(sr.color, new Color(sr.color.r, sr.color.g, sr.color.b, targetAlpha), 5f * Time.deltaTime);

        // Lerping Gradient
        Gradient newGrad = new Gradient();

        GradientAlphaKey[] newAlphaKeys = new GradientAlphaKey[activeGradient.alphaKeys.Length];
        GradientColorKey[] newColorKeys = new GradientColorKey[activeGradient.colorKeys.Length];

        for (int i = 0; i < newAlphaKeys.Length; i++)
        {
            float opacityAtPos = Mathf.Lerp(inactiveGradient.alphaKeys[i].alpha, activeGradient.alphaKeys[i].alpha, sr.color.a);
            float timeAtPos = Mathf.Lerp(inactiveGradient.alphaKeys[i].time, activeGradient.alphaKeys[i].time, sr.color.a);
            newAlphaKeys[i] = new GradientAlphaKey(opacityAtPos, timeAtPos);
        }

        for (int i = 0; i < newColorKeys.Length; i++)
        {
            Color colorAtPos = Color.Lerp(inactiveGradient.colorKeys[i].color, activeGradient.colorKeys[i].color, sr.color.a);
            float timeAtPos = Mathf.Lerp(inactiveGradient.colorKeys[i].time, activeGradient.colorKeys[i].time, sr.color.a);
            newColorKeys[i] = new GradientColorKey(colorAtPos, timeAtPos);
        }

        newGrad.alphaKeys = newAlphaKeys;
        newGrad.colorKeys = newColorKeys;

        line.colorGradient = newGrad;
    }

    public void MoveToBowl(Vector3 bowlLocation)
    {
        Vector3 newPos = cam.WorldToScreenPoint(bowlLocation);
        rect.anchoredPosition = new Vector2(newPos.x, newPos.y);
    }

    public void UpdateLinePullback(Vector2 fingerPosition)
    {
        Vector3 fingerPositionWorld = cam.ScreenToWorldPoint(new Vector3(fingerPosition.x, fingerPosition.y, cam.nearClipPlane));
        Vector3 currentWorldPosition = GetCurrentScreenPosition();

        Vector3 normalisedDirection = Vector3.Normalize(currentWorldPosition - fingerPositionWorld);

        // Moving Line Points to Position
        line.SetPosition(0, currentWorldPosition - normalisedDirection * radius/2f);
        line.SetPosition(1, fingerPositionWorld);
    }

    public void ToggleOpacity(bool on)
    {
        if(on)
        {
            targetAlpha = 1f;
        }
        else
        {
            targetAlpha = 0f;

            lineOffset1 = line.GetPosition(0) - GetCurrentScreenPosition();
            lineOffset2 = line.GetPosition(1) - GetCurrentScreenPosition();

            line.SetPosition(0, GetCurrentScreenPosition() + lineOffset1);
            line.SetPosition(1, GetCurrentScreenPosition() + lineOffset2);
        }
    }

    private Vector3 GetCurrentScreenPosition()
    {
        return cam.ScreenToWorldPoint(new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, cam.nearClipPlane));
    }
}
