using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Solver;

namespace CrateBoss.Tests
{
    /// <summary>Car park and truck (Ana Belge §5.5, §21 "Otopark").</summary>
    public class ParkingTests
    {
        private static readonly RulesEngine Rules = new RulesEngine();

        private static CarData Car(int x0, int y0, int x1, int y1) => new CarData { cells = new[] { new[] { x0, y0 }, new[] { x1, y1 } }, color = "pink" };

        private static int[][] Cells(params (int x, int y)[] c)
        {
            var a = new int[c.Length][];
            for (int i = 0; i < c.Length; i++) a[i] = new[] { c[i].x, c[i].y };
            return a;
        }

        // Row 1 is a lane to the exit at (0,1); the car stands on (3,1)-(4,1), side by side.
        //  #######
        //  ......#
        //  #....T#
        //  #######
        private static GameState Lane(System.Action<LevelData> more = null) => GoalBoards.Make(
            new[] { "#######", "......#", "#....T#", "#######" },
            new[] { ".......", ".......", ".P..B..", "......." },
            d =>
            {
                d.parking = new ParkingData { cars = new[] { Car(3, 1, 4, 1) }, exits = Cells((0, 1)) };
                more?.Invoke(d);
            });

        [Test]
        public void Car_OnlyMovesAlongItsAxis()
        {
            var s = Lane();
            var r = Rules.TryMove(s, PlayerAction.SendCar(0, Direction.Up));
            Assert.IsFalse(r.Success);
            Assert.IsNull(r.Car, "across the car: nothing at all");
            Assert.IsTrue(s.CarPresent(0));
        }

        [Test]
        public void Car_WithAClearWay_DrivesOut_AndCountsAsAMove()
        {
            var s = Lane();
            var r = Rules.TryMove(s, PlayerAction.SendCar(0, Direction.Left));
            Assert.IsTrue(r.Success);
            Assert.IsTrue(r.Car.Left);
            Assert.AreEqual(new GridPos(0, 1), r.Car.ReachedCell);
            Assert.IsFalse(s.CarPresent(0));
            Assert.AreEqual(1, s.MoveCount);
            Assert.AreEqual(-1, s.CarAt(new GridPos(3, 1)), "its bay is free");
        }

        [Test]
        public void Car_ThatHitsSomething_BouncesBack_OnlyTheBumpCounts()
        {
            var s = Lane(d => d.objects = new[] { ".......", ".B.....", ".P..B..", "......." });
            var session = new GameSession(s);
            var before = s.Clone();
            var r = session.Move(PlayerAction.SendCar(0, Direction.Left));
            Assert.IsFalse(r.Success);
            Assert.IsNotNull(r.Car);
            Assert.IsFalse(r.Car.Left);
            Assert.AreEqual(new GridPos(2, 1), r.Car.ReachedCell, "rolled up to the crate");
            Assert.AreEqual(1, session.Collisions);
            Assert.IsTrue(session.State.SameAs(before), "nothing else changed, no move counted");
            Assert.AreEqual(0, session.UndoCount);
        }

        [Test]
        public void Car_BlocksTheWorkerAndCrates()
        {
            var s = Lane(d =>
            {
                d.ground = new[] { "#######", "......#", "#.....#", "#.....#", "#######" };
                d.objects = new[] { ".......", ".......", "...B...", "...P...", "......." };
            });
            Assert.IsFalse(Rules.TryMove(s, Direction.Up).Success, "the crate cannot go into the car");
            s.PlayerPos = new GridPos(2, 2);
            Assert.IsTrue(Rules.TryMove(s, Direction.Up).Success);
            Assert.IsFalse(Rules.TryMove(s, Direction.Right).Success, "the worker cannot walk into the car");
        }

        [Test]
        public void CrateOnItsWay_StopsTheCar_UntilItIsGone()
        {
            var s = Lane(d => d.objects = new[] { ".......", "..B....", ".P.....", "......." });
            Assert.IsFalse(Rules.TryMove(s, PlayerAction.SendCar(0, Direction.Left)).Success);
            s.GetBoxAt(new GridPos(2, 1)).Position = new GridPos(3, 2); // the crate pushed off the lane
            Assert.IsTrue(Rules.TryMove(s, PlayerAction.SendCar(0, Direction.Left)).Car.Left);
        }

        [Test]
        public void SendingACar_CountsDownTheCranes()
        {
            var s = Lane(d => d.cranes = new[] { new CraneData { every = 2, drops = new[] { new CraneDropData { x = 2, y = 2, type = "K" } } } });
            Rules.TryMove(s, PlayerAction.SendCar(0, Direction.Left));
            Assert.AreEqual(1, s.CraneCounter);
        }

        [Test]
        public void CraneWaits_WhileACarStandsOnItsCell()
        {
            var s = Lane(d => d.cranes = new[] { new CraneData { every = 2, drops = new[] { new CraneDropData { x = 4, y = 1, type = "K" } } } });
            Rules.TryMove(s, Direction.Right);
            Rules.TryMove(s, Direction.Left);
            Assert.IsTrue(CraneSystem.IsWaiting(s, 0), "the car holds the crane up");
            Rules.TryMove(s, PlayerAction.SendCar(0, Direction.Left));
            Assert.AreEqual(1, s.CraneIndex, "the car left: the crane drops");
        }

        [Test]
        public void Undo_BringsTheCarBack()
        {
            var session = new GameSession(Lane());
            var before = session.State.Clone();
            session.Move(PlayerAction.SendCar(0, Direction.Left));
            Assert.IsFalse(session.State.CarPresent(0));
            session.Undo();
            Assert.IsTrue(session.State.SameAs(before));
            Assert.IsTrue(session.State.CarPresent(0));
        }

        // ---------- truck ----------

        // Bay (5,1),(5,2); the truck backs in along (6,1),(6,2).
        private static GameState Yard(int trucks, string[] objects) => GoalBoards.Make(
            new[] { "########", "#......#", "#......#", "#......#", "########" },
            objects,
            d =>
            {
                d.goal = new[] { "truck" };
                d.parking = new ParkingData { bay = Cells((5, 1), (5, 2)), truckRoute = Cells((6, 1), (6, 2)), trucks = trucks };
            });

        [Test]
        public void Truck_ComesWhenItsRouteIsClear()
        {
            var s = Yard(1, new[] { "........", "......B.", ".P......", "........", "........" });
            Assert.IsFalse(s.TruckParked);
            Assert.IsNull(Rules.TryMove(s, Direction.Down).TruckLeft, "a crate stands on the route");
            Assert.IsFalse(s.TruckParked);
            s.GetBoxAt(new GridPos(6, 1)).Position = new GridPos(2, 1); // moved away
            var r = Rules.TryMove(s, Direction.Up);
            Assert.IsTrue(r.TruckLeft.Arrived);
            Assert.IsTrue(s.TruckParked);
            Assert.IsTrue(s.IsTruckWall(new GridPos(6, 1)), "the parked truck is a wall");
        }

        [Test]
        public void Truck_LeavesWhenFull_TheNextOneComes_AndTheGoalCountsTrucks()
        {
            var s = Yard(2, new[] { "........", "........", "........", ".P......", "........" });
            Rules.TryMove(s, Direction.Up); // the route is clear: the truck backs in
            Assert.IsTrue(s.TruckParked);
            s.AddBox(BoxType.Normal, new GridPos(4, 1));
            s.AddBox(BoxType.Normal, new GridPos(4, 2));
            s.PlayerPos = new GridPos(3, 1);
            Assert.IsNull(Rules.TryMove(s, Direction.Right).TruckLeft, "half full");
            s.PlayerPos = new GridPos(3, 2);
            var r = Rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.TruckLeft.Left);
            Assert.AreEqual(2, r.TruckLeft.BoxIds.Count);
            Assert.IsTrue(r.TruckLeft.Arrived, "the second truck backs in");
            Assert.AreEqual(1, s.TrucksLeft);
            Assert.IsFalse(r.IsWon);
            Assert.AreEqual(0, s.Boxes.Count, "the crates went with the truck");
        }

        [Test]
        public void LastTruck_Leaving_WinsTheLevel()
        {
            var s = Yard(1, new[] { "........", "....B...", "....B...", ".P......", "........" });
            Rules.TryMove(s, Direction.Up);
            s.PlayerPos = new GridPos(3, 1);
            Rules.TryMove(s, Direction.Right);
            s.PlayerPos = new GridPos(3, 2);
            Assert.IsTrue(Rules.TryMove(s, Direction.Right).IsWon);
        }

        [Test]
        public void WithCargo_OnlyCargoFillsTheTruck()
        {
            // Owner 2026-11-01: the truck takes the cargo crates (Y); a plain crate in the bay only stands in the way.
            var s = Yard(1, new[] { "........", "....B...", "....Y...", ".P......", "........" });
            Assert.IsTrue(s.HasCargo);
            Rules.TryMove(s, Direction.Up);
            s.PlayerPos = new GridPos(3, 1);
            Assert.IsNull(Rules.TryMove(s, Direction.Right).TruckLeft, "a plain crate does not count");
            s.PlayerPos = new GridPos(3, 2);
            Assert.IsNull(Rules.TryMove(s, Direction.Right).TruckLeft);
            Assert.AreEqual(1, s.TrucksLeft);
            s.RemoveBox(s.GetBoxAt(new GridPos(5, 1)));
            s.AddBox(BoxType.Delivery, new GridPos(4, 1), 0, -1);
            s.PlayerPos = new GridPos(3, 1);
            Assert.IsTrue(Rules.TryMove(s, Direction.Right).IsWon, "two cargo crates load the truck");
        }

        // ---------- solver ----------

        [Test]
        public void Solver_SendsTheCar_ThatStandsOnTheWay()
        {
            // The crate goes right to the target; the car on (3,2)-(4,2) must leave through the exit (6,2) first.
            var s = GoalBoards.Make(
                new[] { "#######", "#.....#", "#....T.", "#######" },
                new[] { ".......", ".......", ".PB....", "......." },
                d => d.parking = new ParkingData { cars = new[] { Car(3, 2, 4, 2) }, exits = Cells((6, 2)) });
            var r = new LevelSolver(50000).Solve(s);
            Assert.IsTrue(r.IsSolved, r.ToString());
            Assert.AreEqual(4, r.MoveCount);
            Assert.AreEqual(PlayerAction.SendCar(0, Direction.Right), r.Moves[0]);
        }

        [Test]
        public void SolutionText_RoundTrips()
        {
            var list = SolutionText.Parse("UR3lDD12u");
            Assert.AreEqual(6, list.Count);
            Assert.AreEqual(PlayerAction.SendCar(3, Direction.Left), list[2]);
            Assert.AreEqual(PlayerAction.SendCar(12, Direction.Up), list[5]);
            Assert.AreEqual("UR3lDD12u", SolutionText.Write(list));
            Assert.IsNull(SolutionText.Parse("U3D"), "a car needs a small letter");
            Assert.IsNull(SolutionText.Parse("UX"));
        }
    }
}
