using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Blazor.Validation.Shared
{
  public static class EnumerableExtensions
  {
    public static bool Any(this IEnumerable e) => e.GetEnumerator().MoveNext() == true;

    public static T FirstOrDefault<T>(this IEnumerable e)
    {
      var enumerator = e.GetEnumerator();
      if( enumerator.MoveNext() )
      {
        return (T)enumerator.Current;
      }
      return default(T);
    }
  }
}
