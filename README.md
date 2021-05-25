
# NetfoxCore

  

# About

NFX Detective is a novel Network forensic analysis tool that implements methods for extraction of application content from communication using supported protocols. The implemented functionality includes:

* Analysis project management that enables to analyze multiple PCAPs in a single session. Support for large PCAP files, up to GBs.

* Advanced visualization using different views of various levels of detail - from overview to detailed information about every single packet.

* A collection of parsers and content extraction methods for the most used application protocols.

Filtering and full-text search in captured traffic.

  

NFX Detective is an extensible platform that can be customized to individual requirements:

* Possibility to create new extraction modules for other application protocols.

* Extension of the system with user defined analytical methods. NFX Detective employs open data model that can be accessed or easily modified.

* Definition of new views on the data. Data are stored in a SQL database and can be efficiently accessed through the well-defined interface.

## Supported application protocols

To demonstrate Netfox Detective analysis capcabilitie, you may analyze PCAP files containing following application protocols:

* BTC - Stratum

* DNS

* Facebook*

* FTP

* Hangouts*

* HTTP

* OSCAR - ICQ

* IMAP

* Lide.cz

* Messenger

* Minecrat

* MQTT

* POP3

* RTP

* SIP

* SMTP

* SPDY

* Twitter*

* Webmails - various services*

* Xchat.cz

* XMPP

* YMSG

\* SSL/TLS decryption key is mandatory

  

## Supported PCAP files

* [LibPCAP](https://wiki.wireshark.org/Development/LibpcapFileFormat)

* [Pcap-ng](https://wiki.wireshark.org/Development/PcapNg)

* [Microsoft Network Monitor cap](https://en.wikipedia.org/wiki/Microsoft_Network_Monitor)

  

# Download

  

Netfox Detective can be installed from release builds available on [FourceForge](https://sourceforge.net/projects/netfox-detective/).

  

# Before you start

Before you start installing NFX Detective, please check that your system meets the requirements. Microsoft Windows Vista SP2 and newer is reauired to run Netfox Detective.

### Minimal hardware configuration:

* 1 GHz 64-bit (x64) processor

* 2 GB RAM

* 4 GB available hard disk space

* DirectX 9 graphics device with WDDM 1.0 or higher driver

* 1,024 x 768 with true color

### Recommended hardware configuration

* 1 GHz 64-bit (x64) processor

* 8 GB RAM

* 64 GB available SSD hard disk space

* DirectX 9 graphics device with WDDM 1.0 or higher driver

* 1,920 x 1,200 with true color

  

# Prerequisites for successful compilation and easy development: #

  

## Mandatory tools to download

* Visual Studio 2017 Enterprise

* [Microsoft .NET Framework 4.7 Developer Pack](https://www.microsoft.com/en-us/download/details.aspx?id=55168)

* [Microsoft .NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)

* [SQL Server 2016 SP1 Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) - for data persistence support

* [DotVVM](https://www.dotvvm.com/install)

  

* [Git LFS](https://git-lfs.github.com/)

* `git config --global lfs.dialtimeout 0`

* Without this the HTTP session while pushing LFS tracked file will timeout.

  

It is strongly advised to remove any obj and bin folders from solution upon repeatedly unsuccessful compilation.

  

## Required libraries

* Telerik UI for WPF - [FreeTrial](http://www.telerik.com/products/wpf/overview.aspx) - copy to /lib

* Infragistics WPF - [FreeTrial](https://www.infragistics.com/free-downloads) - copy to /lib

* PostSharp - [FreeTrial](https://www.postsharp.net/download)

* Additional libraries are installed from NuGet.

  

## Contribution

It is **mandatory** to develop according to *GitFlow*. More information can be found in [contributing notes](/docs/CONTRIBUTING.md).

  

## Useful Visual Studio tools

* [Resharper](https://www.jetbrains.com/resharper/download/)

* [MarkdownEditor](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.MarkdownEditor)

* [GitFlow for Visual Studio 2017](https://marketplace.visualstudio.com/items?itemName=vs-publisher-57624.GitFlowforVisualStudio2017)

* [Wix Setup tools](https://wix.codeplex.com/releases/view/624906)

* [Wix VS Extension](https://marketplace.visualstudio.com/items?itemName=RobMensching.WixToolsetVisualStudio2017Extension)

* [NuGet Package Project](https://marketplace.visualstudio.com/items?itemName=NuProjTeam.NuGetPackageProject)