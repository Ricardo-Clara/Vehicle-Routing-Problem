using DataProcessing;
using ObjectiveFunction;

namespace Tabu {
    static class TabuSearch {
        private static Random random = new();
        public static void SetSeed(int seed)
        {
            random = new Random(seed);
        }
        static List<int> GenerateRoute(ProblemData problemData) {
            List<double[]> idDemandsCopy = new (problemData.IdDemands!);
            List<int> route = new();
            int size = idDemandsCopy.Count;
            int randomNumber = random.Next(0, size);

            route.Add((int)idDemandsCopy[randomNumber][0]);
            idDemandsCopy.RemoveAt(randomNumber);

            for (int i = 0; i < size - 1; i++) {
                int[] nearestNeighbor = FindNearestNeighbor(route[i], idDemandsCopy, problemData.DistanceMatrix!);
                route.Add(nearestNeighbor[0]);
                idDemandsCopy.RemoveAt(nearestNeighbor[1]);
            }

            return route;
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

        static int[] FindNearestNeighbor(int currentLocation, List<double[]> remainingLocations, double[,] distanceMatrix) {
            int nearestNeighbor = -1;
            double minDistance = double.MaxValue;
            int index = 0;

            foreach (double[] location in remainingLocations) {
                double distance = distanceMatrix[currentLocation, (int)location[0]];
                if (distance < minDistance) {
                    minDistance = distance;
                    nearestNeighbor = (int)location[0];
                    index = remainingLocations.IndexOf(location);
                }
            }
            int[] result = {nearestNeighbor, index};
            return result;
        }

        public static double Solve(int generations, int tabuSize, ProblemData problemData, int populationSize) {
            List<int> bestSolution = GenerateRoute(problemData);
            List<int> currentSolution = GenerateRoute(problemData);
            HashSet<List<int>> tabuList = new();

            for (int i = 0; i < generations; i++) {
                List<List<int>> neighborhood = GenerateNeighborhood(currentSolution, populationSize);
                List<int> bestNeighbor = new();
                double bestNeighborFitness = double.MaxValue;

                foreach (List<int> neighbor in neighborhood) {
                    if (!tabuList.Contains(neighbor)) {
                        double neighborFitness = Fitness.Calc(neighbor, problemData);

                        if (neighborFitness < bestNeighborFitness) {
                            bestNeighbor = neighbor;
                            bestNeighborFitness = neighborFitness;
                        }
                    }
                }
                //If it didnt find a better solution the loop breaks.
                if (bestNeighborFitness == double.MaxValue) {
                    break;
                }

                currentSolution = bestNeighbor;
                tabuList.Add(currentSolution);

                if (tabuList.Count > tabuSize) {
                    tabuList.Remove(tabuList.First());
                }
                if (Fitness.Calc(bestNeighbor, problemData) < Fitness.Calc(bestSolution, problemData)) {
                    bestSolution = bestNeighbor;
                }
            }
            
            return Fitness.Calc(bestSolution, problemData);
        }
    }
}




