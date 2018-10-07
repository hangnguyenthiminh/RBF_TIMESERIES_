using System.Collections.Generic;

namespace RBF_TIMESERIES.Utils
{
	public class ObjectiveComparator : IComparer<Individual>
	{
		#region Private attibutes

		/// <summary>
		/// Stores the index of the objective to compare
		/// </summary>
		private int nObj;
		private bool ascendingOrder;

		#endregion

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nObj">The index of the objective to compare</param>
		public ObjectiveComparator(int nObj)
		{
			this.nObj = nObj;
			ascendingOrder = true;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nObj">The index of the objective to compare</param>
		/// <param name="descendingOrder">The descending order</param>
		public ObjectiveComparator(int nObj, bool descendingOrder)
		{
			this.nObj = nObj;
			this.ascendingOrder = !descendingOrder;
		}

		#endregion

		#region Implement Interface

		public int Compare(Individual individual1, Individual individual2)
		{
			int result;

			if (individual1 == null)
			{
				result = 1;
			}
			else if (individual2 == null)
			{
				result = -1;
			}
			else
			{
				double objective1 = individual1.Objective[nObj];
				double objective2 = individual2.Objective[nObj];

				if (ascendingOrder)
				{
					if (objective1 < objective2)
					{
						result = -1;
					}
					else if (objective1 > objective2)
					{
						result = 1;
					}
					else
					{
						result = 0;
					}
				}
				else
				{
					if (objective1 < objective2)
					{
						result = 1;
					}
					else if (objective1 > objective2)
					{
						result = -1;
					}
					else
					{
						result = 0;
					}
				}
			}

			return result;
		}

		#endregion
	}
}
