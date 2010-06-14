using System;
using System.Collections.Generic;

namespace Metaheuristics
{
	public class GA2OptFirst42SP : IMetaheuristic
	{
		public void Start(string fileInput, string fileOutput, int timeLimit)
		{
			TwoSPInstance instance = new TwoSPInstance(fileInput);
			
			// Setting the parameters of the GA for this instance of the problem.
			int popSize = (int) Math.Max(10, instance.NumberItems / 3.0);
			double mutProbability = 0.3;
			int[] lowerBounds = new int[instance.NumberItems];
			int[] upperBounds = new int[instance.NumberItems];
			for (int i = 0; i < instance.NumberItems; i++) {
				lowerBounds[i] = 0;
				upperBounds[i] = instance.NumberItems - 1;
			}
			DiscreteGA genetic = new DiscreteGANPS2OptFirst42SP(instance, popSize, mutProbability, lowerBounds, upperBounds);
			
			// Solving the problem and writing the best solution found.
			genetic.Run(timeLimit);
			int[,] coordinates = TwoSPUtils.NPS2Coordinates(instance, genetic.BestIndividual);
			TwoSPSolution solution = new TwoSPSolution(instance, coordinates);
			solution.Write(fileOutput);
		}
		
		public string Name {
			get {
				return "GA using the NPS heuristic with 2-opt (first improvement) local search for 2SP";
			}
		}
		
		public MetaheuristicType Type {
			get {
				return MetaheuristicType.GA;
			}
		}
		
		public ProblemType Problem {
			get {
				return ProblemType.TwoSP;
			}
		}
		
		public string[] Team {
			get {
				return About.Team;
			}
		}
	}
}