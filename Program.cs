using System.Diagnostics;
using DataProcessing;
using GeneticAlgorithm;
using SimulatedAnnealing;
using Tabu;
using System.Text.Json;
using System.Text.Json.Serialization;
using ObjectiveFunction;


// // stopwatch.Start();



// //SimulatedAnnealingVRP.Solve(10000, 2, 0.8, 150, data);
// //TabuSearch.Solve(1000, 100, data, 150);


// Console.WriteLine($"Time elapsed: {stopwatch.Elapsed} milliseconds"); 
// Console.WriteLine(ObjectiveFunction.Fitness.functionCallCount);

//Start
// Used Files: C:\Users\rvcla\Desktop\VRP\Data_Sets\X-n200-k36.xml
//C:\Users\rvcla\Desktop\VRP\Data_Sets\A-n53-k07.xml
//C:\Users\rvcla\Desktop\VRP\Data_Sets\X-n1001-k43.xml
 
string path = @"C:\Users\rvcla\Desktop\VRP\Data_Sets\X-n1001-k43.xml";
ProblemData data = new();
Load.LoadCoordinates(path, data);
DistanceMatrix.Calc(data);

List<Result> results = new();

int generations = 10000;
// temperature = 1000;
//double minTemperature = 0.001;
//double coolingRate = 0.99;
int popSize = 300;
//int tabuSize = 150;
float crossoverChance = 1f;
float mutationChance = 0.01f;


int multiplier = 111; //100

for (int run = (40 * multiplier) + 1; run <= 40 * (multiplier + 1); run++) {
    Result result = new();
    double[] parameters = { generations, popSize, crossoverChance, mutationChance };
    result.Parameters = parameters;
    GeneticAlgorithmVRP.SetSeed(run);

    Stopwatch stopwatch = new();
    stopwatch.Start();

    double fitness = GeneticAlgorithmVRP.Solve(generations,popSize,crossoverChance,mutationChance, data);

    stopwatch.Stop();

    result.TimeElapsed = stopwatch.Elapsed.TotalMilliseconds;
    result.FitnessCallCount = Fitness.functionCallCount;
    result.Seed = run;
    result.FilePath = path;
    result.Fitness = fitness;

    results.Add(result);
}

var options = new JsonSerializerOptions { WriteIndented = true };
string json = JsonSerializer.Serialize(results, options);
File.WriteAllText(@$"C:\Users\rvcla\OneDrive - Universidade do Algarve\MEI\S1\Metaheuristics\Results\GA_X-n1001-k43_final.json", json);

Console.WriteLine("Finished");


public class Result
{
    public double TimeElapsed { get; set; }
    public int FitnessCallCount { get; set; }
    public int Seed { get; set; }
    public double Fitness { get; set; }
    public string ?FilePath {get; set; }
    public double[] ?Parameters { get; set; }
}

