# Create Master Key

The first time you open ShelterVault, it will ask you to create a vault and a master key. The ‘Cancel’ button won’t be available in this scenario.
You can also add a vault by using the ‘+’ button in the ‘Confirm Master Key’ page. The ‘Cancel’ button will be displayed in this scenario and it will allow you to go back to the ‘Confirm Master Key’ page if you pressed it.
Steps to create a master key:
1. Type the vault name
2. Type your master key.
3. Confirm your master key.

The master key must pass a set of validations that are displayed in the page. An error will be displayed if you try to save the vault when validations are not met.
If validations are met, a new file with the vault’s name will be created at %USERPROFILE%/.sheltervault, and you will be redirected to the ‘Confirm Master Key’ page.

# Confirm Master Key

It allows you to add new vaults, select one of the vaults saved at %USERPROFILE%/.sheltervault, and access to one of your vaults by using the master key you created for that vault.

## Add new vault
1. Click in the button with the ‘+’ icon.
2. The ‘Create Master Key’ page will be displayed.
3. Follow the ‘Steps to create a master key’ that were described previously.

## Access to your vault
1. Select one of the vaults in the dropdown.
2. Type the master key you set up for the vault and press ‘Enter’ key.

If the master key is not correct, you will see an error message.
If the master key is correct, you will be redirected to the ShelterVault’s NavigationView page.

# NavigationView (Vault)
It’s the main vault’s content navigation. It contains the next options:
- Home page:
  - It opens the home page.
- New credentials:
  - It opens the ‘Credentials’ page to add new credentials.
- Vault and credentials list:
  - Here you will see the name of your vault and all the credentials you’ve created. You can select one of the credentials to open the ‘Credentials’ page in edit mode, once in edit mode you can delete the credentials if needed.
- Settings: 
  - It’s a page where you can configure your own Azure Cosmos DB connection to synchronize your data with the cloud.

## New credentials
### Available buttons:
- Cancel: 
  - It cancels the current registration process of the new credentials and it will get you back to the home page if all fields are still empty. If you filled at least one field, it will ask for your confirmation to discard the changes.
- Show/Hide: 
  - It shows/hides the text of the password fields. The text of this button changes depending if the password is being shown/masked.
- Save: 
  - It saves the credentials and shows a confirmation message. It will validate if the name and password fields were filled and valid. Otherwise, an error message will be displayed.

### Steps to add the new credentials
1. Fill the fields in the page (only the ‘Title’ and password fields are required).
2. Ensure the password and password confirmation are the same and met the requirements displayed below the password confirmation field.
3. Save.

After saving the credentials, ShelterVault will display the ‘ID’ field in the page. If you see that field, it means the credentials were saved and you can now edit or delete the credentials. Your credentials will be listed on the left panel as well.

## Edit/Delete credentials
### Available buttons:
- Copy: It copies the password to your clipboard.
- Show/Hide: It shows/hides the text of the password fields. The text of this button changes depending if the password is being shown/masked.
- Save: It saves the credentials and shows a confirmation message. It will validate if the name and password fields were filled and valid. Otherwise, an error message will be displayed.
- Delete: It deletes the credentials and shows a confirmation message. It will ask for your confirmation if you edited one of the fields before deleting the credentials.

### Steps to edit credentials
1. Select one of the credentials in the left panel. If there are no credentials, add a new one using the ‘Add Credentials’ option.
2. Edit some fields in the page.
3. Save.
A confirmation message will be displayed. If one field is not valid, an error message will be displayed.

### Steps to delete credentials
1. Select one of the credentials in the left panel. If there are no credentials, add a new one using the ‘Add Credentials’ option.
2. Click on the ‘Delete’ button.
A confirmation message will be displayed. If one was edited before deleting the credentials, a confirmation message will be displayed.

# Settings
You can configure a cloud provider if needed. There are only two options available:
- None: 
  - This is the default options. It indicates ShelterVault to work in local mode only. If a cloud provider was previously configured, it will disable the synchronization but the cloud configuration will not be lost.
- Azure: 
  - When this option is selected, a subpage will be shown. Then, you can configure your Cosmos DB connection to synchronize your vault in the cloud.

## How to set up Azure Cosmos DB in ShelterVault for synchronization
### Pre requisites:
- Cosmos DB container with a partition key named as ‘/type’, see Azure Cosmos DB docs for more info: https://learn.microsoft.com/en-us/azure/cosmos-db/
- Cosmos DB endpoint
- Cosmos DB Key
- Database name
- Container name

### Enable synchronization in ShelterVault
1. Go to Settings > Cloud sync, expand the panel and select ‘Azure’ under cloud providers.
2. Add the Cosmos DB endpoint.
3. Add the Cosmos DB key.
4. Add the Cosmos DB database name of your container.
5. Add the Cosmos DB container name.
6. Click on ‘Test connection’.

If all the values are correct you will see a confirmation message, two new fields will be displayed showing the throughput (RU/s) of your database and the partition key. This configuration will be encrypted and saved in your vault.  Every time you press the ‘Test connection’ the configuration will be saved again.

IMPORTANT: the partition key must be /type, since ShelterVault uses that partition key to distinguish between vault and credentials.

Additionally, the ‘Synchronize’ button will be shown next to the ‘Test connection’ button and the synchronization status will appear at the top of the window showing the current status.

The ‘Synchronize’ button is only displayed after testing the connection with the ‘Test connection’ button. If you navigate to other pages and come back, the ‘Synchronize’ button might not appear, you need to test the connection again to display it. However, the button doesn’t mean the synchronization is not enabled, that will be determined by the synchronization status that is displayed at the top of the window.

### Additional notes about the synchronization
If the synchronization was enabled successfully, ShelterVault will look for new changes every 60 seconds. Due to a technical requirement in the app, ShelterVault will skip the synchronization if a dialog is being displayed in the app. The synchronization works as long as you are in the vault. If you add, edit, or delete credentials, ShelterVault will validate if there’s a cloud configuration and will try to make the updates in the cloud as well. The synchronization works in both ways, from local to cloud and from cloud to local.

### Synchronization Status
This is displayed at the top of the main window and if the cloud configuration is valid. If the cloud provider is set to ‘None’, this status will disappear. The status also works as a button, it will get you to the Settings page if you click it. 

When synchronization is enabled, you can see one of the following statuses:
- Sync not started: 
  - It means the cloud configuration is valid and was saved locally but the first synchronization after enabling the option hasn’t started yet.
- Synchronizing: 
  - It means ShelterVault is validating new changes between the local vault and the vault in the cloud.
- Up to date: 
  - It means the last synchronization was executed successfully and your vault is up to date.
- Sync failed: 
  - It means something failed while synchronizing your vault with the cloud. It could be related to a network issue or some configuration was changed in your cloud resources. You can use the ‘Test connection’ button to validate the configuration if needed.

# Switch the app theme
ShelterVault supports Light, Dark, and Neuromancer theme. You can change the current theme by using the ‘Theme button’ which is at the top of the main window. NOTE: The button doesn’t have text but you can easily identify it by the icon which changes depending of the current theme. Dark theme displays a moon icon, Light theme displays a brightness icon, and Neuromancer theme displays a processor icon.

# Switch to a different vault
If you are currently in a vault in ShelterVault, you can switch to another by using the ‘Switch Vault’ button which is at the top of the main window. This button is shown only when you load a vault with the master key associated to it. After pressing the button you’ll get back to the ‘Confirm Master Key’ page.

# Language support
ShelterVault supports English and Spanish languages. This option is only available while the ‘Create Master Key’ and ‘Confirm Master Key’ pages are being displayed. You can change the language by using the dropdown which is at the top of the main window.
