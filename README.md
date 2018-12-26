# nDocumentation
An extensible documentation window for the Unity Editor that displays rich text pages.
**Minimum Currently Supported Version : Unity 2019**

## Usage

----
### Pages

#### DocumentationWindow
- Extend *DocumentationWindow* to provide a base window to inject documentation pages into.
- Override DrawConstantHeader to provide a header to every page.

#### DocumentationPageRoot
- Extend *DocumentationPageRoot* to define a root page to a *DocumentationWindow*, a root page does not get linked to other than through **Home** links.

#### DocumentationPage
- Extend *DocumentationPage* to define documentation pages.
- These pages can inject buttons above or below other page content, either other *DocumentationPage*s or the *DocumentationPageRoot*.
- The Color and Title provided are used to style these Button Links.
The buttons are sorted by the Order field, with lower values being displayed above others.

#### DocumentationPageAddition
- Extend *DocumentationPageAdditition* to add content to DocumentationPages (either *DocumentationPage*s or the *DocumentationPageRoot*.)
- Additional content is sorted by the Order field, with lower values being displayed above others.
- Color and Title can be overriden to style these Button Links.

----
### Content

#### TODO

## Installation
Use the Package Manager (Window>Package Manager) and add the appropriate package.json files in the root of each folder.
