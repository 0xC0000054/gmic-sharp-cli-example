# gmic-sharp-cli-example

A .NET Core 3.1 Console-based example application for [gmic-sharp](https://github.com/0xC0000054/gmic-sharp).

## Usage

The following example executes the G'MIC water command on `input.png` and writes the result into a folder named `out` in the application directory.

`gmic-sharp-cli --input input.png --output-folder out water[0] 20`

The `--input` and `--output-folder` parameters are optional.   
If no output folder is specified, the results will be written to a randomly named folder in the application directory.

## Dependencies

This repository depends on libraries from the following repositories:

[gmic-sharp](https://github.com/0xC0000054/gmic-sharp), the library that this application uses.   
[gmic-sharp-native](https://github.com/0xC0000054/gmic-sharp-native), provides the native interface between gmic-sharp and [libgmic](https://github.com/dtschump/gmic).

## License

This project is licensed under the terms of the MIT License.   
See [License.txt](License.txt) for more information.

### Native libraries

The gmic-sharp native libraries (libGmicSharpNative*) are dual-licensed under the terms of the either the [CeCILL v2.1](https://cecill.info/licences/Licence_CeCILL_V2.1-en.html) (GPL-compatible) or [CeCILL-C v1](https://cecill.info/licences/Licence_CeCILL-C_V1-en.html) (similar to the LGPL).  
Pick the one you want to use.

This was done to match the licenses used by [libgmic](https://github.com/dtschump/gmic).
