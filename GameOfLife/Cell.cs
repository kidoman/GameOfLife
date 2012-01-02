namespace GameOfLife {
    using System;

    public class Cell {
        public CellState State { get; set; }

        public Cell()
            : this(CellState.Alive) {
        }

        public Cell(CellState state) {
            State = state;
        }

        public override string ToString() {
            switch (State) {
                case CellState.Alive:
                    return Generation.LiveCellChar.ToString();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
