// C# 14 (.NET 10) extension members
using System.Buffers;
using System.Text;

namespace Dsa;

public static class ListExtensions
{
    extension<T>(IList<T> list)
    {
        public void Swap(int i, int j) => (list[i], list[j]) = (list[j], list[i]);
    }
}
