# ShelterVault
ShelterVault helps you to save your passwords in your local machine. All passwords are encrypted with the master key you set up at startup.

# Create a master key
- A database with all your passwords will be saved at user's profile directory.
- All passwords are encrypted using AES and a random initial vector.

![image](https://github.com/user-attachments/assets/aa6c7280-4771-4e97-a42b-40f662f59d8d)

# Log in
![image](https://github.com/user-attachments/assets/c1c8142c-4bab-426c-aafe-473211f0f065)

# Home screen
![image](https://github.com/user-attachments/assets/4343e9ac-17e2-4c76-8e71-e0db848fba66)

# Add new credentials
![image](https://github.com/user-attachments/assets/e6697c26-93f1-4bbc-9bcc-2927fb23aae8)

# View, edit or delete
![image](https://github.com/user-attachments/assets/26c03766-61b2-4ebe-afe1-64e69697b6d8)

# Themes
ShelterVault supports Light, Dark, and Neuromancer mode. It detects and uses your current Windows theme the first time the application is executed.

### Light mode
![image](https://github.com/user-attachments/assets/868e2910-fa76-4f58-ba0c-5f2fb136c733)

### Dark mode
![image](https://github.com/user-attachments/assets/4f286fdd-c5cf-4d81-9eb7-433b9b140b79)

### Neuromancer mode
![image](https://github.com/user-attachments/assets/26c03766-61b2-4ebe-afe1-64e69697b6d8)


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
