using DataProcessing;
using ObjectiveFunction;

namespace Tabu {
    static class TabuSearch {
        static List<int> GenerateSolution(List<double[]> idDemands, ProblemData problemData) {
            Random random = new();

            List<double[]> idDemandsCopy = new (idDemands);
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

        static List<List<int>> GenerateNeighborhood(List<int> route) {
            List<List<int>> neighborhood = new();

            for (int i = 1; i < route.Count - 1; i++) {
                for (int j = i + 1; j < route.Count - 1; j++) {
                    List<int> routeCopy = new(route);
                    (routeCopy[i], route[j]) = (routeCopy[j], route[i]);

                    neighborhood.Add(routeCopy);
                }
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

        static List<int> TabuSearchAlgorithm(List<int> solution, int generations, int tabuSize, ProblemData problemData) {
            List<int> bestSolution = solution;
            List<int> currentSolution = solution;
            List<List<int>> tabuList = new();

            for (int i = 0; i < generations; i++) {
                List<List<int>> neighborhood = GenerateNeighborhood(currentSolution);
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
                if (bestNeighbor.Count == 0) {
                    break;
                }

                currentSolution = bestNeighbor;
                tabuList.Add(currentSolution);

                if (tabuList.Count > tabuSize) {
                    tabuList.RemoveAt(0);
                }
                if (Fitness.Calc(bestNeighbor, problemData) < Fitness.Calc(bestSolution, problemData)) {
                    bestSolution = bestNeighbor;
                }
            }
            return bestSolution;
        }
    }
}




