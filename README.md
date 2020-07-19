# SuperMemo Snippets Plugin

- [Features](#features)
- [Installation](#installation)
- [User Manual](#manual)
- [Contributing](#contributing-guide)

## Features

- Adds IDE-style snippets to SuperMemo HTML Components.
- Integrates with Autocompleter.

## Installation

### Manual Installation

#### Pre-built binaries (**RECOMMENDED**)

(WIP. Not available yet.)

> Note: If you are upgrading from an older version, please delete the old version first.

1. Check the releases tab on this GitHub repository.
2. Download the latest available version.
3. Navigate to the development plugin folder (`C:\Users\<YOUR USERNAME>\SuperMemoAssistant\Plugins\Development`) and extract the zip folder into the directory.

#### Building from source

> See the [building from source guide](https://github.com/bjsi/docs/blob/master/SMA/plugins/BUILD_FROM_SOURCE.md) and feel free to get in touch with me (Jamesb) on the SMA discord if you have trouble.

## Manual

### Usage

> Note: The plugin is activated **after** the first element changed event.

Popup boxes containing your snippets are displayed when you press `Ctrl+Alt+Shift+S`.
Press `Up` and `Down` arrow keys to navigate the autocomplete popup.
Press the `Right Arrow` key to insert the currently selected suggestion.
Press `Ctrl+j` to navigate to the next placeholder.
Press `Ctrl+k` to navigate to the previous placeholder.

### Configuration

#### Settings

> Note: Access the settings of any SMA Plugin by pressing `Ctrl+Shift+Alt+O`.

## Contributing Guide

### Issues and Suggestions

See the [contribution guide](https://github.com/bjsi/docs/blob/master/SMA/plugins/CONTRIBUTING.md) for information on how to report issues or make suggestions.

### Code Contributions

Pull requests are welcome!

1. Firstly, go through the manual installation guide above.
2. You will also require [Git Hooks for VS](https://marketplace.visualstudio.com/items?itemName=AlexisIncogito.VisualStudio-Git-Hooks) which is used to enforce a consistent code style.
> Note: you do not need to build the entire SuperMemoAssistant project to make changes to or debug a plugin.
3. See the code section of the [contribution guide](https://github.com/bjsi/docs/blob/master/SMA/plugins/CONTRIBUTING.md) for pull request instructions.
4. If you need help, don't hesitate to get in touch with me (Jamesb) on the SMA discord channel.
