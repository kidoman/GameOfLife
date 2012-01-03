namespace GameOfLife.Console {
    using System;
    using System.Text;

    public class Runner {
        static void Main() {
            Console.WriteLine("Welcome to GameOfLife (by KiD0M4N)");
            Console.WriteLine();

            Console.Write("Press (s for single step, c for constant simulation) to enter pattern or any other key to exit: ");
            do {
                var choice = Console.ReadKey(true);
                Console.WriteLine();

                if (choice.KeyChar == 'c' || choice.KeyChar == 's') {
                    Console.WriteLine();
                    Console.WriteLine("Enter pattern (one row per line, each row equal length, empty line to stop): ");
                    Console.WriteLine();

                    var grid = new StringBuilder();
                    int rows = 0, cols = 0;
                    do {
                        var line = Console.ReadLine();
                        line = line.Trim();

                        if (line.Length == 0)
                            break;

                        grid.Append(line);

                        cols = line.Length;
                        rows++;
                    } while (true);

                    var singleStep = choice.KeyChar == 's';

                    if (singleStep)
                        Console.WriteLine("Hit any key to update the game grid or q to quit...");

                    var generation = new Generation(grid.ToString(), rows, cols);
                    var count = 0;
                    do {
                        Console.WriteLine();
                        Console.WriteLine("Generation: {0}", count++);
                        Console.WriteLine();
                        Console.WriteLine(generation.ToString(rows, cols));

                        generation = generation.Tick();

                        if (singleStep)
                            choice = Console.ReadKey(true);
                    } while (singleStep && choice.KeyChar != 'q' || !singleStep);
                }
                else
                    break;
            } while (true);
        }
    }
}
