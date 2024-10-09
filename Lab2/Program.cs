using Newtonsoft.Json;

namespace Lab2
{

    internal class Program
    {
        #region Constants
        private const int N = 10;
        private const double INITIAL_TEMPERATURE = 30.0;
        private const double FINAL_TEMPERATURE = 0.05;
        private const double ALFA = 0.98;
        private const int ITERATION_COUNT = 100;
        #endregion
        public class Solution
        {
            public int Energy { get; set; }
            public List<int[]> Edges { get; set; }
            public int[] Vector { get; set; }

            public Solution(List<int[]> edges)
            {
                this.Edges = edges;
                this.Vector = new int[edges.Count];
                // Генерация начального решения
                for (int i = 0; i < this.Edges.Count; i++)
                {
                    this.Vector[i] = this.Edges[i][2];
                }
                this.DefineEnergy();
            }
            private void DefineEnergy()
            {
                int energy = 0;
                for (int i = 1; i < N - 1; i++)
                {
                    int inputs = 0;
                    int outputs = 0;
                    for (int j = 0; j < this.Edges.Count; j++)
                    {
                        if (this.Edges[j][0] == i)
                        {
                            outputs += this.Vector[j];
                        }
                        else if (this.Edges[j][1] == i)
                        {
                            inputs += this.Vector[j];
                        }
                    }
                    energy += Math.Abs(inputs - outputs);
                }
                this.Energy = energy;
            }

            public void Randomize()
            {
                Random random = new Random();
                int index = random.Next(0, this.Edges.Count);
                this.Vector[index] = random.Next(0, this.Edges[index][2] + 1);
                this.DefineEnergy();
            }

            public void Show()
            {
                for (int i = 0; i < this.Edges.Count; i++)
                {
                    Console.WriteLine($"{this.Edges[i][0]} ---> {this.Edges[i][1]} = {this.Vector[i]}");
                }
            }

            public Solution DeepCopy()
            {
                string json = JsonConvert.SerializeObject(this);
                Solution? copy = JsonConvert.DeserializeObject<Solution>(json);
                if (copy == null)
                {
                    throw new InvalidOperationException();
                }
                return copy;
            }

        }



        private static Random random = new Random();
        static void Main(string[] args)
        {
            double EvaluateSolution(Solution workingSolution, Solution currentSolution, double temperature)
            {
                return Math.Exp(-(workingSolution.Energy - currentSolution.Energy) / temperature);
            }

            int[,] GenerateAdjacencyMatrix(int n)
            {
                // Создаем матрицу смежности размером n x n и заполняем ее нулями
                int[,] adjacencyMatrix = new int[n, n];

                // Случайный объект для генерации весов
                Random random = new Random();

                // Гарантируем связность, соединяя каждую вершину с последующей
                for (int i = 0; i < n - 1; i++)
                {
                    adjacencyMatrix[i, i + 1] = random.Next(1, 11);
                }

                // Заполняем остальные элементы матрицы случайными весами
                for (int i = 0; i < n; i++)
                {
                    for (int j = i; j < n; j++)
                    {
                        if (random.Next(2) == 1 && i != j && j != i + 1 && i != n - 1 && j != 0) // С вероятностью 0.5 добавляем ребро
                        {
                            adjacencyMatrix[i, j] = random.Next(1, 11);
                        }
                    }
                }

                return adjacencyMatrix;
            }

            List<int[]> GenerateEdgeList(int[,] adjacencyMatrix)
            {
                int n = adjacencyMatrix.GetLength(0);
                List<int[]> edgeList = new List<int[]>();

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (adjacencyMatrix[i, j] > 0)
                        {
                            int[] edge = new int[3];
                            edge[0] = i; // Исходящая вершина
                            edge[1] = j; // Входящая вершина
                            edge[2] = adjacencyMatrix[i, j]; // Вес ребра
                            edgeList.Add(edge);
                        }
                    }
                }

                return edgeList;
            }

            int[,] matrix = GenerateAdjacencyMatrix(N); // Создаем матрицу для графа с 5 вершинами
            List<int[]> edgeList = GenerateEdgeList(matrix);
            // Выводим матрицу на консоль (для примера)
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("=================");
            foreach (int[] edge in edgeList)
            {
                Console.WriteLine("Исходная вершина: " + edge[0] + ", Входящая вершина: " + edge[1] + ", Вес: " + edge[2]);
            }

            Solution currentSolution = new Solution(edgeList);
            Solution workingSolution = currentSolution.DeepCopy();
            Solution bestSolution = currentSolution.DeepCopy();
            double T = INITIAL_TEMPERATURE;

            while (T > FINAL_TEMPERATURE && bestSolution.Energy != 0) 
            {
                for (int i = 0; i < ITERATION_COUNT; i++)
                {
                    workingSolution.Randomize();

                    if (workingSolution.Energy <= currentSolution.Energy)
                    {
                        currentSolution = workingSolution.DeepCopy();
                        if (currentSolution.Energy < bestSolution.Energy)
                        {
                            bestSolution = currentSolution.DeepCopy();
                        }
                    }
                    else if (random.NextDouble() < EvaluateSolution(workingSolution, currentSolution, T))
                    {
                        currentSolution = workingSolution.DeepCopy();
                        if (currentSolution.Energy < bestSolution.Energy)
                        {
                            bestSolution = currentSolution.DeepCopy();
                        }
                    }
                    else
                    {
                        workingSolution = currentSolution.DeepCopy();
                    }
                }
                Console.WriteLine($"T = {Math.Round(T, 7)} | Энергия = {bestSolution.Energy}");
                //currentSolution.Show();
                T *= ALFA;
            }
            bestSolution.Show();
        }
    }
}
