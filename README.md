<p align="center">
  <img src="https://github.com/itonx/ShelterVault/blob/assets/ShelterVaultApp.png"/>
</p>

--- 

<p align="center">
  <picture>
  <source media="(prefers-color-scheme: dark)" srcset="https://github.com/itonx/ShelterVault/blob/assets/ShelterVaultDarkThemeLogo.png">
  <source media="(prefers-color-scheme: light)" srcset="https://github.com/itonx/ShelterVault/blob/assets/ShelterVaultLightThemeLogo.png">
  <img alt="ShelterVault" src="https://github.com/itonx/ShelterVault/blob/assets/ShelterVaultDarkThemeLogo.png">
  </picture>
</p>
<p align="center">
  <a href="https://github.com/itonx/ShelterVault/actions/workflows/main.yml" target="_blank"><img src="https://github.com/itonx/ShelterVault/actions/workflows/main.yml/badge.svg?branch=main"/></a>
</p>

---

# 🔐 ShelterVault
ShelterVault helps you to save your passwords in your local machine. <ins>All passwords are encrypted</ins> with the master key you set up at startup.

# 🛡️ Main features:
- Create a master key.
- Access to ShelterVault using your master key.
- Create, update, or delete credentials.
- Change the theme at runtime.
- Multi-language (English & Spanish). ![Static Badge](https://img.shields.io/badge/New-6c2987)
- Multi-vault management. ![Static Badge](https://img.shields.io/badge/New-6c2987)

# ℹ️ More about ShelterVault
- Data is saved in a SQLite3 database at user's profile directory.
- All passwords are encrypted using AES and a random initial vector.
- Tested in Windows 11.

# 🎨 Themes
ShelterVault supports Light, Dark, and Neuromancer mode. It detects and uses your current Windows theme the first time the application is executed.

### ☀️ Light mode
![image](https://github.com/user-attachments/assets/868e2910-fa76-4f58-ba0c-5f2fb136c733)

### 🌙 Dark mode
![image](https://github.com/user-attachments/assets/4f286fdd-c5cf-4d81-9eb7-433b9b140b79)

### ☠️ Neuromancer mode ![Static Badge](https://img.shields.io/badge/New-6c2987)

# 🖥️ How to install the beta version
All WinUI3 applications must be signed in order to be installed in your Windows machine. This beta version is using a self-signed certificate so you will need to follow the next steps to install the application.
1. Download the .zip file for x86 or x64 architecture.
2. Unzip the file.
3. Open a CMD or Powershell window.
4. Go to the unzipped folder > ShelterVault_$VERSION_$ARCH_Test
5. Execute the Install_ShelterVault.ps1 script with the next command:
```
pwsh.exe --ExecutionPolicy Bypass -File .\Install_ShelterVault.ps1
```
>Note
>
>Default Add-AppDevPackage.ps1 script created by msbuild has a bug which stops the installation. Step 5 executes a custom script which modifies the Add-AppDevPackage.ps1 script to fix the installation error. However, Install_ShelterVault.ps1 script is not signed during the GitHub workflow execution so that's why you need to execute it with the Bypass policy option.
6. Install the certificate by accepting the options in the script as well as the options to install the application.
![image](https://github.com/user-attachments/assets/8335b0e1-a210-4164-bc58-64fab5f7df89)
7. Once the app is installed look for ShelterVault in Windows.
![image](https://github.com/user-attachments/assets/c32fd34c-3f02-4272-afee-fd7e2e798132)
