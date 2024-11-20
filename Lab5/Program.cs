using System;

namespace Lab5
{
    class Program
    {
        const int MaxItems = 11; // Максимальное количество товаров
        const int MaxClients = 10; // Максимальное количество клиентов
        const int MaxClusters = 5; // Максимальное количество кластеров
        const double Beta = 1.0; // Параметр для расчета сходства
        const double Rho = 0.9; // Порог сходства для определения, можно ли добавить клиента в кластер

        int[][] clusters = new int[MaxClusters][]; // Прототипы кластеров
        int[][] vectorSum = new int[MaxClusters][]; // Сумма всех векторов внутри кластера (популярность элемента в кластере)
        int[] members = new int[MaxClusters]; // Количество членов в каждом кластере
        int[] group = new int[MaxClients]; // Индекс кластера для каждого клиента
        int N; // Общее количество кластеров
        readonly string[] ItemName =
        {
            "Молоток", "Бумага", "Шоколадка", "Отвёртка", "Ручка",
            "Кофе", "Гвоздодёр", "Карандаш", "Конфеты", "Дрель", "Дырокол"
        };

        int[][] Data = // Покупки клиентов, 1 - купил, 0 - не купил
        {
            new int[] {0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0 },
            new int[] {0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1 },
            new int[] {0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0 },
            new int[] {0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1 },
            new int[] {1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0 },
            new int[] {0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1 },
            new int[] {1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
            new int[] {0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0 },
            new int[] {0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0 },
            new int[] {0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0 },
        };

        void Initialize()
        {
            clusters = new int[MaxClusters][]; // Инициализация массива кластеров

            for (int i = 0; i < MaxClusters; i++)
            {
                clusters[i] = new int[MaxItems]; // Иницализация каждого кластера
            }

            vectorSum = new int[MaxClusters][]; // Иниализация массива суммы векторов

            for (int i = 0; i < MaxClusters; i++)
            {
                vectorSum[i] = new int[MaxItems];
            }

            members = new int[MaxClusters];
            group = new int[MaxClients];
            N = 0; // Начальное количество кластеров

            for (int i = 0; i < MaxClusters; i++) // Заполнение нулями
            {
                for (int j = 0; j < MaxItems; j++)
                {
                    clusters[i][j] = 0;
                    vectorSum[i][j] = 0;
                }
                members[i] = 0;
            }

            for (int i = 0; i < MaxClients; i++) // -1 означает, что клиент еще не принадлежит никакому кластеру
            {
                group[i] = -1;
            }
        }

        static int[] AndVectors(int[] V, int[] W) // Выполенение логического И для двух векторов
        {
            int[] resultVector = new int[MaxItems];
            for (int i = 0; i < MaxItems; i++)
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
        
        // Обновление данных о кластере с индексом K на основе данных клиентов, принадлежащих этому кластеру
        void UpdateVectors(int clusterIndex)
        {
            bool isFirstCustomer = true;

            if (clusterIndex < 0 || clusterIndex > MaxClusters)
            {
                return;
            }

            for (int i = 0; i < MaxItems; i++) // Сброс данных кластера
            {
                clusters[clusterIndex][i] = 0;
                vectorSum[clusterIndex][i] = 0;
            }

            for (int i = 0; i < MaxClients; i++) // Перебор клиентов
            {
                if (group[i] == clusterIndex) // Принадлежит ли клиент i кластеру K
                { // Если клиент принадлежит кластеру, то его данные используются для обновления информации
                    if (isFirstCustomer) // Если это первый клиент кластера
                    {
                        clusters[clusterIndex] = (int[])Data[i].Clone(); // Прототип кластера = вектор клиента
                        vectorSum[clusterIndex] = (int[])Data[i].Clone(); // Сумма = вектор клиента
                        isFirstCustomer = false; // Первый клиент обработан
                    }
                    else
                    {
                        clusters[clusterIndex] = AndVectors(clusters[clusterIndex], Data[i]); // Накопление логических И векторов клиентов текущего кластера
                        for (int j = 0; j < MaxItems; j++)
                            vectorSum[clusterIndex][j] += Data[i][j]; // Добавляем покупки клиента к сумме
                    }
                }
            }
        }

        int CreateVector(int[] vector) // Создание нового кластера на основе вектора
        {
            int i = -1; // Возвращается -1, если новый кластер не создан (нет места)

            do // Поиск свободного кластера -- количество членов = 0
            {
                i++;
                if (i >= MaxClusters)
                {
                    return -1;
                }
            }
            while (members[i] != 0);

            N++; // Прибавляем 1 к общему количеству кластеров
            clusters[i] = (int[])vector.Clone(); // Копируем вектор в новый кластер
            members[i] = 1; // Теперь 1 участник
            return i;
        }

        static int OnesVector(int[] V)
        {
            int k = 0;
            for (int j = 0; j < MaxItems; j++)
            {
                if (V[j] == 1)
                {
                    k++;
                }
            }
            return k;
        }

        void ExecuteART1() // Основной алгоритм
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
                for (int i = 0; i < MaxClients; i++) // Перебор клиентов
                {
                    for (int j = 0; j < MaxClusters; j++) // Перебор кластеров
                    {
                        if (members[j] > 0) // Если кластер есть
                        {
                            R = AndVectors(Data[i], clusters[j]); // Пересечение покупок текущего клиента с кластером
                            PE = OnesVector(R); // Количество единиц в пересечении
                            P = OnesVector(clusters[j]); // Количество единиц в текущем кластере
                            E = OnesVector(Data[i]); // Количество единиц в покупках текущего клиента
                            Test = PE / (Beta + P) > E / (Beta + MaxItems); // Тест на схожесть

                            if (Test)
                            {
                                Test = PE / E < Rho; // Тест на внимательность (проверка на резонанс)
                            }

                            if (Test)
                            {
                                Test = group[i] != j; // Убедиться, что покупатель еще не в этом кластере
                            }

                            if (Test)
                            {
                                s = group[i]; // Сохраняем индекс
                                group[i] = j; // Запихиваем клиента в этот кластер
                                if (s > 0)
                                {
                                    members[s]--; // Уменьшаем количество в старом кластере
                                    if (members[s] == -1)
                                    {
                                        N--; // Если кластер становится пустым, то уменьшаем общее количество кластеров
                                    }
                                }
                                members[j]++; // Увеличиваем количество в новом кластере
                                UpdateVectors(s); // Обновляем старый кластер
                                UpdateVectors(j); // Обновляем новый кластер
                                exit = false;
                                break;
                            }
                        }
                    }

                    if (group[i] == -1)
                    {
                        group[i] = CreateVector(Data[i]); // Создаем новый кластер
                        exit = false;
                    }
                }
                iterationCount--;
            }
            while (!exit && iterationCount != 0);
        }

        void ShowClusters()
        {
            for (int i = 0; i < N; i++)
            {
                Console.Write($"Вектор-прототип {i} : ");
                for (int j = 0; j < MaxItems; j++)
                {
                    Console.Write($"{clusters[i][j]} ");
                }
                Console.WriteLine();
                for (int k = 0; k < MaxClients; k++)
                {
                    if (group[k] == i)
                    {
                        Console.Write($"Покупатель {k} :      ");
                        for (int j = 0; j < MaxItems; j++)
                        {
                            Console.Write($"{Data[k][j]} ");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }

        void MakeAdvise(int custoremIndex) // Даем рекомендации клиенту
        {
            int best = -1; // Индекс лучшего элемента для рекомендации
            int max = 0; // Максимальное значение популярности элемента

            for (int i = 0; i < MaxItems; i++) // Нахождение лучшего элемента для рекомендации
            {
                if (Data[custoremIndex][i] == 0 && vectorSum[group[custoremIndex]][i] > max)
                {
                    best = i;
                    max = vectorSum[group[custoremIndex]][i];
                }
            }
            Console.Write($"Для покупателя {custoremIndex} ");
            if (best >= 0)
            {
                Console.WriteLine($"есть рекомендация - {ItemName[best]}");
            }

            else
            {
                Console.WriteLine("нет рекомендаций");
            }

            Console.Write("Уже куплено: ");
            for (int i = 0; i < MaxItems; i++)
            {
                if (Data[custoremIndex][i] != 0)
                {
                    Console.Write($"{ItemName[i]} ");
                }
            }
            Console.WriteLine();
        }

        public void Run()
        {
            Initialize();
            ExecuteART1();
            ShowClusters();

            for (int customer = 0; customer < MaxClients; customer++)
            {
                MakeAdvise(customer);
            }
        }
        public static void Main()
        {
            Program program = new();
            program.Run();
        }
    }
}