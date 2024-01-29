using LazyApiPack.Plugin;
using LazyApiPack.Plugin.Demo.Common;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

var pluginManager = new PluginService<MyPlugin>();
var root = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent;

var p1 = Path.Combine(root.FullName, "LazyApiPack.Plugin.Demo.Plugin1", "bin", "Debug", "net6.0");
var p2 = Path.Combine(root.FullName, "LazyApiPack.Plugin.Demo.Plugin2", "bin", "Debug", "net6.0");

pluginManager.LoadPlugins(p1); // Unsigned
pluginManager.LoadPlugins(p2, null, X509Certificate2.CreateFromCertFile(Path.Combine(Directory.GetCurrentDirectory(), "FullWithPrivate.cer")));
