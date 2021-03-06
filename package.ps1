param([parameter(Mandatory=$true,HelpMessage="Enter the Build Configuration for the Package [Debug | Release]")]$buildConfig,
      [parameter(Mandatory=$true,HelpMessage="Enter the Target Configuration for the Package (leave blank for default)")]$targetConfig)

## Add MSBuild Dir to Path
$NETPath = Convert-Path -Path $env:windir'\Microsoft.NET\Framework\v4.0.30319\'
$env:Path = $env:Path + ";" + $NETPath

## Commands
$PublishInteractiveSDK = "MSBuild.exe InteractiveSdk\InteractiveSdk.Mvc\InteractiveSdk.ccproj /t:Publish /p:Configuration=$buildConfig /p:TargetProfile=$targetConfig /p:PublishDir=..\..\Deployment\InteractiveSdk\"
$PublishService = "MSBuild.exe DataService\Services\Services.ccproj /t:Publish /p:Configuration=$buildConfig /p:TargetProfile=$targetConfig /p:PublishDir=..\..\Deployment\Service\"

Invoke-Expression -Command $PublishInteractiveSDK -ErrorAction Stop
Invoke-Expression -Command $PublishService -ErrorAction Stop