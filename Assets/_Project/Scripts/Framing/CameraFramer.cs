using System.Collections;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Fixed tilted camera (~55° down). Frames the yard by width (~93% of the screen) unless it is
    /// too tall for the space between the HUD bars; "breathes" very slightly so the scene feels alive.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFramer : MonoBehaviour
    {
        public float pitch = 52f;
        /// <summary>Turn round the vertical axis: the yard is seen at an angle so the city round it shows (owner 2026-10-31). 0 = straight.</summary>
        public float yaw = 0f; // owner 2026-11-02: straight on, not diagonal
        public float fieldOfView = 30f;
        public float sideMargin = 0.03f; // the framed area (yard + district padding) fills ~94% of the width
        /// <summary>
        /// Small yards are framed as if they were this big (cells), so the camera never comes too close and the district
        /// round the yard stays in view (owner 2026-10-30: "kamera alana çok yakın").
        /// </summary>
        public float minFrameWidth = 7.4f, minFrameDepth = 8.2f;
        /// <summary>Cells of the district kept in view on each side of the yard (pavement, road, the first buildings).</summary>
        public float padSides = 0f, padDepth = 0.6f; // owner 2026-11-02: the yard reaches the screen edges
        /// <summary>
        /// Smallest cell width as a share of the screen width (the early levels' scale, 31 px on a 390 px phone): a bigger yard is shown at this scale, partly, and the
        /// view glides along with the player (owner 2026-10-31:
        /// "uzakta kalan bölümler ilk bölümler gibi yakından olsun").
        /// </summary>
        public float minCellWidth = 0.075f; // owner 2026-11-02: a little closer; bigger yards follow the worker
        /// <summary>The view follows the player (a yard shown bigger than the screen); never with minCellWidth 0.</summary>
        public bool Follows => follow != Vector2.zero;

        /// <summary>The player (set by the board); the view follows it on yards too big for the screen.</summary>
        public Transform Follow { get; set; }
        public float breathDegrees = 0.3f;
        public float breathPeriod = 7f;

        private Camera cam;
        private Bounds board;
        private Bounds[] extras = new Bounds[0];
        private float zoom = 1f;
        private float distance;
        private Vector2Int lastScreen;
        private float topInset = 0.16f, bottomInset = 0.2f;
        private float shakeUntil;
        private float shakeAmount;

        /// <summary>Camera distance to the board centre (for depth of field and shadow range).</summary>
        public float Distance => distance * zoom;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.fieldOfView = fieldOfView;
        }

        /// <summary>Width on screen, in pixels, of one cell at the middle of the framed yard (for the readability check, Ana Belge §3.1).</summary>
        public float CellPixels(float screenWidth)
        {
            bool easing = easeTime > 0f;
            var c = easing ? easeTo.center : board.center;
            transform.position = c - transform.forward * (easing ? easeTo.distance : distance);
            c.y = 0f;
            var a = cam.WorldToViewportPoint(c - Vector3.right * 0.5f);
            var b = cam.WorldToViewportPoint(c + Vector3.right * 0.5f);
            return (b.x - a.x) * screenWidth;
        }

        /// <summary>Screen fractions covered by the HUD at the top and bottom.</summary>
        public void SetInsets(float top, float bottom)
        {
            topInset = top;
            bottomInset = bottom;
            Apply();
        }

        public void Frame(int width, int height) => Frame(-0.5f, width - 0.5f, -0.5f, height - 0.5f);

        /// <summary>
        /// Frames the playable yard (world x / z edges), not the pavement ring round it (Ana Belge §2.5:
        /// the yard fills about 90% of the screen width; the district spills over the edges).
        /// </summary>
        public void Frame(float minX, float maxX, float minZ, float maxZ) => Frame(minX, maxX, minZ, maxZ, 0f);

        /// <param name="seconds">0 = at once; otherwise ease from the current framing (a growing yard, Ana Belge §5).</param>
        public void Frame(float minX, float maxX, float minZ, float maxZ, float seconds) => Frame(minX, maxX, minZ, maxZ, seconds, null);

        /// <param name="mustShow">Things outside the yard that must be fully on screen too (the cranes, Ana Belge §3.3: "vinçler tam görünmeli").</param>
        public void Frame(float minX, float maxX, float minZ, float maxZ, float seconds, Bounds[] mustShow)
        {
            if (cam == null) cam = GetComponent<Camera>();
            var fromBoard = board;
            float fromDistance = distance;
            extras = mustShow ?? new Bounds[0];
            // Centre on the yard and everything that must show (x / z only).
            float cx0 = minX, cx1 = maxX, cz0 = minZ, cz1 = maxZ;
            foreach (var e in extras)
            {
                cx0 = Mathf.Min(cx0, e.min.x); cx1 = Mathf.Max(cx1, e.max.x);
                cz0 = Mathf.Min(cz0, e.min.z); cz1 = Mathf.Max(cz1, e.max.z);
            }
            float w = Mathf.Max(maxX - minX + 2f * padSides, minFrameWidth), dz = Mathf.Max(maxZ - minZ + 2f * padDepth, minFrameDepth);
            // From a bit below the floor to the top of the barrier and the boxes.
            board = new Bounds(new Vector3((cx0 + cx1) / 2f, 0.3f, (cz0 + cz1) / 2f), new Vector3(w, 1.4f, dz));
            yard = new Bounds(new Vector3((minX + maxX) / 2f, 0.3f, (minZ + maxZ) / 2f), new Vector3(maxX - minX, 1.4f, maxZ - minZ));
            zoom = 1f;
            if (seconds <= 0f) { pan = Vector3.zero; easeTime = 0f; }
            Apply();
            if (seconds > 0f && fromBoard.size != Vector3.zero)
            {
                easeFrom = (fromBoard.center, fromDistance);
                easeTo = (board.center, distance);
                easeStart = Time.time;
                easeTime = seconds;
                board.center = fromBoard.center;
                distance = fromDistance;
            }
        }

        /// <summary>
        /// Changes the play scale (smallest cell width as a share of the screen; 0 = every yard whole) and eases there: owner
        /// 2026-11-02 "şekerler bitince işçiye iyice yakınlaşsın" (a big value follows the worker close up).
        /// </summary>
        public void SetCellWidth(float cellWidth, float seconds)
        {
            float fromDistance = distance;
            minCellWidth = cellWidth;
            easeTime = 0f;
            Apply();
            if (seconds <= 0f || board.size == Vector3.zero) return;
            easeFrom = (board.center, fromDistance);
            easeTo = (board.center, distance);
            easeStart = Time.time;
            easeTime = seconds;
            distance = fromDistance;
        }

        /// <summary>
        /// Owner 2026-11-02: combos, long chains and the paint gun lift the view to show the whole yard; it comes back when the
        /// moment ends (<see cref="Peek"/> false).
        /// </summary>
        public void Peek(bool on)
        {
            if (on && !peekWanted)
            {
                if (cam == null) cam = GetComponent<Camera>();
                var keep = transform.position;
                var safe = new Rect(sideMargin, bottomInset, 1f - 2f * sideMargin, 1f - topInset - bottomInset);
                wholeDistance = Search(d => Fits(d, safe));
                transform.position = keep;
            }
            peekWanted = on;
        }

        private bool peekWanted;
        private float peek, wholeDistance;

        private Bounds yard;
        private (Vector3 center, float distance) easeFrom, easeTo;
        private float easeStart, easeTime;

        /// <summary>Win shot: ease slightly closer.</summary>
        public IEnumerator ZoomIn(float factor, float duration)
        {
            float from = zoom;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                zoom = Mathf.Lerp(from, factor, Mathf.SmoothStep(0, 1, t / duration));
                yield return null;
            }
            zoom = factor;
        }

        /// <summary>Short jolt (heavy box).</summary>
        public void Shake(float duration = 0.05f, float amount = 0.04f)
        {
            shakeUntil = Time.time + duration;
            shakeAmount = amount;
        }

        private void LateUpdate()
        {
            if (lastScreen.x != Screen.width || lastScreen.y != Screen.height) Apply();
            if (board.size == Vector3.zero) return;
            if (easeTime > 0f)
            {
                float k = Mathf.Clamp01((Time.time - easeStart) / easeTime);
                k = Mathf.SmoothStep(0f, 1f, k);
                board.center = Vector3.Lerp(easeFrom.center, easeTo.center, k);
                distance = Mathf.Lerp(easeFrom.distance, easeTo.distance, k);
                if (k >= 1f) easeTime = 0f;
            }
            if (gliding)
            {
                pan = Vector3.Lerp(pan, panGoal, 1f - Mathf.Exp(-6f * Time.deltaTime));
                if ((pan - panGoal).sqrMagnitude < 1e-4f) { pan = panGoal; gliding = false; }
            }
            if (returning)
            {
                pan = Vector3.Lerp(pan, Vector3.zero, 1f - Mathf.Exp(-6f * Time.deltaTime));
                if (pan.sqrMagnitude < 1e-4f) { pan = Vector3.zero; returning = false; }
            }
            // Owner 2026-11-02: a level opens high over the whole district, looking down, and glides into the play view.
            float fly = 0f;
            if (flyTime > 0f)
            {
                float k = Mathf.Clamp01((Time.time - flyStart) / flyTime);
                fly = 1f - Mathf.SmoothStep(0f, 1f, k);
                if (k >= 1f) flyTime = 0f;
            }
            float phase = Time.time * Mathf.PI * 2f / breathPeriod;
            transform.rotation = Quaternion.Euler(Mathf.Lerp(pitch + Mathf.Sin(phase) * breathDegrees, FlyPitch, fly), yaw, 0f);
            peek = Mathf.MoveTowards(peek, peekWanted ? 1f : 0f, Time.deltaTime / 0.35f);
            float pk = Mathf.SmoothStep(0f, 1f, peek);
            if (pk > 0f) transform.rotation = Quaternion.Euler(Mathf.Lerp(transform.eulerAngles.x, pitch + 10f, pk), yaw, 0f);
            float far = Mathf.Lerp(distance * zoom, Mathf.Max(distance * zoom, wholeDistance * 1.05f), pk);
            var pos = board.center + Vector3.Lerp(pan + FollowOffset(), Vector3.zero, pk) - transform.forward * Mathf.Lerp(far, Mathf.Max(far * 3f, FlyDistance), fly);
            if (Time.time < shakeUntil) pos += Random.insideUnitSphere * shakeAmount;
            transform.position = pos;
        }

        private const float FlyPitch = 86f, FlyDistance = 120f;
        private float flyStart, flyTime;

        /// <summary>Starts the bird's-eye opening: high over the whole city, then down into the play view.</summary>
        public void FlyIn(float seconds)
        {
            flyStart = Time.time;
            flyTime = seconds;
            LateUpdate();
        }

        // ---------- looking round the city (owner 2026-10-31: drag outside the yard moves the camera, within limits) ----------

        /// <summary>How far (world units) the view may slide from its framing, on the ground.</summary>
        public float panLimitX = 3f, panLimitZ = 3f;
        private Vector3 pan;

        /// <summary>The ground point under a screen position, or null when the ray misses the floor.</summary>
        public Vector3? GroundAt(Vector2 screenPos)
        {
            if (cam == null) cam = GetComponent<Camera>();
            var ray = cam.ScreenPointToRay(screenPos);
            if (ray.direction.y > -1e-4f) return null;
            return ray.origin + ray.direction * (-ray.origin.y / ray.direction.y);
        }

        /// <summary>True when the screen position is over the playable yard (swipes there move the player).</summary>
        public bool IsOverYard(Vector2 screenPos)
        {
            var g = GroundAt(screenPos);
            if (!g.HasValue || yard.size == Vector3.zero) return false;
            var p = g.Value;
            return p.x >= yard.min.x && p.x <= yard.max.x && p.z >= yard.min.z && p.z <= yard.max.z;
        }

        /// <summary>Drags the view so the ground under <paramref name="from"/> ends under <paramref name="to"/>; clamped to the limits.</summary>
        public void Drag(Vector2 from, Vector2 to)
        {
            var a = GroundAt(from);
            var b = GroundAt(to);
            if (!a.HasValue || !b.HasValue) return;
            var p = pan + (a.Value - b.Value);
            // A big yard (followed) can be looked over whole: the limits grow by the follow range.
            float lx = panLimitX + follow.x, lz = panLimitZ + follow.y;
            gliding = false;
            pan = new Vector3(Mathf.Clamp(p.x + followAt.x, -lx, lx) - followAt.x, 0f, Mathf.Clamp(p.z + followAt.z, -lz, lz) - followAt.z);
            returning = false;
            LateUpdate();
        }

        /// <summary>
        /// Glides the view so the ground point comes to the middle (owner 2026-11-02: a tap on a candy crate at the edge of the
        /// screen brings that side into view); clamped to the same limits as a drag.
        /// </summary>
        public void LookAt(Vector3 ground)
        {
            var p = ground - board.center - followAt;
            float lx = panLimitX + follow.x, lz = panLimitZ + follow.y;
            panGoal = new Vector3(Mathf.Clamp(p.x + followAt.x, -lx, lx) - followAt.x, 0f, Mathf.Clamp(p.z + followAt.z, -lz, lz) - followAt.z);
            gliding = true;
            returning = false;
        }

        private Vector3 panGoal;
        private bool gliding;

        /// <summary>Back to the framed view.</summary>
        public void ResetPan() { pan = Vector3.zero; gliding = false; }

        /// <summary>Glide back to the player (after a move that followed a look round).</summary>
        public void ReturnPan() => returning = pan != Vector3.zero;

        private bool returning;

        private void Apply()
        {
            lastScreen = new Vector2Int(Screen.width, Screen.height);
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null || board.size == Vector3.zero) return;
            cam.fieldOfView = fieldOfView;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            var safe = new Rect(sideMargin, bottomInset, 1f - 2f * sideMargin, 1f - topInset - bottomInset);

            // Binary search for the closest distance where every board corner is inside the safe area. A big yard (no
            // must-show boxes) that would come out smaller than the early levels is shown at their scale instead: part
            // of it fills the screen and the view follows the player, never leaving the yard.
            float far = Search(d => Fits(d, safe));
            follow = Vector2.zero;
            float cell = CellAt(far);
            if (extras.Length == 0 && cell < minCellWidth)
            {
                float s = cell / minCellWidth;
                far = Search(d => CellAt(d) <= minCellWidth);
                follow = new Vector2(board.size.x, board.size.z) * ((1f - s) / 2f);
            }
            // While easing in, a new screen size or HUD inset only moves the end of the glide.
            if (easeTime > 0f) { easeTo = (easeTo.center, far); return; }
            distance = far;
            Place(distance);
        }

        // ---------- big yards: the view follows the player ----------

        private Vector2 follow;
        private Vector3 followAt;

        /// <summary>Ground shift of the view towards the player, clamped so the view stays over the yard; eased.</summary>
        private Vector3 FollowOffset()
        {
            if (follow == Vector2.zero || Follow == null) { followAt = Vector3.zero; return followAt; }
            var d = Follow.position - board.center;
            var want = new Vector3(Mathf.Clamp(d.x, -follow.x, follow.x), 0f, Mathf.Clamp(d.z, -follow.y, follow.y));
            followAt = Vector3.Lerp(followAt, want, 1f - Mathf.Exp(-4f * Time.deltaTime));
            return followAt;
        }

        /// <summary>The smallest distance (1–300) for which <paramref name="ok"/> holds; ok must stay true further out.</summary>
        private static float Search(System.Func<float, bool> ok)
        {
            float near = 1f, far = 300f;
            for (int i = 0; i < 40; i++)
            {
                float mid = (near + far) / 2f;
                if (ok(mid)) far = mid;
                else near = mid;
            }
            return far;
        }

        /// <summary>Cell width (share of the screen width) at the yard centre with the camera at distance <paramref name="d"/>.</summary>
        private float CellAt(float d)
        {
            var c = board.center;
            transform.position = c - transform.forward * d;
            c.y = 0f;
            return (cam.WorldToViewportPoint(c + Vector3.right * 0.5f).x - cam.WorldToViewportPoint(c - Vector3.right * 0.5f).x);
        }

        private void Place(float d) => transform.position = board.center - transform.forward * d;

        private bool Fits(float d, Rect safe)
        {
            Place(d);
            // The framed box round the yard (at least the minimum size, centred on the yard), then each must-show box.
            var frameBox = new Bounds(yard.size == Vector3.zero ? board.center : new Vector3(yard.center.x, board.center.y, yard.center.z), board.size);
            if (!Inside(frameBox, safe)) return false;
            foreach (var e in extras) if (!Inside(e, safe)) return false;
            return true;
        }

        private bool Inside(Bounds b, Rect safe)
        {
            var min = b.min;
            var max = b.max;
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
                var v = cam.WorldToViewportPoint(corner);
                if (v.z <= 0 || !safe.Contains(new Vector2(v.x, v.y))) return false;
            }
            return true;
        }
    }
}
