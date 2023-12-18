using DataProcessing;
using ObjectiveFunction;

namespace GeneticAlgorithm {
    public class GeneticAlgorithmVRP
    {
        private static readonly Random random = new();
        static List<List<int>> GenerateFirstPopulation(List<double[]> idDemands, int populationSize) {
            List<List<int>> population = new();
            int currentPopulationSize = 0;
            

            while (currentPopulationSize < populationSize) {
                List<double[]> idDemandsCopy = new (idDemands);
                int size = idDemandsCopy.Count;
                List<int> route = new();

                for (int i = 0; i < size; i++) {
                    int randomNumber = random.Next(0, idDemandsCopy.Count);

                    route.Add(Convert.ToInt32(idDemandsCopy[randomNumber][0]));
                    idDemandsCopy.RemoveAt(randomNumber);
                    
                }
                population.Add(route);
                currentPopulationSize++;
            }
            return population;
        }

        static List<List<int>> Crossover(List<List<int>> population, double crossoverChance) {
            List<List<int>> newPopulation = new();

            for (int i = 0; i < population.Count - 1; i += 2) {
                int crossoverPoint1 = random.Next(0, population[i].Count - 1);
                int crossoverPoint2 = random.Next(0, population[i].Count - 1);
                int startIndex = Math.Min(crossoverPoint1, crossoverPoint2);
                int endIndex = Math.Max(crossoverPoint1, crossoverPoint2);
                double randomNumber = random.NextDouble();
                

                while (crossoverPoint1 == crossoverPoint2 || (crossoverPoint1 == 0 && crossoverPoint2 == population[i].Count - 1) || (crossoverPoint1 == population[i].Count - 1 && crossoverPoint2 == 0)) {
                    crossoverPoint2 = random.Next(0, population[i].Count - 1);
                }

                if (randomNumber < crossoverChance) {
                    List<int> offspring0 = OrderedCrossover(population[i], population[i + 1], startIndex, endIndex);
                    List<int> offspring1 = OrderedCrossover(population[i + 1], population[i], startIndex, endIndex);

                    newPopulation.Add(offspring0);
                    newPopulation.Add(offspring1);
                } else {
                    newPopulation.Add(population[i]);
                    newPopulation.Add(population[i + 1]);
                }     
            }
            return newPopulation;
        }

        static private List<int> OrderedCrossover(List<int> mother, List<int> father, int startIndex, int endIndex) {
            List<int> offspring = new();
            HashSet<int> visitedIds = new();
            int index = 0;

            for (int i = startIndex; i <= endIndex; i++) {
                visitedIds.Add(mother[i]);
            }

            while (offspring.Count != startIndex) {
                if (visitedIds.Contains(father[index])) {
                    index++;
                } else {
                    offspring.Add(father[index]);
                    index++;
                }
            }

            for (int i = startIndex; i <= endIndex; i++) {
                offspring.Add(mother[i]);
            }

            while (index < father.Count) {
                if (visitedIds.Contains(father[index])) {
                    index++;
                } else {
                    offspring.Add(father[index]);
                    index++;
                }
            }

            return offspring;
        }

        static private List<List<int>> Mutation(List<List<int>> population, float mutationChance) {
            //Choose two indexes and swap them
            List<List<int>> newPopulation = new();

            for (int i = 0; i < population.Count; i++) {

                for (int j = 0; j < population[i].Count; j++) {
                    double randomNumber = random.NextDouble();

                    if (randomNumber < mutationChance) {
                        int gene1 = i;
                        int gene2 = random.Next(0, population.Count - 1);

                        while (gene1 == gene2) {
                            gene2 = random.Next(0, population.Count - 1);
                        }

                        (population[gene1], population[gene2]) = (population[gene2], population[gene1]);
                    }
                }
                newPopulation.Add(population[i]);
            }
            return newPopulation;
        }

        /*
        Roulette wheel selection (RWS) - chance de ser escolhido é proporcional ao valor do fitness;

        Elitism selection (ES) - uma percentagem da população, ordenada por fitness, é sempre transferida para a próxima
                                população, assim as melhores soluções não são perdidas.
        
        Stochastic universal sampling selection (SUSS) - rolar a roleta apenas uma vez e preencher a próxima geração.

        Tournament selection (TS) - escolher dois individuos ao calhas, o que tiver o maior valor de fitness passa para a próxima
        geração
        */
        static private List<List<int>> TournamentSelection(List<List<int>> population, ProblemData problemData) {
            List<List<int>> selectedPopulation = new();
            int size = population.Count;

            while (selectedPopulation.Count < size) {
                int firstIndex = random.Next(0, size);
                int secondIndex = random.Next(0, size);
                while (firstIndex == secondIndex) {
                    secondIndex = random.Next(0, size);
                }

                if (Fitness.Calc(population[firstIndex], problemData) < Fitness.Calc(population[secondIndex], problemData)) {
                    selectedPopulation.Add(population[firstIndex]);
                } else {
                    selectedPopulation.Add(population[secondIndex]);
                }
            }
            return selectedPopulation;
        }

        public static void Solve(int generations, int populationSize, float crossoverChance, float mutationChance, ProblemData problemData) {
            
            
            // Get population size from user
            List<List<int>> population = GenerateFirstPopulation(problemData.IdDemands!, populationSize);
            // Get generations from user
            

            for (int i = 0; i < generations; i++) {
                population = TournamentSelection(population, problemData);
                // Get crossover chance from user
                population = Crossover(population, crossoverChance);
                // Get mutation chance from user
                population = Mutation(population, mutationChance);
            }

            List<int> bestRoute = population.OrderBy(route => Fitness.Calc(route, problemData)).First();
            double bestFitness = Fitness.Calc(bestRoute, problemData);
            Console.WriteLine($"Best solution: {Print.Route(bestRoute, problemData)} with fitness {bestFitness:0.00}");
        }
    }
}