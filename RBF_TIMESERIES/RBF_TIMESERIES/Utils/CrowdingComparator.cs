using System.Collections.Generic;

namespace RBF_TIMESERIES.Utils
{
	public class CrowdingComparator : IComparer<Individual_NSGA>
	{
		public int Compare(Individual_NSGA s1, Individual_NSGA s2)
		{
			if (s1 == null)
			{
				return 1;
			}
			else if (s2 == null)
			{
				return -1;
			}

			int flagComparatorRank = RankCompare(s1, s2);
			if (flagComparatorRank != 0)
			{
				return flagComparatorRank;
			}

			/* His rank is equal, then distance crowding comparator */
			double distance1 = s1.CrowdingDistance;
			double distance2 = s2.CrowdingDistance;
			if (distance1 > distance2)
			{
				return -1;
			}

			if (distance1 < distance2)
			{
				return 1;
			}

			return 0;
		}

        public int RankCompare(Individual_NSGA s1, Individual_NSGA s2)
        {
            if (s1 == null)
            {
                return 1;
            }
            else if (s2 == null)
            {
                return -1;
            }


            if (s1.Rank < s2.Rank)
            {
                return -1;
            }

            if (s1.Rank > s2.Rank)
            {
                return 1;
            }

            return 0;
        }
    }
}
