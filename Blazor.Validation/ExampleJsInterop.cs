using System;
using Microsoft.AspNetCore.Blazor.Browser.Interop;

namespace Blazor.Validation
{
    public class ExampleJsInterop
    {
        public static string Prompt(string message)
        {
            return RegisteredFunction.Invoke<string>(
                "Blazor.Validation.ExampleJsInterop.Prompt",
                message);
        }
    }
}
