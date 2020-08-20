# CommandForwarder

Console application for forwarding commands and arguments.

### How to

Easily create a composite console application that forwards calls to other applications, along with arguments, through a JSON defintion. 

```json
{
	"definitions": [
		{
			"name": "myApp",
			"definitions": [
				{
					"name": "verb1",
					"definitions": [
						{
							"name": "verb2",
							"command": "path/to/application"
						}
					]
				}
			]
		},
		{
			"name": "otherApp",
			"command": "path/to/application"
		}
	]
}
```

Usage:

```bat
CommandForwarder.exe -c path/to/config -a myApp verb1 verb2 "arguments" "to" "forward"
```

Or (for Windows) create a batch file or similar like the following that's available in PATH:

```bat
:: startMyApp.bat

@echo off
CommandForwarder.exe -c path/to/config -a myApp %*
```

And use it as follows from everywhere:

```bat
startMyApp verb1 verb2 "arguments" "to" "forward"
```
