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

  
    void LoadCoordinates(string path, ProblemData problemData) {
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


    private void CalcDistanceMatrix(List<double[]> coordinates, ProblemData problemData) {
            int numLocations = coordinates.Count;
            double[,] distanceMatrix = new double[numLocations, numLocations];

            for (int i = 0; i < numLocations; i++) {
                for (int j = 0; j < numLocations; j++) {
                    
                    distanceMatrix[i, j] = Math.Sqrt(Math.Pow(coordinates[i][0] - coordinates[j][0], 2) + Math.Pow(coordinates[i][1] - coordinates[j][1], 2));
                }
            }
        problemData.DistanceMatrix = distanceMatrix;
    }
    

    List<List<int>> GenerateFirstPopulation(List<double[]> idDemands, List<double> capacities, int populationSize) {
        List<List<int>> population = new();
        int currentPopulationSize = 0;
        Random random = new();

        while (currentPopulationSize <= populationSize) {
            List<double[]> idDemandsCopy = new (idDemands);
            int size = idDemandsCopy.Count;
            List<int> route = new(){0};
            double currentCapacity = 0;

            for (int i = 0; i < size; i++) {
                int randomNumber = random.Next(0, idDemandsCopy.Count);

                if  (currentCapacity + idDemandsCopy[randomNumber][1] >= capacities[0]) {
                    route.Add(0);
                    route.Add(Convert.ToInt32(idDemandsCopy[randomNumber][0]));
                    idDemandsCopy.RemoveAt(randomNumber);
                    currentCapacity = 0;

                } else {
                    currentCapacity += idDemandsCopy[randomNumber][1];
                    route.Add(Convert.ToInt32(idDemandsCopy[randomNumber][0]));
                    idDemandsCopy.RemoveAt(randomNumber);
                }
            }
            route.Add(0);
            population.Add(route);
            currentPopulationSize++;
        }
        return population;
    }

    private double Fitness(List<int> route, double[,] distanceMatrix) {
        double totalDistance = 0;

        
        for (int i = 0; i < route.Count - 1; i++) {
            totalDistance += distanceMatrix[i, i + 1];
        }

        return totalDistance;
    }
    

    private List<List<int>> Crossover(List<List<int>> population, double crossoverChance) {
        Random random = new();
        List<List<int>> newPopulation = new();

        for (int i = 0; i < population.Count; i += 2) {
            double randomNumber = random.NextDouble();

            if (randomNumber < crossoverChance) {
                List<List<int>> offsprings = new();
                
                int crossoverPoint = random.Next(1, population[i].Count - 2);
                offsprings = OrderCrossover(population[i], population[i + 1], crossoverPoint);
                newPopulation.AddRange(offsprings);

            } else {
                newPopulation.Add(population[i]);
                newPopulation.Add(population[i + 1]);
            }
        }

        return newPopulation;
    }

    private List<List<int>> OrderCrossover(List<int> parent1, List<int> parent2, int crossoverPoint) {
        List<int> offspring1 = new();
        List<int> offspring2 = new();

        //Single Point Crossover in the same order
        //Try to implement Order Crossover and/or multiple point crossover
        List<int> sublist1 = new(parent1.GetRange(0, crossoverPoint)); 
        List<int> sublist2 = new(parent1.GetRange(crossoverPoint, parent1.Count - crossoverPoint));
        List<int> sublist3 = new(parent2.GetRange(0, crossoverPoint));
        List<int> sublist4 = new(parent2.GetRange(crossoverPoint, parent2.Count - crossoverPoint));

        offspring1.AddRange(sublist1);
        offspring1.AddRange(sublist4);

        offspring2.AddRange(sublist3);
        offspring2.AddRange(sublist2);

        // List<List<int>> offsprings = new(){offspring1, offspring2};

        return new(){offspring1, offspring2};
    }


    private List<int> Mutation(List<int> route, float mutationChance) {
        //Choose two indexes and swap them
        double randomNumber;
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
    private List<List<int>> TournamentSelection(List<List<int>> population, ProblemData problemData) {
        List<List<int>> selectedPopulation = new();
        int size = population.Count;
        Random random = new();

        while (selectedPopulation.Count < size) {
            int firstIndex = random.Next(0, size);
            int secondIndex = random.Next(0, size);

            if (Fitness(population[firstIndex], problemData.DistanceMatrix!) < Fitness(population[secondIndex], problemData.DistanceMatrix!)) {
                selectedPopulation.Add(population[firstIndex]);
            } else {
                selectedPopulation.Add(population[secondIndex]);
            }
        }

        return selectedPopulation;
    }

    static void Main(string[] args) {
        GeneticAlgorithmVRP geneticAlgorithmVRP = new();
        ProblemData problemData = new();
        // Get path from user
        geneticAlgorithmVRP.LoadCoordinates("", problemData);
        geneticAlgorithmVRP.CalcDistanceMatrix(problemData.Coordinates!, problemData);
        // Get population size from user
        List<List<int>> population = geneticAlgorithmVRP.GenerateFirstPopulation(problemData.IdDemands!, problemData.Capacities!, 10);
        // Get generations from user
        int generations = 1000;

        for (int i = 0; i < generations; i++) {
            population = geneticAlgorithmVRP.TournamentSelection(population, problemData);
            // Get crossover chance from user
            population = geneticAlgorithmVRP.Crossover(population, 0.8);
            // Get mutation chance from user
            population = population.Select(route => geneticAlgorithmVRP.Mutation(route, 0.1f)).ToList();
        }

        List<int> bestRoute = population.OrderBy(route => geneticAlgorithmVRP.Fitness(route, problemData.DistanceMatrix!)).First();
        double bestFitness = geneticAlgorithmVRP.Fitness(bestRoute, problemData.DistanceMatrix!);
        Console.WriteLine($"Best solution: {bestRoute} with fitness {bestFitness}");
    }
}
