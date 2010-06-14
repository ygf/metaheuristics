using System;
using System.Collections;
using System.Collections.Generic;

namespace Metaheuristics
{
	public static class TwoSPUtils
	{
		// Normal Pattern Shifting (NPS) heuristic to encode a solution of the 2SP as an
		// integer vector for the ordering of the items. This method calculates the coordinates 
		// of all items in the strip from an ordering of the items following the NPS heuristic.
		public static int[,] NPS2Coordinates(TwoSPInstance instance, int[] ordering)
		{
			int x = 0, y = 1;
			int[,] coordinates = new int[instance.NumberItems,2];
			List<int[]> topLeftSeeds = new List<int[]>();
			List<int[]> bottomRightSeeds = new List<int[]>();
			List<int[]> candidatePositions = new List<int[]>();
			bool[] allocatedItem = new bool[instance.NumberItems];
			int[] finalPosition = new int[2];
			
			for (int i = 0; i < instance.NumberItems; i++) {
				int item = ordering[i];
				
				if (i == 0) {
					// The first item uses the starting position.
					finalPosition[x] = 0;
					finalPosition[y] = 0;
				}
				else {
					candidatePositions.Clear();
					
					foreach (int[] position in topLeftSeeds) {
						// Move left the item as much as possible.

						// Check if we can put the item next to the left border of the strip.
						coordinates[item,x] = 0;
						coordinates[item,y] = position[y];
						if (NPSIsFeasible(instance, coordinates, allocatedItem, item)) {
							// If this is valid, it's the best position.
							position[x] = coordinates[item,x];
							position[y] = coordinates[item,y];
							candidatePositions.Add(new int[] {position[x], position[y]});
						}
						else {
							// Try to put the item next to other items.
							position[x] = int.MaxValue;
							for (int otherItem = 0; otherItem < instance.NumberItems; otherItem++) {
								coordinates[item,x] = coordinates[otherItem,x] + instance.ItemsWidth[otherItem];
								if (allocatedItem[otherItem] && coordinates[item,x] < position[x]) {
									if (NPSIsFeasible(instance, coordinates, allocatedItem, item)) {
										position[x] = coordinates[item,x];
										position[y] = coordinates[item,y];
									}
								}
							}
							if (position[x] != int.MaxValue) {
								candidatePositions.Add(new int[] {position[x], position[y]});
							}
						}
					}
					
					foreach (int[] position in bottomRightSeeds) {
						// Move down the item as much as possible.

						// Check if we can put the item at the bottom of the strip.
						coordinates[item,x] = position[x];
						coordinates[item,y] = 0;
						if (NPSIsFeasible(instance, coordinates, allocatedItem, item)) {
							// If this is valid, it's the best position.
							position[x] = coordinates[item,x];
							position[y] = coordinates[item,y];
							candidatePositions.Add(new int[] {position[x], position[y]});
						}
						else {
							// Try to put the item on top of other item.
							position[y] = int.MaxValue;
							for (int otherItem = 0; otherItem < instance.NumberItems; otherItem++) {
								coordinates[item,y] = coordinates[otherItem,y] + instance.ItemsHeight[otherItem];
								if (allocatedItem[otherItem] && coordinates[item,y] < position[y]) {
									if (NPSIsFeasible(instance, coordinates, allocatedItem, item)) {
										position[x] = coordinates[item,x];
										position[y] = coordinates[item,y];
									}
								}
							}
							if (position[y] != int.MaxValue) {
								candidatePositions.Add(new int[] {position[x], position[y]});
							}
						}
					}

					// Choose the lowest and leftmost position that maximizes the quality.
					double bestQuality = double.MinValue, currentQuality;
					foreach (int[] position in candidatePositions) {
						coordinates[item,x] = position[x];
						coordinates[item,y] = position[y];
						currentQuality = NPSQuality(instance, coordinates, allocatedItem, item);
						if (currentQuality > bestQuality) {
							finalPosition[x] = position[x];
							finalPosition[y] = position[y];
						}
						else if (currentQuality == bestQuality && position[x] <= finalPosition[x] && position[x] <= finalPosition[x]) {
							finalPosition[x] = position[x];
							finalPosition[y] = position[y];
						}
					}
				}
				
				// Set the position of the current item.
				allocatedItem[item] = true;
				coordinates[item,x] = finalPosition[x];
				coordinates[item,y] = finalPosition[y];
				
				// Update the lists with the seed positions.
				topLeftSeeds.Add(new int[] {coordinates[item,x], coordinates[item,y] + instance.ItemsHeight[item]});
				bottomRightSeeds.Add(new int[] {coordinates[item,x] + instance.ItemsWidth[item], coordinates[item,y]});
			}
			
			return coordinates;
		}
		
		public static bool IsFeasible(TwoSPInstance instance, int[,] coordinates)
		{
			bool[] allocatedItems = new bool[instance.NumberItems];
			
			for (int item = 0; item < allocatedItems.Length; item++) {
				allocatedItems[item] = true;
			}
			for (int item = 0; item < coordinates.GetLength(0); item++) {
				allocatedItems[item] = false;
				if (!IsFeasible(instance, coordinates, allocatedItems, item)) {
					return false;
				}
				allocatedItems[item] = true;
			}
			return true;
		}
		
		public static int Fitness(TwoSPInstance instance, int[,] coordinates)
		{
			int totalHeight = 0;
			
			for (int item = 0; item < coordinates.GetLength(0); item++) {
				int itemHeight = coordinates[item,1] + instance.ItemsHeight[item];
				if (itemHeight > totalHeight) {
					totalHeight = itemHeight;
				}
			}

			return totalHeight;
		}
		
		private static double NPSQuality(TwoSPInstance instance, int[,] coordinates, bool[] allocatedItem, int item)
		{
			int y = 1;
			double numerator = 0, denominator;			
			double currentItemHeight, maxHeight = 0;

			for (int currentItem = 0; currentItem < instance.NumberItems; currentItem++) {
				if (allocatedItem[currentItem] || currentItem == item) {
					currentItemHeight = coordinates[currentItem,y] + instance.ItemsHeight[currentItem];		
					numerator += instance.ItemsWidth[currentItem] * currentItemHeight;
					if (currentItemHeight > maxHeight) {
						maxHeight = currentItemHeight;
					}
				}
			}
			denominator = instance.StripWidth * maxHeight;
			
			return numerator / denominator;
		}		
		
		private static bool NPSIsFeasible(TwoSPInstance instance, int[,] coordinates, bool[] allocatedItems, int item)
		{
			int x = 0, y = 1;
			int itemXStart = coordinates[item,x];
			int itemXEnd = coordinates[item,x] + instance.ItemsWidth[item];
			int itemYStart = coordinates[item,y];
			int itemYEnd = coordinates[item,y] + instance.ItemsHeight[item];
			bool leftAdjacent = false;
			bool bottomAdjacent = false;	
			
			// The left-hand edge and the bottom edges should be both adjacent to other
			// items or to the edges of the strip.
			if (itemXStart == 0) leftAdjacent = true;
			if (itemYStart == 0) bottomAdjacent = true;			
			
			// Checking if the item is located inside the strip.
			if (itemXStart < 0 || itemXEnd > instance.StripWidth) {
				return false;
			}
			
			// Check if the item collapses with other item.
			for (int otherItem = 0; otherItem < coordinates.GetLength(0); otherItem++) {
				if (allocatedItems[otherItem]) {
					int otherItemXStart = coordinates[otherItem,x];
					int otherItemXEnd = coordinates[otherItem,x] + instance.ItemsWidth[otherItem];
					int otherItemYStart = coordinates[otherItem,y];
					int otherItemYEnd = coordinates[otherItem,y] + instance.ItemsHeight[otherItem];
					
					if (((otherItemXStart >= itemXStart && otherItemXStart < itemXEnd) ||
					     (otherItemXEnd > itemXStart && otherItemXEnd <= itemXEnd)) &&
					    ((otherItemYStart >= itemYStart && otherItemYStart < itemYEnd) ||
					     (otherItemYEnd > itemYStart && otherItemYEnd <= itemYEnd))) {
						return false;
					}
					
					// The left-hand edge and the bottom edges should be both adjacent to other
					// items or to the edges of the strip.						
					if (((otherItemXStart >= itemXStart && otherItemXStart < itemXEnd) ||
					     (otherItemXEnd > itemXStart && otherItemXEnd <= itemXEnd)) &&
					    (itemYStart == otherItemYEnd)) {
						bottomAdjacent = true;
					}
					
					if (((otherItemYStart >= itemYStart && otherItemYStart < itemYEnd) ||
					     (otherItemYEnd > itemYStart && otherItemYEnd <= itemYEnd)) &&
					    (itemXStart == otherItemXEnd)) {
						leftAdjacent = true;
					}
					
					if (leftAdjacent && bottomAdjacent) {
						return true;
					}
				}
			}
			
			return false;
		}
		
		// Check if the location given in the coordinates array for the given 
		// item is valid considering the location of the rest of the items.
		private static bool IsFeasible(TwoSPInstance instance, int[,] coordinates, bool[] allocatedItem, int item)
		{
			int x = 0, y = 1;
			int itemXStart = coordinates[item,x];
			int itemXEnd = coordinates[item,x] + instance.ItemsWidth[item];
			int itemYStart = coordinates[item,y];
			int itemYEnd = coordinates[item,y] + instance.ItemsHeight[item];
			
			// Checking if the item is located inside the strip.
			if (itemXStart < 0 || itemXEnd > instance.StripWidth) {
				return false;
			}
			
			// Check if the item collapses with other item.
			for (int otherItem = 0; otherItem < coordinates.GetLength(0); otherItem++) {
				if (allocatedItem[otherItem]) {
					int otherItemXStart = coordinates[otherItem,x];
					int otherItemXEnd = coordinates[otherItem,x] + instance.ItemsWidth[otherItem];
					int otherItemYStart = coordinates[otherItem,y];
					int otherItemYEnd = coordinates[otherItem,y] + instance.ItemsHeight[otherItem];
					
					if (((otherItemXStart >= itemXStart && otherItemXStart < itemXEnd) ||
					     (otherItemXEnd > itemXStart && otherItemXEnd <= itemXEnd)) &&
					    ((otherItemYStart >= itemYStart && otherItemYStart < itemYEnd) ||
					     (otherItemYEnd > itemYStart && otherItemYEnd <= itemYEnd))) {
						return false;
					}
				}
			}
			
			return true;
		}		

		public static void Repair(TwoSPInstance instance, int[] individual)
		{			
			int itemsAllocatedCount = 0;
			bool[] itemsAllocated = new bool[instance.NumberItems];
			bool[] itemsRepeated = new bool[instance.NumberItems];
				
			// Get information to decide if the individual is valid.
			for (int item = 0; item < instance.NumberItems; item++) {
				if (!itemsAllocated[individual[item]]) {
					itemsAllocatedCount += 1;
					itemsAllocated[individual[item]] = true;
				}
				else {
					itemsRepeated[item] = true;
				}
			}

			// If the individual is invalid, make it valid.
			if (itemsAllocatedCount != instance.NumberItems) {
				for (int item = 0; item < itemsRepeated.Length; item++) {
					if (itemsRepeated[item]) {
						int count = Statistics.RandomDiscreteUniform(1, instance.NumberItems - itemsAllocatedCount);
						for (int i = 0; i < itemsAllocated.Length; i++) {
							if (!itemsAllocated[i]) {
								count -= 1;
								if (count == 0) {
									individual[item] = i;
									itemsRepeated[item] = false;
									itemsAllocated[i] = true;
									itemsAllocatedCount += 1;
									break;
								}
							}
						}
					}
				}
			}
		}
		
		// Implementation of the 2-opt (first improvement) local search algorithm.
		public static void NPSLocalSearch2OptFirst(TwoSPInstance instance, int[] ordering)
		{
			int tmp;
			double currentFitness, bestFitness;

			bestFitness = Fitness(instance, NPS2Coordinates(instance, ordering));			
			for (int j = 1; j < ordering.Length; j++) {
				for (int i = 0; i < j; i++) {
					// Swap the items.
					tmp = ordering[j];
					ordering[j] = ordering[i];
					ordering[i] = tmp;
					
					// Evaluate the fitness of this new solution.
					currentFitness = Fitness(instance, NPS2Coordinates(instance, ordering));
					if (currentFitness < bestFitness) {
						return;
					}
					
					// Undo the swap.
					tmp = ordering[j];
					ordering[j] = ordering[i];
					ordering[i] = tmp;
				}
			}
		}
		
		// Implementation of the 2-opt (best improvement) local search algorithm.
		public static void NPSLocalSearch2OptBest(TwoSPInstance instance, int[] ordering)
		{
			int tmp;
			int firstSwapItem = 0, secondSwapItem = 0;
			double currentFitness, bestFitness;
			
			bestFitness = Fitness(instance, NPS2Coordinates(instance, ordering));
			for (int j = 1; j < ordering.Length; j++) {
				for (int i = 0; i < j; i++) {
					// Swap the items.
					tmp = ordering[j];
					ordering[j] = ordering[i];
					ordering[i] = tmp;
					
					// Evaluate the fitness of this new solution.
					currentFitness = Fitness(instance, NPS2Coordinates(instance, ordering));
					if (currentFitness < bestFitness) {
						firstSwapItem = j;
						secondSwapItem = i;
						bestFitness = currentFitness;
					}
					
					// Undo the swap.
					tmp = ordering[j];
					ordering[j] = ordering[i];
					ordering[i] = tmp;
				}
			}
			
			// Use the best assignment.
			if (firstSwapItem != secondSwapItem) {
				tmp = ordering[firstSwapItem];
				ordering[firstSwapItem] = ordering[secondSwapItem];
				ordering[secondSwapItem] = tmp;
			}
		}
		
		public static int[] RandomSolution(TwoSPInstance instance)
		{
			int[] solution = new int[instance.NumberItems];
			List<int> items = new List<int>();
			
			for (int item = 0; item < instance.NumberItems; item++) {
				items.Add(item);
			}
			for (int i = 0; i < instance.NumberItems; i++) {
				int itemIndex = Statistics.RandomDiscreteUniform(0, items.Count - 1);
				int item = items[itemIndex];
				items.RemoveAt(itemIndex);
				solution[i] = item;
			}
			
			return solution;
		}
		
		public static int[] GetNeighbor(TwoSPInstance instance, int[] solution)
		{
			int[] neighbor = new int[instance.NumberItems];
			int a = Statistics.RandomDiscreteUniform(0, solution.Length - 1);
			int b = a;

			while (b == a) {
				b = Statistics.RandomDiscreteUniform(0, solution.Length - 1);
			}
			for (int i = 0; i < solution.Length; i++) {
				if (i == a) {
					neighbor[i] = solution[b];
				}
				else if (i == b) {
					neighbor[i] = solution[a];
				}
				else {
					neighbor[i] = solution[i];
				}
			}

			return neighbor;
		}		
	}
}
