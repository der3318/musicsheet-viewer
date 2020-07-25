## 🎵 Music Sheet Viewer

![ver](https://img.shields.io/badge/version-1.0.2-blue.svg)
![windows](https://img.shields.io/badge/windows-10.0.18362.0-green.svg)
![flavor](https://img.shields.io/badge/flavor-x86/x64/arm-brightgreen.svg)
![size](https://img.shields.io/badge/size-32.2%20MB-yellow.svg)
![license](https://img.shields.io/badge/license-MIT-blueviolet.svg)

A UWP (Universal Windows Platform) based application that provides an user-friendly interface to view piano sheets on Windows tablets.

| <img src="/demo.gif?raw=true" width="600px"> |
| :-: |
| <img src="https://i.imgur.com/n7NJhki.jpg" width="600px"> |


### 🗨 Features
1. Support piano sheets formatted in both raw images and PDF documents.
2. Tap or click to view the next three pages of a sheet.
3. Automatically keep the screen alive if required.


### 🧱 References
| Name | Version | Note |
| :- | :-: | :- |
| Microsoft.NETCore.UniversalWindowsPlatform | 6.2.10 | imported package |
| Microsoft.Toolkit.Uwp.UI.Animations | 6.1.0 | imported package |
| Microsoft.Toolkit.Uwp.UI.Controls | 6.1.0 | imported package |
| Telerik.UI.for.UniversalWindowsPlatform | 1.0.1.9 | imported package |
| [Microsoft App Sample - Photo Lab](https://github.com/microsoft/Windows-appsample-photo-lab) | - | layout and brush |
| [Microsoft Fluentui System Icons](https://github.com/microsoft/fluentui-system-icons) | 1.1.31 | assets |


### 🎼 Installation
| Step | Description |
| :-: | :- |
| #1 | go through <kbd>ms-settings:developers</kbd> and enable developer mode |
| #2 | download released certificate - [DerChien.cer](https://github.com/der3318/musicsheet-viewer/releases/download/v1.0.2.0/DerChien.cer) |
| #3 | right click to import the self-signed certificate |
| #4 | choose <kbd>Local Machine</kbd> and <kbd>Trusted Root Certification Authorities</kbd> |
| #5 | download released installer - [PianoSheetViewer.msixbundle](https://github.com/der3318/musicsheet-viewer/releases/download/v1.0.2.0/PianoSheetViewer.msixbundle) |
| #6 | double click to install the app |

| <img src="https://i.imgur.com/LQMrwOq.png" width="600px"> |
| :-: |
| <img src="https://i.imgur.com/1HPGv5F.png" width="600px"> |


### 🗑️ Uninstallation
| Step | Description |
| :-: | :- |
| #1 | go through <kbd>ms-settings:appsfeatures</kbd> and find the app |
| #2 | click and confirm to uninstall |


### 📄 Build and Redistribution
| Step | Description |
| :-: | :- |
| #1 | clone the repository and open using [Visual Studio 2019](https://visualstudio.microsoft.com/) |
| #2 | modify the source to meet customized requirements |
| #3 | generate self-signed certificate |

```powershell
New-SelfSignedCertificate -Type Custom -Subject "CN=[YOUR NAME OR COMPANY]" -KeyUsage DigitalSignature -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}") -KeyExportPolicy Exportable -NotAfter (Get-Date).AddYears(50)
```
| Step | Description |
| :-: | :- |
| #4 | <kbd>Windows+R</kbd> and input <kbd>certmgr.msc</kbd> to view the certificate thumbprint |
| #5 | set password and export as PFX file |

```powershell
$pwdtmp = ConvertTo-SecureString -String [YOUR PASSWORD] -Force -AsPlainText
Export-PfxCertificate -cert "Cert:\CurrentUser\My\[YOUR THUMBPRINT]" -FilePath certificate.pfx -Password $pwdtmp
```

| Step | Description |
| :-: | :- |
| #6 | open <kbd>Package.appxmanifest</kbd> in VS and select <kbd>certificate.pfx</kbd> in package |
| #7 | type password to use the certificate |
| #8 | build, publish and redistribute the installer as well as the certificate under <kbd>AppPackages/*</kbd> |

