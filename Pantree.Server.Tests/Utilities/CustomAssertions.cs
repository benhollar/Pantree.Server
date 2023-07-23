using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pantree.Server.Tests.Utilities
{
    internal static class CustomAssertions
    {
        internal static void EqualUnordered<T>(IEnumerable<T>? collection1, IEnumerable<T>? collection2)
        {
            if (collection1 is null || collection2 is null)
            {
                Assert.Null(collection1);
                Assert.Null(collection2);
                return;
            }

            List<T> lhs = collection1.ToList();
            List<T> rhs = collection2.ToList();

            Assert.Equal(lhs.Count, rhs.Count);
            foreach (T item in lhs)
                Assert.Equal(1, rhs.Count(x => item?.Equals(x) ?? false));
        }
    }
}
