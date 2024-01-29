using LazyApiPack.Plugin.Demo.Common;
using System.Diagnostics;

namespace LazyApiPack.Plugin.Demo.Plugin2
{
    public class Class1 : MyPlugin
    {
        public override string Foo(string foo)
        {
            Debug.WriteLine("Hello from Plugin 2");
            return base.Foo(foo);
        }
    }
}
