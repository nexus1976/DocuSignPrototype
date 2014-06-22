This is an ASP.NET MVC5 application. 
You must have installed Visual Studio 2013 Premium or better with all of the updates and an internet connection in order to download any missing nuget packages.
This project is using the Razor templating engine and utilizes JQuery and Bootstrap.js for the browser-side technology.
This project is also using Entity Framework 6 Code First with Code First Migrations. This requires that you have some locally installed version of SQL Server.
By default it will look for SQL Server Express 2012 at the instanced name .\SQLEXPRESS. 
Both Visual Studio 2012 and 2013 (if you do the full install) will provide you a sufficient SQLExpress engine that will get used automatically when running the project
or you can manually download it from Microsoft.

The purpose of this application is to serve as a prototype for proving out functionality related to integrating with DocuSign's SaaS product that facilitates the electronic signing
of documents.

You must also update the web.config to specify your own DocuSign account info. You can register a demo account at http://www.docusign.com/developer-center .

The settings you will need to update are the following keys in the appSettings section:
DocuSign.UserName
DocuSign.Password
DocuSign.IntegratorKey
DocuSign.AccountId

Author: Daniel Graham
5/22/2014