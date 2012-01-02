namespace GameOfLife.Console {
    using System;
    using System.Text;

    public class Runner {
        static void Main() {
            Console.WriteLine("Welcome to GameOfLife (by KiD0M4N)");
            Console.WriteLine();

            Console.Write("Press c to enter pattern or any other key to exit: ");
            do {
                var choice = Console.ReadKey(true);
                Console.WriteLine();

                if (choice.KeyChar == 'c') {
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

                    Console.WriteLine("Hit any key to update the game grid or q to quit...");

                    var generation = new Generation(grid.ToString(), rows, cols);
                    var count = 0;
                    do {
                        Console.WriteLine();
                        Console.WriteLine("Generation: {0}", count++);
                        Console.WriteLine();
                        Console.WriteLine(generation.ToString(rows, cols));

                        generation = generation.Tick();

                        choice = Console.ReadKey(true);
                    } while (choice.KeyChar != 'q');
                }
                else
                    break;
            } while (true);
        }
    }
}
