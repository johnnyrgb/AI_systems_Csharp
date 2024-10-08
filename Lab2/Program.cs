using System.Collections;

namespace Lab2
{
    internal class Program
    {
        static void Main(string[] args)
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
                    adjacencyMatrix[i, i + 1] = random.Next(1, 11);
                }

                // Заполняем остальные элементы матрицы случайными весами
                for (int i = 0; i < n; i++)
                {
                    for (int j = i + 2; j < n; j++)
                    {
                        if (random.Next(2) == 1) // С вероятностью 0.5 добавляем ребро
                        {
                            adjacencyMatrix[i, j] = random.Next(1, 11);
                        }
                    }
                }

                return adjacencyMatrix;
            }

            List<ArrayList> GenerateEdgeList(int[,] adjacencyMatrix)
            {
                int n = adjacencyMatrix.GetLength(0);
                List<ArrayList> edgeList = new List<ArrayList>();

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (adjacencyMatrix[i, j] > 0)
                        {
                            ArrayList edge = new ArrayList();
                            edge.Add(i); // Исходящая вершина
                            edge.Add(j); // Входящая вершина
                            edge.Add(adjacencyMatrix[i, j]); // Вес ребра
                            edgeList.Add(edge);
                        }
                    }
                }

                return edgeList;
            }

            int[,] matrix = GenerateAdjacencyMatrix(10); // Создаем матрицу для графа с 5 вершинами
            List<ArrayList> edgeList = GenerateEdgeList(matrix);
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
            foreach (ArrayList edge in edgeList)
            {
                Console.WriteLine("Исходная вершина: " + edge[0] + ", Входящая вершина: " + edge[1] + ", Вес: " + edge[2]);
            }


        }


    }
}
