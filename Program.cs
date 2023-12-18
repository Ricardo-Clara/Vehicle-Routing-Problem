using DataProcessing;
using GeneticAlgorithm;
using SimulatedAnnealing;
using Tabu;

ProblemData data = new();
Load.LoadCoordinates("C:\\Users\\rvcla\\Desktop\\VRP\\Data_Sets\\A-n80-k10.xml", data);
DistanceMatrix.Calc(data);
GeneticAlgorithmVRP.Solve(1000, 100, 0.8f, 0.1f, data);
// SimulatedAnnealingVRP.Solve(10000, 2, 0.8, 150, data);
// TabuSearch.Solve(1000, 100, data, 150); 1400