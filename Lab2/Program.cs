using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Lab2
{
    #region Solution
    [Serializable]
    public class Solution
    {
        public double Energy { get; set; }
        public List<int> Vector { get; set; }

        public Solution(List<int> vector)
        {
            this.Vector = vector;
        }

        private void DefineEnergy()
        {
            int energy = 0;
            for (int i = 0; i < this.Vector.Count - 1; i++)
            {
                for (int j = i + 1; j < this.Vector.Count; j++)
                {
                    if (Math.Abs(this.Vector[i] - this.Vector[j]) == Math.Abs(i - j))
                    {
                        energy++;
                    }
                }
            }
            this.Energy = energy;
        }

        public void Swap()
        {
            Random random = new Random();
            int first = random.Next(0, this.Vector.Count);
            int second;
            do
            {
                second = random.Next(0, this.Vector.Count);
            } while (second == first);
            var buffer = this.Vector[first];
            this.Vector[first] = this.Vector[second];
            this.Vector[second] = buffer;
        }

        public void Show()
        {
            List<List<char>> result = new List<List<char>>();
            foreach (var queen in this.Vector)
            {
                List<char> row = Enumerable.Repeat('_', this.Vector.Count).ToList();
                row[queen] = 'Q';
                result.Add(row);
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
    #endregion




    #region Program class
    internal class Program
    {
        #region Constants
        const int N = 10;
        const double INITIAL_TEMPERATURE = 30.0;
        const double FINAL_TEMPERATURE = 0.5;
        const double ALFA = 0.97;
        const int ITERATION_COUNT = 100;
        #endregion

        private static Random random = new Random();

        static void Main(string[] args)
        {
            Solution currentSolution = new Solution(Enumerable.Range(0, N).ToList());
            Solution workingSolution = currentSolution.DeepCopy();
            Solution bestSolution = currentSolution.DeepCopy();
            double T = INITIAL_TEMPERATURE;

            double EvaluateSolution(Solution workingSolution, Solution currentSolution, double temperature)
            {
                return Math.Exp(-(workingSolution.Energy - currentSolution.Energy) / temperature);
            }

            while (T > FINAL_TEMPERATURE && bestSolution.Energy != 0)
            {
                for (int i = 0; i < ITERATION_COUNT; i++)
                {
                    workingSolution.Swap();
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
                T *= ALFA;
            }

        }
    }
    #endregion
}
