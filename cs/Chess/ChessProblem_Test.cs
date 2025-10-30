using System;
using System.IO;
using NUnit.Framework;

namespace Chess
{
    [TestFixture]
    public class ChessProblem_Test
    {
        [Test]
        public void RepeatedMethodCallDoNotChangeBehaviour()
        {
            var boardLines = new[]
            {
                "        ",
                "        ",
                "        ",
                "   q    ",
                "    K   ",
                " Q      ",
                "        ",
                "        ",
            };
            
            var board = new BoardParser().ParseBoard(boardLines);
            var chessProblem = new ChessProblem(board);
            var chessStatus = chessProblem.CalculateChessStatus();
            Assert.AreEqual(ChessStatus.Check, chessStatus);

            // Now check that internal board modifications during the first call do not change answer
            chessStatus = chessProblem.CalculateChessStatus();
            Assert.AreEqual(ChessStatus.Check, chessStatus);
        }

        [Test]
        public void AllTests()
        {
            var dir = TestContext.CurrentContext.TestDirectory;
            var testsCount = 0;
            foreach (var filename in Directory.GetFiles(Path.Combine(dir, "ChessTests"), "*.in"))
            {
                TestOnFile(filename);
                testsCount++;
            }
            Console.WriteLine("Tests passed: " + testsCount);
        }

        private static void TestOnFile(string filename)
        {
            var boardLines = File.ReadAllLines(filename);
            var board = new BoardParser().ParseBoard(boardLines);
            var chessProblem = new ChessProblem(board);
            var expectedAnswer = File.ReadAllText(Path.ChangeExtension(filename, ".ans")).Trim();
            Assert.AreEqual(expectedAnswer, chessProblem.CalculateChessStatus().ToString().ToLower(), "Failed test " + filename);
        }
    }
}