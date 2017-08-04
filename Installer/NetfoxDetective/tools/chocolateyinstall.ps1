$ErrorActionPreference = 'Stop'; # stop on all errors

$packageName= 'NetfoxDetective' # arbitrary name for the package, used in messages
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'http://netfox.fit.vutbr.cz/Downloads/Netfox.Detective.Setup.20160505.msi' # download url
$url64      = 'http://netfox.fit.vutbr.cz/Downloads/Netfox.Detective.Setup.20160505.msi' # 64bit URL here or remove - if installer is both, use $url

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  fileType      = 'msi' #only one of these: exe, msi, msu
  url           = $url
  url64bit      = $url64
  validExitCodes= @(0, 3010, 1641)
  silentArgs   = '/qn'             # none; make silent with input macro script like AutoHotKey (AHK)
                                 #       https://chocolatey.org/packages/autohotkey.portable
  softwareName  = 'NetfoxDetective*' #part or all of the Display Name as you see it in Programs and Features. It should be enough to be unique
  checksum      = ''
  checksumType  = 'md5' #default is md5, can also be sha1
  checksum64    = ''
  checksumType64= 'md5' #default is checksumType
}

Install-ChocolateyPackage @packageArgs