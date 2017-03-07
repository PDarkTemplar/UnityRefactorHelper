# Unity Refactor Helper
Visual Studio 2015, 2017 helper extension for Unity script refactoring.

<a href="https://visualstudiogallery.msdn.microsoft.com/87cc3382-41b0-42d8-83fc-a5ba53b13cd3">Download</a>

This extension will be helpful if you keep Unity scripts in separate libraries. Extension works only with C#. Scripts references in the scenes and prefabs will be broken after refactoring (changing namespace or class name). This extension monitors changes in project files and update scene and prefab files after assembly build. 

Set the asset serialization to force text and version control to visible Meta files in Unity (<b>Edit -> Project Settings -> Editor</b>).

Visual Studio extension located at <b>View -> Other Windows -> Unity Refactor Helper</b>. It is become active after solution is open and all projects are loaded. 

#Possible settings
<ul>
<li><b>Enable</b> – start extension</li>
<li><b>Unity project folder</b> - path to the Assets folder in Unity project</li>
<li><b>Projects</b> - select monitored library</li>
<li><b>Project GUID</b> – Get this GUID from the assembly Meta file in Unity folder</li>
<li><b>Watch projects</b> - tracked libraries</li>
</ul>

#Additional Information
You can edit project GUID by clicking on the project name in the watch projects. Extension creates two files (configuration and cache) in the *.sln file folder. It tracks file save and synchronize files after build if it is required. Tracking occurs only inside the library, file move tracking between libraries is not supported.

#License
MIT license
