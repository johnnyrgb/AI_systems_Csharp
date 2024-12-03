using System.Runtime.Serialization;
using System.Text.Json.Nodes;
using System.Text.Json;
using Newtonsoft.Json;

namespace Lab8
{
    internal class Program
    {
        public class Population
        {
            // Особь
            public class Individual
            {
                public List<int> Path { get; set; } = new List<int>(); // Путь особи, содержащий вершины
                public double Flow { get; set; } = 0.0; // Поток особи
            }

            public int[,] Graph { get; set; } // Граф
            public int Source { get; set; } // Исток
            public int Sink { get; set; } // Сток
            public List<Individual> Individuals { get; set; } = new List<Individual>(); // Популяция (список особей)
            public int Size { get; set; }

            public Population(int[,] graph, int source, int sink, int size)
            {
                Graph = graph;
                Source = source;
                Sink = sink;
                Size = size;
            }

            // Генерация начальной популяции, состоящей из случайных путей
            public void Initialize()
            {
                Random random = new Random();
                for (int i = 0; i < Size; i++)
                {
                    var path = GenerateRandomPath(Source, Sink, random);
                    var flow = EvaluatePath(path);
                    Individuals.Add(new Individual { Path = path, Flow = flow });
                }
                Individuals = Individuals.OrderByDescending(ind => ind.Flow).ToList(); // Сортировка особей по убыванию потока
            }

            // Генерация случайного пути из истока в сток
            private List<int> GenerateRandomPath(int source, int sink, Random random)
            {
                List<int> path;
                do
                {
                    path = new List<int> { source }; // Начинаем путь с истока
                    int current = source;
                    while (current != sink) // Пока текущая вершина не сток
                    {
                        var neighbors = Enumerable.Range(0, Graph.GetLength(0))
                            .Where(i => Graph[current, i] > 0 && !path.Contains(i))
                            .ToList(); // Выбираем соседей текущей вершины

                        if (!neighbors.Any()) // Если соседей нет
                            break; // Прерываем построение пути

                        current = neighbors[random.Next(neighbors.Count)]; // Случайный выбор из соседей
                        path.Add(current); // Добавляем вершину в путь
                    }
                } while (!path.Contains(sink)); // Повторяем, пока путь не включает сток

                return path;
            }

            // Подсчет максимального потока для пути
            private double EvaluatePath(List<int> path)
            {
                double flow = double.MaxValue;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    flow = Math.Min(flow, Graph[path[i], path[i + 1]]);
                }
                return flow;
            }

            public void GetSolution(int iterations)
            {
                Random random = new Random();
                for (int iter = 0; iter < iterations; iter++)
                {
                    // Селекция методом рулетки

                    //var flowSums = new List<double>();
                    //for (var i = 0; i < this.Individuals.Count; i++)
                    //{
                    //    flowSums.Add(0);
                    //    for (var j = 0; j <= i; j++)
                    //    {
                    //        flowSums[i] += Individuals[j].Flow;
                    //    }
                    //}


                    //var rand = new Random().NextDouble() * flowSums.First() + (flowSums.Last() - flowSums.First());
                    //var borderIndex = 0;
                    //while (flowSums[borderIndex] <= rand) borderIndex++;
                    //var selected = Individuals.GetRange(0, borderIndex);


                    // Селекция без рулетки
                    var selected = Individuals.Take(Size / 2).ToList();

                    // Скрещивание
                    var children = new List<Individual>();
                    while (children.Count + selected.Count < Size)
                    {
                        var parent1 = selected[random.Next(selected.Count)];
                        var parent2 = selected[random.Next(selected.Count)];
                        var childPath = Crossover(parent1.Path, parent2.Path);
                        var childFlow = EvaluatePath(childPath);
                        children.Add(new Individual { Path = childPath, Flow = childFlow });
                    }

                    // Мутация
                    foreach (var child in children)
                    {
                        if (random.NextDouble() < 0.7)
                        {
                            Mutate(child, random);
                        }
                    }
                    // Формируем новую популяцию детей и родителей
                    Individuals = selected.Concat(children).OrderByDescending(ind => ind.Flow).ToList();
                }
            }

            
            private List<int> Crossover(List<int> path1, List<int> path2)
            {
                // Пересечение путей
                int commonNode = path1.Intersect(path2).Skip(1).FirstOrDefault(); // Находим первый общий узел
                if (commonNode == 0) return path1; // Если нет общих узлов
                // Берем часть первого пути до общего узла, берем часть второго пути после общего узла
                var newPath = path1.TakeWhile(x => x != commonNode)
                    .Concat(path2.SkipWhile(x => x != commonNode))
                    .ToList();
                return newPath;
            }

            private void Mutate(Individual individual, Random random)
            {
                // Убедимся, что длина пути позволяет провести мутацию
                if (individual.Path.Count > 2)
                {
                    // Выбираем случайный индекс в пределах внутренних вершин пути
                    int index = random.Next(1, individual.Path.Count - 1);
                    int iterations = 0;
                    int newNode;
                    do
                    {
                        // Выбираем случайный узел в графе
                        newNode = random.Next(0, Graph.GetLength(0));
                        iterations++;
                    }
                    while (
                        (!(Graph[individual.Path[index - 1], newNode] > 0) || // Проверяем существование ребра от предыдущей вершины
                         !(Graph[newNode, individual.Path[index + 1]] > 0) || // Проверяем существование ребра до следующей вершины
                        individual.Path.Contains(newNode)) && iterations != 1000 // Убедимся, что новый узел отсутствует в пути
                    );

                    // Заменяем вершину на новую
                    individual.Path[index] = newNode;

                    // Пересчитываем значение потока
                    individual.Flow = EvaluatePath(individual.Path);
                }
            }
        }


        public static void Main(string[] args)
        {
            int[,] GenerateAdjacencyMatrix(int n)
            {
                // Создаем матрицу смежности размером n x n и заполняем ее нулями
                int[,] adjacencyMatrix = new int[n, n];

                // Случайный объект для генерации весов
                Random random = new Random();

                // Гарантируем связность, соединяя каждую вершину с последующей
                for (int i = 0; i < n - 1; i++)
                {
                    adjacencyMatrix[i, i + 1] = random.Next(1, 40);
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

            // Пример графа в виде матрицы смежности (0 - отсутствие ребра)
            int[,] matrix = JsonConvert.DeserializeObject<int[,]>(File.ReadAllText("matrix.json"));
            //int[,] matrix = GenerateAdjacencyMatrix(100);
            // Выводим матрицу на консоль (для примера)
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i, j] + " ");
                }
                Console.WriteLine();
            }

            File.WriteAllText("matrix.json", JsonConvert.SerializeObject(matrix)); // Запись в файл

            int source = 0; // Исток
            int sink = 100 - 1;   // Сток
            int populationSize = 15; // Размер популяции
            int iterations = 1000; // Количество итераций генетического алгоритма

            // Создание популяции
            Population population = new Population(matrix, source, sink, populationSize);
            population.Initialize();

            Console.WriteLine("Изначальная популяция:");
            foreach (var individual in population.Individuals)
            {
                Console.WriteLine($"Путь: {string.Join(" -> ", individual.Path)}, Поток: {individual.Flow}");
            }

            // Запуск генетического алгоритма с выводом лучших результатов
            Console.WriteLine("\nИтерации генетического алгоритма:");
            for (int iter = 0; iter < iterations; iter++)
            {
                population.GetSolution(1); // Выполняем одну итерацию
                var bestIndividual = population.Individuals.First();
                Console.WriteLine($"Итерация {iter + 1}: Лучший путь: {string.Join(" -> ", bestIndividual.Path)}, Поток: {bestIndividual.Flow}");
            }

            // Финальный результат
            Console.WriteLine("\nРезультат после выполнения генетического алгоритма:");
            var finalBestIndividual = population.Individuals.First();
            Console.WriteLine($"Лучший путь: {string.Join(" -> ", finalBestIndividual.Path)}");
            Console.WriteLine($"Максимальный поток: {finalBestIndividual.Flow}");
        }
    }
}
