namespace SimmulatedAnnealing {

    public class ProblemData {

        public List<double[]>? Coordinates { get; set; }
        public double[,]? DistanceMatrix { get; set; }
        public double? Capacity { get; set; }
        public List<double[]>? IdDemands { get; set; }
        public int Offset { get; set;}
        public int DepartureNodeId { get; set; }
    }

    static class SimmulatedAnnealingVRP {
        static List<int> GenerateRoute(List<double[]> idDemands, ProblemData problemData) {
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

        static double Fitness(List<int> route, ProblemData problemData) {
            double totalDistance = problemData.DistanceMatrix![problemData.DepartureNodeId, route[0]];
            double totalCapacity = problemData.IdDemands![route[0]][1];

            
            for (int i = 0; i < route.Count - 1; i++) {
                int currentNode = route[i];
                int nextNode = route[i + 1];

                if (totalCapacity + problemData.IdDemands[nextNode][1] > problemData.Capacity) {
                    totalCapacity = 0;
                    totalDistance += problemData.DistanceMatrix[currentNode, problemData.DepartureNodeId];
                    totalDistance += problemData.DistanceMatrix[problemData.DepartureNodeId, nextNode];
                } else {
                    totalCapacity += problemData.IdDemands[nextNode][1];
                    totalDistance += problemData.DistanceMatrix[currentNode, nextNode];
                }
            }

            return totalDistance + problemData.DistanceMatrix[route[^1], problemData.DepartureNodeId];
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


        static List<int> SimulatedAnnealingAlgorithm(List<int> solution, double temperature, double minTemperature, double alpha, ProblemData problemData) {
            List<int> bestSolution = solution;
            List<int> currentSolution = solution;
            Random randomNumber = new();

            while (temperature > minTemperature) {
                List<List<int>> neighborhood = GenerateNeighborhood(currentSolution);
                double bestSolutionFitness = Fitness(bestSolution, problemData);
                double currentFitness = Fitness(currentSolution, problemData);

                foreach (List<int> neighbor in neighborhood) {
                    double neighborFitness = Fitness(neighbor, problemData);

                    if (neighborFitness < currentFitness || randomNumber.NextDouble() < Math.Exp((currentFitness - neighborFitness) / temperature)) {
                        currentSolution = neighbor;
                        currentFitness = neighborFitness;
                    }
                }
                if (Fitness(currentSolution, problemData) < bestSolutionFitness) {
                    bestSolution = currentSolution;
                }

                temperature *= alpha;
            }
            return bestSolution;
        }
    }
}

