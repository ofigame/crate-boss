using CrateBoss.Core;
using CrateBoss.Solver;
using NUnit.Framework;

namespace CrateBoss.Tests
{
    /// <summary>The variety filter's push fingerprint (Ana Belge §10.5 "Çeşitlilik").</summary>
    public class VarietyTests
    {
        [Test]
        public void Signature_IsOneTokenPerPush_CratesNamedInTheOrderTouched()
        {
            var s = TestBoards.Row("...T", "PB..");
            var result = new LevelSolver().Solve(s);
            Assert.IsTrue(result.IsSolved);
            Assert.AreEqual("aRaR", SolutionSignature.Of(s, result.Moves), "two pushes of the first crate to the right; walking adds nothing");
        }

        [Test]
        public void Similarity_SameStoryIsOne_DifferentStoryIsLow()
        {
            Assert.AreEqual(1.0, SolutionSignature.Similarity("aRaRbU", "aRaRbU"), 1e-9);
            Assert.AreEqual(1.0 - 1.0 / 3, SolutionSignature.Similarity("aRaRbU", "aRaRbD"), 1e-9);
            Assert.Less(SolutionSignature.Similarity("aRaRaRaR", "bUbUaLaL"), SolutionSignature.TooSimilar);
            Assert.AreEqual(0.0, SolutionSignature.Similarity("aR", ""), 1e-9);
        }
    }
}
