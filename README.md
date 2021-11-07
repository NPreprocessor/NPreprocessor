# NPreprocessor
 [<img src="https://img.shields.io/nuget/vpre/NPreprocessor.svg">]( https://www.nuget.org/packages/NPreprocessor)

NPreprocessor is a general purpose macro processor for .NET. 
The library takes ideas from GNU m4 library: https://www.gnu.org/software/m4/

At the moment following base macros are supported:
- define(name, [expansion])
- undefine(name)
- ifdef(name, ifdef (name, string-1, [string-2])
- ifndef(name, ifdef (name, string-1, [string-2])
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

Future:
- Add features from m4
- Add features from gcc preprocessor
- Add features from C# preprocessor
