<p align="center">
  <img src="https://github.com/itonx/ShelterVault/blob/assets/ShelterVaultApp.png"/>
</p>

---

<p align="center">
  <img width="200" alt="ShelterVault" src="https://github.com/itonx/ShelterVault/blob/main/ShelterVault/ShelterVault/Images/ShelterVault.png">
</p>
<p align="center">
  <a href="https://github.com/itonx/ShelterVault/actions/workflows/main.yml" target="_blank"><img src="https://github.com/itonx/ShelterVault/actions/workflows/main.yml/badge.svg?branch=main"/></a>
</p>

<p align="center">
	<a href="https://apps.microsoft.com/detail/9nhvsnjsx74g?mode=direct">
		<img src="https://get.microsoft.com/images/en-us%20light.svg" width="200"/>
	</a>
</p>

---

# 🔐 ShelterVault

ShelterVault helps you to save your passwords in your local machine. **All passwords are encrypted** with the master key you set up at startup.

Install it from the Microsoft Store.

<p align="center">
  <img alt="Desktiny.WinUI" src="https://github.com/itonx/Desktiny.WinUI/blob/main/assets/desktiny.png">
</p>
<p align="center">
	<a target="_blank" href="https://github.com/itonx/Desktiny.WinUI"><strong>Powered by Desktiny</strong></a>
</p>

# 🛡️ Main features:

- Create a master key.
- Access to ShelterVault using your master key.
- Create, update, or delete credentials.
- Change the theme at runtime.
- Multi-language (English & Spanish).
- Multi-vault management.
- Synchronize your vault by using your own Azure Cosmos DB.

# Azure Cosmos DB synchronization ![Static Badge](https://img.shields.io/badge/New-6c2987)

Although ShelterVault doesn't provide infrastructure for the cloud, you can configure ShelterVault to use your own Azure Cosmos DB. Your data will be synchronized across all devices with the same configuration. However, you have to configure ShelterVault device by device.

## Pre requisites:

- Cosmos DB container with a partition key named as ‘/type’
- Cosmos DB endpoint
- Cosmos DB Key
- Database name
- Container name

> See more about Azure Cosmos DB [here](https://learn.microsoft.com/en-us/azure/cosmos-db/)

## Steps to configure ShelterVault:

1. Go to Settings > Cloud Sync > Select Azure
2. Fill out the values of your Azure Cosmos DB configuration
3. Test the connection

If the connection was successful, the configuration will be encrypted and saved in your vault. The first synchronization will happen after 1 minute but a synchronization button will be available after testing the connection.

<p>
<img src="https://github.com/itonx/ShelterVault/blob/assets/AzureCosmosDBTestConnection.png"/>
</p>

## More about the synchronization

- ShelterVault will synchronize your vault every 60s by using short-polling technique.
- The configuration is valid for the vault you're working with.
- Short-polling requests to Azure Cosmos DB occur when you open a vault and only if you have a valid configuration.
- Azure Cosmos DB free tier should be enough for personal use.
- You're responsible for limiting your resources.

# 📚 Multi-language

Support for English & Spanish

<p>
<img src="https://github.com/itonx/ShelterVault/blob/assets/Languages.png"/>
</p>
<p>
<img src="https://github.com/itonx/ShelterVault/blob/assets/LanguageOptions.png"/>
</p>

# 🎨 Themes

ShelterVault supports Light, Dark, and Neuromancer mode. It detects and uses your current Windows theme the first time the application is executed.

### ☀️ Light mode

<p>
<img src="https://github.com/itonx/ShelterVault/blob/assets/LightTheme.png"/>
</p>

### 🌙 Dark mode

<p>
<img src="https://github.com/itonx/ShelterVault/blob/assets/DarkTheme.png"/>
</p>

### ☠️ Neuromancer mode ![Static Badge](https://img.shields.io/badge/New-6c2987)

<p>
<img src="https://github.com/itonx/ShelterVault/blob/assets/NeuromancerTheme.png"/>
</p>

# ℹ️ More about ShelterVault

- Data is saved in a SQLite3 database at user's profile directory.
- All passwords are encrypted using AES-256 and a random initial vector.

# 📝 Privacy Policy

[See policy](https://github.com/itonx/ShelterVault/blob/assets/PrivacyPolicy.md)
