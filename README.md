# Tsuki-tag

Tsuki-tag is an aggregated imageboard browser and collection manager, which lets you browse multiple image sources in a combined view, search images using tags, download images into configured workspaces (folders) with customization and filename templating support, and manage or curate multiple collections of your images. Tsuki-tag can inject EXIF metadata information (tag, image attributes, copyright notice, etc.) into the images during downloading or saving, with an additional option to convert non-JPG images to JPG images which support EXIF metadata. This could provide artists with a great way of creating and organizing a collection of images for reference, or to manage and appropriately annotate their own images with author and copyright information.

![Tsuki-tag main window](./docs/images/main01.jpg)

![Tsuki-tag main window](./docs/images/main02.jpg)

# Features

- Browse an aggregated collection of images retrieved from a number of pre-configured image providers in a paginated view.
- Fine-tune which providers are queried, along with what type of images should be shown ('safe', 'questionable', 'explicit').
- Search for images using tags and tag combinations with wildcard support, exclude images from being displayed using another set of tags with wildcard and regular expression support.
- Hover over images for a quick look at their attributes and information, or select them to keep them available for batch operations.
- Open images in separate tabs and continue browsing.
- Multi-selection system with support for batch operations (batch add to / remove from workspaces and online lists, batch update or annotate).
- Create online lists, which are a local collection of image metadata which can be browsed just as the online providers.
- Create workspaces, which on top of the features of online lists, lets you download images to configured folders with customizable behavior.
- Import local files or folders to workspaces, or convert one of your image folders to a workspace.
- Create metadata groups, which are a collection of information such as author, copyright notice, description and notes, which can be directly injected into the EXIF metadata of the JPG images files during workspace operations.
- Convert non-JPG images to JPG for EXIF support.
- Add, edit, or remove image tags before workspace or online list operations.
- Create tag rules for your online lists or workspaces with wildcard and regular expression support, to automatically process an image across multiple lists.
- Automatically add or remove tags when an image is added to, or removed from a workspace or online list, with templating support.  
- Specify the file names of files to be downloaded to a workspace, with templating support. 
- Global tag blacklisting with wildcard and regular expression support.
- Several other features such as opening images in your default image editor, opening or copying the image website URL, re-downloading the image from the original provider, and more.

# Online providers

Tsuki-tag uses a pre-configured list of image providers available for online browsing:

- Safebooru
- Gelbooru
- Danbooru
- Yandere
- Konachan
- R34

# Online lists

Tsuki-tag allows you to create and manage multiple collections, which are a set of images under a common name. <br>
These lists feature:

- Browse online lists the same way as you would browse the online providers.
- The image metadata and thumbnail will be saved into a local database upon adding to an online list. When the image is being opened, it will always be downloaded from the original online provider.
- Automatically add or remove tags from an image metadata when adding it to an online list, with templating support to use image attributes as tags.
- Create optional or mandatory tag combinations to an online list. An image then can be automatically added to and processed by multiple online lists using a single operation, if it passes the configured tag requirements of a given online list.
- Modify the tag list of the local metadata, annotate the local metadata with metadata group information such as author, description, copyright notice, etc.
- Re-download the metadata from the online provider to refresh the online attributes such as a fresh tag list and score.

# Workspaces

In addition to the features of online lists, Tsuki-tag also allows you to create workspaces, which are online lists with a configured folder on your local drive. When adding images to a workspace, the image is also downloaded to the specified folder. <br> 
Workspace features include:

- Browse workspaces the same way as you would browse online providers.
- The image metadata and thumbnail will be saved into a local database upon adding to a workspace, while the image itself will be downloaded to the specified folder. When the image is being opened, it will open the locally saved version of the image if it is still available.
- Automatically add or remove tags from an image metadata when adding it to a workspace, with templating support to use image attributes as tags.
- Create optional or mandatory tag combinations to a workspace. An image then can be automatically be added to and processed by multiple workspaces using a single operation, if it passes the configured tag requirements of a given workspace.
- Customize the download, such as whether to use the original or the sample version of the image, whether to convert non-JPG images into JPG, and to whether inject tag or metadata group information into the image file EXIF metadata. 
- Customize the file name of the downloaded image, with templating support to use image attributes as parts of the file name.
- Import local images or folders to an existing workspace to be processed, or convert one of your local image folders to a workspace.
- Modify the tag list of the local metadata, annotate the local metadata with metadata group information such as author, description, copyright notice, etc. A workspace can also be configured to inject this data directly into the image file itself.
- Re-download the metadata from the online provider to refresh the online attributes such as a fresh tag list and score (if the image was originally added from an online provider).

# Metadata groups

Metadata groups are a collection of information which can be applied to an image metadata, or can be directly injected into images with JPG format. It provides artists with a way to add author, description, and copyright notices to their images during exporting, on top of managing their tags.

Metadata group data injection can be manually applied to images contained in a workspace, or the data injection can happen automatically when the image is added to the workspace. The applied information, if the image is not converted into JPG format, or the feature is not enabled for a workspace, will still stay within the stored metadata of the image in the local database.

All metadata group fields offer templating support, which allows the use of the attributes of the image itself.

# Future features

- Quality of life and UI improvements.
- Optionally integrating with more online image providers.
- Other major or minor features.

# Releases

Releases of Tsuki-Tag can be found under the releases section. Windows x86-x64 versions are available for Windows 10, and x64 versions are available for Linux and MacOS. Windows and Linux versions are being tested.
<br><br>

## MacOS and Linux

MacOS and Linux releases are available, however, the MacOS release is completely untested and the release version may not function at all.

# Build

On Windows 10, Visual Studio 2019 Community is advised, or alternatively Visual Studio Code with .NET Core 5.0 SDK should be adequate to build the project on both Windows and Linux operating systems.

# License

GNU GPL v3