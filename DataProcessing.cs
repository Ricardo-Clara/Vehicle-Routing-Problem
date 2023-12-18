using System.Xml.Linq;

namespace DataProcessing {
    public class ProblemData {
        public List<double[]>? Coordinates { get; set; }
        public double[,]? DistanceMatrix { get; set; }
        public double? Capacity { get; set; }
        public List<double[]>? IdDemands { get; set; }
        public int Offset { get; set;}
        public int DepartureNodeId { get; set; }
    }

    public class Load {
        public static void LoadCoordinates(string path, ProblemData problemData) {
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
    }

    public class DistanceMatrix {
        public static void Calc(ProblemData problemData) {
            int numLocations = problemData.Coordinates!.Count;
            double[,] distanceMatrix = new double[numLocations, numLocations];

            for (int i = 0; i < numLocations; i++) {
                for (int j = 0; j < numLocations; j++) {
                    
                    distanceMatrix[i, j] = Math.Sqrt(Math.Pow(problemData.Coordinates[i][0] - problemData.Coordinates[j][0], 2) + Math.Pow(problemData.Coordinates[i][1] - problemData.Coordinates[j][1], 2));
                }
            }
        problemData.DistanceMatrix = distanceMatrix;
    }
    }

    public class Print {
        public static string Route(List<int> route, ProblemData problemData) {
            double totalCapacity = problemData.IdDemands![route[0]][1];
            string routeString = $"{problemData.DepartureNodeId + problemData.Offset} -> ";
            
            for (int i = 0; i < route.Count - 1; i++) {
                //Console.WriteLine(totalCapacity);
                int nextNode = route[i + 1];

                if (totalCapacity + problemData.IdDemands[nextNode][1] > problemData.Capacity) {
                    totalCapacity = problemData.IdDemands[nextNode][1];
                    routeString += $"{route[i] + problemData.Offset} -> {problemData.DepartureNodeId + problemData.Offset} -> ";
                } else {
                    totalCapacity += problemData.IdDemands[nextNode][1];
                    routeString += $"{route[i] + problemData.Offset} -> ";
                }
            }

            return routeString + $"{route[^1] + problemData.Offset} -> {problemData.DepartureNodeId + problemData.Offset}";
        }
    }
}