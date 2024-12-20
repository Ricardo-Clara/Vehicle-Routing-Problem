using DataProcessing;

namespace ObjectiveFunction {
    public class Fitness {
        public static int functionCallCount;
        public static double Calc(List<int> route, ProblemData problemData) {
            double totalDistance = problemData.DistanceMatrix![problemData.DepartureNodeId, route[0]];
            double totalCapacity = problemData.IdDemands![route[0]][1];
            int depoNode = problemData.DepartureNodeId - 1;
            
            for (int i = 0; i < route.Count - 1; i++) {
                int currentNode = route[i];
                int nextNode = route[i + 1];

                if (totalCapacity + problemData.IdDemands[nextNode][1] > problemData.Capacity) {
                    totalCapacity = 0;
                    totalDistance += problemData.DistanceMatrix[currentNode, depoNode];
                    totalDistance += problemData.DistanceMatrix[depoNode, nextNode];
                } else {
                    totalCapacity += problemData.IdDemands[nextNode][1];
                    totalDistance += problemData.DistanceMatrix[currentNode, nextNode];
                }
            }
            functionCallCount++;
            return totalDistance + problemData.DistanceMatrix[route[^1], depoNode];
        }
    }
}