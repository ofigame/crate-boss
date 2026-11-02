using NUnit.Framework;
using CrateBoss.Core;

namespace CrateBoss.Tests
{
    public class WinTests
    {
        private readonly RulesEngine rules = new RulesEngine();

        [Test]
        public void PushingLastBoxOntoTarget_Wins()
        {
            var s = TestBoards.Row("..T", "PB.");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.IsWon);
            Assert.IsTrue(s.IsSolved());
        }

        [Test]
        public void NotAllTargetsCovered_NotWon()
        {
            var s = TestBoards.Row("..TT", "PB..");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).IsWon);
        }

        [Test]
        public void AnyBoxTypeCounts_AndExtraBoxesAreFine()
        {
            var s = TestBoards.Row("T.T..", "IP.B.");
            Assert.IsFalse(s.IsSolved());
            s = TestBoards.Row("T.T..", "H.IBP");
            Assert.IsTrue(s.IsSolved());
        }

        [Test]
        public void IceSlidingPastTarget_DoesNotWin()
        {
            var s = TestBoards.Row("..T.", "PI..");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).IsWon);
        }
    }
}
