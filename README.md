# NPreprocessor
[<img src="https://img.shields.io/nuget/vpre/NPreprocessor.svg">]( https://www.nuget.org/packages/NPreprocessor)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=CFGToolkit_NPreprocessor&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=CFGToolkit_NPreprocessor)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=CFGToolkit_NPreprocessor&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=CFGToolkit_NPreprocessor)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=CFGToolkit_NPreprocessor&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=CFGToolkit_NPreprocessor)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=CFGToolkit_NPreprocessor&metric=coverage)](https://sonarcloud.io/summary/new_code?id=CFGToolkit_NPreprocessor)

## Description

NPreprocessor is a general purpose macro processor for .NET. 

The library takes ideas from GNU m4 library: https://www.gnu.org/software/m4/

At the moment following base macros are supported:
- define(name, [expansion])
- undefine(name)
- ifdef(name, string-1, [string-2])
- ifndef(name, string-1, [string-2])
- incr(variable)
- decr(varible)
- include(name)
- dnl

There are also following macros supported which interally uses base macros:
- #include 
- #define 
- #undefine
- #ifdef
- #ifndef
- #endif
- #else

The # character can be replaced by any string.

## Roadmap
- Add features from m4
- Add features from gcc preprocessor
- Add features from C# preprocessor
