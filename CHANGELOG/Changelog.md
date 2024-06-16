# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]
### Added
### Changed
### Deprecated
### Removed
### Fixed

---
## [2.2.11]
### Added
- _**Counter**_ component: good, old-fashione counter, attach a trigger and start counting. Not sure why I haven't added this earlier, since I'm using it more times than i can coun... oh, wait
### Changed
- _**ICONS**_ got some overdue love! They were improved for a better readability and understanding of each component's purpose, consolidating the color scheme and overall visual consistency
- _**Directory Reader**_ component: implemented a recursive search behavior to search subfolders, with a parameter for the search depth level
- _**Trilateration**_ component: inputs are now 3 spheres (instead of 3 points + 3 radiuses), easing the workload on the user

### Deprecated

### Removed

### Fixed

---
## [2.2.10]
### Added
- _**Orient on Grid**_ component (under *Geometry*): this component orients a list of objects on a grid. Grid cell size is automatically computed from the larges X and Y dimensions of the object's bounding boxes (aligned with the World XY plane). Works also with groups and blocks.
### Changed
- _**Space Filling Polyhedra**_ component: doubled the size of the *Bisymmetric Hendecahedron* to make it similar to the other Polyhedra; changed the way code behaves to improve performance when generating multiple geometries

### Deprecated

### Removed

### Fixed
- _**Space Filling Polyhedra**_ component: fixed some points coordinates to remove imprecisions
- general checks for compatibility with Rhino 8

## [2.2.9]
### Added
- _**Select Rhino Objects**_ component: this component selects objects in Rhino by their GUID.
- _**Camera Control And Zoom To Geometry**_ component: this component replaces both _**Camera Control**_ and _**Zoom To Object**_, adding new functionalities: it is now possible to choose the viewport to control, the display mode and the projection type (Perspective, Parallel, Two-point Perspective).
- _**Mesh Offest Extended**_ component: this component replaces _**Mesh Offset Weighted**_, integrating different offset options from [a Daniel Piker sample code](https://discourse.mcneel.com/t/proper-mesh-offset/148952/8)

### Changed
- _**Mesh Point Inside**_ now uses the Mesh Winding Number (MWN) method - see Jacobson et al http://igl.ethz.ch/projects/winding-number/
- _**Mesh Report**_ component has been thoroughly improved, noticeably with 2 outputs: one with the report text and the other with corresponding data (this will ease automation based on mesh data - for example do something if non-manifold edges or degenerate faces are detected). Other improvements: display of non-manifold edges, report is more thorough.
- _**Is Polyline Clockwise**_ icon is now clearer
- _**Froggle**_ and _**Toggle Autostop**_ have their own separate section inside the "Data" tab

### Deprecated
- _**Camera Control**_ and _**Zoom To Object**_ components (see Added)
- _**Mesh Offset Weighted**_ component (see Added)

### Removed
- _**Font List**_ component: there is an already excellent version of this component in the Human plugin, so, to avoid redundancies and given that Human is a pretty handy and widespread plugin, I've decided to remove it.

### Fixed
- several code clean-ups for better clarity & compatibility with Rhino 8

## [2.2.8]
### Added
- _**Get Euler Angles ZYZ**_ and _**Get Euler Angles ZYX**_ components: they compute Euler angles for the rotation part of an affine rigid Transformation in the ZYZ and ZYX format respectively. ZYX Euler angles are also known as Yaw, Pitch and Roll.
- _**Rotation from ZYZ Euler Angles**_ and _**Rotation from ZYX Euler Angles**_ components perform the inverse operation, computing a Rotation Transformation from Euler angles

### Changed

### Deprecated

### Removed

### Fixed

## [2.2.7]
### Added

### Changed
- _**Camera Report**_ can report data for Named Views or the current active view; update values for active view can be done with external toggle or by double-clicking the component
- _**Slider Value display**_ by default Slider channels have hidden wires; some internal spacing parameters adjusted

### Deprecated

### Removed

### Fixed


## [2.2.6]
### Added
- _**Froggle**_: a special kind of toggle that flips status upon double-click or whenever an input that can be cast as True is detected on change or solution. Any False value will not change its status. In practice, you can attach a button and the Froggle will flip status at every button push, or attach a counter and have it flip status, or get creative (for example: try attaching a panel containing '1' and connect a trigger to the panel to have time-driven boolean alternates). By design, it also resets to False each time a file that contains this component is reopened.
- _**Space-Filling Polyhedra Generator**_: added 2 more space-filling polyhedra: Elongated Dodecahedron and Gyrobifastigium.

### Changed
- _**SaveGHFile**_ now outputs a boolean value upon successful save
- _**GHFilePathInfo**_ can be updated with a double-click
- _**Slider Value display**_ now uses a variable zoomable interface to manage sliders input - see example file for details

### Deprecated

### Removed

### Fixed
- Fixed a small bug in _**DirectoryReader**_ to adjust input paths that do not end with a '\\' character - this affected output Files and Directories lists.

## [2.2.5]
### Added

### Changed
- _**ZoomToObject**_ now can also Zoom in; it's actually a new version of the component, old one is marked obsolete
- Changelog format updated

### Deprecated

### Removed

### Fixed

## [2.2.4]
### Added

### Changed

### Deprecated

### Removed

### Fixed
- corrected a bug in the  _**Space Filling Polyhedra Generator**_ component that generated inconsistent polyline directions for face contours

## [2.2.3]
### Added
- _**Data Tree Graph**_ component
### Changed

### Deprecated

### Removed

### Fixed


## [2.2.2]
### Added
- _**L-Plolyline from Plane**_ component
### Changed
- changed Save String to File behavior (removed empty line at end of file)
### Deprecated

### Removed

### Fixed

## [2.2.1]
### Added
- _**Smallest Enclosing Circle**_ component
- _**Extract Mesh Edges**_ component

### Changed
- adjusted _**Extract Mesh Faces**_ icon for coherence and clarity
- Mesh topology components outputs now sorted by topology edge sequence

### Deprecated

### Removed

### Fixed
- added a check for null meshes in _**Extract Edge with Tolerance (multiple meshes)**_ - null meshes in the input list would cause the component to throw an error with no output
- added a null check in _**Mass Boolean**_ - null data in an input tree branch would cause the component to throw an error with no output