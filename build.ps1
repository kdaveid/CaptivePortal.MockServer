##########################################################################
# This is the Cake bootstrapper script for PowerShell.
# This file was downloaded from https://github.com/cake-build/resources
# Feel free to change this file to fit your needs.
##########################################################################

<#

.SYNOPSIS
This is a Powershell script to bootstrap a Cake build.

.DESCRIPTION
This Powershell script will download NuGet if missing, restore NuGet tools (including Cake)
and execute your Cake build script with the parameters you provide.

.PARAMETER Script
The build script to execute.
.PARAMETER Target
The build script target to run.
.PARAMETER Configuration
The build configuration to use.
.PARAMETER Verbosity
Specifies the amount of information to be displayed.
.PARAMETER Experimental
Tells Cake to use the latest Roslyn release.
.PARAMETER WhatIf
Performs a dry run of the build script.
No tasks will be executed.
.PARAMETER Mono
Tells Cake to use the Mono scripting engine.
.PARAMETER SkipToolPackageRestore
Skips restoring of packages.
.PARAMETER ScriptArgs
Remaining arguments are added here.

.LINK
http://cakebuild.net

#>

[CmdletBinding()]
Param(
    [string]$Script = "build.cake",
    [string]$Target = "Default",
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity = "Verbose",
    [switch]$Experimental,
    [Alias("DryRun","Noop")]
    [switch]$WhatIf,
    [switch]$Mono,
    [switch]$SkipToolPackageRestore,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

[Reflection.Assembly]::LoadWithPartialName("System.Security") | Out-Null
function MD5HashFile([string] $filePath)
{
    if ([string]::IsNullOrEmpty($filePath) -or !(Test-Path $filePath -PathType Leaf))
    {
        return $null
    }

    [System.IO.Stream] $file = $null;
    [System.Security.Cryptography.MD5] $md5 = $null;
    try
    {
        $md5 = [System.Security.Cryptography.MD5]::Create()
        $file = [System.IO.File]::OpenRead($filePath)
        return [System.BitConverter]::ToString($md5.ComputeHash($file))
    }
    finally
    {
        if ($file -ne $null)
        {
            $file.Dispose()
        }
    }
}

Write-Host "Preparing to run build script..."

if(!$PSScriptRoot){
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$ADDINS_DIR = Join-Path $TOOLS_DIR "addins"
$MODULES_DIR = Join-Path $TOOLS_DIR "modules"
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
$CAKE_EXE = Join-Path $TOOLS_DIR "Cake/Cake.exe"
$NUGET_URL = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
$PACKAGES_CONFIG = Join-Path $TOOLS_DIR "packages.config"
$PACKAGES_CONFIG_MD5 = Join-Path $TOOLS_DIR "packages.config.md5sum"
$ADDINS_PACKAGES_CONFIG = Join-Path $ADDINS_DIR "packages.config"
$MODULES_PACKAGES_CONFIG = Join-Path $MODULES_DIR "packages.config"

# Should we use mono?
$UseMono = "";
if($Mono.IsPresent) {
    Write-Verbose -Message "Using the Mono based scripting engine."
    $UseMono = "-mono"
}

# Should we use the new Roslyn?
$UseExperimental = "";
if($Experimental.IsPresent -and !($Mono.IsPresent)) {
    Write-Verbose -Message "Using experimental version of Roslyn."
    $UseExperimental = "-experimental"
}

# Is this a dry run?
$UseDryRun = "";
if($WhatIf.IsPresent) {
    $UseDryRun = "-dryrun"
}

# Make sure tools folder exists
if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR)) {
    Write-Verbose -Message "Creating tools directory..."
    New-Item -Path $TOOLS_DIR -Type directory | out-null
}

# Make sure that packages.config exist.
if (!(Test-Path $PACKAGES_CONFIG)) {
    Write-Verbose -Message "Downloading packages.config..."
    try { (New-Object System.Net.WebClient).DownloadFile("http://cakebuild.net/download/bootstrapper/packages", $PACKAGES_CONFIG) } catch {
        Throw "Could not download packages.config."
    }
}

# Try find NuGet.exe in path if not exists
if (!(Test-Path $NUGET_EXE)) {
    Write-Verbose -Message "Trying to find nuget.exe in PATH..."
    $existingPaths = $Env:Path -Split ';' | Where-Object { (![string]::IsNullOrEmpty($_)) -and (Test-Path $_ -PathType Container) }
    $NUGET_EXE_IN_PATH = Get-ChildItem -Path $existingPaths -Filter "nuget.exe" | Select -First 1
    if ($NUGET_EXE_IN_PATH -ne $null -and (Test-Path $NUGET_EXE_IN_PATH.FullName)) {
        Write-Verbose -Message "Found in PATH at $($NUGET_EXE_IN_PATH.FullName)."
        $NUGET_EXE = $NUGET_EXE_IN_PATH.FullName
    }
}

# Try download NuGet.exe if not exists
if (!(Test-Path $NUGET_EXE)) {
    Write-Verbose -Message "Downloading NuGet.exe..."
    try {
        (New-Object System.Net.WebClient).DownloadFile($NUGET_URL, $NUGET_EXE)
    } catch {
        Throw "Could not download NuGet.exe."
    }
}

# Save nuget.exe path to environment to be available to child processed
$ENV:NUGET_EXE = $NUGET_EXE

# Restore tools from NuGet?
if(-Not $SkipToolPackageRestore.IsPresent) {
    Push-Location
    Set-Location $TOOLS_DIR

    # Check for changes in packages.config and remove installed tools if true.
    [string] $md5Hash = MD5HashFile($PACKAGES_CONFIG)
    if((!(Test-Path $PACKAGES_CONFIG_MD5)) -Or
      ($md5Hash -ne (Get-Content $PACKAGES_CONFIG_MD5 ))) {
        Write-Verbose -Message "Missing or changed package.config hash..."
        Remove-Item * -Recurse -Exclude packages.config,nuget.exe
    }

    Write-Verbose -Message "Restoring tools from NuGet..."
    $NuGetOutput = Invoke-Expression "&`"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$TOOLS_DIR`""

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while restoring NuGet tools."
    }
    else
    {
        $md5Hash | Out-File $PACKAGES_CONFIG_MD5 -Encoding "ASCII"
    }
    Write-Verbose -Message ($NuGetOutput | out-string)
    
    Pop-Location
}

# Restore addins from NuGet
if (Test-Path $ADDINS_PACKAGES_CONFIG) {
    Push-Location
    Set-Location $ADDINS_DIR

    Write-Verbose -Message "Restoring addins from NuGet..."
    $NuGetOutput = Invoke-Expression "&`"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$ADDINS_DIR`""

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while restoring NuGet addins."
    }

    Write-Verbose -Message ($NuGetOutput | out-string)

    Pop-Location
}

# Restore modules from NuGet
if (Test-Path $MODULES_PACKAGES_CONFIG) {
    Push-Location
    Set-Location $MODULES_DIR

    Write-Verbose -Message "Restoring modules from NuGet..."
    $NuGetOutput = Invoke-Expression "&`"$NUGET_EXE`" install -ExcludeVersion -OutputDirectory `"$MODULES_DIR`""

    if ($LASTEXITCODE -ne 0) {
        Throw "An error occured while restoring NuGet modules."
    }

    Write-Verbose -Message ($NuGetOutput | out-string)

    Pop-Location
}

# Make sure that Cake has been installed.
if (!(Test-Path $CAKE_EXE)) {
    Throw "Could not find Cake.exe at $CAKE_EXE"
}

# Start Cake
Write-Host "Running build script..."
Invoke-Expression "& `"$CAKE_EXE`" `"$Script`" -target=`"$Target`" -configuration=`"$Configuration`" -verbosity=`"$Verbosity`" $UseMono $UseDryRun $UseExperimental $ScriptArgs"
exit $LASTEXITCODE
# SIG # Begin signature block
# MIIPgQYJKoZIhvcNAQcCoIIPcjCCD24CAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUgeK9JEmr3/flQ9u/NvXD1NNM
# IdagggvAMIIF2DCCA8CgAwIBAgIQbDvSft08lJ6Vjiips8dXoDANBgkqhkiG9w0B
# AQsFADB9MQswCQYDVQQGEwJJTDEWMBQGA1UEChMNU3RhcnRDb20gTHRkLjErMCkG
# A1UECxMiU2VjdXJlIERpZ2l0YWwgQ2VydGlmaWNhdGUgU2lnbmluZzEpMCcGA1UE
# AxMgU3RhcnRDb20gQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkwHhcNMTUxMjE2MDEw
# MDA1WhcNMzAxMjE2MDEwMDA1WjB1MQswCQYDVQQGEwJJTDEWMBQGA1UEChMNU3Rh
# cnRDb20gTHRkLjEpMCcGA1UECxMgU3RhcnRDb20gQ2VydGlmaWNhdGlvbiBBdXRo
# b3JpdHkxIzAhBgNVBAMTGlN0YXJ0Q29tIENsYXNzIDIgT2JqZWN0IENBMIIBIjAN
# BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAuRQEWPeyxYYsCDJgrQgmwIF3uWgZ
# 2RUrHRhp5NoalgWXLmR5Gqk9UTNa0Hdq9AKTQcOOunAbq9h7dG+Y6Ne5qT5odqSJ
# oCKsF9Yp+Lu4YZ/SB9BmDjBHICtwAh7+cwkccTS14n6prKin8Y46QAZ2ksr3eGzv
# WAVzfX+DUOmiVQLjAK6Wp8bCZHvj+FhAlS5Ne7/dggDeSVWnMyPm2k/5YKOTVXEx
# JJaAlYkmyH1OiC3soTkkGb6aJjGJPHiaiNJ4pjkySX5l2p4DQ7K1/J6ft5Vw9Puq
# wmYrF0ViGnn38kzB2d9UI9Q+dFmHUbV+cnr+FoGl6CiUDd5ZIF1HMrb8hwIDAQAB
# o4IBWjCCAVYwDgYDVR0PAQH/BAQDAgEGMBMGA1UdJQQMMAoGCCsGAQUFBwMDMBIG
# A1UdEwEB/wQIMAYBAf8CAQAwMgYDVR0fBCswKTAnoCWgI4YhaHR0cDovL2NybC5z
# dGFydHNzbC5jb20vc2ZzY2EuY3JsMGYGCCsGAQUFBwEBBFowWDAkBggrBgEFBQcw
# AYYYaHR0cDovL29jc3Auc3RhcnRzc2wuY29tMDAGCCsGAQUFBzAChiRodHRwOi8v
# YWlhLnN0YXJ0c3NsLmNvbS9jZXJ0cy9jYS5jcnQwHQYDVR0OBBYEFD5ik5rXxxnu
# Po9JEIVVFSDjlIQcMB8GA1UdIwQYMBaAFE4L7xqkQFulF2mHMMo0aEPQQa7yMD8G
# A1UdIAQ4MDYwNAYEVR0gADAsMCoGCCsGAQUFBwIBFh5odHRwOi8vd3d3LnN0YXJ0
# c3NsLmNvbS9wb2xpY3kwDQYJKoZIhvcNAQELBQADggIBAGOlPNWzbSco2Ou6U68w
# C+pKXRLV+ZrKcPpMY4zXTVR+RupS54WhJCManab2P1ncPlHTbRMbPjfHnyj0sIdp
# vwcV49n0nizMF3MBxaKJEnBBEfHs9Krgjc4qKjR2nOywlzxJ0M27RthR5XjyjQ1o
# fHlOisYgMzcyKyMT7YYpxxoC0wTgAh0DNmE5Q/GKFOaDd3S5gTqrR9AQzGaC3IxC
# KBFtcwvk51W98lNRtMbm+oJze5T+dL2wIhyWK58sEIl2paAVfAfWGH3umYL46scL
# n8BXDFchN1Jgrg07DqY6gxCqSdubPhVHZInuVagktWmrnS6N9V/vVLz+OaX4Mkas
# 8n1J1RIR+GV8ZQVmTM49l6L+fpv/h95MWLhQOcXanbIY/2cdNEuz5AkhfvDNTQnL
# xYEMIyMOtW2QIwwZdz92vMTU17G9goxXYjSm09yw+iBniH9G/xGz39BV3bwa8ZtK
# HzDoZ54HT6JT2AraDhrWTwFXv8Xrvv2cir+k0h5bIWlDtImH7Jm152edb77f5JI8
# JrPf6jxcUrhNH4xHxe2kGs8ERA39oYlT0dKQIb0obTN6FOF63hBRFFhGB7NuX2Fe
# FjJsZFCkoJkpsEauObb7Rh+C02+fnHfoi6ivKwUC9BOsWlI4xn7GMe27niL6k7wp
# K0L6MTG5/6gxwosqaMA1aukwMIIF4DCCBMigAwIBAgIQEMr8Kn0NOBZUDMsePMiX
# VDANBgkqhkiG9w0BAQsFADB1MQswCQYDVQQGEwJJTDEWMBQGA1UEChMNU3RhcnRD
# b20gTHRkLjEpMCcGA1UECxMgU3RhcnRDb20gQ2VydGlmaWNhdGlvbiBBdXRob3Jp
# dHkxIzAhBgNVBAMTGlN0YXJ0Q29tIENsYXNzIDIgT2JqZWN0IENBMB4XDTE2MDYz
# MDIzMzE1OFoXDTE4MDYzMDIzMzE1OFowTDELMAkGA1UEBhMCQ0gxEDAOBgNVBAgM
# B1RodXJnYXUxFDASBgNVBAcMC0tyZXV6bGluZ2VuMRUwEwYDVQQDDAxEYXZpZCBL
# ZWxsZXIwggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQCr5rR1wXaK0TTn
# u5pR+QINIkv7PwLQX6aypgHNZbEvbG8IO08DukujGF5nCa4pf4iCJbXUfZPDDcDy
# FeW9ez+4MQxUz6NC/DwNZKbXOddwhGXR9oxC6kwMtFIunI2lEr3kbXmDCPOUSGZ4
# +bh8m5gKWhIZjLaBgENYvOQOR+P9aqdVR4V7fnHe07jYF24Ikt06/CSU8nNsn7AZ
# iOEEReo5VNqGJEKVze1KRO2DVtjcuJG36ufGMl6zEYGE8sSm6DoL4DsimAV9IB5N
# +N1f6vwtK9qw+J/eQMjyv9SMLb3ZAYkkTcm0/v87CqUO8+jtPUmipwmD6NAgkmh0
# mFizW3Ov85GaR13hp+xqlJ9IlFrfDPpyjHoU/r6Hthdm+QTpnfi0l9qNRSnR2EQK
# vW2W0VTaZf7rh9kyQiM76Yot9E8qkNU0bxoYBg3tknisNIXkX0i5mx4g6ca3xweN
# wRR8ZCaJWzYg2ct8kg0BOL3WgvncalvqhPd4ozDgCabJVlJYE6l4mYiIq4pEnUqk
# LZeZXSBjXRrCEqfCKey2RefcxtK4O7tlDSd9Kc1BU7LcDjwU7MrcCHBuLF+fEvDl
# ADGsAYkulliP8OMmmCpXsJqsvR0ZDGuonm99BHUeMGxKiHbz0YrdCVwhdFt9louM
# suo0om8wbHJvrwU8c8FVT3Y1MzERDQIDAQABo4IBkzCCAY8wDgYDVR0PAQH/BAQD
# AgeAMBMGA1UdJQQMMAoGCCsGAQUFBwMDMAkGA1UdEwQCMAAwHQYDVR0OBBYEFHqZ
# S4Ql+jV22+PuiyQMSCI0wmlwMB8GA1UdIwQYMBaAFD5ik5rXxxnuPo9JEIVVFSDj
# lIQcMG0GCCsGAQUFBwEBBGEwXzAkBggrBgEFBQcwAYYYaHR0cDovL29jc3Auc3Rh
# cnRzc2wuY29tMDcGCCsGAQUFBzAChitodHRwOi8vYWlhLnN0YXJ0c3NsLmNvbS9j
# ZXJ0cy9zY2EuY29kZTIuY3J0MDYGA1UdHwQvMC0wK6ApoCeGJWh0dHA6Ly9jcmwu
# c3RhcnRzc2wuY29tL3NjYS1jb2RlMi5jcmwwIwYDVR0SBBwwGoYYaHR0cDovL3d3
# dy5zdGFydHNzbC5jb20vMFEGA1UdIARKMEgwCAYGZ4EMAQQBMDwGCysGAQQBgbU3
# AQIFMC0wKwYIKwYBBQUHAgEWH2h0dHBzOi8vd3d3LnN0YXJ0c3NsLmNvbS9wb2xp
# Y3kwDQYJKoZIhvcNAQELBQADggEBACnlpFAzi/kKpM0ttidqXdBznKiBJoCcRo5V
# qagZJy5in2TCWqsrDMw9YN/M3LjN/63i0W/bCNGTZx8IlKDUvJzYYk6DPzXd4LXN
# 0bZv8WZJesCEtpLxVqK6xT8eQXQP4GLp4WBngkFWbBAaIV6UjjQ3jtP3uBRwcxq1
# N4gwWW3tozA3ZILrc4TCqFG6tX4m690B/DsgUdhEwjXIf7FbNsoa8t9ndQe8PjQk
# 7Ks7wPXHGjNAEI6Jdesv5NxWgTe8QZJOEulViZYnYOjDY3Jyv9/qll3Ywtj2drh1
# lknfRezVw8H00+tW4uQM1WPrkGj+X/ujWdaxKJdSN9TD2Cvk894xggMrMIIDJwIB
# ATCBiTB1MQswCQYDVQQGEwJJTDEWMBQGA1UEChMNU3RhcnRDb20gTHRkLjEpMCcG
# A1UECxMgU3RhcnRDb20gQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkxIzAhBgNVBAMT
# GlN0YXJ0Q29tIENsYXNzIDIgT2JqZWN0IENBAhAQyvwqfQ04FlQMyx48yJdUMAkG
# BSsOAwIaBQCgeDAYBgorBgEEAYI3AgEMMQowCKACgAChAoAAMBkGCSqGSIb3DQEJ
# AzEMBgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEEAYI3AgEVMCMG
# CSqGSIb3DQEJBDEWBBTTAVkQ9vWmG/SXOE6rMn4nzai+NjANBgkqhkiG9w0BAQEF
# AASCAgCZQQW6RhrJITTX5pxfiPm1NjiGEaqpsNmApl5fe0RzNdtc4CCQccX1jEs+
# 3scPF4R+NWe7BW6h3AyjH4rwsE8M3EJqN8Rt7X3VLOsvXcHmOXs/SL50Skv7qUoI
# MXXkpTgCSATd/e9F767ite4QV4tT2EiuGULhlRhxcu+oiRH1DGqQ9AL9ij9R1ChH
# how/5qvJu37js97mLS55CGA5wCrxY5TJ/Gu3QRj1II1dK88mHcTP9l4MOLmKuMNn
# eHGhka8CN6IdKEy/WfFrJzHI1Y8AY7pkzWYN50F1BxB+gIY2ToY7jgLVNqXA8aR4
# ZvmD9+Y0vfa9BpNcwYVWmUzDPcx3Q6HBplg5EY21xkaw3+mFJoZ1WW+Yxz5C5ZQG
# mt9+uJz3XuzyGx4iUWwt4EZhYOHFQmEWVYTyBB+4r3Z7dATOJwTaVhK4p87N5QA5
# dI5qA8Ci5mwRKLI7l4NBPbpnK04adAmoGg7z9MCmHt5pDpwSKy0mqTjamlK3BjHr
# 6rUBZo3ZIAuNCJ0IP713Egi3sSAmwenh0iwtRGmxEgKApT6Cs563R71OrMkxSLPj
# Y6KBce+MjcoAsHbhmumaB6vXs7ad0OJQY6qvoWej6aVqnbHJQekAHuc7fa3HeQza
# h0MPihq02KJ5oeOb7EEyan2XvNLmokpQhs7pGf8t5Ml4Zua0Yg==
# SIG # End signature block
