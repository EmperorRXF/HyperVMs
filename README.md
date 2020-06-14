# HyperVMs

This system tray utility will enumerate all your Hyper-V VMs and allow you to `start` & `stop` the VMs when necessary. It also has an `Open Shell` option where you can easily SSH into the selected VM.

## Pre-Requisites

- A Windows OS (tested on Windows 10 Pro)
- .NET Framework 4.0 Runtime or higher - [Download](https://www.microsoft.com/en-us/download/details.aspx?id=17851)
- An SSH Client - (Git Bash, Windows built-in SSH, PuTTY, etc.)
- Administrative Privileges in Windows - Required for executing Hyper-V PowerShell Commands

## Configuration Options

The `settings.json` in the same directory as the HyperVMs binary contains all the configuration needed to run it.

- `defaultShell` - By default this is set to `windowsTerminal`, and possible values are `cmd`, `powershell`, `powershellCore` & `windowsTerminal`. You can even setup a completely new shell with correct arguments and use it.
- `sshCommand` - The `ssh` client command used to shell into the VM. The default would work if the ssh client has a command `ssh` available in the `PATH` variable.
- `autorun` - Determines if `HyperVMs` would autorun on windows startup

```json
{
  "shells": {
    "cmd": {
      "command": "cmd.exe",
      "arguments": "/c"
    },
    "powershell": {
      "command": "powershell.exe",
      "arguments": ""
    },
    "powershellCore": {
      "command": "pwsh.exe",
      "arguments": "-c"
    },
    "windowsTerminal": {
      "command": "wt.exe",
      "arguments": ""
    }
  },
  "sshCommand": "ssh",
  "defaultShell": "windowsTerminal",
  "autorun": true,
  "hosts": {}
}
```

## Notes

- Prior to using the `Open Shell` action, SSH connectivity needs to be setup between host & guest (i.e. Installing SSH Server, setting up private & public keys)

- Preferred shell to open via `Open Shell` can be configured in Settings (i.e. use `Open Settings` menu item)

- By default CMD, PowerShell, PowerShell Core & Windows Terminal shells are configured in `settings.json`, and one can be picked as the default shell

- The user name used to SSH into the VM will asked the 1st time, and will be saved in the settings.json for subsequent uses.

## Limitations

- Currently HyperVMs can directly shell into VMs configured with the Hyper-V **Default Switch**. If an **Internal** or **External** Switch is used, then the host records need to be setup in the `%WINDIR%/System32/drivers/etc/hosts` file.

  If the Hyper-V VM Name is "SomeVM", then the hostname `SomeVM.mshome.net` would need to be mapped into the corresponding IP manually.

## Screenshots

- Context Menu

  ![Image](https://i.imgur.com/fbY0coR.png)

- Managing VMs

  ![Image](https://i.imgur.com/wfc1eyG.png)

- SSH Into VM

  ![Image](https://i.imgur.com/hmJQMCG.png)

- VM Power On/Off

  ![Image](https://i.imgur.com/cMQcUUH.png)

## Contributing

.NET Framework 4.0 SDK is needed to build the project - [Download](https://www.microsoft.com/en-us/download/details.aspx?id=17851).

- Clone the repository
- Open the `HyperVMs.sln` in Visual Studio to build & run
