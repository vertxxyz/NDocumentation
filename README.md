# nDocumentation
A documentation window for Unity Editor that displays rich text pages.

## Usage

### Pages

#### DocumentationWindow
Extend *DocumentationWindow* to provide a base window to inject documentation pages into.

#### DocumentationPageRoot
Extend *DocumentationPageRoot* to define a root page to a *DocumentationWindow*, a root page does not get linked to other than through **Home** links.

#### DocumentationPage
Extend *DocumentationPage* to define documentation pages.
These pages can inject buttons above or below other page content, either other *DocumentationPage*s or the *DocumentationPageRoot*.
The Color and Title provided are used to style these Button Links.
The buttons are sorted by the Order field, with lower values being displayed above others.

#### DocumentationPageAddition
Extend *DocumentationPageAdditition* to add content to DocumentationPages (either *DocumentationPage*s or the *DocumentationPageRoot*.)
Additional content is sorted by the Order field, with lower values being displayed above others.

### Content

#### TODO

## Installation
Use the Package Manager (Window>Package Manager) and add the appropriate package.json files in the root of each folder.