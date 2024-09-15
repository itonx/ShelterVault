Write-Output 'Fixing Add-AppDevPackage.ps1...'
(Get-Content .\Add-AppDevPackage.ps1).Replace('$DependencyPackages = @()', '[string[]]$DependencyPackages = @()') | Set-Content
.\Add-AppDevPackage.ps1
Write-Output 'Installing ShelterVault...'
.\Install.ps1