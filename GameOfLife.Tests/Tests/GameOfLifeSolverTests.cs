using System;

public class GameOfLifeSolverTests {
}

public class GameOfLifeSolver : IGameOfLifeSolver {
    public bool[,] Solve(bool[,] input, int moves) {
        throw new NotImplementedException();
    }
}

public interface IGameOfLifeSolver {
    bool[,] Solve(bool[,] input, int moves);
}