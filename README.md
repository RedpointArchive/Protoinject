Protoinject
=====================

_A hierarchical dependency injection system_

Protoinject is a hierarchical dependency injection system.  Unlike conventional dependency injection libraries which only concern themselves with the resolution of services, Protoinject manages the lifecycle of services and associated them into a hierarchy of objects.

This model allows Protoinject to seperate planning, verification and resolution of dependencies, allowing for many new scenarios such as:

* Caching the planning results from the startup of your application, resulting in faster startup times.
* Validating that all dependencies can be satisified before any objects are created, ensuring no side-effects occur if a request can not be satisified.
* Creating custom plans from external sources, such as game levels.  This allows you to verify that all objects in a scene have their dependencies satisified.
* Navigating and querying the hierarchy at runtime, which maps extremely well to entity-component systems.

Getting Started
------------------

Protoinject is commonly used within [Protogame](https://protogame.org/).  If you're already using Protogame, then you don't need to manually install this library; it is already available to you.

If you are not using Protogame, or you want to use Protoinject for general application purposes, you can install Protoinject into your project using [Protobuild](https://protobuild.org/), like so:

```
Protobuild.exe --install https://protobuild.org/hach-que/Protoinject
```

Then reference Protoinject from your project, like so:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Name="MyProject" Path="MyProject" Type="Console">
  <References>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="Protoinject" />
  </References>
  <Files>
    <!-- ... -->
  </Files>
</Project>
```

An example project is provided in the repository.

Supported Platforms
----------------------

Protoinject supports the following platforms:

* Windows
* MacOS
* Linux
* Android
* iOS

Build Status
-------------

Projects are built and tested against all supported platforms.

|     | Status |
| --- | ----- |
| Protoinject | ![](https://jenkins.redpointgames.com.au/buildStatus/icon?job=RedpointGames/Protoinject/master) |

Features
------------

* Multi-phase dependency resolution; side-effect free.
* Supports many different platforms.
* No runtime code generation, allowing even automatic factories to work on platforms like iOS.

How to Contribute
--------------------

To contribute to Protoinject, submit a pull request to `RedpointGames/Protoinject`.

The developer chat is hosted on [Gitter](https://gitter.im/RedpointGames/Protoinject)

[![Gitter](https://badges.gitter.im/RedpointGames/Protoinject.svg)](https://gitter.im/RedpointGames/Protoinject?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

Providing Feedback / Obtaining Support
-----------------------------------------

To provide feedback or get support about issues, please file a GitHub issue on this repository.

License Information
---------------------

Protoinject is licensed under the MIT license.

```
Copyright (c) 2015 Redpoint Games Pty Ltd

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```

Community Code of Conduct
------------------------------

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behavior in our community. For more information see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).
