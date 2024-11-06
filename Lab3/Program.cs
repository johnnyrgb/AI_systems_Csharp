using Newtonsoft.Json;
using System;
using System.Text.Json.Nodes;

namespace Lab3
{
    public class AntColony
    {
        private int _numCities; // КОЛИЧЕСТВО ГОРОДОВ
        private double[,] _distances; // РАССТОЯНИЯ МЕЖДУ ГОРОДАМИ
        private double[,] _pheromones; // ФЕРОМОНЫ МЕЖДУ ГОРОДАМИ
        private Random _random = new Random();

        private const double Alpha = 1.0; // Параметр для влияния феромонов
        private const double Beta = 1.0; // Параметр для влияния расстояния
        private const double EvaporationRate = 0.2; // Коэффициент испарения феромонов (Rho)
        private const double Q = 100.0; // Константа для обновления феромонов

        public AntColony(int numCities, double[,] distances)
        {
            _distances = distances;
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
            double bestLength = double.MaxValue; // Длина лучшего маршрута

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                List<List<int>> allTours = new List<List<int>>(); // Список маршрутов всех муравьев
                List<double> allTourLengths = new List<double>(); // Список длин маршрутов всех муравьев

                for (int ant = 0; ant < numAnts; ant++)
                {
                    var tour = GenerateTour(); // Генерация маршрута для муравья
                    double tourLength = CalculateTourLength(tour); // Расчет длины маршрута муравья

                    allTourLengths.Add(tourLength);
                    allTours.Add(tour);

                    if (tourLength < bestLength) 
                    {
                        bestLength = tourLength;
                        bestTour = new List<int>(tour);
                    }

                }

                UpdatePheromones(allTours, allTourLengths); // Обновление феромонов на итерации
                Console.WriteLine($"Итерация {iteration + 1}, Лучшая длина {bestLength}");
            }
            return (bestTour, bestLength);

        }

        private List<int> GenerateTour()
        {
            List<int> tour = new List<int> { _random.Next(_numCities) };
            HashSet<int> visited = new HashSet<int>(tour); // Множество посещенных вершин

            while (tour.Count < _numCities)
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

            for (int i = 0; i < _numCities; i++)
            {
                if (visited.Contains(i)) continue;

                double pheromone = Math.Pow(_pheromones[currentCity, i], Alpha); // Влияние феромона на выбор ребра
                double distance = Math.Pow(1.0 / _distances[currentCity, i], Beta); // Влияние длины ребра на выбор ребра
                probabilities[i] = pheromone * distance; // Числитель
                sum += probabilities[i]; // Знаменатель
            }

            double randomValue = _random.NextDouble() * sum; // Случайное значение для выбора случайного ребра
            double cumulative = 0.0; // Сумма для выбора случайного ребра

            for (int i = 0; i < _numCities; i++)
            {
                if (visited.Contains(i)) continue;

                cumulative += probabilities[i]; // Прибавляем к сумме вероятность похода в ребро
                if (cumulative >= randomValue) // Если сумма стала больше или равна случайному значению, то выбираем это ребро
                    return i;
            }
            return -1;
        }

        private double CalculateTourLength(List<int> tour)
        {
            double length = 0.0;

            for (int i = 0; i < tour.Count - 1; i++) // Считаем длину пути
                length += _distances[tour[i], tour[i + 1]];

            length += _distances[tour.Last(), tour.First()]; // Добавляем длину пути в начало
            return length;
        }

        private void UpdatePheromones(List<List<int>> allTours, List<double> allTourLengths)
        {
            // Испарение феромонов
            for (int i = 0; i < _numCities; i++)
                for (int j = 0; j < _numCities; j++)
                    _pheromones[i, j] *= (1.0 - EvaporationRate);

            // Добавление новых феромонов в зависимости от пройденных маршрутов
            for (int ant = 0; ant < allTours.Count; ant++)
            {
                var tour = allTours[ant]; // Маршрут муравья
                double tourLength = allTourLengths[ant]; // Длина маршрута муравья
                double pheromoneContribution = Q / tourLength; // Распределение феромона на каждое ребро

                for (int i = 0; i < tour.Count - 1; i++)
                    _pheromones[tour[i], tour[i + 1]] += pheromoneContribution; // Мажем феромон на ребра

                _pheromones[tour.Last(), tour.First()] += pheromoneContribution; // Мажем феромон на путь в начало
            }
        }

    }

    public class Program
    {
        public static double[,] GenerateSymmetricDistanceMatrix(int numCities, int maxDistance = 20)
        {
            var random = new Random();
            double[,] distances = new double[numCities, numCities];

            for (int i = 0; i < numCities; i++)
            {
                for (int j = i + 1; j < numCities; j++)
                {
                    // Генерируем случайное расстояние между городами i и j
                    double distance = random.Next(1, maxDistance);

                    // Устанавливаем одинаковое расстояние в обоих направлениях для симметрии
                    distances[i, j] = distance;
                    distances[j, i] = distance;
                }
                // Расстояние до самого себя равно 0
                distances[i, i] = 0;
            }
            return distances;
        }
        public static void Main()
        {
            int numCities = 30;
            double[,] distances = JsonConvert.DeserializeObject<double[,]>(File.ReadAllText("distances.json"));
            File.WriteAllText("distances.json", JsonConvert.SerializeObject(distances)); 
            var antColony = new AntColony(numCities, distances);
            var result = antColony.Solve(numAnts: 10, maxIterations: 30000);

            Console.WriteLine("Best tour found:");
            foreach (var city in result.bestTour)
                Console.Write($"{city} ");

            Console.WriteLine($"\nTour length: {result.bestLength}");
        }
    }
}
