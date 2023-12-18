using DataProcessing;
using ObjectiveFunction;

namespace SimulatedAnnealing {
    static class SimulatedAnnealingVRP {
        private static readonly Random random = new();
        public static List<int> GenerateRoute(ProblemData problemData) {
            List<double[]> idDemandsCopy = new (problemData.IdDemands!);
            List<int> route = new();
            int size = idDemandsCopy.Count;
            int randomNumber = random.Next(0, size);

            route.Add((int)idDemandsCopy[randomNumber][0]);
            idDemandsCopy.RemoveAt(randomNumber);

            for (int i = 0; i < size - 1; i++) {
                int[] nearestNeighbor = FindNearestNeighbor(route[i], idDemandsCopy, problemData);
                route.Add(nearestNeighbor[0]);
                idDemandsCopy.RemoveAt(nearestNeighbor[1]);
            }

            return route;
        }

        static int[] FindNearestNeighbor(int currentLocation, List<double[]> remainingLocations, ProblemData problemData) {
            int nearestNeighbor = -1;
            double minDistance = double.MaxValue;
            int index = 0;

            foreach (double[] location in remainingLocations) {
                double distance = problemData.DistanceMatrix![currentLocation, (int)location[0]];
                if (distance < minDistance) {
                    minDistance = distance;
                    nearestNeighbor = (int)location[0];
                    index = remainingLocations.IndexOf(location);
                }
            }
            int[] result = {nearestNeighbor, index};
            return result;
        }
        static List<List<int>> GenerateNeighborhood(List<int> route, int populationSize) {
            List<List<int>> neighborhood = new();

            for (int i = 0; i < populationSize; i++) {
                List<int> routeCopy = new(route);
                int index1 = random.Next(0, route.Count);
                int index2 = random.Next(0, route.Count);

                while (index1 == index2) {
                    index2 = random.Next(0, route.Count);
                }

                (routeCopy[index1], routeCopy[index2]) = (routeCopy[index2], routeCopy[index1]);
                neighborhood.Add(routeCopy);
            }
            
            return neighborhood;
        }


        public static void Solve(double temperature, double minTemperature, double coolingRate, int populationSize, ProblemData problemData) {
            List<int> bestSolution = GenerateRoute(problemData);
            List<int> currentSolution = bestSolution;
            double bestSolutionFitness = 0;

            while (temperature > minTemperature) {
                List<List<int>> neighborhood = GenerateNeighborhood(currentSolution, populationSize);
                bestSolutionFitness = Fitness.Calc(bestSolution, problemData);
                double currentFitness = Fitness.Calc(currentSolution, problemData);

                foreach (List<int> neighbor in neighborhood) {
                    double neighborFitness = Fitness.Calc(neighbor, problemData);

                    if (neighborFitness < currentFitness || random.NextDouble() < Math.Exp((currentFitness - neighborFitness) / temperature)) {
                        currentSolution = neighbor;
                        currentFitness = neighborFitness;
                    }
                }
                if (Fitness.Calc(currentSolution, problemData) < bestSolutionFitness) {
                    bestSolution = currentSolution;
                }

                temperature *= coolingRate;
            }
            Console.WriteLine($"Best solution: {Print.Route(bestSolution, problemData)} with fitness {bestSolutionFitness:0.00}");
        }

    }
}