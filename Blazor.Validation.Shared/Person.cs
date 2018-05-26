using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Blazor.Validation.Shared
{
  public class Person : IDataErrorInfo, INotifyDataErrorInfo
  {
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public int Age { get; set; }

    const int minAge = 18;

    public string this[string property]
    {
      get
      {
        return GetErrors(property).FirstOrDefault<string>();
      }
    }

    public string Error
    {
      get
      {
        return null;
      }
    }

    public bool HasErrors => GetErrors(null).Any();

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    public IEnumerable GetErrors(string property)
    {
      if (property == null || property == nameof(this.FirstName))
      {
        if (string.IsNullOrEmpty(this.FirstName))
        {
          yield return $"{nameof(FirstName)} is mandatory";
        }
        if( this.FirstName.Length < 2 )
        {
          yield return $"{nameof(FirstName)} '{this.FirstName}' is too short.";
        }
        if( this.FirstName == "Q")
        {
          yield return $"{nameof(FirstName)} 'Q' is reserved for extra-dimensional beings!";
        }
      }
      if (property == null || property == nameof(this.LastName))
      {
        if (string.IsNullOrEmpty(this.LastName))
        {
          yield return $"{nameof(LastName)} is mandatory";
        }
        if (this.LastName == "Doe")
        {
          yield return $"{nameof(LastName)} cannot be 'Doe'";
        }
      }
      if (property == null || property == nameof(this.Age))
      {
        if (Age < minAge)
        {
          yield return $"{nameof(Age)} should be at least {minAge}";
        }
      }
    }
  }
}
