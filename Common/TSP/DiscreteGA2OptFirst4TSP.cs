using System;

namespace Metaheuristics
{
	public class DiscreteGA2OptFirst4TSP : DiscreteGA
	{
		public TSPInstance Instance { get; protected set; }
		
		public DiscreteGA2OptFirst4TSP(TSPInstance instance, int popSize, double mutationProbability,
		                              int[] lowerBounds, int[] upperBounds)
			: base(popSize, mutationProbability, lowerBounds, upperBounds)
		{
			Instance = instance;
			RepairEnabled = true;
			LocalSearchEnabled = true;
		}
		
		protected override void Repair(int[] individual)
		{
			TSPUtils.Repair(Instance, individual);
		}
		
		protected override void LocalSearch(int[] individual)
		{
			TSPUtils.LocalSearch2OptFirst(Instance, individual);
		}		
		
		protected override double Fitness(int[] individual)
		{
			return TSPUtils.Fitness(Instance, individual);
		}
		
		protected override int[] InitialSolution ()
		{
			return TSPUtils.RandomSolution(Instance);
		}

	}
}
