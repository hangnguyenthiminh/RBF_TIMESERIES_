using System;
using System.Collections.Generic;

namespace RBF_TIMESERIES.Utils
{
	public interface IConstraintViolationComparator : IComparer<Individual>
	{
		bool NeedToCompare(Individual s1, Individual s2);
	}
}
