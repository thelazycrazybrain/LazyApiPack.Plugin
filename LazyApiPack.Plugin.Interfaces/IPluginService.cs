using System.Security.Cryptography.X509Certificates;

namespace LazyApiPack.Plugin.Interfaces
{
    public delegate void PluginEventHandler<TPlugin>(object sender, TPlugin plugin);
    public interface IPluginService<TPlugin>
    {
        event PluginEventHandler<TPlugin> PluginLoaded;
        /// <summary>
        /// Loads all avaliable plugins of type TPlugin.
        /// </summary>
        /// <param name="directory">Directory to search plugins in.</param>
        /// <param name="filter">File filter.</param>
        /// <param name="signatureCertificate">If set, the signature of the plugin is checked against this certificate.</param>
        /// <returns>True, if plugins could be loaded.</returns>
        bool LoadPlugins(string directory, string? filter = null, X509Certificate? signatureCertificate = null);
        /// <summary>
        /// List of loaded plugins.
        /// </summary>
        IEnumerable<TPlugin> LoadedPlugins { get; }

    }
}
