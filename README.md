# LogCastle Library

LogCastle is a library designed to make logging operations easier and more flexible in .NET projects. Using the AOP (Aspect-Oriented Programming) approach, it allows you to enable logging operations on the basis of class, interface and method. This library, built on the proxy design pattern, is used to log the runtime behavior of certain methods.
## Initially  


###  Setup 

You can add the LogCastle library to your project via the NuGet package manager. 

```csharp 
dotnet add package LogCastle   
```

__How does it work?:__   

LogCastle has a LogInterceptor class that performs logging before and after method calls. By default this class prints log messages to the console or file. However, it can be customized depending on the need.

__Configuration:__  

A configuration file is needed to use LogCastle. The required registration process in the Program.cs or Startup.cs file is as follows.

```csharp  
using LogCastle.Extensions;

var builder = WebApplication.CreateBuilder(args);

// LogCastle servislerini ve ayarlarını yapılandırma dosyasından yükle.
builder.Services.AddLogCastleConfigurations(builder.Configuration);
var app = builder.Build();  

```
__appsettings.json__  

```json
"LogCastle": {
    "Enabled": true,
    "MinimumLevel": "Information",
    "Filter": {
      "IgnoreTypes": [ "SomeNamespace.SomeClass" ],
      "IgnoreMethods": [ "SomeMethod" ]
    },
    "Providers": {
      "Console": {
        "Type": "LogCastle.Providers.ConsoleLogProvider, LogCastle",
        "Enabled": true
      },
      "File": {
        "Type": "LogCastle.Providers.FileLogProvider, LogCastle",
        "Enabled": false,
        "Parameters": {
          "FilePath": "path/to/your/logfile.txt"
        }
      }
    }
  }  

```

__Description of Configuration File__:  

This configuration file defines whether logging is enabled, the minimum logging level, which types and methods will be exempt from logging, and which logging providers to use.

_"Enabled":_  Indicates whether logging is generally enabled or not.  
_"MinimumLevel":_ Specifies the minimum level at which logging will occur. In this example, level _"Information"_ and above will be logged.  
_"Filter":_ Used to exclude certain types or methods from logging.  
_"IgnoreTypes":_ Contains class types to exclude from logging.      
_"IgnoreMethods":_ Contains method names to be excluded from logging.   
_"Providers":_ It contains the destinations to which log messages are sent and their parameters.  
_"Console":_ It defines the provider that will log to the console.  
_"Type":_ Contains the fully qualified class name of the Provider and the assembly in which it is located.   
_"Enabled":_ Indicates whether this provider is active or not.  
_"File":_ It defines the provider that will log to the file.  
_"Type":_ Contains the fully qualified class name of the Provider and the assembly in which it is located.  
_"Enabled":_ Indicates whether this provider is active or not.  
_"Parameters":_ Contains extra parameters for the Provider. In this example, the file path is specified.  


### How to use 

__Using LogCastleAttribute:__  

`LogCastleAttribute` You can log methods at the Method, Class, Interface level you want using  

__Level Parameter:__  

There are 7 log levels. These are respectively:    

```csharp
namespace LogCastle.Enums
{
    public enum Level
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical,
        Fatal
    }
}

```

The default log Level value is 'Information'.   

__Class Level Usage:__  

You can use `LogCastleAttribute` at the class level to log all methods in the class. The methods you want to log must be `virtual`.


```csharp  

[LogCastle]
public class MyService
{
    public virtual void MyMethod()
    {
        // Logic
    }
}

```  

Then you can register your `MyService` class with LogCastle proxy. The default lifetime is `Scoped`.    


```csharp 

builder.Services.AddLogCastle<MyService>();  

```  

To register to an IoC container with a different lifetime:


```csharp  
builder.Services.AddLogCastle<MyService>(ServiceLifetime.Transient);   

``` 

__Use at Method Level:__  

You can use the specific methods you want to log by marking them with 'LogCastleAttribute'. The methods you want to log must be `virtual`.  

```csharp  

public class MyService
{
    [LogCastle]
    public virtual void MyMethod()
    {
        // Logic
    }

    [LogCastle(Level.Fatal)]
    public virtual void MyMethod2(string arg)
    {
        // Logic
    }
}  

```


Then you can register your `MyService` class in the IoC container with LogCastle proxy. The default lifecycle is `Scoped`.

```csharp  

builder.Services.AddLogCastle<MyService>();    

```  

To register to an IoC container with a different lifetime:  

```csharp  

builder.Services.AddLogCastle<MyService>(ServiceLifetime.Transient);  

```

__Interface Level Usage:__  

You can use the following to log the interface and the classes that implement it.  


```csharp
[LogCastle]
public interface IMyService
{
    void MyMethod();
}

public class MyService : IMyService
{
    public void MyMethod()
    {
        // Logic
    }
}

or

public interface IMyService
{
    [LogCastle]
    void MyMethod();
    [LogCastle(Level.Error)]
    int GetMethod();
}

public class MyService : IMyService
{
    public void MyMethod()
    {
        // Logic
    }

    public void GetMethod()
    {
        // Logic
    }
}


```
Then you can register with LogCastle proxy for `IMyService` Interface based proxy registration. The default lifetime is `Scoped`.  


```csharp  

builder.Services.AddLogCastle<IMyService, MyService>();

``` 
To register to an IoC container with a different lifetime: 

```csharp
builder.Services.AddLogCastle<IMyInterface, MyService>(ServiceLifetime.Singleton);  

```


__Decorator Usage:__

You can use the `DecorateWithLogCastle<>` method to decorate already registered services with the LogCastle proxy. 

```csharp
builder.Services.AddTransient<IMyService, MyService>();
builder.Services.DecorateWithLogCastle<IMyService, MyService>();  

```

__Assembly Level Registration:__  

In the specified assembly, it scans the interfaces marked with LogCastleAttribute and their implementations, creates a proxy for them and decorates the existing service record with the proxy. Each decorated service preserves the lifetime (scoped, transient, singleton) of the original service record.


```csharp  
builder.Services.AddLogCastleFromAssembly(Assembly.GetExecutingAssembly());  
```

__Usage at Controller Level:__  

A special lifetime is used for the .NET Core controller. As at the class level, the methods you want to log must be `virtual`.   


```csharp
using LogCastle.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace ValuesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [LogCastle]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public virtual IActionResult Get()
        {
            return Ok();
        }
    }
}

```

Then you can use the `AddLogCastleForControllers` method to proxy register __all__ controllers in the application.

```csharp
builder.Services.AddLogCastleForControllers();  

```

For a __specific__ contoller proxy registration, you can use the `AddLogCastleForController<>` method.    


```csharp
builder.Services.AddLogCastleForController<WeatherForecastController>();  

```

__Using MaskAttribute__:

When logging with LogCastle, it masks your sensitive `string` data with `*` of a certain length starting from a certain starting point with `MaskAttribute`.


With the Start and Length parameters, the index and masking length at which the masking process will begin are determined.
The Start parameter cannot be less than 0 and the Length parameter must be greater than 1.


__Property Level Usage:__   

The following usage masks the first 6 characters of the `Password` property and the first 9 characters of the `CreditCardNumber` property in the `UserRequest` class.


__Test Service__  

```csharp   

using LogCastle.Attributes;

namespace TestLogCastleCoreAPI.Services
{
    [LogCastle]
    public interface ITestServices
    {      
        UserRequest GetUser(UserRequest user);    
    }

    public class TestServices : ITestServices
    {
        public UserRequest GetUser(UserRequest user)
        {
            return user;
        }
    }
    public class UserRequest
    {
        public string UserName { get; set; }
        [Mask(1, 6)]
        public string Password { get; set; }
        [Mask(1, 9)]
        public string CreditCardNumber { get; set; }
    }
}  

```

__Controller__  


```csharp  

using LogCastle.Attributes;
using Microsoft.AspNetCore.Mvc;
using TestLogCastleCoreAPI.Services;

namespace TestLogCastleCoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [LogCastle]
    public class TestController : ControllerBase
    {
        private readonly ITestServices testServices;

        public TestController(ITestServices testServices)
        {
            this.testServices = testServices;
        }     

        [HttpGet("GetUser")]
        public virtual IActionResult GetUser()
        {
            var user = new UserRequest()
            {
                CreditCardNumber = "5425233430109903",
                Password = "v€RySecreTPa$$word",
                UserName = "samet.kayikci@turk.net",
            };
            var response = testServices.GetUser(user);
            return Ok(response);
        }      
    }
}  
   
```    

Console output will look like below.

```
[TimeStamp] 2023.11.17 01:14:28 [Level] Information [Namespace] TestLogCastleCoreAPI.Services.TestServices.UserRequest.GetUser(UserRequest user) [Args] user={UserName="samet.kayikci@turk.net", Password=v******reTPa$$word, CreditCardNumber=5*********109903} [Host] TNL5CG2174S52 [AppName] TestLogCastleCoreAPI.dll [ElapsedTime] 3 ms [ReturnValue] {UserName="samet.kayikci@turk.net", Password=v******reTPa$$word, CreditCardNumber=5*********109903}  
[TimeStamp] 2023.11.17 01:14:28 [Level] Information [Namespace] TestLogCastleCoreAPI.Controllers.TestController.IActionResult.GetUser() [Args]  [Host] TNL5CG2174S52 [AppName] TestLogCastleCoreAPI.dll [ElapsedTime] 10 ms [ReturnValue] {"Value":"{UserName=\samet.kayikci@turk.net\, Password=v******reTPa$$word, CreditCardNumber=5*********109903}","StatusCode":200}
```

__Parameter Level Usage:__  

You can use it in the MaskAttribute method parameter.  

If we want to mask the 'Email' information in the return value, we can use MaskAttribute in the relevant class.  

__Service__  


```csharp  
using LogCastle.Attributes;
namespace TestLogCastleCoreAPI.Services
{
    [LogCastle]
    public interface IUserServices
    {
        LoginResponse Login(string username, [Mask(2, 5)] string password);
    }

    public class UserServices : IUserServices
    {
        public LoginResponse Login(string username, [Mask(2, 5)] string password)
        {
            return new LoginResponse { Email = "samet.kayikci@gmail.com", FullName = "Samet KAYIKCI" };
        }
    }
}  

```  

```csharp  

using LogCastle.Attributes;
namespace TestLogCastleCoreAPI.Services
{   
    public class LoginResponse
    {
        public string FullName { get; set;}
        [Mask(3, 11)]
        public string Email { get; set;}
    }
}

```  

__TestController__   

```csharp   
using LogCastle.Attributes;
using Microsoft.AspNetCore.Mvc;
using TestLogCastleCoreAPI.Services;

namespace TestLogCastleCoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [LogCastle]
    public class TestController : ControllerBase
    {
        private readonly IUserServices userServices;

        public TestController(IUserServices userServices)
        {
            this.userServices = userServices;
        }

        [HttpPost("Login")]
        public virtual IActionResult Login(string username, [Mask(2, 5)] string password)
        {
            var response = userServices.Login(username, password);
            return Ok(response);
        }
    }
}

```   

Console output will look like this:    


```   

[TimeStamp] 2023.11.17 12:16:35 [Level] Information [Namespace] TestLogCastleCoreAPI.Services.UserServices.LoginResponse.Login(String username, String password) [Args] username=sametk, password=93*****word [Host] TNL5CG2174S52 [AppName] TestLogCastleCoreAPI.dll [ElapsedTime] 0 ms [ReturnValue] {FullName="Samet KAYIKCI", Email=sam***********turk.net}
[TimeStamp] 2023.11.17 12:16:35 [Level] Information [Namespace] TestLogCastleCoreAPI.Controllers.TestController.IActionResult.Login(String username, String password) [Args] username=sametk, password=93*****word [Host] TNL5CG2174S52 [AppName] TestLogCastleCoreAPI.dll [ElapsedTime] 15 ms [ReturnValue] {"Value":"{FullName=\"Samet KAYIKCI\", Email=sam***********turk.net}","StatusCode":200}
```   

__Logging with Logger Class:__   

`Logger` class allows you to create simple and general purpose log records at different levels (trace, debug, information, warning, error, fatal, critical).


__How to use?__  

```csharp  
// Example Usage
Logger.LogInformation("Application is starting...");

```  

Console output will look like this:
```
[TimeStamp] 24.11.2023 11:37:20 [Level] Information [AppName] TestLogCastleCoreAPI [Host] TNL5CG2174S52 Application is starting...  
```

__Farklı Seviyelerde Loglama:__ 

```csharp  
Logger.LogTrace("This is a trace message.");
Logger.LogDebug("This is a debug message.");
Logger.LogInformation("This is an informational message.");
Logger.LogWarning("This is a warning message.");
Logger.LogFatal("This is a fatal error message.");
Logger.LogCritical("This is a critical error message.");  
```   

Console output will look like this:

```  
[TimeStamp] 24.11.2023 11:46:52 [Level] Trace [AppName] TestLogCastleCoreAPI [Host] TNL5CG2174S52 This is a trace message.
[TimeStamp] 24.11.2023 11:46:52 [Level] Debug [AppName] TestLogCastleCoreAPI [Host] TNL5CG2174S52 This is a debug message.
[TimeStamp] 24.11.2023 11:46:52 [Level] Information [AppName] TestLogCastleCoreAPI [Host] TNL5CG2174S52 This is an informational message.
[TimeStamp] 24.11.2023 11:46:52 [Level] Warning [AppName] TestLogCastleCoreAPI [Host] TNL5CG2174S52 This is a warning message.
[TimeStamp] 24.11.2023 11:46:52 [Level] Fatal [AppName] TestLogCastleCoreAPI [Host] TNL5CG2174S52 This is a fatal error message.
[TimeStamp] 24.11.2023 11:46:52 [Level] Critical [AppName] TestLogCastleCoreAPI [Host] TNL5CG2174S52 This is a critical error message.

```  
__Özel Hata Mesajları:__  

```csharp  
// When the application is started
Logger.LogInformation("Uygulama başlatılıyor...");

try
{
    // Application logic
    object? user = null;
    if (user is null)   
        throw new ArgumentNullException(nameof(user), "The user parameter cannot be null.");

catch (Exception ex)
{
    // In case of error
    Logger.LogError("Application error", ex);
}
```  

Console output will look like this:

```  
[TimeStamp] 24.11.2023 12:25:12 [Level] Information [AppName] TestLogCastleCoreAPI [Host] TNL5CG2174S52 Uygulama başlatılıyor...
[TimeStamp] 24.11.2023 12:25:12 [Level] Error [AppName] TestLogCastleCoreAPI [Host] TNL5CG2174S52 Uygulama hatası - [Error] user parametresi null olamaz. (Parameter 'user'),[StackTrace]    at TestLogCastleCoreAPI.Controllers.TestController.GetUser() in C:\Users\samet.kayikci\source\repos\TestLogCastleAPI\TestLogCastleCoreAPI\Controllers\TestController.cs:line 34
```  

__License__
All contents of this package are licensed under the Apache License 2.0.