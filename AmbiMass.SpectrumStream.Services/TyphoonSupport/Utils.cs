using System.Collections.Generic;
using System.Linq;

namespace AmbiMass.SpectrumStream.Services.TyphoonSupport
{
    class Utils
    {
        public static string CollapseToRange(IEnumerable<int> list)
        {
            return string.Join(",", Ranges(list).Select(r => r.end == r.start ? $"{r.start}" : $"{r.start}-{r.end}"));
        }

        private static IEnumerable<(int start, int end)> Ranges(IEnumerable<int> numbers)
        {
            if (numbers == null) yield break;
            using var e = numbers.GetEnumerator();
            if (!e.MoveNext()) yield break;

            int start = e.Current;
            int end = start;
            while (e.MoveNext())
            {
                if (e.Current - end != 1)
                {
                    if (end - start == 1)
                    {
                        yield return (start, start);
                        yield return (end, end);
                    }
                    else
                    {
                        yield return (start, end);
                    }

                    start = e.Current;
                }

                end = e.Current;
            }

            yield return (start, end);
        }
    }

}