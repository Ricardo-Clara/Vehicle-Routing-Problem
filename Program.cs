using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;

public class GeneticAlgorithmVRP
{
    
    public class ProblemData {

        public List<double[]>? Coordinates { get; set; }
        public double[,]? DistanceMatrix { get; set; }
        public List<double>? Capacities { get; set; }
        public List<double[]>? IdDemands { get; set; }
    }

    ProblemData problemData = new();
    
    void LoadCoordinates(string path) {
        XDocument doc = XDocument.Load(path);

        List<double[]> coordinates = new();
        List<double> capacities = new();
        List<double[]> idDemands = new();
        
        
            foreach (XElement element in doc.Descendants("node")) {
                double x = double.Parse(element.Element("cx")!.Value);
                double y = double.Parse(element.Element("cy")!.Value);
                double[] coords = {x, y}; 
                
                coordinates.Add(coords);
            }

            foreach (XElement element in doc.Descendants("vehicle_profile")) {
                double capacity = double.Parse(element.Element("capacity")!.Value);

                capacities.Add(capacity);
            }

            foreach (XElement element in doc.Descendants("request")) {
                double demand = double.Parse(element.Element("quantity")!.Value);
                double id = double.Parse(element.Attribute("id")!.Value);
                
                double[] id_demand = { id, demand};
                idDemands.Add(id_demand);
            }

            problemData.Coordinates = coordinates;
            problemData.Capacities = capacities;
            problemData.IdDemands = idDemands;
    }


    private void CalcDistanceMatrix(List<double[]> coordinates) {
            int numLocations = coordinates.Count;
            double[,] distanceMatrix = new double[numLocations, numLocations];

            for (int i = 0; i < numLocations; i++) {
                for (int j = 0; j < numLocations; j++) {
                    
                    distanceMatrix[i, j] = Math.Sqrt(Math.Pow(coordinates[i][0] - coordinates[j][0], 2) + Math.Pow(coordinates[i][1] - coordinates[j][1], 2));
                }
            }
        problemData.DistanceMatrix = distanceMatrix;
    }
    

    List<List<int>> GenerateFirstPopulation(List<double[]> idDemands, List<double> capacities) {
        List<List<int>> population = new();
        int amount = 0;
        Random random = new();

        while (amount <= 10) {
            List<double[]> idDemandsCopy = new (idDemands);
            int size = idDemandsCopy.Count;
            List<int> route = new List<int>(){0};
            double count = 0;

            for (int i = 0; i < size; i++) {
                int randomNumber = random.Next(0, idDemandsCopy.Count);

                if (count + idDemandsCopy[randomNumber][1] >= capacities[0]) idDemandsCopy.RemoveAt(randomNumber); 
                else {
                    count += idDemandsCopy[randomNumber][1];
                    route.Add(Convert.ToInt32(idDemandsCopy[randomNumber][0]));
                    idDemandsCopy.RemoveAt(randomNumber);
                }
            }
            route.Add(0);
            population.Add(route);
            amount++;
        }
        return population;
    }

    private double Fitness(string route, double[,] distanceMatrix) {
        double totalDistance = 0;

        
        for (int i = 0; i < route.Length - 1; i++) {
            totalDistance += distanceMatrix[i, i + 1];
        }

        return totalDistance;
    }
    

    private List<List<int>> Crossover(List<int> parent1, List<int> parent2, int crossoverPoint, double crossoverChance) {

        List<int> offspring1 = new();
        List<int> offspring2 = new();

        //Single Point Crossover in the same order
        //Try to implement Order Crossover and/or multiple point crossover
        List<int> sublist1 = parent1.GetRange(0, crossoverPoint);
        List<int> sublist2 = parent1.GetRange(crossoverPoint, parent1.Count - crossoverPoint);
        List<int> sublist3 = parent2.GetRange(0, crossoverPoint);
        List<int> sublist4 = parent2.GetRange(crossoverPoint, parent2.Count - crossoverPoint);

        offspring1.AddRange(sublist1);
        offspring1.AddRange(sublist4);

        offspring2.AddRange(sublist3);
        offspring2.AddRange(sublist2);

        List<List<int>> offsprings = new(){offspring1, offspring2};

        return offsprings;
    }

    private List<int> Mutation(List<int> route, float mutationChance) {
        //Choose two indexes and swap them
        double randomNumber = 0;
        Random random = new();

        for (int i = 1; i < route.Count - 1; i++) {
            randomNumber = random.NextDouble();

            if (randomNumber < mutationChance) {
                int index1 = i;
                int index2 = random.Next(1, route.Count - 2);

                (route[index1], route[index2]) = (route[index2], route[index1]);
            }
        }
        return route;
    }

    /*
    Roulette wheel selection (RWS) - chance de ser escolhido é proporcional ao valor do fitness;

    Elitism selection (ES) - uma percentagem da população, ordenada por fitness, é sempre transferida para a próxima
                             população, assim as melhores soluções não são perdidas.
    
    Stochastic universal sampling selection (SUSS) - rolar a roleta apenas uma vez e preencher a próxima geração.

    Tournament selection (TS) - escolher dois individuos ao calhas, o que tiver o maior valor de fitness passa para a próxima
    geração
    */
    private string TournamentSelection(List<string> population) {
        //xota

    }

    private string Solve(List<string> population) {
        
    }
}
