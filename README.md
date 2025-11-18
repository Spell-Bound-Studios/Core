# README

[Documentation](https://spell-bound-studios.gitbook.io/spellbound-docs/)

# Introduction

# Console
The console is broken down into two parts:
1) ConsoleCommandClass
2) ConsoleUtilityCommand

### ConsoleCommandClass
The console command class is constructed via the ICommand interface and implemented at the class level. It allows users to implement specific commands to do very specific things. For instance, the help or clear commands in the console window come built and and they are both examples of how one might use the ICommand interface.

### ConsoleUtilityCommand
The console utility command is simply a method level attribute that allows the user to call methods with said attribute in the console.

### Core Infrastructure
* CommandRegistry.cs
* PresetCommandRegistry.cs
* PresetConsoleRegistry.cs
* ConsoleCommandRouter.cs

### Command System
* ICommand.cs
* CommandResult.cs
* CommandParameterParser.cs
* UtilityCommandInfo.cs

### Attributes
* ConsoleCommandClassAttribute.cs
* ConsolePresetCommandAttribute.cs
* ConsoleUtilityCommandAttribute.cs

### Built-in Commands
* HelpCommand.cs
* ListCommand.cs
* ClearCommand.cs

### UI/Controller
* ConsoleController.cs
* ConsoleLogger.cs

### Preset Integration
* ConsoleModule.cs