# AnalyzerWebApiNoVoidReturn
Roslyn anaylzer that prohibits public WebApi methods to have 'void' return type, since it sometimes causes problem with corporate proxies.
Also contains a fix that will replace void return with a random integer return. 

This library is used as a medium complexity project in a series of tutorials on Roslyn:
http://ikoshelev.azurewebsites.net/search/id/5/Roslyn-beyond-'Hello-world'-01-Important-concepts-and-development-setup
http://ikoshelev.azurewebsites.net/search/id/7/Roslyn-beyond-'Hello-world'-02-Visual-Studio-extension-for-refactoring
http://ikoshelev.azurewebsites.net/search/id/8/Roslyn-beyond-'Hello-world'-03-Symbol-Graph-and-analyzer-diagnostics
