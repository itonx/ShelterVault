# ShelterVault
ShelterVault helps you to save your passwords in your local machine. All passwords are encrypted with the master key you set up at startup.

# Create a master key
- A database with all your passwords will be saved at user's profile directory.
- All passwords are encrypted using AES and a random initial vector.

![image](https://github.com/user-attachments/assets/d7982ac7-52fd-44b0-b59e-cc1e57b74e3d)

# Log in
![image](https://github.com/user-attachments/assets/34647d85-bdf6-4a01-9b80-db457ec7660d)

# Initial screen
![image](https://github.com/user-attachments/assets/f219e6b6-4699-420c-929a-64793e37f48e)

# Add new credentials
![image](https://github.com/user-attachments/assets/3ef0c5e2-94c2-4a1c-b78f-93bba3cac021)

# View, edit or delete
![image](https://github.com/user-attachments/assets/87e09fae-733b-423f-bcac-1b3c6de119e1)

# Themes
ShelterVault is available in Light and Dark mode. It detects and uses your current Windows theme.

### Dark mode
![image](https://github.com/user-attachments/assets/6d000fbd-da25-4908-a759-eccc37afa3ed)
![image](https://github.com/user-attachments/assets/51be8bcc-060a-475c-9be0-7a305ada39f4)
![image](https://github.com/user-attachments/assets/36d3d4da-a72c-4bd1-ae35-7a9934358025)
![image](https://github.com/user-attachments/assets/d10b210f-f300-46dc-9934-9ee022dd1559)
![image](https://github.com/user-attachments/assets/4b5674a7-50d3-4efc-b64d-67da7347bd29)

# How to install beta version
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
