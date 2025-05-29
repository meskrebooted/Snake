internal class Program
{
    public struct Persona
    {
        public string salvnome;
        public float salvaltezza;
        public int salvpeso;
    }

    private static string salvpFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Snake.txt");

    private static void Main(string[] salvargs)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Benvenuto.");
        Console.ResetColor();

        string salvscelt;
        bool salvfuori = false;

        while (!salvfuori)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Menu:");
            Console.ResetColor();
            Console.WriteLine("1. Gioca a Snake");
            Console.WriteLine("2. Visualizza punteggi");
            Console.WriteLine("3. Elimina punteggio");
            Console.WriteLine("0. Esci");
            Console.Write("Seleziona un'opzione: ");
            salvscelt = Console.ReadLine();

            switch (salvscelt)
            {
                case "1":
                    snake();
                    break;
                case "2":
                    punteggi();
                    break;
                case "3":
                    elim();
                    break;
                case "4":
                    salvfuori = true;
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine();
                    Console.WriteLine("Arrivederci!");
                    Console.ResetColor();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine("Opzione non valida. Riprova.");
                    Console.ResetColor();
                    break;
            }
        }
    }
    private static void snake()
    {
        Console.Clear();
        Console.Write("Inserisci il tuo nome: ");
        string nomeGiocatore = Console.ReadLine();

        int speedInput;
        string prompt = "Seleziona velocità [1], [2] (default), o [3]: ";
        Console.Write(prompt);
        string? input;
        while (!int.TryParse(input = Console.ReadLine(), out speedInput) || speedInput < 1 || speedInput > 3)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                speedInput = 2;
                break;
            }
            Console.WriteLine("Input non valido. Riprova...");
            Console.Write(prompt);
        }

        int[] velocities = { 100, 70, 50 };
        int velocity = velocities[speedInput - 1];
        TimeSpan sleep = TimeSpan.FromMilliseconds(velocity);

        int width = Console.WindowWidth;
        int height = Console.WindowHeight - 2;
        Tile[,] map = new Tile[width, height];
        Direction? direction = null;
        Queue<(int X, int Y)> snake = new();
        (int X, int Y) = (width / 2, height / 2);
        bool closeRequested = false;
        int score = 0;

        try
        {
            Console.CursorVisible = false;
            Console.Clear();
            snake.Enqueue((X, Y));
            map[X, Y] = Tile.Snake;
            PositionFood();
            DrawSnakeHead(X, Y);


            while (!direction.HasValue && !closeRequested)
            {
                GetDirection();
            }


            while (!closeRequested)
            {
                if (Console.WindowWidth != width || Console.WindowHeight != height + 2)
                {
                    Console.Clear();
                    Console.Write("La console è stata ridimensionata. Il gioco è terminato.");
                    return;
                }


                switch (direction)
                {
                    case Direction.Up: Y--; break;
                    case Direction.Down: Y++; break;
                    case Direction.Left: X--; break;
                    case Direction.Right: X++; break;
                }


                if (X < 0 || X >= width || Y < 0 || Y >= height || map[X, Y] == Tile.Snake)
                {
                    Console.Clear();
                    Console.WriteLine($"Game Over. Punteggio: {score}.");
                    SalvaPunteggio(nomeGiocatore, score);
                    return;
                }


                DrawSnakeHead(X, Y);
                snake.Enqueue((X, Y));

                if (map[X, Y] == Tile.Food)
                {
                    score++;
                    PositionFood();
                }
                else
                {

                    (int tailX, int tailY) = snake.Dequeue();
                    map[tailX, tailY] = Tile.Open;
                    Console.SetCursorPosition(tailX, tailY);
                    Console.Write(' ');
                }

                map[X, Y] = Tile.Snake;


                Console.SetCursorPosition(0, height);
                Console.Write($"Giocatore: {nomeGiocatore} | Punteggio: {score}");

                if (Console.KeyAvailable)
                {
                    GetDirection();
                }

                System.Threading.Thread.Sleep(sleep);
            }
        }
        finally
        {
            Console.CursorVisible = true;
            Console.Clear();
        }

        void GetDirection()
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow: if (direction != Direction.Down) direction = Direction.Up; break;
                case ConsoleKey.DownArrow: if (direction != Direction.Up) direction = Direction.Down; break;
                case ConsoleKey.LeftArrow: if (direction != Direction.Right) direction = Direction.Left; break;
                case ConsoleKey.RightArrow: if (direction != Direction.Left) direction = Direction.Right; break;
                case ConsoleKey.Escape: closeRequested = true; break;
            }
        }

        void PositionFood()
        {
            List<(int X, int Y)> possibleCoordinates = new();
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (map[i, j] == Tile.Open)
                    {
                        possibleCoordinates.Add((i, j));
                    }
                }
            }

            if (possibleCoordinates.Count > 0)
            {
                int index = Random.Shared.Next(possibleCoordinates.Count);
                (int foodX, int foodY) = possibleCoordinates[index];
                map[foodX, foodY] = Tile.Food;
                Console.SetCursorPosition(foodX, foodY);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write('O');
                Console.ResetColor();
            }
        }

        void DrawSnakeHead(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write('@');
            Console.ResetColor();
        }

        void SalvaPunteggio(string nome, int punteggio)
        {
            using StreamWriter writer = new StreamWriter(salvpFile, true);
            writer.WriteLine($"{nome};{punteggio};{DateTime.Now:HH:mm:ss}");
            Console.WriteLine("\nPunteggio salvato.");
        }
    }

    private static void punteggi()
    {
        Console.Clear();
        if (!File.Exists(salvpFile))
        {
            Console.WriteLine("Nessun punteggio salvato.");
        }
        else
        {
            Console.WriteLine("Punteggi salvati:");
            string[] salvrighe = File.ReadAllLines(salvpFile);
            foreach (string salvriga in salvrighe)
            {
                string[] salvdati = salvriga.Split(';');
                Console.WriteLine($"Nome: {salvdati[0]}, Punteggio: {salvdati[1]}, Tempo: {salvdati[2]}");
            }
        }
        Console.WriteLine("Premi un tasto per continuare...");
        Console.ReadKey();
    }

    private static void elim()
    {
        Console.Clear();
        if (!File.Exists(salvpFile))
        {
            Console.WriteLine("Nessun punteggio salvato.");
        }
        else
        {
            string[] salvrighe = File.ReadAllLines(salvpFile);
            for (int salvindex = 0; salvindex < salvrighe.Length; salvindex++)
            {
                string[] salvdati = salvrighe[salvindex].Split(';');
                Console.WriteLine($"{salvindex + 1}. Nome: {salvdati[0]}, Punteggio: {salvdati[1]}, Tempo: {salvdati[2]}");
            }
            Console.Write("Seleziona il numero del punteggio da eliminare: ");
            int salvindice = int.Parse(Console.ReadLine()) - 1;
            if (salvindice >= 0 && salvindice < salvrighe.Length)
            {
                string[] salvnuovoArray = salvrighe.Where((salvriga, salvindex) => salvindex != salvindice).ToArray();
                File.WriteAllLines(salvpFile, salvnuovoArray);
                Console.WriteLine("Punteggio eliminato con successo!");
            }
            else
            {
                Console.WriteLine("Indice non valido.");
            }
        }
        Console.WriteLine("Premi un tasto per continuare...");
        Console.ReadKey();
    }

    enum Direction
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    enum Tile
    {
        Open = 0,
        Snake,
        Food,
    }
}
