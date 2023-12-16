using System.ComponentModel;
using System.Xml;
using System.Xml.Linq;

public class GeneticAlgorithmVRP
{
    
    public class ProblemData {

        public List<double[]>? Coordinates { get; set; }
        public double[,]? DistanceMatrix { get; set; }
        public double? Capacity { get; set; }
        public List<double[]>? IdDemands { get; set; }
        public int DepartureNodeId { get; set; }
    }

  
    void LoadCoordinates(string path, ProblemData problemData) {
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

            if (problemData.IdDemands![0][0] == 0) {
                for (int i = 0; i < problemData.IdDemands.Count; i++) {
                    problemData.IdDemands[i][0] += 1;
                }
            }      
    }


    private void CalcDistanceMatrix(ProblemData problemData) {
            int numLocations = problemData.Coordinates!.Count;
            double[,] distanceMatrix = new double[numLocations, numLocations];

            for (int i = 0; i < numLocations; i++) {
                for (int j = 0; j < numLocations; j++) {
                    
                    distanceMatrix[i, j] = Math.Sqrt(Math.Pow(problemData.Coordinates[i][0] - problemData.Coordinates[j][0], 2) + Math.Pow(problemData.Coordinates[i][1] - problemData.Coordinates[j][1], 2));
                }
            }
        problemData.DistanceMatrix = distanceMatrix;
    }
    

    List<List<int>> GenerateFirstPopulation(List<double[]> idDemands, int populationSize) {
        List<List<int>> population = new();
        int currentPopulationSize = 0;
        Random random = new();
        

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

    private double Fitness(List<int> route, ProblemData problemData) {
        double totalDistance = problemData.DistanceMatrix![problemData.DepartureNodeId - 1, route[0] - 1];
        double totalCapacity = problemData.IdDemands![route[0] - 1][1];

        
        for (int i = 0; i < route.Count - 1; i++) {
            int currentNode = route[i] - 1;
            int nextNode = route[i + 1] - 1;

            if (totalCapacity + problemData.IdDemands[nextNode][1] > problemData.Capacity) {
                totalCapacity = 0;
                totalDistance += problemData.DistanceMatrix[currentNode, problemData.DepartureNodeId - 1];
                totalDistance += problemData.DistanceMatrix[problemData.DepartureNodeId - 1, nextNode];
            } else {
                totalCapacity += problemData.IdDemands[nextNode][1];
                totalDistance += problemData.DistanceMatrix[currentNode, nextNode];
            }
        }

        return totalDistance + problemData.DistanceMatrix[route[^1] - 1, problemData.DepartureNodeId - 1];
    }
    

    private List<List<int>> Crossover(List<List<int>> population, double crossoverChance) {
        Random random = new();
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

    private List<int> OrderedCrossover(List<int> mother, List<int> father, int startIndex, int endIndex) {
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

    private List<List<int>> Mutation(List<List<int>> population, float mutationChance) {
        //Choose two indexes and swap them
        List<List<int>> newPopulation = new();
        Random random = new();

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
    private List<List<int>> TournamentSelection(List<List<int>> population, ProblemData problemData) {
        List<List<int>> selectedPopulation = new();
        int size = population.Count;
        Random random = new();

        while (selectedPopulation.Count < size) {
            int firstIndex = random.Next(0, size);
            int secondIndex = random.Next(0, size);
            while (firstIndex == secondIndex) {
                secondIndex = random.Next(0, size);
            }

            if (Fitness(population[firstIndex], problemData) < Fitness(population[secondIndex], problemData)) {
                selectedPopulation.Add(population[firstIndex]);
            } else {
                selectedPopulation.Add(population[secondIndex]);
            }
        }
        return selectedPopulation;
    }

    private string printRoute(List<int> route, ProblemData problemData) {
        double totalCapacity = problemData.IdDemands![route[problemData.DepartureNodeId - 1] - 1][1];
        string routeString = $"{problemData.DepartureNodeId} -> ";
        
        for (int i = 0; i < route.Count - 1; i++) {
            //Console.WriteLine(totalCapacity);
            int nextNode = route[i + 1] - 1;

            if (totalCapacity + problemData.IdDemands[nextNode][1] > problemData.Capacity) {
                totalCapacity = problemData.IdDemands[nextNode][1];
                routeString += $"{route[i]} -> {problemData.DepartureNodeId} -> ";
            } else {
                totalCapacity += problemData.IdDemands[nextNode][1];
                routeString += $"{route[i]} -> ";
            }
        }

        return routeString + $"{route[^1]} -> {problemData.DepartureNodeId}";
    }


    static void Main(string[] args) {
        GeneticAlgorithmVRP geneticAlgorithmVRP = new();
        ProblemData problemData = new();
        // Get path from user
        geneticAlgorithmVRP.LoadCoordinates("C:\\Users\\rvcla\\OneDrive\\Desktop\\VeRoLogV08_16.xml", problemData);
        geneticAlgorithmVRP.CalcDistanceMatrix(problemData);
        // Get population size from user
        List<List<int>> population = geneticAlgorithmVRP.GenerateFirstPopulation(problemData.IdDemands!, 100);
        // Get generations from user
        int generations = 2000;

        for (int i = 0; i < generations; i++) {
            population = geneticAlgorithmVRP.TournamentSelection(population, problemData);
            // Get crossover chance from user
            population = geneticAlgorithmVRP.Crossover(population, 0.8);
            // Get mutation chance from user
            population = geneticAlgorithmVRP.Mutation(population, 0.1f);
        }

        List<int> bestRoute = population.OrderBy(route => geneticAlgorithmVRP.Fitness(route, problemData)).First();
        double bestFitness = geneticAlgorithmVRP.Fitness(bestRoute, problemData);
        Console.WriteLine($"Best solution: {geneticAlgorithmVRP.printRoute(bestRoute, problemData)} with fitness {bestFitness:0.00}");
    }
}
