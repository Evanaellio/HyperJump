using MelonLoader;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(Evanaellio.HyperJump.BuildInfo.Name)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Evanaellio.HyperJump.BuildInfo.Company)]
[assembly: AssemblyProduct(Evanaellio.HyperJump.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + Evanaellio.HyperJump.BuildInfo.Author)]
[assembly: AssemblyTrademark(Evanaellio.HyperJump.BuildInfo.Company)]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
//[assembly: Guid("")]
[assembly: AssemblyVersion(Evanaellio.HyperJump.BuildInfo.Version)]
[assembly: AssemblyFileVersion(Evanaellio.HyperJump.BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(Evanaellio.HyperJump.HyperJump), Evanaellio.HyperJump.BuildInfo.Name, Evanaellio.HyperJump.BuildInfo.Version, Evanaellio.HyperJump.BuildInfo.Author, Evanaellio.HyperJump.BuildInfo.DownloadLink)]


// Create and Setup a MelonModGame to mark a Mod as Universal or Compatible with specific Games.
// If no MelonModGameAttribute is found or any of the Values for any MelonModGame on the Mod is null or empty it will be assumed the Mod is Universal.
// Values for MelonModGame can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame("Stress Level Zero", "BONEWORKS")]