using System.Collections.Generic;
using RBF_TIMESERIES.Utils;

namespace RBF_TIMESERIES
{
	/// <summary>
	/// This class implements some facilities for ranking solutions. Given a
	/// <code>SolutionSet</code> object, their solutions are ranked according to
	/// scheme proposed in NSGA-II; as a result, a set of subsets are obtained. The
	/// subsets are numbered starting from 0 (in NSGA-II, the numbering starts from
	/// 1); thus, subset 0 contains the non-dominated solutions, subset 1 contains
	/// the non-dominated solutions after removing those belonging to subset 0, and
	/// so on.
	/// </summary>
	public class Ranking
	{
		/// <summary>
		/// The <code>SolutionSet</code> to rank
		/// </summary>
		private Population population;

		/// <summary>
		/// An array containing all the fronts found during the search
		/// </summary>
		private Population[] ranking;

		/// <summary>
		/// stores a <code>Comparator</code> for dominance checking
		/// </summary>
		private static readonly IComparer<Individual> dominance = new DominanceComparator();

		/// <summary>
		/// stores a <code>Comparator</code> for Overal Constraint Violation Comparator checking
		/// </summary>
		private static readonly IComparer<Individual> constraint = new ConstraintViolationComparator();

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="population">The <code>SolutionSet</code> to be ranked.</param>
		public Ranking(Population population)
		{
			this.population = population;

			// dominateMe[i] contains the number of solutions dominating i        
			int[] dominateMe = new int[this.population.Size()];

			// iDominate[k] contains the list of solutions dominated by k
			List<int>[] iDominate = new List<int>[this.population.Size()];

			// front[i] contains the list of individuals belonging to the front i
			List<int>[] front = new List<int>[this.population.Size() + 1];

			// flagDominate is an auxiliar encodings.variable
			int flagDominate;

			// Initialize the fronts 
			for (int i = 0; i < front.Length; i++)
			{
				front[i] = new List<int>();
			}

			//-> Fast non dominated sorting algorithm
			// Contribution of Guillaume Jacquenot
			for (int p = 0; p < this.population.Size(); p++)
			{
				// Initialize the list of individuals that i dominate and the number
				// of individuals that dominate me
				iDominate[p] = new List<int>();
				dominateMe[p] = 0;
			}
			for (int p = 0; p < (this.population.Size() - 1); p++)
			{
				// For all q individuals , calculate if p dominates q or vice versa
				for (int q = p + 1; q < this.population.Size(); q++)
				{
					flagDominate = constraint.Compare(population.Get(p), population.Get(q));
					if (flagDominate == 0)
					{
						flagDominate = dominance.Compare(population.Get(p), population.Get(q));
					}
					if (flagDominate == -1)
					{
						iDominate[p].Add(q);
						dominateMe[q]++;
					}
					else if (flagDominate == 1)
					{
						iDominate[q].Add(p);
						dominateMe[p]++;
					}
				}
				// If nobody dominates p, p belongs to the first front
			}
			for (int p = 0; p < this.population.Size(); p++)
			{
				if (dominateMe[p] == 0)
				{
					front[0].Add(p);
					population.Get(p).Rank = 0;
				}
			}

			//Obtain the rest of fronts
			int k = 0;

			while (front[k].Count != 0)
			{
				k++;
				foreach (var it1 in front[k - 1])
				{
					foreach (var it2 in iDominate[it1])
					{
						int index = it2;
						dominateMe[index]--;
						if (dominateMe[index] == 0)
						{
							front[k].Add(index);
							this.population.Get(index).Rank = k;
						}
					}
				}
			}
			//<-

			ranking = new Population[k];
			//0,1,2,....,i-1 are front, then i fronts
			for (int j = 0; j < k; j++)
			{
				ranking[j] = new Population(front[j].Count);
				foreach (var it1 in front[j])
				{
					ranking[j].Add(population.Get(it1));
				}
			}
		}

		/// <summary>
		/// Returns a <code>SolutionSet</code> containing the solutions of a given rank.
		/// </summary>
		/// <param name="rank">The rank</param>
		/// <returns>Object representing the <code>SolutionSet</code>.</returns>
		public Population GetSubfront(int rank)
		{
			return ranking[rank];
		}

		/// <summary>
		/// Returns the total number of subFronts founds.
		/// </summary>
		/// <returns></returns>
		public int GetNumberOfSubfronts()
		{
			return ranking.Length;
		}
	}
}
