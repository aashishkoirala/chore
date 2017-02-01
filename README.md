### **Chore** - A task management tool
##### By [Aashish Koirala](https://aashishkoirala.github.io)

[GitHub Repository](https://github.com/aashishkoirala/chore) | [Go to Application](http://chore.apphb.com)

CHORE is a task management application that I built for personal use. Hopefully it comes in handy for you too. This version consists of the following features:

+ Create and track tasks based on deadlines or recurrence.
+ Organize tasks into folders.
+ Use built-in or custom filters to look for tasks.
+ Get a weekly calendar view based on your tasks.
+ Export and import tasks to/from flat data.
+ Export and import all user data.

###
#### Technology

The following is an outline of all the major tools, technologies, patterns and practices in building this application.  

**Platform**

*   OS- [Windows](http://windows.microsoft.com/en-us/windows/home)
*   Runtime- [Microsoft .NET 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653)
*   Database- [MongoDB](http://www.mongodb.com/)

**Languages**

*   [C#](http://msdn.microsoft.com/en-us/vstudio/hh341490.aspx) (Server-side)
*   [JavaScript](https://developer.mozilla.org/en-US/docs/Web/JavaScript) (Client-side)

**Patterns/Principles**

*   [Domain Driven Design (DDD)](http://en.wikipedia.org/wiki/Domain-driven_design)
*   [Service Oriented Architecture (SOA)](http://en.wikipedia.org/wiki/Service-oriented_architecture)
*   [SOLID](http://en.wikipedia.org/wiki/SOLID_%28object-oriented_design%29)
*   [Aspect Oriented Programming (AOP)](http://en.wikipedia.org/wiki/Aspect-oriented_programming)
*   [Single Page Application (SPA)](http://en.wikipedia.org/wiki/Single-page_application)
*   [Unit-of-Work](http://martinfowler.com/eaaCatalog/unitOfWork.html)
*   [Repository](http://martinfowler.com/eaaCatalog/repository.html)
*   [Federated Claims-Based Authentication](http://en.wikipedia.org/wiki/Federated_identity)
*   [Responsive Web Design (RWD)](http://en.wikipedia.org/wiki/Responsive_web_design)

**Major Libraries**

*   [ASP.NET Web API](http://www.asp.net/web-api) (REST API)
*   [ASP.NET MVC](http://www.asp.net/mvc) (Server side web)
*   [WCF](http://msdn.microsoft.com/en-us/library/ms731082%28v=vs.110%29.aspx) (Communication with [AK-Login](http://aashishkoirala.github.io/login/))
*   [WIF](http://msdn.microsoft.com/en-us/library/hh291066%28v=vs.110%29.aspx) (Federated authentication with [AK-Login](http://aashishkoirala.github.io/login/))
*   [LINQ](http://msdn.microsoft.com/en-us/library/bb397926.aspx) (General language feature)
*   [LINQ Expressions](http://msdn.microsoft.com/en-us/library/bb397926.aspx) (Predicate mapping)
*   [MEF](http://msdn.microsoft.com/en-us/library/dd460648%28v=vs.110%29.aspx) (Dependency Injection)
*   [AngularJS](https://angularjs.org/) (Client side application)
*   [AngularJS Routes](https://docs.angularjs.org/tutorial/step_07) (Client side routing)
*   [AngularJS Resoruces](https://docs.angularjs.org/api/ngResource/service/$resource) (Communication with REST API)
*   [Bootstrap](http://getbootstrap.com/) (UI Design)
*   [UI-Bootstrap](http://angular-ui.github.io/bootstrap/) (Angular-Bootstrap interface)
*   [SignalR](http://signalr.net/) (Push notifications)
*   [Commons Library](http://aashishkoirala.github.io/commons/) (Foundational framework)
*   [Commons Web Library](http://aashishkoirala.github.io/commons/) (Foundational framework for web)
*   [Commons Library MongoDB Provider](http://aashishkoirala.github.io/commons-providers/) (UOW/Repo abstraction for MongoDB)
*   [MongoDB C# Driver](http://docs.mongodb.org/ecosystem/drivers/csharp/) (Communication with MongoDB)
*   [MS Test](http://en.wikipedia.org/wiki/MSTest) (C#-based Tests)
*   [Jasmine](http://jasmine.github.io/) (JavaScript-based Tests)
*   [Moq](https://github.com/Moq/moq4) (Mocking for C#-based Tests)
*   [Angular Mocks](https://docs.angularjs.org/api/ngMock) (Mocking for Angular components in JavaScript-based Tests)

**Tools**

*   IDE- [Visual Studio 2012](http://www.visualstudio.com/)

###
#### Terms of Service/Privacy Policy

Links to the terms of service and privacy policy for the application will be presented to you when logging in. By continuing to log in, you will denote that you agree to these terms of service and privacy policy.  

Do note that the application will allow you to download all your data and delete your profile at any point should you choose to stop using it.