<p align="center">
<img src="art/vs_logo.png" width="400px">
</p>
> * V# 0.1.32

![GitHub repo creation date](https://img.shields.io/badge/created-July%202024-brightgreen)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/funcieqDEV/VSharp)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)
![GitHub stars](https://img.shields.io/github/stars/funcieqDev/VSharp)


<br>

**an interpreted programming language that allows you to write all types of programs, from simple console applications to even games**


V# is a new and modern programming language. 
characteristics:
- **it is an interpreted language**
- **made in C#**
- **easy to learn**

### installation
 - first you need to **download V#**
 - then **unzip** the file to a location you like
 - the next step is to *run* **install.bat**

at this point you can already use V#

### Requirements 
- **.net 7.0** install.bat will download the installer file for you 
- the file editor can be a regular notepad
- administrator permissions
- Console

### Compilation
- **Download source**
after you download source you shoud check if<br> you have .net 7.0 and right code editor like Visual studio

- **Create new project in Visual studio**
once you have created a new project you should add files from the **src** folder to your project

- **Edit main.cs/program.cs**
if you want use V# interpreter first you need to create a Lexer class with input(your V# code) as an argument,
<br> then create a Parser class. In the Parser constructor use the Lexer method `Tokenize` as an argument which will return a list of tokens<br> next step is create a Interpreter class and in constructor put Parser method `Parse` it will return ProgramNode, now you can use Interpreter method `Interpret`




### documentation
 You can find documentation on our [official website](https://github.com/funcieqDEV/VSharp)
 or you can find it here on github [docs](https://github.com/funcieqDEV/VSharp-docs/tree/main)

- **run the project**
```bash
VSharp run <file.vshrp>
```

