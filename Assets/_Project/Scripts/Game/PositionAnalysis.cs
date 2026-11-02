using System.Threading;
using System.Threading.Tasks;
using CrateBoss.Core;
using CrateBoss.Solver;

namespace CrateBoss.Game
{
    /// <summary>
    /// Solves a position in the background (the rules are plain C#, so this is safe off the main thread).
    /// Used after every move for "you are stuck" (Ana Belge §10) and hints, and on level load for the best solution.
    /// Starting a new search cancels the old one, so fast play never piles up searches (that froze big levels).
    /// </summary>
    public sealed class PositionAnalysis
    {
        public const int DefaultStateLimit = 150000;

        private readonly int stateLimit;
        private Task<SolveResult> task;
        private CancellationTokenSource cancel;
        private GameState position;

        public PositionAnalysis(int stateLimit = DefaultStateLimit) => this.stateLimit = stateLimit;

        public float StartedAt { get; private set; }
        public bool IsRunning => task != null && !task.IsCompleted;
        public bool IsDone => task != null && task.IsCompleted && !task.IsFaulted;
        public SolveResult Result => IsDone ? task.Result : null;
        public GameState Position => position;

        /// <summary>True only when the search finished and proved there is no way to win from here.</summary>
        public bool IsStuck => IsDone && !task.Result.IsSolved && !task.Result.HitLimit;

        public void Start(GameState state, float now)
        {
            Clear();
            position = state.Clone();
            var copy = position.Clone();
            StartedAt = now;
            cancel = new CancellationTokenSource();
            var token = cancel.Token;
            int limit = stateLimit;
            // A* with the 60 s limit (Ana Belge §10.3, §14): the same shortest answer, much faster on big yards; never a wrong hint.
            // Its own low-priority thread: the search must never take time from the frame (it made the game stutter on phones).
            var done = new TaskCompletionSource<SolveResult>();
            var thread = new Thread(() =>
            {
                try { done.SetResult(new LevelSolver(limit) { UseAStar = true, MaxSeconds = 60 }.Solve(copy, token)); }
                catch (System.Exception e) { done.SetException(e); }
            }) { IsBackground = true, Priority = ThreadPriority.BelowNormal, Name = "PositionAnalysis" };
            thread.Start();
            task = done.Task;
        }

        public void Clear()
        {
            cancel?.Cancel();
            cancel = null;
            task = null;
            position = null;
        }
    }
}
