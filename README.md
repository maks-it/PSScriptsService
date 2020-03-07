# Power Shell Scripts Service

This project has an aim to allow **System Administrators** and also to who **Thinks to be System Administrator** to launch **Power Shell** scripts as **Windows Service**.

## Latest builds

* [PSScriptsService_v1.1.7371.25098](builds/PSScriptsService_v1.1.7371.25098.zip)

## How It Works

When you **install** and then **start** this **Windows Service**, it scans default folder in its root **PowerShellScripts**, then serches for default script **StartScript.ps1** in parent directories:

```bash
    .
    ├── ...
    ├── PowerShellScripts
    |   ├── SomeStuff_1
    |   |   ├── ...
    |   |   ├── StartScript.ps1
    |   |   └── ...
    |   ├── SomeStuff_2
    |   |   ├── ...
    |   |   ├── StartScript.ps1
    |   |   └── ...
    |   └── ...
    └── ...
```

To each script following command parameters are being passed:

```C#
    myCommand.Parameters.Add(new CommandParameter("Automated", true));
    myCommand.Parameters.Add(new CommandParameter("CurrentDateTimeUtc", DateTime.UtcNow.ToString("o")));
```

which you can retrieve on the script side this way:

```PowerShell
    [CmdletBinding()]
    param (
        [switch]$Automated,
        [string]$CurrentDateTime
    )

    if($CurrentDateTime) {
        [datetime]$CurrentDateTime = [datetime]::parseexact($CurrentDateTime, 'dd/MM/yyyy HH:mm:ss', $null)
    }

    Write-Host "Automated: $Automated" -ForegroundColor Green
    Write-Host "CurrentDateTime: $CurrentDateTime" -ForegroundColor Green
```

Thanks to that, it's possible to create standalone scripts or automated scheduled scripts, which will be executed according to the script managed schedule logic.

Every script is launched in its **own thread**, so if one crashes, others are able to run anyway:

```bash
    Power Shell Scripts Service Thread
    └── DoWork Thread
       ├── SomeStuff_1 / StartScript.ps1 Thread
       ├── SomeStuff_2 / StartScript.ps1 Thread
       └── ...
```

> I have set to execute only **signed** scrips by default, but if you don't care about your environment security, it's possible to launch them in **unrestricted** mode.
>
> Continue to read to see other possible settings...

## Customizations

Here are all currently available customizations, managed into handy json file:

```json
    {
    "ServiceName": "PSScriptsService",
    "Description": "Windows service, which allows you to invoke PowerShell Scripts",
    "DisplayName": "PowerShell Scripts Service",
    "LogPath": "",
    "LogSize": "20",
    "ScriptsPath": "",
    "TargetScript": "",
    "SignedScripts": true
    }
```

Let's see each one:

* ServiceName - System service name. I suggest to use short names without spaces or other strange characters. See [What are valid characters in a Windows service (key) name?](https://stackoverflow.com/questions/801280/what-are-valid-characters-in-a-windows-service-key-name).
* Description - Description you wants to give to this service. Just put something very serious and technically complex to admire what kind of DUDE you are!
* DisplayName - Same thing like for ServiceName, but you are free to use spaces.
* LogPath - If empty, the service will create **Logs** folder in its root.
* LogSize - Integer Megabytes.
* ScriptsPath - If empty, the service will check **PowerShellScripts** folder in its root.
* TargetScript - Normally service is looking for default **StartScript.ps1**, you are free to specify another **entry point script** name.
* SignedScripts - **true** for **AllSigned** or **false** for **Unrestricted**

You can create multiple services just create another service root folder and set different: **ServiceName**, **Description**, **DisplayName**; also: **LogPath**, **ScriptsPath**; in case of custom values.

## Install and Uninstall Power Shell Scripts Service

I have prepared 2 *.cmd files to simplify service system integration:

Install.cmd

```bat
    "%~dp0PSScriptsService.exe" install
    pause
```

and

Uninstall.cmd

```bat
    "%~dp0PSScriptsService.exe" uninstall
    pause
```

>These ***.cmd** files have to be launched with **Admin** privileges.

After installation you have to start your newly created windows service: Win+R -> services.msc -> Enter -> Search by DisplayName.
