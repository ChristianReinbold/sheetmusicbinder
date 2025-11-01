SheetMusicBinder
=======

SheetMusicBinder is a small tool to rearrange pdf pages of sheet music such that flipping only is required when at rest.

How to build
------------

The project is set up as a Visual Studio Net project. It requires

- a recent version of Visual Studio (tested with VS2022),
- the `Net desktop development` workload and `NuGet package manager` component from the Visual Studio installer.

After setting up your environment, build the `SheetMusicBinder` project in Visual Studio. It will generate build artifacts in `<solutiondir>/SheetMusicBinder/build/`.

You can also deploy the project with Visual Studio's Publish Wizard. It outputs an installer in `<solutiondir>/deploy/` which you can use to install or update SheetMusicBinder on your machine. Please make sure to check and understand the project's license as well as those of utilized NuGet packages before distributing your build.

How to use
----------

1. Start up the application
2. choose a pdf to rearrange
3. set up the number of pages your stand can hold (usually two, for piano four might be reasonable)
4. Mark rests for each page
5. Print the generated pdf (optimized layouts for duplex printing are supported)
6. Bind pages along the marking signs inserted by the application
7. Enjoy stress-free flipping while playing music

Note that the tool is only availabe in German.

License
-------

This project is licensed under the [GNU Affero General Public License, Version 3](LICENSE). Dependencies such as NuGet packages may be licensed differently.

Contact
-------

If you are interested in this project and plan to use it, feel free to contact me at dev[at]creinbold[dot]de.
