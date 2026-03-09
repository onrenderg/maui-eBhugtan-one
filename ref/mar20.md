
eBhugtan
Hp

55154989442
pra

sol eBhugtan


RefrencePrj


```xaml
	<!-- remove following folder in root of prj  from compilation  -->
	<ItemGroup>
	  <Compile Remove="RefrencePrj\**" />
	  <None Remove="RefrencePrj\**" />
	  <Content Remove="RefrencePrj\**" />
	  <MauiXaml Remove="RefrencePrj\**" />
	  <EmbeddedResource Remove="RefrencePrj\**" />
	</ItemGroup>
	
```


rm -f -r .git



nuget 

sqlite-net-pcl
Newtonsoft.Json
SQLitePCLRaw.bundle_e_sqlite3


```powershell
Get-ChildItem -File | Rename-Item -NewName { $_.Name.ToLower() }
```

```xaml
<!-- BEFORE (Xamarin) -->
xmlns="http://xamarin.com/schemas/2014/forms"

<!-- AFTER (MAUI) -->
xmlns="http://schemas.microsoft.com/dotnet/2021/maui"


<!-- BEFORE -->
xmlns:ios="clr-namespace:Xamarin.Forms.PlatformConfiguration.iOSSpecific;assembly=Xamarin.Forms.Core"

<!-- AFTER -->
xmlns:ios="clr-namespace:Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;assembly=Microsoft.Maui.Controls"



```


```cs
using Xamarin.Forms;	
using Microsoft.Maui.Controls;


using Xamarin.Forms.Xaml;	
using Microsoft.Maui.Controls.Xaml;

using Xamarin.Essentials;	
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage; (for Preferences, SecureStorage)
using Microsoft.Maui.Networking; (for Connectivity)
using Microsoft.Maui.Devices; (for DeviceInfo)


Plugin.Permissions	
Microsoft.Maui.ApplicationModel (built-in)

Plugin.Media	
Microsoft.Maui.Media (built-in in MAUI)

Plugin.Connectivity	
Microsoft.Maui.Networking (built-in)

```




MAUI PORT: Updated 'using' statements changes
```cs
using Xamarin.Forms.Xaml;

using Xamarin.Forms;
Microsoft.Maui.Controls 

Xamarin.Essentials
Microsoft.Maui.ApplicationModel;



```


```cs
//using Xamarin.Essentials;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Storage;            // Preferences
using Microsoft.Maui.Networking;         // Connectivity
using Microsoft.Maui.ApplicationModel;   // Launcher, AppInfo
using Microsoft.Maui.Devices;            // DeviceInfo
```





```xml

xmlns="http://xamarin.com/schemas/2014/forms"
xmlns="http://schemas.microsoft.com/dotnet/2021/maui"

xmlns:ios="clr-namespace:Xamarin.Forms.PlatformConfiguration.iOSSpecific;assembly=Xamarin.Forms.Core"
xmlns:ios="clr-namespace:Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;assembly=Microsoft.Maui.Controls"
```



## Table model stuff 

```cs
# App.xaml.cs
public static string Dbname = "test.db"

//conn = DependencyService.Get<ISQLite>().GetConnection();
//conn.CreateTable<Accounts>();
var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.Dbname);
conn = new SQLiteConnection(dbPath);
conn.CreateTable<Accounts>();
 
//using Xamarin.Essentials;
//using Xamarin.Forms;
using Microsoft.Maui.Controls;

//conn = DependencyService.Get<ISQLite>().GetConnection();
//conn.CreateTable<Groundkeywords>();
var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.Dbnam);
            conn = new SQLiteConnection(dbPath);
            conn.CreateTable<CitizenProfile>();
```



# Tools 
* Gitingest
	* local-gitingest -exclude .log,.tmp,.bak -o my_repo.txt -size-limit -max-size 102400


Filter for your app
```powershell
adb logcat -c
adb logcat -d
// carsh then 
adb logcat -d | findstr /I "ArgumentNullException"

adb logcat | Select-String "MauiApp1"
adb logcat | Select-String "ArgumentNullException\|cancel"



```

```powershell
Remove-Item -Recurse -Force bin,obj
adb uninstall  com.companyname.ebhugtan
adb uninstall com.companyname.gacappeal
```


```cs
if ((int)response.StatusCode == 404)
{
    System.Diagnostics.Debug.WriteLine($"mgogo: {response}");
    await App.Current.MainPage.DisplayAlert("mgogo HttpPut", $"{response}", "OK");


}
```


```


Prompt 
* updated .cs files with Xamarin code commented out alonside the updated MAUI-safe code added with comment for usage of ported maui part :


```csharp

// using Xamarin.Forms;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
// using Xamarin.Essentials;
using Microsoft.Forms.Essentials;
// using Xamarin.Essentials;
using Microsoft.Forms.Essentials;

//using Xamarin.Essentials;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Storage;            // Preferences
using Microsoft.Maui.Networking;         // Connectivity
using Microsoft.Maui.ApplicationModel;   // Launcher, AppInfo
using Microsoft.Maui.Devices;            // DeviceInfo



using System.Threading.Tasks;

// --- MAUI MainThread Change ---
// Xamarin.Forms: 
Device.BeginInvokeOnMainThread(async () =>
// MAUI: Use 
MainThread.BeginInvokeOnMainThread

- Removed invalid `using Android.Media.TV;`
- Added missing `using System.IO;`
```




Replace the RelativeLayout activity1 block with an AbsoluteLayout equivalent in each file:


# Content view nav 


```xaml
xmlns:views="clr-namespace:eBhugtan">
<NavigationPage.TitleView>
        <views:NavTitleView />
</NavigationPage.TitleView>

```

```xaml
<?xml version="1.0" encoding="utf-8"?>
<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    x:Class="eBhugtan.NavTitleView">
    <Grid x:Name="Custom_Navigation"
        ColumnDefinitions="Auto,*"
        HorizontalOptions="Fill"
        VerticalOptions="Center">
        <Image Grid.Column="0" Source="Hpgov.png"
            VerticalOptions="Center" />
        <Label Grid.Column="1"
            Text="e-भुगतान ,HP"
            TextColor="#05083B"
            FontSize="19"
            FontAttributes="Bold"
            HorizontalTextAlignment="Center"
            HorizontalOptions="Fill"
            VerticalTextAlignment="Center" />
    </Grid>
</ContentView>

```


```cs
namespace eBhugtan;

public partial class NavTitleView : ContentView
{
	public NavTitleView()
	{
		InitializeComponent();
	}
}
```













    <Grid.RowDefinitions>
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>


    <Grid RowDefinitions="*,Auto">


    Primary=  #2196f3



    		<!-- App Icon -->
		<!-- <MauiIcon Include="Resources\AppIcon\appicon.svg"
			ForegroundFile="Resources\AppIcon\appiconfg.svg" Color="White" /> -->
		<MauiIcon Include="Resources\AppIcon\appicon.svg" ForegroundFile="Resources\AppIcon\appiconfg-ios.svg" Color="White" Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'" />

		<MauiIcon Include="Resources\AppIcon\appicon.svg" ForegroundFile="Resources\AppIcon\appiconfg-and.svg" Color="White" Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'" />





todo 
icon selected and unselceted

adb uninstall com.companyname.etransfers


MidnightBlue  Gray900
VECTOR PNG 
https://pngtree.com/
https://www.svgrepo.com/
https://himexam.com/






\\wsl.localhost\Ubuntu-24.04\home\usr\Downloads
\\wsl$
https://github.com/onrenderg/obsidian-base-deploy.git
python3 -m http.server
https://learn.microsoft.com/en-us/java/openjdk/download + C:\Program Files\Microsoft\jdk-17.0.18.8-hotspot\



adb uninstall nic.hp.etransfers
Remove-Item -Path bin, obj -Recurse -Force


android:usesCleartextTraffic="true"
  <uses-permission android:name="android.permission.INTERNET" />

http://10.146.50.78/DigitalNagrik

https://play.google.com/store/apps/details?id=hp.nic.himweb&hl=en




one issue if i keep "CandidateImage, which is base64 very logn string in this it comes in rows of other columns ? may be due to \n in stirng or what how to rightly handle it to place it ints column not mess othere 




adb uninstall nic.hp.etransfers




# Wipe Linux-side remnants
rm -rf ~/.antigravity ~/.config/antigravity ~/.cache/antigravity ~/.local/share/antigravity

# Wipe Windows-side remnants (accessed via WSL mount)
rm -rf /mnt/c/Users/parth-nic/.antigravity
rm -rf /mnt/c/Users/parth-nic/AppData/Roaming/antigravity
rm -rf /mnt/c/Users/parth-nic/AppData/Local/antigravity





cd C:\Users\parth-nic\AppData\Roaming\Code\User\globalStorage\ms-dotnettools.dotnet-interactive-vscode
del "C:\Users\parth-nic\AppData\Roaming\Code - Insiders\User\globalStorage\ms-dotnettools.dotnet-interactive-vscode\dotnet-tools.json"
  155  * Gitingest
  156:  * local-gitingest -exclude .log,.tmp,.bak -o my_repo.txt -size-limit -max-size 102400
  157  
>C:\Program Files (x86)\Android\android-sdk\emulator\emulator.EXE -netfast -accel on -avd Pixel_9a -prop monodroid.avdname=Pixel_9a

# antigravity 

```bash
cd
rm -rf ~/.antigravity ~/.config/antigravity ~/.cache/antigravity ~/.local/share/antigravity
ls -al
rm -r .gemini/
ls -al
ls -al .local
rm -r .cache
rm -r .*
ls
ls -al
clear rm -rf ~/.config/antigravity
rm -rf ~/.config/antigravityrm -rf ~/.config/antigravity
rm -rf ~/.config/antigravity
clear
rm -rf ~/.cache/antigravity
rm -rf ~/.local/share/antigravity
ls -la ~ | grep -i antigravity
rm -rf ~/.antigravity
antigravity
history
```

Get-ChildItem -File | Rename-Item -NewName { $_.Name.ToLower() }