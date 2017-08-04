To install use  
$ cinst NetfoxDetective --s "$pwd" -f # from binary directory ... 

References:
	https://github.com/chocolatey/choco/wiki/CreatePackagesQuickStart
	https://github.com/chocolatey/choco/wiki/CreatePackages

Deployment:
	choco apiKey -k a57fb895-1628-4e9a-a738-84db7898c3f1 -source https://chocolatey.org/
	choco push NetfoxDetective.1.0.nupkg 

