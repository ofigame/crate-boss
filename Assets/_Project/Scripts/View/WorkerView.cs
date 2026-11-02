using System.Collections;
using CrateBoss.Core;
using CrateBoss.UI;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// The Mixamo worker (Ana Belge §19): the game moves him cell by cell (root motion off) while the
    /// animator plays Walk / Push / PushHeavy; 0.15 s without a new move he settles back to Idle (the look-around loop). His face cannot change,
    /// so feelings come from the clips and small signs over his head (!, heart, ?).
    /// </summary>
    public class WorkerView : MonoBehaviour, IPlayerView
    {
        private const float Fade = 0.08f, TurnTime = 0.08f, IdleDelay = 0.15f;

        private Transform model;
        private Animator animator;
        private Transform head;
        private Transform emote;
        private Material emoteMat;
        private Coroutine emoteRoutine;
        private string state = "Idle";
        private float lastMove = -10f;
        private bool busy;
        private bool worried;
        private Vector3? lookTarget;

        /// <summary>True when the worker prefab exists (Tools > Crate Boss > Normalize Models).</summary>
        public static bool Available => VisualCatalog.Has("Worker");

        public void Build()
        {
            model = new GameObject("Model").transform;
            model.SetParent(transform, false);
            var go = VisualCatalog.TryInstantiate("Worker", model);
            animator = go != null ? go.GetComponentInChildren<Animator>() : null;
            rig = go != null ? go.transform : null;
            if (animator != null && animator.isHuman)
            {
                head = animator.GetBoneTransform(HumanBodyBones.Head);
                hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                feet = new[]
                {
                    animator.GetBoneTransform(HumanBodyBones.LeftToes) ?? animator.GetBoneTransform(HumanBodyBones.LeftFoot),
                    animator.GetBoneTransform(HumanBodyBones.RightToes) ?? animator.GetBoneTransform(HumanBodyBones.RightFoot),
                };
            }
            BuildEmote();
            OutfitDresser.DressWorker(rig, animator);
        }

        private Transform rig, hips;
        private Transform[] feet;
        /// <summary>Toe bones sit a little above the sole.</summary>
        private const float SoleBelowToes = 0.035f;

        // The clips carry their own body height and drift (Mixamo); after the animator, pull the body back over the
        // cell centre and put the lower foot on the floor, so the worker never floats or stands off his cell.
        private void Ground()
        {
            if (rig == null || hips == null || feet == null || feet[0] == null || feet[1] == null) return;
            float lowest = Mathf.Min(feet[0].position.y, feet[1].position.y) - SoleBelowToes * rig.lossyScale.y;
            var floor = transform.position.y + model.localPosition.y;
            var drift = hips.position - transform.position;
            rig.position += new Vector3(-drift.x, floor - lowest, -drift.z);
        }

        public bool Worried
        {
            get => worried;
            set
            {
                if (value && !worried) ShowEmote(IconKind.Exclaim, Palette.CraneWarning, 1.2f);
                worried = value;
            }
        }

        public void Face(Direction dir) => transform.rotation = Quaternion.Euler(0, Yaw(dir), 0);

        public void Watch(Vector3? worldTarget) => lookTarget = worldTarget;

        public void Cheer()
        {
            Play("Happy", true);
            ShowEmote(IconKind.Heart, new Color(1f, 0.36f, 0.45f), 1.1f);
        }

        public void Think()
        {
            Play("Think", true);
            ShowEmote(IconKind.Question, new Color(0.45f, 0.75f, 1f), 1.4f);
        }

        // ---------- moves ----------

        public IEnumerator Walk(Vector3 target, Direction dir, float duration)
        {
            Moved();
            Play("Walk");
            yield return Slide(target, dir, duration);
        }

        public IEnumerator Push(Vector3 target, Direction dir, float duration, bool heavy)
        {
            Moved();
            Play(heavy ? "PushHeavy" : "Push");
            yield return Slide(target, dir, duration);
        }

        /// <summary>Blocked move: a short bounce back (code only, 0.12 s) and a "!".</summary>
        public IEnumerator Bump(Direction dir, float duration = 0.15f)
        {
            Moved();
            Face(dir);
            ShowEmote(IconKind.Exclaim, Color.white, 0.5f);
            const float time = 0.12f;
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                float k = t / time;
                model.localPosition = new Vector3(0f, 0f, Mathf.Sin(k * Mathf.PI) * (k < 0.4f ? 0.06f : -0.05f));
                yield return null;
            }
            model.localPosition = Vector3.zero;
        }

        /// <summary>Win: turn to the camera and play the victory loop.</summary>
        public IEnumerator Celebrate()
        {
            busy = true;
            transform.rotation = Quaternion.Euler(0, 180f, 0);
            Play("Victory", true);
            ShowEmote(IconKind.Heart, new Color(1f, 0.36f, 0.45f), 1.5f);
            yield return new WaitForSeconds(1.2f);
        }

        /// <summary>A box landed right next to him: a little hop and a "!".</summary>
        public IEnumerator Startle()
        {
            ShowEmote(IconKind.Exclaim, Palette.CraneAlarm, 0.8f);
            const float duration = 0.25f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                model.localPosition = Vector3.up * (Mathf.Sin(t / duration * Mathf.PI) * 0.12f);
                yield return null;
            }
            model.localPosition = Vector3.zero;
        }

        private void Moved()
        {
            lastMove = Time.time;
        }

        // Turns toward the move direction in 0.08 s while sliding to the next cell.
        private IEnumerator Slide(Vector3 target, Direction dir, float duration)
        {
            var start = transform.position;
            var fromRot = transform.rotation;
            var toRot = Quaternion.Euler(0, Yaw(dir), 0);
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(start, target, t / duration);
                transform.rotation = Quaternion.Slerp(fromRot, toRot, Mathf.Clamp01(t / TurnTime));
                lastMove = Time.time;
                yield return null;
            }
            transform.position = target;
            transform.rotation = toRot;
            lastMove = Time.time;
        }

        // ---------- animator ----------

        private void Play(string name, bool force = false)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;
            if (!force && state == name) return;
            animator.CrossFadeInFixedTime(name, Fade);
            state = name;
        }

        private void Update()
        {
            if (animator == null || busy) return;
            bool moving = state == "Walk" || state == "Push" || state == "PushHeavy";
            if (moving && Time.time - lastMove > IdleDelay) Play("Idle");
            // Happy / Think go back to Idle inside the animator; keep our copy in step.
            if ((state == "Happy" || state == "Think") && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !animator.IsInTransition(0))
                state = "Idle";
        }

        // ---------- sitting (home screen) ----------

        private float? seat;
        private Vector3 seatFacing;
        private bool seatTurned;
        private Transform[] thighs, shins;

        /// <summary>
        /// Home screen (owner 2026-11-01): he sits on a crate whose top is at <paramref name="seatHeight"/> (world y), facing
        /// <paramref name="facing"/>, and swings his legs. There is no sitting clip: the wait loop plays and the legs are bent
        /// over it after the animator.
        /// </summary>
        public void Sit(float seatHeight, Vector3 facing)
        {
            seat = seatHeight;
            seatFacing = new Vector3(facing.x, 0f, facing.z).normalized;
            if (animator == null || !animator.isHuman) return;
            thighs = new[] { animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg), animator.GetBoneTransform(HumanBodyBones.RightUpperLeg) };
            shins = new[] { animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), animator.GetBoneTransform(HumanBodyBones.RightLowerLeg) };
        }

        private void SitPose()
        {
            if (rig == null || hips == null || thighs == null || thighs[0] == null || shins[0] == null) return;
            // His own right and front, from the hip joints (the model may be turned inside its root).
            var side = thighs[1].position - thighs[0].position;
            side.y = 0f;
            if (side.sqrMagnitude < 1e-6f) return;
            side.Normalize();
            var front = Vector3.Cross(side, Vector3.up);
            if (!seatTurned)
            {
                // Once: turn the whole worker so that his front is the seat's front (toward the camera).
                seatTurned = true;
                transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(front, seatFacing, Vector3.up), Vector3.up) * transform.rotation;
                return;
            }
            for (int i = 0; i < 2; i++)
            {
                // Thighs forward (down → front is −90° round his right), shins swinging out of step, only in front of the
                // vertical (owner 2026-11-01: the feet went into the crate on the back swing).
                float swing = (Mathf.Sin(Time.time * 3.2f + i * Mathf.PI) * 0.5f + 0.5f) * 32f;
                thighs[i].rotation = Quaternion.AngleAxis(-86f, side) * thighs[i].rotation;
                shins[i].rotation = Quaternion.AngleAxis(68f - swing, side) * shins[i].rotation;
            }
            // Hips just above the seat, over the root (the clip's drift is pulled back).
            var drift = hips.position - transform.position;
            rig.position += new Vector3(-drift.x, seat.Value + 0.1f * rig.lossyScale.y - hips.position.y, -drift.z);
        }

        // After the animator: turn the head bone toward what he watches (ice box, the crane's drop cell).
        private void LateUpdate()
        {
            if (seat.HasValue) SitPose(); else Ground();
            if (head != null && lookTarget.HasValue)
            {
                var to = lookTarget.Value - head.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.01f)
                {
                    var local = Quaternion.Inverse(transform.rotation) * Quaternion.LookRotation(to);
                    float yaw = Mathf.Clamp(Mathf.DeltaAngle(0f, local.eulerAngles.y), -70f, 70f);
                    head.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * head.rotation;
                }
            }
            if (emote != null && emote.gameObject.activeSelf && Camera.main != null)
                emote.rotation = Quaternion.LookRotation(emote.position - Camera.main.transform.position);
        }

        // ---------- signs over the head ----------

        private void BuildEmote()
        {
            emoteMat = MaterialLibrary.Unlit(Color.white, IconFactory.Get(IconKind.Exclaim).texture);
            var quad = Shapes.Quad(transform, new Vector3(0f, 1.72f, 0f), new Vector3(0.42f, 0.42f, 1f), Quaternion.identity, emoteMat, "Emote");
            Shapes.NoShadows(quad);
            emote = quad.transform;
            emote.gameObject.SetActive(false);
        }

        private void ShowEmote(IconKind kind, Color color, float seconds)
        {
            if (emote == null) return;
            if (emoteRoutine != null) StopCoroutine(emoteRoutine);
            emoteRoutine = StartCoroutine(EmoteFor(kind, color, seconds));
        }

        private IEnumerator EmoteFor(IconKind kind, Color color, float seconds)
        {
            MaterialLibrary.SetTexture(emoteMat, IconFactory.Get(kind).texture);
            MaterialLibrary.SetColor(emoteMat, color);
            emote.gameObject.SetActive(true);
            for (float t = 0; t < seconds; t += Time.deltaTime)
            {
                // Pop in, a gentle bob, fade out over the last quarter.
                float pop = Mathf.Min(1f, t / 0.12f);
                float s = 0.42f * (pop < 1f ? Mathf.Lerp(0.3f, 1.15f, pop) : 1f);
                emote.localScale = new Vector3(s, s, 1f);
                emote.localPosition = new Vector3(0f, 1.72f + Mathf.Sin(t * 6f) * 0.03f, 0f);
                float fade = Mathf.Clamp01((seconds - t) / (seconds * 0.25f));
                MaterialLibrary.SetColor(emoteMat, new Color(color.r, color.g, color.b, fade));
                yield return null;
            }
            emote.gameObject.SetActive(false);
            emoteRoutine = null;
        }

        private static float Yaw(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return 0f;
                case Direction.Right: return 90f;
                case Direction.Down: return 180f;
                default: return 270f;
            }
        }
    }
}
