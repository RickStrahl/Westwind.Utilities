# Westwind.Utilities 

[![NuGet](https://img.shields.io/nuget/v/Westwind.Utilities.svg)](https://www.nuget.org/packages/Westwind.Utilities/)
![](https://img.shields.io/nuget/dt/Westwind.Utilities.svg)
[![Documentation](https://img.shields.io/badge/documentation-blue.svg)](https://docs.west-wind.com/westwind.utilities/)

### A general purpose utility and helper library for .NET development

> Moved from [Westwind.Toolkit repo](https://github.com/rickstrahl/WestwindToolkit)
> * Added support for .NET Standard 2.0
> * Targets .NET 4.5, 4.0 and NetStandard 2.0

### Installation
You can install the package [from NuGet](http://nuget.org/packages/Westwind.Utilities/) using the Visual Studio Package Manager or NuGet UI:

```
PM> install-package westwind.utilities
```

or the `dotnet` command line:

```
dotnet add package westwind.utilities
```

### What is it?
Every .NET application requires small, common and often repeated tasks. This library is a collection of those things that I commonly need on a regular basis and have compiled over the years.

* [Get it on NuGet](https://nuget.org/packages/Westwind.Utilities/)
* [Usage Documentation](https://docs.west-wind.com/westwind.utilities/_5am0u0jou.htm)
* [Class Documentation](https://docs.west-wind.com/westwind.utilities/_5am0u09dd.htm)

It includes tools for:

* [**Application Configuration**](https://docs.west-wind.com/westwind.utilities?page=_2le027umn.htm)  
class to create code-first strongly typed configuration classes for your applications

* [**Lightweight ADO.NET Data Access Layer**](https://docs.west-wind.com/westwind.utilities?=page=_3ou0v2jum.htm)  
ideal for components or apps that need data access but don't need the bulk of Entity Framework or similar ORM

* **General Purpose Utility Classes**:
	* [StringUtils](https://docs.west-wind.com/westwind.utilities?topic=Class%20StringUtils)
    * [HtmlUtils](https://docs.west-wind.com/westwind.utilities?topic=Class%20HtmlUtils)
	* [ReflectionUtils](https://docs.west-wind.com/westwind.utilities?topic=Class%20ReflectionUtils)
	* [SerializationUtils](https://docs.west-wind.com/westwind.utilities?topic=Class%20SerializationUtils)
	* [DataUtils](https://docs.west-wind.com/westwind.utilities?topic=Class%20DataUtils)	
	* [FileUtils](https://docs.west-wind.com/westwind.utilities?topic=Class%20FileUtils)
    * [TimeUtils](https://docs.west-wind.com/westwind.utilities?topic=Class%20TimeUtils)	
    * [XmlUtils](https://docs.west-wind.com/westwind.utilities?topic=Class%20TimeUtils)	    
    * [StringSerializer](https://docs.west-wind.com/westwind.utilities?topic=Class%20StringSerializer)
    * [Expando](https://docs.west-wind.com/westwind.utilities?topic=Class%20Expando)
	* [PropertyBag](https://docs.west-wind.com/westwind.utilities?topic=Class%20PropertyBag)
    * [Scheduler](https://docs.west-wind.com/westwind.utilities?topic=Class%20Scheduler) (for background processing) 
    * [Encryption](https://docs.west-wind.com/westwind.utilities?topic=Class%20Encryption)
    * [HttpClient](https://docs.west-wind.com/westwind.utilities?topic=Class%20HttpClient) (HttpWebRequest wrapper)
    * [HttpUtils](https://docs.west-wind.com/westwind.utilities?topic=Class%20HttpUtils) (Simple REST client helpers)
    * [SmptClientNative](https://docs.west-wind.com/westwind.utilities?topic=Class%20SmtpClientNative) (SmtpClient Wrapper)
    
    * [DelegateFactory](https://docs.west-wind.com/westwind.utilities?topic=Class%20DelegateFactory)

and much, much more.

It's worthwhile to browse through the source code or the documentation
to find out the myriad of useful functionality that is available, all
in a small single assembly.

This assembly is the base for most other West Wind libraries.