using UnityEngine;

// Keeps the arena walls on the real screen edges, using the same
// orthographicSize * aspect the Player's movement clamp uses, so bubbles and the
// Player stop at the same place at any aspect ratio (and nothing leaves the view).
// Refits when the camera's aspect, size or position changes (window resize,
// Maximize on Play).
public class ScreenBoundsFitter : MonoBehaviour
{
    public BoxCollider2D wallLeft;
    public BoxCollider2D wallRight;
    public BoxCollider2D wallTop;
    public BoxCollider2D wallFloor;

    private Camera cam;
    private float lastAspect = -1f;
    private float lastSize = -1f;
    private Vector3 lastCamPosition;

    private void Start()
    {
        cam = Camera.main;
        Fit();
    }

    private void LateUpdate()
    {
        if (cam == null) return;
        if (!Mathf.Approximately(cam.aspect, lastAspect) ||
            !Mathf.Approximately(cam.orthographicSize, lastSize) ||
            cam.transform.position != lastCamPosition)
            Fit();
    }

    private void Fit()
    {
        if (cam == null) return;

        lastAspect = cam.aspect;
        lastSize = cam.orthographicSize;
        lastCamPosition = cam.transform.position;

        float halfWidth = cam.orthographicSize * cam.aspect;
        float camX = cam.transform.position.x;

        // Side walls: inner edge exactly on the screen edge.
        PlaceX(wallLeft, camX - halfWidth - HalfWidth(wallLeft));
        PlaceX(wallRight, camX + halfWidth + HalfWidth(wallRight));

        // Top/floor: span the whole screen plus both side walls, so there are no
        // gaps in the corners.
        float span = 2f * halfWidth + 2f * Mathf.Max(Width(wallLeft), Width(wallRight));
        Stretch(wallTop, span, camX);
        Stretch(wallFloor, span, camX);
    }

    private static float Width(BoxCollider2D c) => c ? c.size.x * Mathf.Abs(c.transform.lossyScale.x) : 0f;
    private static float HalfWidth(BoxCollider2D c) => Width(c) / 2f;

    private static void PlaceX(BoxCollider2D c, float centreX)
    {
        if (!c) return;
        Vector3 p = c.transform.position;
        p.x = centreX - c.offset.x * c.transform.lossyScale.x;
        c.transform.position = p;
    }

    private static void Stretch(BoxCollider2D c, float worldWidth, float centreX)
    {
        if (!c) return;
        c.size = new Vector2(worldWidth / Mathf.Abs(c.transform.lossyScale.x), c.size.y);
        PlaceX(c, centreX);
    }
}
