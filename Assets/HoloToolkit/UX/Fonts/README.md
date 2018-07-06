# Microsoft Selawik
Selawik is an open source replacement for Segoe UI. 

# Known open issues:
* Selawik is missing kerning to match Segoe UI. 
* Selawik needs improved hinting. 

[Microsoft's Open Source Fonts](https://github.com/Microsoft/fonts)

# Font Management
The Fonts package contains a set of components for managing fonts and icons for your project. Fonts and their associated materials can be assigned to text objects through a Font menu in the Unity Editor and icons and there materials are assigned using the Icon Viewer window. The font and material applied with one click, it's a quick way to add and swap fonts in your project.

![Getting Started](Documentation/Images/GettingStarted.jpg =600x)

# Getting Started
1. Select Fonts/Generate Fonts Menu from the Unity Editor main bar (Fig 1).
	* This will create a FontSets directory containing font and icon sets with a default set of fonts (Selawik), icons (sample weather icons) and their associated materials.
1. Now under the same Fonts menu will appear a list of fonts and the Icon Viewer.
	* See below to learn how to customize the Fonts list and Icon Viewer icons.

![Fig 1. Generate Fonts Menu](Documentation/Images/Menu01.jpg =300x)
*Fig 1 - Generate Fonts Menu*

![Full Fonts Menu](Documentation/Images/Menu02.jpg =300x)
*Example of a Fonts menu using Segoe UI fonts - Generate Fonts Menu*

*Due to licensing restrictions, the defualt Font and Icon sets include the Selawik font and a custom weather icon font created by exporting SVG files of some icons drawn in Adobe Illustrator, uploading them to Fontastic.me, generating a TTF file and dragging them into our Unity Project.*
*To import additional icons or fonts, drag desired font files (TTF) into your Unity Project and follow the directions below. Be aware of licensing rights when using any 3rd party fonts, only use fonts that you have the rights to use for the purpose they are being used.*

# Using the Icon Viewer
To open the Icon Viewer, select Fonts/Icon Viewer from the Unity Editor menu bar (Fig 2).
There can be multiple icon sets installed, when setup, use the drop-down at the top right of the window to switch sets (Fig 3).
Select a Text or TextMesh object in the Scene and click an icon from the Icon Viewer to assign.

![Fig 2. Icon Viewer](Documentation/Images/IconViewer.jpg =460x)
*Fig 2 - Icon Viewer*

![Fig 3. Icon Viewer icon sets](Documentation/Images/IconViewerMenu.jpg =460x)
*Fig 3 - Icon Viewer icon sets*

# Using the Fonts list
Included by default is a basic set of Selawik Fonts listed in the Fonts menu.
Select a UI Text or TextMesh object in the scene, then select a font from the Fonts menu.

# Advanced Features
* Use Font Sets to customize the Fonts menu.
	* Add a font to the menu by creating a new Font Set using the Assets Create menu, Assets/Create/Fonts/Font Set.
	* Enable or disable existing Font Sets, using the Enabled toggle, to control which font options show up in the menu. The default Font set is located in the FontSets directory. Uncheck Enabled to disable it, then select Fonts/Generate Fonts Menu to rebuild the list.
	* Usage Scenarios
		* Creating a font set for each font family each containing different styles like Regular, Bold, Italic.
		* Create a font set for each project using names like Header, Footer, Subhead instead of actual font names. 
		* Customize and limit the Fonts menu to just show the fonts that reflect the project brand. This helps to make sure new content is created using the appropriate fonts and materials.
		* Font sets can apply the same font with different material configurations, like culling or render queue. To do this click the Add Font button twice, then assign the same font to both positions. Assign unique names to each instance that idicates the configuration, like appending "BackCulling", then click Create Material. Adjust the settings on the materials and select Fonts/Generate Fonts Menu for the font configurations to appear in the list. 
	* Select Fonts/Generate Fonts Menu for any changes to the Font Sets to take effect.
* Use Icon Sets to control the content available in the Icon Viewer.
	* Usage Scenarios
		* Create a custom icon font for a project and make it easy for everyone to select the correct icons.
		* Add new icons and/or turn off old icons.

# Creating a new Font Set.
1. Right-click in the project view and select Create/Fonts/Font Set, you can also do this through the Assets menu.
1. Import the Font into the Unity project by dragging it into the Assets folder.
1. Assign the Font to the Font Set.
1. Add a Font Display Name that should show up in the Font Menu (Fig 4).
1. Create a material manually or with the Create button (Fig 5).
	* The material name will be based on the display name.
1. Make sure the font set is enabled.
1. Finally, select Fonts/Generate Fonts Menu to rebuild the Fonts menu list.

![Fig 4. FontSet Setup](Documentation/Images/FontSet02.jpg =400x)
*Fig 4 - FontSet Setup*

![Fig 5. Create Material](Documentation/Images/FontSet03.jpg =400x)
*Fig 5 - Create Material*

# Creating a new Icon Set
1. Right-click in the project view and select Create/Fonts/Icon Set, you can also do this through the Assets menu.
1. Import the Icon Font into the Unity project by dragging it into the Assets folder.
1. Assign the Font to the Icon Set.
1. Add an Icon Font Display Name that should show up in the Icon Viewer Menu (Fig 6).
1. Create a material manually or with the Create button (Fig 6).
	* The material name will be based on the display name (Fig 7).
1. Click the Import Glyphs button to create a new list of icon symbols (Fig 8).
	* Each glyph can be disabled individually once the list has been created.
1. Make sure the Icon set is enabled.
1. Select Fonts/Icon Viewer. (If Icon Viewer does not exist in the menu, select Fonts/Generate Fonts Menu first.)

![Fig 6. IconSet Setup](Documentation/Images/IconSet02.jpg =400x)
*Fig 6 - IconSet Setup*

![Fig 7. Import Glyphs](Documentation/Images/IconSet03.jpg =400x)
*Fig 7 - Import Glyphs*

![Fig 8. Disable Glyphs](Documentation/Images/IconSet04.jpg =400x)
*Fig 8 - Disable Glyphs*
