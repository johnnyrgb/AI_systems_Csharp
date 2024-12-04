using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

public class Ecosystem : Form
{
    const int N = 30;           // Размер игрового поля
    const int Pmax = 35;        // Максимальное количество растений
    const int Amax = 36;        // Максимальное количество агентов
    const int Nin = 12;         // Число входов
    const int Nout = 4;         // Число выходов
    const int Nw = ((Nin * Nout) + Nout); // Число весов

    const int EAmax = 60;       // Энергия нового агента
    const int EFmax = 15;       // Энергия еды
    const double Erep = 0.9;    // Энергия репродукции

    const int NORTH = 0;        // Северное направление
    const int SOUTH = 1;        // Южное направление
    const int EAST = 2;         // Восточное направление
    const int WEST = 3;         // Западное направление

    const int HERB_PLANE = 0;   // Уровень травоядных
    const int CARN_PLANE = 1;   // Уровень хищников
    const int PLANT_PLANE = 2;  // Уровень еды

    const int HERBIVORE = 0;    // Тип травоядного
    const int CARNIVORE = 1;    // Тип хищника
    const int DEAD = -1;        // Тип съеденного

    const int ACTION_LEFT = 0;  // Влево
    const int ACTION_RIGHT = 1; // Вправо
    const int ACTION_MOVE = 2;  // Вперед
    const int ACTION_EAT = 3;   // Кушать

    static Random random = new Random();
    int[,,] map = new int[3, N, N]; // Карта

    struct TXY
    {
        public int X, Y;
    }

    class Agent
    {
        public int Type;
        public int Energy;
        public int Age;
        public int Generation;
        public TXY Location;
        public int Direction;
        public int[] Inputs = new int[Nin];
        public int[] Weights = new int[Nin * Nout];
        public int[] Biases = new int[Nout];
    }

    Agent[] agents = new Agent[Amax];
    TXY[] plants = new TXY[Pmax];

    public Ecosystem()
    {
        Init();
        this.DoubleBuffered = true;
        this.Paint += DrawHandler;
        this.Load += (s, e) =>
        {
            new Thread(() =>
            {
                while (true)
                {
                    // Используем Invoke для вызова Refresh из основного потока
                    this.Invoke((MethodInvoker)(() => Refresh())); 
                    // Thread.Sleep(100);
                }
            }).Start();
        };
    }

    private void DrawHandler(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        int cellWidth = ClientSize.Width / N;
        int cellHeight = ClientSize.Height / N;

        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                Brush brush;
                if (map[CARN_PLANE, y, x] != 0) brush = Brushes.Blue;
                else if (map[HERB_PLANE, y, x] != 0) brush = Brushes.Red;
                else if (map[PLANT_PLANE, y, x] != 0) brush = Brushes.Green;
                else brush = Brushes.White;

                g.FillRectangle(brush, x * cellWidth, y * cellHeight, cellWidth, cellHeight);
                g.DrawRectangle(Pens.Black, x * cellWidth, y * cellHeight, cellWidth, cellHeight);
            }
        }
    }

    void Init()
    {
        // Очистка карты
        Array.Clear(map, 0, map.Length);

        // Посадка растений
        for (int p = 0; p < Pmax; p++)
        {
            plants[p] = AddInEmptyCell(PLANT_PLANE);
        }

        // Инициализация агентов
        for (int a = 0; a < Amax; a++)
        {
            agents[a] = new Agent
            {
                Type = a < Amax / 2 ? HERBIVORE : CARNIVORE,
                Energy = EAmax / 2,
                Age = 0,
                Generation = 1
            };
            InitAgent(agents[a]);
        }
    }

    TXY AddInEmptyCell(int level)
    {
        TXY res;
        do
        {
            res.X = random.Next(N);
            res.Y = random.Next(N);
        } while (map[level, res.Y, res.X] != 0);
        map[level, res.Y, res.X]++;
        return res;
    }

    void InitAgent(Agent agent)
    {
        agent.Location = AddInEmptyCell(agent.Type);
        agent.Direction = random.Next(4);
        for (int i = 0; i < Nin * Nout; i++) agent.Weights[i] = random.Next(-1, 9);
        for (int i = 0; i < Nout; i++) agent.Biases[i] = random.Next(-1, 9);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        int cellWidth = ClientSize.Width / N;
        int cellHeight = ClientSize.Height / N;

        for (int y = 0; y < N; y++)
        {
            for (int x = 0; x < N; x++)
            {
                Brush brush;
                if (map[CARN_PLANE, y, x] != 0) brush = Brushes.Blue;
                else if (map[HERB_PLANE, y, x] != 0) brush = Brushes.Red;
                else if (map[PLANT_PLANE, y, x] != 0) brush = Brushes.Green;
                else brush = Brushes.White;

                g.FillRectangle(brush, x * cellWidth, y * cellHeight, cellWidth, cellHeight);
                g.DrawRectangle(Pens.Black, x * cellWidth, y * cellHeight, cellWidth, cellHeight);
            }
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.Run(new Ecosystem
        {
            Width = 600,
            Height = 600,
            Text = "Ecosystem Simulation"
        });
    }
}
