# CommandForwarder

Console application for forwarding commands and arguments.

### How to

Easily create a composite console application that forwards calls to other applications, along with arguments, through a JSON defintion. 

```json
{
  "verbs": [
    {
      "name": "myApp",
      "description": "description of the contents of this verb",
      "verbs": [
        {
          "name": "subVerb",
          "actions": [
            {
              "name": "action",
              "command": "path/to/application",
              "description": "description of what this command will do"
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
CommandForwarder.exe -c path/to/config -a myApp subVerb action "arguments" "to" "forward"
```

Or (for Windows) create a batch file or similar like the following that's available in PATH:

```bat
:: myApp.bat

@echo off
CommandForwarder.exe -c path/to/config -a myApp %*
```

And use it as follows from everywhere:

```bat
myApp subVerb action "arguments" "to" "forward"
```
