using Newtonsoft.Json;
using System;
using System.Text.Json.Nodes;

namespace Lab4
{
    public class AntColony
    {
        private int _numCities; // КОЛИЧЕСТВО ГОРОДОВ
        private int[,] _capacities; // РАССТОЯНИЯ МЕЖДУ ГОРОДАМИ
        private double[,] _pheromones; // ФЕРОМОНЫ МЕЖДУ ГОРОДАМИ
        private Random _random = new Random();

        private const double Alpha = 1.0; // Параметр для влияния феромонов
        private const double Beta = 2.0; // Параметр для влияния расстояния
        private const double EvaporationRate = 0.5; // Коэффициент испарения феромонов (Rho)
        private const double Q = 10.0; // Константа для обновления феромонов

        public AntColony(int numCities, int[,] capacities)
        {
            _capacities = capacities;
            _numCities = numCities;
            _pheromones = new double[numCities, numCities];
            InitializePheromones();
        }

        private void InitializePheromones()
        {
            for (int i = 0; i < _numCities; i++)
                for (int j = 0; j < _numCities; j++)
                    _pheromones[i, j] = 1.0;
        }

        public (List<int> bestTour, double bestLength) Solve(int numAnts, int maxIterations)
        {
            List<int> bestTour = null; // Лучший маршрут
            double bestFlow = double.MinValue; // Длина лучшего маршрута

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                Console.WriteLine($"\n====================\nИтерация {iteration + 1}");
                List<List<int>> allTours = new List<List<int>>(); // Список маршрутов всех муравьев
                List<double> allTourFlows = new List<double>(); // Список длин маршрутов всех муравьев

                for (int ant = 0; ant < numAnts; ant++)
                {
                    var tour = GenerateTour(); // Генерация маршрута для муравья
                    double tourFlow = CalculateTourFlow(tour); // Расчет длины маршрута муравья

                    allTourFlows.Add(tourFlow);
                    allTours.Add(tour);

                    if (tourFlow > bestFlow)
                    {
                        bestFlow = tourFlow;
                        bestTour = new List<int>(tour);
                    }
                    
                    Console.WriteLine($"\nМуравей {ant}");
                    foreach (var city in tour)
                    {
                        
                        Console.Write($"{city} ");
                    }
                    Console.Write($"| {tourFlow}");  

                }

                UpdatePheromones(allTours, allTourFlows); // Обновление феромонов на итерации
                Console.WriteLine($"Итерация {iteration + 1}, Лучшая длина {bestFlow}");
                foreach (var city in bestTour)
                    Console.Write($"{city} ");
            }
            return (bestTour, bestFlow);

        }

        private List<int> GenerateTour()
        {
            List<int> tour = new List<int> { 0 };
            HashSet<int> visited = new HashSet<int>(tour); // Множество посещенных вершин

            while (tour.Last() != _numCities - 1)
            {
                int lastCity = tour.Last();
                int nextCity = SelectNextCity(lastCity, visited);
                tour.Add(nextCity);
                visited.Add(nextCity);
            }

            return tour;
        }

        private int SelectNextCity(int currentCity, HashSet<int> visited)
        {
            double[] probabilities = new double[_numCities]; // Вероятности похода по ребру
            double sum = 0.0;

            for (int i = currentCity; i < _numCities; i++)
            {
                if (visited.Contains(i) || _capacities[currentCity, i] == 0) continue;

                double pheromone = Math.Pow(_pheromones[currentCity, i], Alpha); // Влияние феромона на выбор ребра
                double distance = Math.Pow(_capacities[currentCity, i], Beta); // Влияние длины ребра на выбор ребра
                probabilities[i] = pheromone * distance; // Числитель
                sum += probabilities[i]; // Знаменатель
            }

            double randomValue = _random.NextDouble() * sum; // Случайное значение для выбора случайного ребра
            double cumulative = 0.0; // Сумма для выбора случайного ребра

            for (int i = currentCity; i < _numCities; i++)
            {
                if (visited.Contains(i)) continue;

                cumulative += probabilities[i]; // Прибавляем к сумме вероятность похода в ребро
                if (cumulative >= randomValue) // Если сумма стала больше или равна случайному значению, то выбираем это ребро
                    return i;
            }
            return -1;
        }

        private double CalculateTourFlow(List<int> tour)
        {
            double flow = Double.MaxValue;
            
            for (int i = 0; i < tour.Count - 1; i++) // Считаем длину пути
            {
                if (_capacities[tour[i], tour[i + 1]] < flow)
                    flow = _capacities[tour[i], tour[i + 1]];
            }
            return flow;
        }

        private void UpdatePheromones(List<List<int>> allTours, List<double> allTourFlows)
        {
            // Испарение феромонов
            for (int i = 0; i < _numCities; i++)
                for (int j = 0; j < _numCities; j++)
                    _pheromones[i, j] *= (1.0 - EvaporationRate);

            // Добавление новых феромонов в зависимости от пройденных маршрутов
            for (int ant = 0; ant < allTours.Count; ant++)
            {
                var tour = allTours[ant]; // Маршрут муравья
                double tourFlow = allTourFlows[ant]; // Поток маршрута муравья
                double pheromoneContribution = tourFlow / Q; // Распределение феромона на каждое ребро

                for (int i = 0; i < tour.Count - 1; i++)
                    _pheromones[tour[i], tour[i + 1]] += pheromoneContribution; // Мажем феромон на ребра

            }
        }

    }

    public class Program
    {
        public static int[,] GenerateAdjacencyMatrix(int n)
        {
            // Создаем матрицу смежности размером n x n и заполняем ее нулями
            int[,] adjacencyMatrix = new int[n, n];

            // Случайный объект для генерации весов
            Random random = new Random();

            // Гарантируем связность, соединяя каждую вершину с последующей
            for (int i = 0; i < n - 1; i++)
            {
                adjacencyMatrix[i, i + 1] = random.Next(1, 100);
            }

            // Заполняем остальные элементы матрицы случайными весами
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    if (random.Next(2) == 1 && i != j && j != i + 1 && i != n - 1 && j != 0) // С вероятностью 0.5 добавляем ребро
                    {
                        adjacencyMatrix[i, j] = random.Next(1, 100);
                    }
                }
            }

            return adjacencyMatrix;
        }
        public static void Main()
        {
            int numCities = 30;
            int[,] capacities = GenerateAdjacencyMatrix(numCities);
            for (int i = 0; i < numCities; i++)
            {
                for (int j = 0; j < numCities; j++)
                {
                    Console.Write($"{capacities[i, j]} ");
                }
                Console.Write("\n");
            }
            File.WriteAllText("capacities.json", JsonConvert.SerializeObject(capacities));
            var antColony = new AntColony(numCities, capacities);
            var result = antColony.Solve(numAnts: 10, maxIterations: 100);

            Console.WriteLine("Найден максимальный поток");
            foreach (var city in result.bestTour)
                Console.Write($"{city} ");

            Console.WriteLine($"\nПоток маршрута: {result.bestLength}");
        }
    }
}
