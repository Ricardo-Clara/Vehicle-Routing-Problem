using System.Xml.Linq;

namespace Tabu {

    public class ProblemData {

        public List<double[]>? Coordinates { get; set; }
        public double[,]? DistanceMatrix { get; set; }
        public double? Capacity { get; set; }
        public List<double[]>? IdDemands { get; set; }
        public int Offset { get; set;}
        public int DepartureNodeId { get; set; }
    }

    


    static class TabuSearch {

        static void LoadCoordinates(string path, ProblemData problemData) {
            XDocument doc = XDocument.Load(path);

            double capacity = 0;
            int departureId = 0;
            List<double[]> coordinates = new();
            List<double[]> idDemands = new();
            
            
                foreach (XElement element in doc.Descendants("node")) {
                    double x = double.Parse(element.Element("cx")!.Value);
                    double y = double.Parse(element.Element("cy")!.Value);
                    double[] coords = {x, y};
                
                    coordinates.Add(coords);
                }

                foreach (XElement element in doc.Descendants("vehicle_profile")) {
                    capacity = double.Parse(element.Element("capacity")!.Value);
                    departureId = int.Parse(element.Element("departure_node")!.Value);
                }

                foreach (XElement element in doc.Descendants("request")) {
                    double demand = double.Parse(element.Element("quantity")!.Value);
                    double id = double.Parse(element.Attribute("id")!.Value);
                    
                    
                    double[] id_demand = { id , demand };
                    // Console.WriteLine(id_demand[0]);
                    idDemands.Add(id_demand);
                }

                problemData.Coordinates = coordinates;
                problemData.Capacity = capacity;
                problemData.IdDemands = idDemands;
                problemData.DepartureNodeId = departureId;

                if (problemData.IdDemands![0][0] != 0) {
                    problemData.Offset = (int)problemData.IdDemands![0][0];
                    problemData.DepartureNodeId -= problemData.Offset; 
                }
                for (int i = 0; i < problemData.IdDemands.Count; i++) {
                    problemData.IdDemands[i][0] -= problemData.Offset;
                }      
        }
        

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
                        double neighborFitness = Fitness(neighbor, problemData);

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
                if (Fitness(bestNeighbor, problemData) < Fitness(bestSolution, problemData)) {
                    bestSolution = bestNeighbor;
                }
            }
            return bestSolution;
        }
    }
}




