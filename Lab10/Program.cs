using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Simulation
{
    class Program
    {
        const int N = 30;           // размер игрового поля
        const int Pmax = 35;        // предельное число растений
        const int Amax = 36;        // предельное число агентов

        const int Nin = 12;         // число входов
        const int Nout = 4;         // число выходов
        const int Nw = ((Nin * Nout) + Nout);  // число весов

        const int EAmax = 60;       // энергия нового агента
        const int EFmax = 15;       // энергия еды
        const double Erep = 0.9;    // энергия репродукции

        const int NORTH = 0;        // северное направление
        const int SOUTH = 1;        // южное направление
        const int EAST = 2;         // восточное направление
        const int WEST = 3;         // западное направление

        const int HERB_PLANE = 0;   // уровень травоядных
        const int CARN_PLANE = 1;   // уровень хищника
        const int PLANT_PLANE = 2;  // уровень еды

        const int HERBIVORE = 0;    // тип травоядного
        const int CARNIVORE = 1;    // тип хищника
        const int DEAD = -1;        // тип съеденного

        const int ACTION_LEFT = 0;  // влево
        const int ACTION_RIGHT = 1; // вправо
        const int ACTION_MOVE = 2;  // вперед
        const int ACTION_EAT = 3;   // кушать

        const int HERB_FRONT = 0;   // травоядное впереди
        const int CARN_FRONT = 1;   // хищник впереди
        const int PLANT_FRONT = 2;  // еда впереди
        const int HERB_LEFT = 3;    // травоядное слева
        const int CARN_LEFT = 4;    // хищник слева
        const int PLANT_LEFT = 5;   // еда слева
        const int HERB_RIGHT = 6;   // травоядное справа
        const int CARN_RIGHT = 7;   // хищник справа
        const int PLANT_RIGHT = 8;  // еда справа
        const int HERB_PROXIMITY = 9; // травоядное вблизи
        const int CARN_PROXIMITY = 10; // хищник вблизи
        const int PLANT_PROXIMITY = 11; // еда вблизи

        static Random random = new Random();

        struct TXY
        {
            public int X, Y;
        }

        struct TAgent
        {
            public int type;                // тип агента
            public int energy;              // энергия агента
            public int parent;
            public int age;                 // возраст агента в итерациях
            public int generation;          // поколение
            public TXY location;            // координаты агента
            public int direction;           // направление
            public int[] inputs;            // входы
            public int[] weights;           // веса
            public int[] biass;             // смещения
            public int[] actions;           // действия агента
        }

        static int[,,] map = new int[3, N, N]; // карта
        static TXY[] plants = new TXY[Pmax];   // координаты растений
        static TAgent[] agents = new TAgent[Amax]; // агенты

        static int[] agentTypeCounts = new int[2] { 0, 0 }; // количество агентов по типам
        static int[] agentMaxAge = new int[2] { 0, 0 };     // возраст агентов по типам
        static int[] agentBirths = new int[2] { 0, 0 };     // количество рождений по типам
        static int[] agentDeaths = new int[2] { 0, 0 };     // количество гибелей по типам
        static TAgent[] bestAgent = new TAgent[2];          // старейшие погибшие агенты
        static int[] agentMaxGen = new int[2] { 0, 0 };     // наибольшие поколения по типам

        static RenderWindow window;
        static Texture texture;
        static Sprite sprite;

        static void Main(string[] args)
        {
            Init();
            GraphInit();

            while (window.IsOpen)
            {
                window.DispatchEvents();
                for (int i = 0; i < 600; i++)
                {
                    for (int t = HERBIVORE; t <= CARNIVORE; t++)
                        for (int j = 0; j < Amax; j++)
                            if (agents[j].type == t) Simulate(ref agents[j]);
                    Screen();
                }
                CloseGraph();
                ShowStat();
            }
        }

        static void GraphInit()
        {
            window = new RenderWindow(new VideoMode(800, 800), "Simulation");
            window.SetFramerateLimit(60);
            texture = new Texture(N, N);
            sprite = new Sprite(texture);
        }

        static void Screen()
        {
            texture.Update(map);
            window.Clear();
            window.Draw(sprite);
            window.Display();
            System.Threading.Thread.Sleep(100);
        }

        static void CloseGraph()
        {
            window.Close();
        }

        static TXY AddInEmptyCell(int Level)
        {
            TXY res;
            do
            {
                res.X = random.Next(N);
                res.Y = random.Next(N);
            } while (map[Level, res.Y, res.X] != 0);
            map[Level, res.Y, res.X]++;
            return res;
        }

        static void AgentToMap(ref TAgent agent)
        {
            agent.location = AddInEmptyCell(agent.type);
            agent.direction = random.Next(4);
        }

        static void InitAgent(ref TAgent agent)
        {
            agent.energy = (EAmax / 2);
            agent.age = 0;
            agent.generation = 1;
            agentTypeCounts[agent.type]++;
            AgentToMap(ref agent);
            agent.weights = new int[(Nin * Nout)];
            agent.biass = new int[Nout];
            agent.inputs = new int[Nin];
            agent.actions = new int[Nout];
            for (int i = 0; i < (Nin * Nout); i++) agent.weights[i] = GetWeight();
            for (int i = 0; i < Nout; i++) agent.biass[i] = GetWeight();
        }

        static void Init()
        {
            for (int l = 0; l < 3; l++)
                for (int y = 0; y < N; y++)
                    for (int x = 0; x < N; x++) map[l, y, x] = 0;
            for (int p = 0; p < Pmax; p++)
                plants[p] = AddInEmptyCell(PLANT_PLANE);
            for (int a = 0; a < Amax; a++)
            {
                agents[a].type = CARNIVORE;
                if (a < (Amax / 2)) agents[a].type = HERBIVORE;
                InitAgent(ref agents[a]);
            }
        }

        static int Clip(int z)
        {
            if (z > N - 1) z = (z % N);
            else if (z < 0) z = (N + z);
            return z;
        }

        static void Percept(int x, int y, int[] inputs, TXY[] offsets, int neg)
        {
            for (int p = HERB_PLANE; p <= PLANT_PLANE; p++)
            {
                int i = 0;
                inputs[p] = 0;
                while (offsets[i].X != 9)
                {
                    int xoff = Clip(x + (offsets[i].X * neg));
                    int yoff = Clip(y + (offsets[i].Y * neg));
                    if (map[p, yoff, xoff] != 0) inputs[p]++;
                    i++;
                }
            }
        }

        static void Turn(int action, ref TAgent agent)
        {
            if (agent.direction == NORTH)
                if (action == ACTION_LEFT) agent.direction = WEST; else agent.direction = EAST;
            if (agent.direction == SOUTH)
                if (action == ACTION_LEFT) agent.direction = EAST; else agent.direction = WEST;
            if (agent.direction == EAST)
                if (action == ACTION_LEFT) agent.direction = NORTH; else agent.direction = SOUTH;
            if (agent.direction == WEST)
                if (action == ACTION_LEFT) agent.direction = SOUTH; else agent.direction = NORTH;
        }

        static void Move(ref TAgent agent)
        {
            TXY[] offsets = new TXY[4] { new TXY { X = -1, Y = 0 }, new TXY { X = 1, Y = 0 }, new TXY { X = 0, Y = 1 }, new TXY { X = 0, Y = -1 } };
            map[agent.type, agent.location.Y, agent.location.X]--;
            agent.location.X = Clip(agent.location.X + offsets[agent.direction].X);
            agent.location.Y = Clip(agent.location.Y + offsets[agent.direction].Y);
            map[agent.type, agent.location.Y, agent.location.X]++;
        }

        static void KillAgent(ref TAgent agent)
        {
            agentDeaths[agent.type]++;
            map[agent.type, agent.location.Y, agent.location.X]--;
            agentTypeCounts[agent.type]--;
            if (agent.age > bestAgent[agent.type].age)
            {
                bestAgent[agent.type] = agent;
            }
            if (agentTypeCounts[agent.type] < (Amax / 4)) InitAgent(ref agent);
            else
            {
                agent.location.X = -1;
                agent.location.Y = -1;
                agent.type = DEAD;
            }
        }

        static void ReproduceAgent(ref TAgent agent)
        {
            TAgent child = new TAgent();
            int i;

            if (agentTypeCounts[agent.type] < (Amax / 2))
            {
                for (i = 0; i < Amax; i++)
                {
                    if (agents[i].type == DEAD) break;
                }
                if (i < Amax)
                {
                    child = agent;
                    AgentToMap(ref child);
                    if (random.NextDouble() <= 0.2)
                    {
                        child.weights[random.Next(Nw)] = GetWeight();
                    }
                    child.generation = child.generation + 1;
                    child.age = 0;
                    if (agentMaxGen[child.type] < child.generation) agentMaxGen[child.type] = child.generation;
                    child.energy = agent.energy = (EAmax / 2);
                    agentTypeCounts[child.type]++;
                    agentTypeReproductions[child.type]++;
                }
            }
        }

        static int ChooseObject(int plane, int ax, int ay, TXY[] offsets, int neg, out int ox, out int oy)
        {
            int xoff, yoff, i = 0;
            ox = 0;
            oy = 0;

            while (offsets[i].X != 9)
            {
                xoff = Clip(ax + (offsets[i].X * neg));
                yoff = Clip(ay + (offsets[i].Y * neg));
                if (map[plane, yoff, xoff] != 0)
                {
                    ox = xoff;
                    oy = yoff;
                    return 1;
                }
                i++;
            }
            return 0;
        }

        static void Eat(ref TAgent agent)
        {
            int plane = 0, ox, oy, ret = 0, i;

            if (agent.type == CARNIVORE) plane = HERB_PLANE;
            else if (agent.type == HERBIVORE) plane = PLANT_PLANE;

            int ax = agent.location.X;
            int ay = agent.location.Y;

            if (agent.direction == NORTH) ret = ChooseObject(plane, ax, ay, northProx, 1, out ox, out oy);
            if (agent.direction == SOUTH) ret = ChooseObject(plane, ax, ay, northProx, -1, out ox, out oy);
            if (agent.direction == WEST) ret = ChooseObject(plane, ax, ay, westProx, 1, out ox, out oy);
            if (agent.direction == EAST) ret = ChooseObject(plane, ax, ay, westProx, -1, out ox, out oy);

            if (ret != 0)
            {
                if (plane == PLANT_PLANE)
                {
                    for (i = 0; i < Pmax; i++)
                    {
                        if ((plants[i].X == ox) && (plants[i].Y == oy))
                            break;
                    }
                    if (i < Pmax)
                    {
                        agent.energy += EFmax;
                        if (agent.energy > EAmax) agent.energy = EAmax;
                        map[PLANT_PLANE, oy, ox]--;
                        plants[i] = AddInEmptyCell(PLANT_PLANE);
                    }
                }
                else if (plane == HERB_PLANE)
                {
                    for (i = 0; i < Amax; i++)
                    {
                        if ((agents[i].location.X == ox) && (agents[i].location.Y == oy))
                            break;
                    }
                    if (i < Amax)
                    {
                        agent.energy += (EFmax * 2);
                        if (agent.energy > EAmax) agent.energy = EAmax;
                        KillAgent(ref agents[i]);
                    }
                }

                if (agent.energy > (Erep * EAmax))
                {
                    ReproduceAgent(ref agent);
                    agentBirths[agent.type]++;
                }
            }
        }

        static void Simulate(ref TAgent agent)
        {
            int x = agent.location.X;
            int y = agent.location.Y;

            switch (agent.direction)
            {
                case NORTH:
                    Percept(x, y, agent.inputs, northFront, 1);
                    Percept(x, y, agent.inputs, northLeft, 1);
                    Percept(x, y, agent.inputs, northRight, 1);
                    Percept(x, y, agent.inputs, northProx, 1);
                    break;

                case SOUTH:
                    Percept(x, y, agent.inputs, northFront, -1);
                    Percept(x, y, agent.inputs, northLeft, -1);
                    Percept(x, y, agent.inputs, northRight, -1);
                    Percept(x, y, agent.inputs, northProx, -1);
                    break;

                case WEST:
                    Percept(x, y, agent.inputs, westFront, 1);
                    Percept(x, y, agent.inputs, westLeft, 1);
                    Percept(x, y, agent.inputs, westRight, 1);
                    Percept(x, y, agent.inputs, westProx, 1);
                    break;

                case EAST:
                    Percept(x, y, agent.inputs, westFront, -1);
                    Percept(x, y, agent.inputs, westLeft, -1);
                    Percept(x, y, agent.inputs, westRight, -1);
                    Percept(x, y, agent.inputs, westProx, -1);
                    break;
            }

            for (int out = 0; out < Nout; out++)
            {
                agent.actions[out] = agent.biass[out];
                for (int in = 0; in < Nin; in++)
                {
                    agent.actions[out] += (agent.inputs[in] * agent.weights[(out *Nin) + in]);
                }
            }

            int largest = -9;
            int winner = -1;
            for (int out = 0; out < Nout; out++)
            {
                if (agent.actions[out] >= largest)
                {
                    largest = agent.actions[out];
                    winner = out;
                }
            }

            if (winner == ACTION_LEFT) Turn(winner, ref agent);
            if (winner == ACTION_RIGHT) Turn(winner, ref agent);
            if (winner == ACTION_MOVE) Move(ref agent);
            if (winner == ACTION_EAT) Eat(ref agent);

            if (agent.type == HERBIVORE) agent.energy -= 2; else agent.energy -= 1;

            if (agent.energy <= 0) KillAgent(ref agent);
            else
            {
                agent.age++;
                if (agent.age > agentMaxAge[agent.type])
                {
                    agentMaxAge[agent.type] = agent.age;
                }
            }
        }

        static void ShowStat()
        {
            Console.WriteLine("Результаты:");
            Console.WriteLine("Травоядных всего                - " + agentTypeCounts[HERBIVORE]);
            Console.WriteLine("Хищников всего                  - " + agentTypeCounts[CARNIVORE]);
            Console.WriteLine("Возраст травоядных              - " + agentMaxAge[HERBIVORE]);
            Console.WriteLine("Возраст хищников                - " + agentMaxAge[CARNIVORE]);
            Console.WriteLine("Рождений травоядных             - " + agentBirths[HERBIVORE]);
            Console.WriteLine("Рождений хищников               - " + agentBirths[CARNIVORE]);
            Console.WriteLine("Гибелей травоядных              - " + agentDeaths[HERBIVORE]);
            Console.WriteLine("Гибелей хищников                - " + agentDeaths[CARNIVORE]);
            Console.WriteLine("Репродукций травоядных          - " + agentTypeReproductions[HERBIVORE]);
            Console.WriteLine("Репродукций хищников            - " + agentTypeReproductions[CARNIVORE]);
            Console.WriteLine("Наибольшие поколения травоядных - " + agentMaxGen[HERBIVORE]);
            Console.WriteLine("Наибольшие поколения хищников   - " + agentMaxGen[CARNIVORE]);

            Console.WriteLine("\nВеса лучшего травоядного:");
            for (int i = 0; i < Nout; i++)
            {
                Console.Write("{0,4} ", bestAgent[HERBIVORE].biass[i]);
            }
            Console.WriteLine("\n");
            for (int o = 0; o < Nout; o++)
            {
                for (int i = 0; i < Nin; i++)
                {
                    Console.Write("{0,4} ", bestAgent[HERBIVORE].weights[(o * Nin) + i]);
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nВеса лучшего хищника:");
            for (int i = 0; i < Nout; i++)
            {
                Console.Write("{0,4} ", bestAgent[CARNIVORE].biass[i]);
            }
            Console.WriteLine("\n");
            for (int o = 0; o < Nout; o++)
            {
                for (int i = 0; i < Nin; i++)
                {
                    Console.Write("{0,4} ", bestAgent[CARNIVORE].weights[(o * Nin) + i]);
                }
                Console.WriteLine();
            }
        }

        static int GetWeight()
        {
            return random.Next(9) - 1;
        }

        static TXY[] northFront = new TXY[] { new TXY { X = -2, Y = -2 }, new TXY { X = -2, Y = -1 }, new TXY { X = -2, Y = 0 }, new TXY { X = -2, Y = 1 }, new TXY { X = -2, Y = 2 }, new TXY { X = 9, Y = 9 } };
        static TXY[] northLeft = new TXY[] { new TXY { X = 0, Y = -2 }, new TXY { X = -1, Y = -2 }, new TXY { X = 9, Y = 9 } };
        static TXY[] northRight = new TXY[] { new TXY { X = 0, Y = 2 }, new TXY { X = -1, Y = 2 }, new TXY { X = 9, Y = 9 } };
        static TXY[] northProx = new TXY[] { new TXY { X = 0, Y = -1 }, new TXY { X = -1, Y = -1 }, new TXY { X = -1, Y = 0 }, new TXY { X = -1, Y = 1 }, new TXY { X = 0, Y = 1 }, new TXY { X = 9, Y = 9 } };
        static TXY[] westFront = new TXY[] { new TXY { X = 2, Y = -2 }, new TXY { X = 1, Y = -2 }, new TXY { X = 0, Y = -2 }, new TXY { X = -1, Y = -2 }, new TXY { X = -2, Y = -2 }, new TXY { X = 9, Y = 9 } };
        static TXY[] westLeft = new TXY[] { new TXY { X = 2, Y = 0 }, new TXY { X = 2, Y = -1 }, new TXY { X = 9, Y = 9 } };
        static TXY[] westRight = new TXY[] { new TXY { X = -2, Y = 0 }, new TXY { X = -2, Y = -1 }, new TXY { X = 9, Y = 9 } };
        static TXY[] westProx = new TXY[] { new TXY { X = 1, Y = 0 }, new TXY { X = 1, Y = -1 }, new TXY { X = 0, Y = -1 }, new TXY { X = -1, Y = -1 }, new TXY { X = -1, Y = 0 }, new TXY { X = 9, Y = 9 } };
    }
}