# About this project
This library enables you to build plugins and load them into your application with a handy service.

# Building a plugin
To build a plugin, create a project for your plugin and a project with the plugin interface.

## Code in Common.dll
```cs
public interface IPlugin {
	string Foo();

}

```

## Code in Plugin.dll
```cs
public class Plugin1 : IPlugin {
	public string Foo() {
		return "Hello from Plugin 1";
	}

}

```

## Code in Application.exe
```cs
var service = new PluginService<IPlugin>(); 
service.LoadPlugins("PathToPlugins", "*Plugin*.dll", X509Certificate2.CreateFromCertFile("PathToCerFileWithoutPrivateKey")));

foreach (var plugin in service.LoadedPlugins) {
	Debug.WriteLine(plugin.Foo());
}
```

# Security checks
If you specify a public certificate, the plugins will be checked if they are signed by the same certificate. If you pass NULL, the signature is not checked and the plugin is loaded anyway.

# Search Pattern
If you don't specify a search pattern, all dll files are checked for plugins in the given directory.
