
using System;

namespace Metaheuristics
{


	public class DiscreteILS2OptBest4TSP : DiscreteILS
	{
		public TSPInstance Instance { get; protected set; }
		
		public DiscreteILS2OptBest4TSP (TSPInstance instance, int restartIterations, 
		                                 int perturbationPoints, int[] lowerBounds, 
		                                 int[] upperBounds) 
			: base ( restartIterations, perturbationPoints, lowerBounds, upperBounds)
		{
			Instance = instance;
			RepairEnabled = true;
		}
		
		
		protected override void Repair(int[] individual)
		{
			TSPUtils.Repair(Instance, individual);
		}
		
		protected override void LocalSearch(int[] individual)
		{
			TSPUtils.LocalSearch2OptBest(Instance, individual);
		}		
		
		protected override double Fitness(int[] individual)
		{
			return TSPUtils.Fitness(Instance, individual);
		}
		
		protected override int[] InitialSolution ()
		{
			return TSPUtils.RandomSolution(Instance);
		}
		
		protected override void PerturbateSolution (int[] solution, int perturbation)
		{
			TSPUtils.PerturbateSolution(solution, perturbation);
		}


	}
}