namespace Lab7
{
    public class Population
    {
        public struct City
        {
            public string Name { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        public struct Individual // Особь
        {
            public Individual()
            {
                Route = new List<int>()
                {
                    0, 100, 100, 100, 100, 100,
                    100, 100, 100, 100, 100, 100, 0
                };
                Fitness = 0.0;
            }

            public List<int> Route { get; set; } // маршрут (индексы городов)
            public double Fitness { get; set; } // оценочная функция
        }

        public int Size { get; set; } // размер популяции
        public List<City> Cities { get; set; } // список генов популяции (города)
        public List<Individual> Individuals { get; set; } // список особей популяции (пути)

        public Population()
        {
        }

        public Population(int size, List<City> cities)
        {
            Size = size;
            Cities = cities;
        }

        public void Initialize()
        {
            this.Individuals = new List<Individual>(); // Инициализация списка популяции
            for (int i = 0; i < this.Size; i++) // Создание популяции из случайных маршрутов
            {
                List<int> randomRoute =
                    Enumerable.Range(1, Cities.Count() - 1).ToList(); // Исключаем Иваново и маршрута
                int n = randomRoute.Count; // создание случайного маршрута с помощью тасования Фишера-Йетса
                while (n > 1)
                {
                    n--;

                    int k = new Random().Next(0, n + 1);

                    var value = randomRoute[k];
                    randomRoute[k] = randomRoute[n];
                    randomRoute[n] = value;

                }

                randomRoute.Insert(0, 0); // начинаем обход из Иванова
                randomRoute.Add(0); // заканчиваем обход в Иванове
                double fitness = 0.0;
                for (int j = 1; j < randomRoute.Count(); j++) // Считаем оценочную функцию для маршрута
                    fitness += GetDistance(Cities[randomRoute[j]],
                        Cities[randomRoute[j - 1]]);
                Individuals.Add(new Individual
                {
                    Route = randomRoute,
                    Fitness = fitness

                });
                ;
            }

            this.Individuals = this.Individuals.OrderByDescending(item =>
                item.Fitness).ToList(); // Сортируем по убыванию оценочной функции
        }

        public void GetSolution(int iterations)
        {
            for (int iter = 0; iter < iterations; iter++)
            {
                // СЕЛЕКЦИЯ
                List<double>
                    fitnessesSums = new List<double>(); // сопоставление каждому решению суммы оценочных функций
                for (int j = 0; j < this.Individuals.Count(); j++)
                {
                    fitnessesSums.Add(0.0);
                    for (int k = 0; k <= j; k++)

                        fitnessesSums[j] +=
                            this.Individuals[k]
                                .Fitness; // Каждой особи сопоставляется кумулятивная сумма оценочных функций
                }

                double rand = new Random().NextDouble() *
                              (fitnessesSums.Last() - 1.0); // случайное число от 0 до суммы оценочных функций минус 1.0
                int borderIndex = 0;
                while (fitnessesSums[borderIndex] <= rand) // Ищем границу в сумме оценочных функций
                    borderIndex++;
                var sliceIndividuals =
                    this.Individuals.GetRange(borderIndex,
                        Size - borderIndex); // Делаем срез по граничному индексу, в нем хранятся лучшие особи

                // СКРЕЩИВАНИЕ
                int childrenToBorn =
                    this.Size - sliceIndividuals
                        .Count; // Остальных особей нужно заменить новыми детьми, считаем, сколько нужно родить детей
                List<Individual> children = new List<Individual>(); // создание массива детей
                for (int count = 0; count < childrenToBorn; count++) // совокупление родителей childrenToBorn раз
                {
                    Individual parent1 = sliceIndividuals[new Random().Next(0,
                        sliceIndividuals.Count)]; // Родитель 1 (случайный)


                    Individual parent2 = sliceIndividuals[new Random().Next(0,
                        sliceIndividuals.Count)]; // Родитель 2 (случайный)
                    Individual child = new Individual(); // Ребенок-квадробер
                    List<bool> masque = new List<bool>(); // двоичная маска
                    for (int j = 0;
                         j < parent1.Route.Count() - 2;
                         j++) // генерация случайной двоичной маски на 2 меньше длины хромосомы (из-за Иванова)
                        masque.Add(new Random().Next(2) == 0);
                    List<int>
                        nulls = new List<int>(masque.Count); // список генов первого родителя, соотв. нулям в маске
                    for (int i = 1; i < child.Route.Count() - 1; i++)
                    {
                        if (masque[i - 1] == true) // Если элемент маски True, то берем ген первого родителя
                            child.Route[i] = parent1.Route[i];
                        else
                            nulls.Add(parent1
                                .Route[i]); // Сюда записываются гены, которые не попали от первого родителя
                    }

                    nulls = nulls.OrderBy(x => parent2.Route.IndexOf(x)).ToList();
                    // сортировка списка генов как в порядке второго родителя
                    nulls.Reverse();

                    for (int i = 1; i < child.Route.Count() - 1; i++) // Добавляем оставшиеся гены

                    {
                        if (masque[i - 1] == false)
                        {
                            child.Route[i] = nulls.Last();
                            nulls.Remove(nulls.Last());

                        }
                    }

                    // МУТАЦИЯ 15%
                    if (new Random().Next(0, 101) >= 85)
                    {
                        int startIndex = new Random().Next(1, child.Route.Count / 2);
                        int countOfElements = new Random().Next(1, child.Route.Count - startIndex - 1);
                        var shuffle =
                            child.Route.Skip(startIndex).Take(countOfElements).ToList();
                        int n = shuffle.Count;

                        while (n > 1)
                        {
                            n--;

                            int k = new Random().Next(0, n + 1);

                            var value = shuffle[k];
                            shuffle[k] = shuffle[n];
                            shuffle[n] = value;

                        }

                        child.Route.RemoveRange(startIndex, countOfElements);
                        child.Route.InsertRange(startIndex, shuffle);

                    }

                    child.Fitness = 0.0; // подсчет оценочной функции ребенка
                    for (int i = 1; i < child.Route.Count - 1; i++)

                    {
                        child.Fitness += GetDistance(Cities[child.Route[i]],
                            Cities[child.Route[i - 1]]);
                    }

                    children.Add(child);

                }

                this.Individuals.AddRange(children);
                this.Individuals = this.Individuals.OrderByDescending(item =>
                    item.Fitness).ToList();
                this.Individuals = this.Individuals.Skip(this.Individuals.Count -
                                                         Size).ToList();
                //this.Print();
                Console.WriteLine($"Итерация #{iter} | ОФ: {this.Individuals.Last().Fitness}\n");
                foreach (var item in this.Individuals.Last().Route)
                {
                    Console.Write(item + " ");
                }
            }
        }

        private double GetDistance(City city1, City city2) // вычисление дистанции между двумя городами
        {

            int a = city1.X - city2.X;
            int b = city1.Y - city2.Y;
            return Math.Sqrt(a * a + b * b);
        }

        public void Print()
        {
            var sortedIndividuals = Individuals.OrderBy(item =>
                item.Fitness).ToList();
            for (int i = 0; i < 10; i++)
            {
                Console.Write("Особь #" + (i + 1) + " : ");
                for (int ii = 0; ii < sortedIndividuals[i].Route.Count; ii++)
                {
                    Console.Write(sortedIndividuals[i].Route[ii] + " ");
                }

                Console.WriteLine("| " + sortedIndividuals[i].Fitness);
            }
        }
    }

    public class Program
    {
        private const int POPULATION_SIZE = 200;
        private const int ITERATIONS = 500;

        private static void Main(string[] args)
        {
            //List<Population.City> cities = new List<Population.City> // список городов
            //    {
            //        new Population.City {Name = "Ivanovo", X = 500, Y = 500 },
            //        new Population.City {Name = "Shuya", X = 540, Y = 470},
            //        new Population.City {Name = "Furmanov", X = 510, Y = 560},
            //        new Population.City {Name = "Teykovo", X = 440, Y = 470},
            //        new Population.City {Name = "Kineshma", X = 600, Y = 620},
            //        new Population.City {Name = "Kostroma", X = 495, Y = 740},
            //        new Population.City {Name = "Yaroslavl", X = 350, Y = 720},
            //        new Population.City {Name = "Vladimir", X = 410, Y = 260},
            //        new Population.City {Name = "Nizhniy Novgorod", X = 1000, Y = 290},
            //        new Population.City {Name = "Puchezh", X = 870, Y = 500},
            //        new Population.City {Name = "Lukh", X = 610, Y = 510},
            //        new Population.City {Name = "Pereslavl-Zalesskiy", X = 100, Y = 420},
            //        new Population.City {Name = "Rostov", X = 270, Y = 550},
            //        new Population.City {Name = "Gavrilov Posad", X = 380, Y = 370},
            //        new Population.City {Name = "Petushki", X = 280, Y = 220}
            //    };
            List<Population.City> cities = new List<Population.City> // список городов
            {
                new Population.City { Name = "Ivanovo", X = 0, Y = 0 }, //
                new Population.City { Name = "Furmanov", X = 15, Y = 30 },//
                new Population.City { Name = "Kokhma", X = 10, Y = -7 },//
                new Population.City { Name = "Shuya", X = 27, Y = -17 },//
                new Population.City { Name = "Rodniki", X = 53, Y = 12 },//
                new Population.City { Name = "Vichuga", X = 67, Y = 25 },//
                new Population.City { Name = "Teykovo", X = -30, Y = -16 },//
                new Population.City { Name = "Komsomolsk", X = -40, Y = 5 },//
                new Population.City { Name = "Yuzha", X = 73, Y = -55 },//
                new Population.City { Name = "Plyos", X = 40, Y = 57 },//
                new Population.City { Name = "Gavrilov Yam", X = -75, Y = 42 },
                new Population.City { Name = "Gavrilov Posad", X = -57, Y = -50 },
            };
            Population population = new Population(POPULATION_SIZE, cities);
            population.Initialize();
            population.GetSolution(ITERATIONS);
            population.Print();
        }
    }
}

