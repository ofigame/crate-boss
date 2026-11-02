using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Solver;

namespace CrateBoss.Tests
{
    /// <summary>Quality filters (Ana Belge §10.5 "Kalite filtreleri").</summary>
    public class QualityFilterTests
    {
        [Test]
        public void Monotone_StraightCorridor_IsRejected()
        {
            var s = TestBoards.Row("....T", "PB...");
            Assert.IsTrue(MonotonePushes.Solvable(s), "pushing straight at the target needs no idea");
        }

        [Test]
        public void Monotone_DetourRoundACorner_IsAccepted()
        {
            var s = TestBoards.Make(
                new[] { "########", "#T.....#", "#####..#", "#......#", "#......#", "########" },
                new[] { "........", "........", "........", "...B....", "..P.....", "........" });
            Assert.IsTrue(new LevelSolver().Solve(s).IsSolved);
            Assert.IsFalse(MonotonePushes.Solvable(s), "the box must first move away from the target");
        }

        [Test]
        public void MachinesMatter_UselessCrane_IsRejected()
        {
            var s = TestBoards.Row("......T", "PB.....");
            s.SetCrane(new CraneDefinition(50, new[] { new CraneDrop(TestBoards.At(6 - 1), BoxType.Container) }));
            var q = QualityCheck.Evaluate(s, "test");
            Assert.IsFalse(q.MachinesMatter);
            CollectionAssert.Contains(q.Failures, "makineler önemsiz: onlarsız aynı çözüm, tolerans ≥ 8");
        }

        [Test]
        public void MachinesMatter_Rihtim_IsAccepted()
        {
            var loader = new LevelLoader();
            var data = loader.Load("level_06");
            var q = QualityCheck.Evaluate(loader.BuildState(data), data.design.idea);
            Assert.IsTrue(q.MachinesMatter);
            Assert.AreEqual(-1, q.MovesWithoutMachines, "Rıhtım cannot be won without the crane");
            Assert.IsTrue(q.UsesLock, "the solution parks a crate on the second drop cell");
            Assert.IsTrue(q.MachineFillsHole);
        }

        [Test]
        public void IdeaRequired_And_ToleranceTerm()
        {
            Assert.AreEqual(12, QualityCheck.MachineTerm(2));
            Assert.AreEqual(0, QualityCheck.MachineTerm(6));
            Assert.AreEqual(0, QualityCheck.MachineTerm(CraneTolerance.NoCrane));
            var q = QualityCheck.Evaluate(TestBoards.Row("..T", "PB."), "");
            CollectionAssert.Contains(q.Failures, "fikir cümlesi (design.idea) yok");
            CollectionAssert.Contains(q.Failures, "monoton test: kutuları hiç geri itmeden çözülüyor");
        }

    }
}
