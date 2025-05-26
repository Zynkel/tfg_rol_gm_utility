using UnityEngine;

public class UIUtils
{
    public static Vector2 LimitarPosicionDentroRectTransform(RectTransform rectTransform, Vector2 localPos, float margen = 0f)
    {
        Vector2 size = rectTransform.rect.size;
        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;

        localPos.x = Mathf.Clamp(localPos.x, -halfWidth + margen, halfWidth - margen);
        localPos.y = Mathf.Clamp(localPos.y, -halfHeight + margen, halfHeight - margen);

        return localPos;
    }
}
