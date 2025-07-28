using UnityEngine;
using UnityEngine.UI;

/**
 * Este script se encarga de generar dinámicamente una cuadrícula sobre el visor.
 */
public class GridOverlay : Graphic
{
    public float cellSize = 64f; // Tamaño de cada celda
    public Color lineColor = new Color(0.2f, 0.2f, 0.2f, 0.2f); // Color de la cuadrícula

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = rectTransform.rect;

        float scaleX = transform.lossyScale.x;
        float scaleY = transform.lossyScale.y;

        float cellWidth = cellSize * scaleX;
        float cellHeight = cellSize * scaleY;

        float left = rect.xMin;
        float right = rect.xMax;
        float top = rect.yMax;
        float bottom = rect.yMin;

        // Líneas verticales
        for (float x = left; x <= right; x += cellWidth)
            DibujarLinea(vh, new Vector2(x, bottom), new Vector2(x, top));

        // Líneas horizontales
        for (float y = bottom; y <= top; y += cellHeight)
            DibujarLinea(vh, new Vector2(left, y), new Vector2(right, y));
    }

    void DibujarLinea(VertexHelper vh, Vector2 start, Vector2 end)
    {
        float thickness = 1f;

        Vector2 perpendicular = Vector2.Perpendicular((end - start).normalized) * thickness;

        UIVertex[] verts = new UIVertex[4];

        verts[0].position = start - perpendicular;
        verts[1].position = start + perpendicular;
        verts[2].position = end + perpendicular;
        verts[3].position = end - perpendicular;

        for (int i = 0; i < 4; i++)
        {
            verts[i].color = lineColor;
        }

        int idx = vh.currentVertCount;
        vh.AddUIVertexQuad(verts);
    }
}
