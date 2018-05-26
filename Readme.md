# Blazor Validation

Blazor is a framework that allows you to build rich web pages and SPAs using .NET standard libraries and C#.

> At the time of writing Blazor is experimental!

In this blog post I want to talk about forms validation with blazor. Since there is no official validation framework yet in blazor I've decided to build it myself. Here is an example of validation in action:

![Blazor Validation](https://u2ublogimages.blob.core.windows.net/peter/BlazorValidation.PNG)

So how does this work? My blazor component looks like this:

``` cshtml
@page  "/validation"
@addTagHelper *, Blazor.Validation

<div class="validation">
  <p>
    <label for="firstname">First Name</label>
    <input id="firstname" type="text" bind="@Person.FirstName" />
    <ValidationError Subject="@Person" Property="@nameof(Person.FirstName)" />
  </p>
  <p>
    <label for="lastname">Last Name</label>
    <input id="lastname" type="text" bind="@Person.LastName" />
    <ValidationError Subject="@Person" Property="@nameof(Person.LastName)" />
  </p>
  <p>
    <label for="age">Age</label>
    <input id="age" type="number" bind="@Person.Age" />
    <ValidationError Subject="@Person" Property="@nameof(Person.Age)" />
  </p>
</div>
<hr/>
<div>
  <ValidationSummary Subject="@Person"/>
</div>

@functions {
public Person Person { get; set; } = new Person { FirstName = "John", LastName = "Doe", Age = 32 };
}
```

As you can see I'm using two components: `ValidationError` and `ValidationSummary`. The first one is used to display specific problems with a `Subject`'s `Property`, and the second one gives you an overview of all validation errors on the `Subject`.

## Validating entities with .NET

.NET has a couple of built-in mechanisms for validation, and I support the two most important: `IDataErrorInfo` and `INotifyDataErrorInfo`.

`System.ComponentModel.IDataErrorInfo` has been around since .NET 1.0, and allows you to return an error for each property, and for the whole object:

> `System.ComponentModel` contains classes and interfaces which are available throughout most of .NET, including `INotifyPropertyChanged` and is in my opinion one of the most important namespaces!

``` csharp
// Summary:
//     Provides the functionality to offer custom error information that a user interface
//     can bind to.
[DefaultMember("Item")]
public interface IDataErrorInfo
{
  //
  // Summary:
  //     Gets the error message for the property with the given name.
  //
  // Parameters:
  //   columnName:
  //     The name of the property whose error message to get.
  //
  // Returns:
  //     The error message for the property. The default is an empty string ("").
  string this[string columnName] { get; }

  //
  // Summary:
  //     Gets an error message indicating what is wrong with this object.
  //
  // Returns:
  //     An error message indicating what is wrong with this object. The default is an
  //     empty string ("").
  string Error { get; }
}
```

The biggest drawback of `IDataErrorInfo` is that you can only return one error per property, but on the other hand this interface is supported by WinForms, WPF, ASP.NET Mvc, etc...

`INotifyDataErrorInfo` is a more modern version of `IDataErrorInfo`:

``` csharp
// Summary:
//     Defines members that data entity classes can implement to provide custom synchronous
//     and asynchronous validation support.
public interface INotifyDataErrorInfo
{
  //
  // Summary:
  //     Gets a value that indicates whether the entity has validation errors.
  //
  // Returns:
  //     true if the entity currently has validation errors; otherwise, false.
  bool HasErrors { get; }

  //
  // Summary:
  //     Occurs when the validation errors have changed for a property or for the entire
  //     entity.
  event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

  //
  // Summary:
  //     Gets the validation errors for a specified property or for the entire entity.
  //
  // Parameters:
  //   propertyName:
  //     The name of the property to retrieve validation errors for; or null or System.String.Empty,
  //     to retrieve entity-level errors.
  //
  // Returns:
  //     The validation errors for the property or entity.
  IEnumerable GetErrors(string propertyName);
}
```

This interface allows multiple errors per property.

Here is my implementation of a simple `Person` class supporting both interfaces:

``` csharp
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
```

## Onto ValidationError

Let's look at the `ValidationError` Blazor component:

``` csharp
@using System.ComponentModel
@using Microsoft.AspNetCore.Blazor.Components

@if (Errors.Any())
{
  <div class="validationerror">
    <ul>
      @foreach (var error in Errors)
      {
        <li>@error</li>
      }
    </ul>
  </div>
}

@functions {

    [Parameter]
    protected object Subject { get; set; }

    [Parameter]
    protected string Property { get; set; }

    public IEnumerable<string> Errors
    {
      get
      {
        switch (Subject)
        {
          case null:
            yield return $"{nameof(Subject)} has not been set!";
            yield break;
          case INotifyDataErrorInfo ine:
            if( Property == null )
            {
              yield return $"{nameof(Property)} has not been set!";
              yield break;
            }
            foreach (var err in ine.GetErrors(Property))
            {
              yield return (string)err;
            }
            break;
          case IDataErrorInfo ide:
            if( Property == null )
            {
              yield return $"{nameof(Property)} has not been set!";
              yield break;
            }
            string error = ide[Property];
            if (error != null)
            {
              yield return error;
            }
            else
            {
              yield break;
            }
            break;
        }
      }
    }
}
```

As you can see, it is pretty simple. First I check if there are any error, and then I use a `div` with nested `ul` and one `li` per error.

The `div` has a class `validationerror` associated with it so you can style it:

``` css
.validationerror > ul > li {
  color:red;
}
```

The `Errors` property returns an `IEnumerable<string>` of errors, which checks the `Subject` for one of the interfaces using the new C# 7 pattern matching syntax:

``` csharp
switch (Subject)
{
  case null:
    yield return $"{nameof(Subject)} has not been set!";
    yield break;
  case INotifyDataErrorInfo ine:
    ...
  case IDataErrorInfo ide:
    ...
}
```

You can learn more about C# 7 pattern mathing (here)[https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-7#pattern-matching]:

If `Subject` has not been set I return a validation error to the developer! Otherwise I read the validation errors through the interface and return them using C#'s `yield` syntax.

Again, if `yield` is not familiar, read [this](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/yield).

## ValidationSummary

The `ValidationSummary` components works in very much the same way:

``` cshtml
@using System.ComponentModel
@using Microsoft.AspNetCore.Blazor.Components
@using System.Reflection

@if (Errors.Any())
{
  <div class="validationsummary">
    <ul>
      @foreach (var error in Errors)
      {
        <li>@error</li>
      }
    </ul>
  </div>
}

@functions {

    [Parameter]
    protected object Subject { get; set; }

    public IEnumerable<string> Errors
    {
      get
      {
        string error = null;
        switch (Subject)
        {
          case null:
            yield return $"{nameof(Subject)} has not been set!";
            yield break;
          case INotifyDataErrorInfo ine:
            foreach (var property in PropertyNames)
            {
              foreach (string err in ine.GetErrors(property))
              {
                yield return err;
              }
            }
            break;
          case IDataErrorInfo ide:
            // IDataErrorInfo.Error: what is wrong with this object.
            error = ide.Error;
            if( error != null )
            {
              yield return error;
            }
            // IDataErrorInfo[property]: what is wrong with this object's property
            foreach (var property in PropertyNames)
            {
              error = ide[property];
              if (error != null)
              {
                yield return ide[property];
              }
            }
            break;
        }
      }
    }

    private List<string> propertyNames = null;

    private List<string> PropertyNames
    => propertyNames ?? (propertyNames = GetSubjectPropertyNames().ToList());

    private IEnumerable<string> GetSubjectPropertyNames()
    {
      return Subject.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(pi => pi.Name);
    }
}
```

The big difference here is that I need to show all property's errors, and for this I use a little reflection:

``` csharp
private List<string> propertyNames = null;

private List<string> PropertyNames
=> propertyNames ?? (propertyNames = GetSubjectPropertyNames().ToList());

private IEnumerable<string> GetSubjectPropertyNames()
{
  return Subject.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(pi => pi.Name);
}
```

Because **reflection is slow** I cache all properties so I only have to get them once.

## Summary

That's it! Because Blazor allows me to use the same constructs as normal .net, I can use familiar interfaces and techniques.

The full project is available on (my GitHub)[https://github.com/PeterHimschoot/Blazor.Validation].




