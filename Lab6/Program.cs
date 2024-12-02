namespace Lab6
{
    public class Program
    {
        private int[][] _clusters = new int[MaxClusters][];
        private int[][] _vectorSum = new int[MaxClusters][];
        private int[] _members = new int[MaxClusters];
        private int[] _group = new int[MaxCars];
        private int _clustersNumber;

        private const int MaxNodes = 8;
        private const int MaxCars = 10; // Максимальное количество автомобилей
        private const int MaxClusters = 4; // Максимальное количество кластеров
        private const double Beta = 1.0;
        private const double Rho = 0.9;

        private readonly int[][] _edges = // Матрица весов графа
        [
            [0, 10, 11, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 11, 10, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 6, 10, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 8, 0, 0, 0],
            [0, 0, 0, 0, 0, 0, 7, 8, 0, 0],
            [0, 0, 0, 0, 0, 0, 0, 9, 7, 0],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 6],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 7],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
        ];

        private readonly int[][] _data =
        [
            [0, 1, 0, 0, 1, 0, 0, 0],
            [0, 0, 0, 0, 1, 0, 0, 0],
            [0, 1, 0, 0, 1, 0, 0, 1],
            [1, 0, 1, 0, 0, 1, 0, 0],
            [1, 0, 1, 0, 0, 0, 0, 0],
            [0, 0, 1, 0, 0, 1, 0, 0],
            [0, 1, 0, 1, 0, 0, 0, 0],
            [0, 1, 0, 0, 0, 1, 0, 0],
            [0, 1, 0, 1, 0, 1, 0, 0],
            [0, 0, 0, 1, 0, 1, 0, 0]
        ];

        private void Initialize()
        {
            _clusters = new int[MaxClusters][]; // Инициализация массива кластеров

            for (int i = 0; i < MaxClusters; i++)
            {
                _clusters[i] = new int[MaxNodes]; // Инициализация каждого кластера
            }

            _vectorSum = new int[MaxClusters][]; // Инициализация массива суммы векторов

            for (int i = 0; i < MaxClusters; i++)
            {
                _vectorSum[i] = new int[MaxNodes];
            }

            _members = new int[MaxClusters];
            _group = new int[MaxCars];
            _clustersNumber = 0; // Начальное количество кластеров

            for (int i = 0; i < MaxClusters; i++) // Заполнение нулями
            {
                for (int j = 0; j < MaxNodes; j++)
                {
                    _clusters[i][j] = 0;
                    _vectorSum[i][j] = 0;
                }

                _members[i] = 0;
            }

            for (int i = 0; i < MaxCars; i++) // -1 означает, что клиент еще не принадлежит никакому кластеру
            {
                _group[i] = -1;
            }
        }

        private static int[] AndVectors(int[] V, int[] W) // Выполнение логического И для двух векторов
        {
            int[] resultVector = new int[MaxNodes];
            for (int i = 0; i < MaxNodes; i++)
            {
                if (W[i] != 1 || V[i] != 1)
                {
                    resultVector[i] = 0;
                }

                else
                {
                    resultVector[i] = 1;
                }
            }

            return resultVector;
        }

        private static int OnesVector(int[] V) // Подсчет количества единиц в векторе
        {
            int k = 0;
            for (int j = 0; j < MaxNodes; j++)
            {
                if (V[j] == 1)
                {
                    k++;
                }
            }

            return k;
        }

        private void UpdateVectors(int clusterIndex)
        {
            bool isFirstCustomer = true;

            if (clusterIndex < 0 || clusterIndex > MaxClusters)
            {
                return;
            }

            for (int i = 0; i < MaxNodes; i++) // Сброс данных кластера
            {
                _clusters[clusterIndex][i] = 0;
                _vectorSum[clusterIndex][i] = 0;
            }

            for (int i = 0; i < MaxCars; i++) // Перебор автомобилей
            {
                if (_group[i] == clusterIndex) // Принадлежит ли авто i кластеру K
                {
                    // Если авто принадлежит кластеру, то его данные используются для обновления информации
                    if (isFirstCustomer) // Если это первый автомобиль кластера
                    {
                        _clusters[clusterIndex] = (int[])_data[i].Clone(); // Прототип кластера = вектор авто
                        _vectorSum[clusterIndex] = (int[])_data[i].Clone(); // Сумма = вектор авто
                        isFirstCustomer = false; // Первый авто обработан
                    }
                    else
                    {
                        _clusters[clusterIndex] =
                            AndVectors(_clusters[clusterIndex],
                                _data[i]); // Накопление логических И векторов авто текущего кластера
                        for (int j = 0; j < MaxNodes; j++)
                            _vectorSum[clusterIndex][j] += _data[i][j]; // Добавляем авто к сумме
                    }
                }
            }
        }

        public int CreateVector(int[] vector) // Создание нового кластера на основе вектора
        {
            int i = -1; // Возвращается -1, если новый кластер не создан (нет места)

            do // Поиск свободного кластера -- количество членов = 0
            {
                i++;
                if (i >= MaxClusters)
                {
                    return -1;
                }
            } while (_members[i] != 0);

            _clustersNumber++; // Прибавляем 1 к общему количеству кластеров
            _clusters[i] = (int[])vector.Clone(); // Копируем вектор в новый кластер
            _members[i] = 1; // Теперь 1 участник
            return i;

        }

        private void ExecuteAlgorithm() // Основной алгоритм
        {
            bool exit; // Флаг завершения
            int iterationCount = 50; // Количество итераций
            int[] R; // Хранение результата побитового И для двух векторов
            int PE; // Количество единиц в пересечении (значимость)
            int P; // Количество единиц вектора-прототипа (значимость)
            int E; // Количество единиц вектора покупателя (значимость)
            bool Test; // Флаг прохождения теста
            int s; // Индекс предыдущего кластера

            do // Цикл продолжается пока exit = false и не достигнуто количество итераций
            {
                exit = true;
                for (int i = 0; i < MaxCars; i++) // Перебор клиентов
                {
                    for (int j = 0; j < MaxClusters; j++) // Перебор кластеров
                    {
                        if (_members[j] > 0) // Если кластер есть
                        {
                            R = AndVectors(_data[i], _clusters[j]); // Пересечение покупок текущего клиента с кластером
                            PE = OnesVector(R); // Количество единиц в пересечении
                            P = OnesVector(_clusters[j]); // Количество единиц в текущем кластере
                            E = OnesVector(_data[i]); // Количество единиц в покупках текущего клиента
                            Test = PE / (Beta + P) > E / (Beta + MaxNodes); // Тест на схожесть

                            if (Test)
                            {
                                Test = PE / E < Rho; // Тест на внимательность (проверка на резонанс)
                            }

                            if (Test)
                            {
                                Test = _group[i] != j; // Убедиться, что покупатель еще не в этом кластере
                            }

                            if (Test)
                            {
                                s = _group[i]; // Сохраняем индекс
                                _group[i] = j; // Запихиваем клиента в этот кластер
                                if (s > 0)
                                {
                                    _members[s]--; // Уменьшаем количество в старом кластере
                                    if (_members[s] == -1)
                                    {
                                        _clustersNumber
                                            --; // Если кластер становится пустым, то уменьшаем общее количество кластеров
                                    }
                                }

                                _members[j]++; // Увеличиваем количество в новом кластере
                                UpdateVectors(s); // Обновляем старый кластер
                                UpdateVectors(j); // Обновляем новый кластер
                                exit = false;
                                break;
                            }
                        }
                    }

                    if (_group[i] == -1)
                    {
                        _group[i] = CreateVector(_data[i]); // Создаем новый кластер
                        exit = false;
                    }
                }

                iterationCount--;
            } while (!exit && iterationCount != 0);
        }


        private void ShowClusters()
        {
            for (int i = 0; i < _clustersNumber; i++)
            {
                Console.Write($"Вектор-прототип {i} : ");
                for (int j = 0; j < MaxNodes; j++)
                {
                    Console.Write($"{_clusters[i][j]} ");
                }

                Console.WriteLine();
                for (int k = 0; k < MaxCars; k++)
                {
                    if (_group[k] == i)
                    {
                        Console.Write($"Автомобиль {k} :      ");
                        for (int j = 0; j < MaxNodes; j++)
                        {
                            Console.Write($"{_data[k][j]} ");
                        }

                        Console.WriteLine();
                    }
                }
            }
        }

        public void FindRoutes(int carIndex)
        {
            int maxFlow = int.MaxValue, i = 1, currentNode = 0;

            Console.Write("Для авто " + (carIndex) + " ");
            Console.Write("Рекомендованный путь: ");
            while (i < _edges.Length - 1)
            {

                if (_vectorSum[_group[carIndex]][i - 1] != 0) // Проверка доступности узла для автомобиля
                {
                    if (_edges[currentNode][i] != 0)
                    {
                        maxFlow = maxFlow > _edges[currentNode][i] ? _edges[currentNode][i] : maxFlow;
                        Console.Write((currentNode) + "-");
                        currentNode = i;
                    }
                    else
                    {
                        Console.WriteLine($"{currentNode}-Нет пути-{i}");
                        break;
                    }

                }

                i++;
            }



            if (_edges[currentNode][_edges.Length - 1] != 0) // Проверка последнего узла на соединение со стоком
            {
                maxFlow = maxFlow > _edges[currentNode][MaxNodes + 1] ? _edges[currentNode][MaxNodes + 1] : maxFlow;
                Console.WriteLine((currentNode) + "-" + (_edges.Length - 1));
                Console.WriteLine("Поток пути - " + maxFlow);
            }
            else Console.WriteLine("Не определён");
        }



        public void Run()
        {
            Initialize();
            ExecuteAlgorithm();
            ShowClusters();

            for (int carIndex = 0; carIndex < MaxCars; carIndex++)
            {
                FindRoutes(carIndex);
            }
        }

        public static void Main()
        {
            Program program = new();
            program.Run();
        }
    }
}
