using LazyApiPack.Plugin.Interfaces;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace LazyApiPack.Plugin
{

    public class PluginService<TPlugin> : IPluginService<TPlugin>
    {
        private Dictionary<string, IEnumerable<TPlugin>> _loadedPlugins = new Dictionary<string, IEnumerable<TPlugin>>();

        public IEnumerable<TPlugin> LoadedPlugins { get => _loadedPlugins.SelectMany(p => p.Value); }

        public event PluginEventHandler<TPlugin> PluginLoaded;

        public bool LoadPlugins(string directory, string? searchPattern = null, X509Certificate? signatureCertificate = null)
        {
            if (!Directory.Exists(directory))
            {
                Debug.WriteLine($"{directory} does not exist.");
                return false;
            }
            var loaded = false;
            foreach (var file in Directory.EnumerateFiles(directory, string.IsNullOrWhiteSpace(searchPattern) ? "*.dll" : searchPattern))
            {
                if (_loadedPlugins.Any(p => p.Key == file))
                {
                    Debug.WriteLine("{file} was already loaded.");
                    continue;
                }
                var plugins = new List<TPlugin>();
                if (!IsManagedAssembly(file))
                {
                    Debug.WriteLine($"{file} is not managed and can't be loaded.");
                    continue;
                }
                if (signatureCertificate != null && !IsSigned(file, signatureCertificate))
                {
                    Debug.WriteLine("{file} is not properly signed.");
                    continue;
                }
                var assembly = Assembly.LoadFrom(file);

                var types = assembly.GetExportedTypes().Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(TPlugin)));
                if (types.Any())
                {
                    foreach (var plugin in types)
                    {
                        var pluginInstance = (TPlugin)Activator.CreateInstance(plugin);
                        if (pluginInstance == null) continue;
                        PluginLoaded?.Invoke(this, pluginInstance);
                        plugins.Add(pluginInstance);
                    }
                }

                if (plugins.Any())
                {
                    _loadedPlugins.Add(file, plugins);
                    loaded = true;
                }


            }
            return loaded;
        }

        private bool IsManagedAssembly(string fileName)
        {
            using (Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (BinaryReader binaryReader = new BinaryReader(fileStream))
            {
                if (fileStream.Length < 64)
                {
                    return false;
                }

                //PE Header starts @ 0x3C (60). Its a 4 byte header.
                fileStream.Position = 0x3C;
                uint peHeaderPointer = binaryReader.ReadUInt32();
                if (peHeaderPointer == 0)
                {
                    peHeaderPointer = 0x80;
                }

                // Ensure there is at least enough room for the following structures:
                //     24 byte PE Signature & Header
                //     28 byte Standard Fields         (24 bytes for PE32+)
                //     68 byte NT Fields               (88 bytes for PE32+)
                // >= 128 byte Data Dictionary Table
                if (peHeaderPointer > fileStream.Length - 256)
                {
                    return false;
                }

                // Check the PE signature.  Should equal 'PE\0\0'.
                fileStream.Position = peHeaderPointer;
                uint peHeaderSignature = binaryReader.ReadUInt32();
                if (peHeaderSignature != 0x00004550)
                {
                    return false;
                }

                // skip over the PEHeader fields
                fileStream.Position += 20;

                const ushort PE32 = 0x10b;
                const ushort PE32Plus = 0x20b;

                // Read PE magic number from Standard Fields to determine format.
                var peFormat = binaryReader.ReadUInt16();
                if (peFormat != PE32 && peFormat != PE32Plus)
                {
                    return false;
                }

                // Read the 15th Data Dictionary RVA field which contains the CLI header RVA.
                // When this is non-zero then the file contains CLI data otherwise not.
                ushort dataDictionaryStart = (ushort)(peHeaderPointer + (peFormat == PE32 ? 232 : 248));
                fileStream.Position = dataDictionaryStart;

                uint cliHeaderRva = binaryReader.ReadUInt32();
                if (cliHeaderRva == 0)
                {
                    return false;
                }

                return true;
            }
        }


        private bool IsSigned(string fileName, X509Certificate publicCertificate)
        {try
            {
                var fileSignatureCertificate = X509Certificate.CreateFromSignedFile(fileName);

                if (publicCertificate != null)
                {
                    return publicCertificate.GetCertHashString() == fileSignatureCertificate.GetCertHashString();
                }
                else
                {
                    return true;
                }
            } catch
            {
                return false; // not signed
            }
        }

    }
}
