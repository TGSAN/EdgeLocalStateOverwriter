using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdgeLocalStateOverwriter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 1)
            {
                var configPath = args[0];
                var edgeLocalStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\Edge\\User Data\\Local State");

                if (File.Exists(args[0]))
                { 
                    var config = File.ReadAllText(configPath);
                    var configObj = JObject.Parse(config);
                    if (File.Exists(edgeLocalStatePath))
                    {
                        var edgeLocalState = File.ReadAllText(edgeLocalStatePath);
                        var edgeLocalStateObj = JObject.Parse(edgeLocalState);
                        Console.WriteLine($"Start Overwriting");
                        edgeLocalStateObj = Overwrite(configObj, edgeLocalStateObj);
                        File.WriteAllText(edgeLocalStatePath, edgeLocalStateObj.ToString(Formatting.None));
                    }
                }
            }
        }

        static JObject Overwrite(JObject src, JObject dist)
        {
            foreach (var kv in src)
            {
                if (kv.Value.Type == JTokenType.Object)
                {
                    if (dist.ContainsKey(kv.Key))
                    {
                        dist[kv.Key] = Overwrite(kv.Value.ToObject<JObject>(), dist[kv.Key].ToObject<JObject>());
                    }
                    else
                    {
                        dist[kv.Key] = kv.Value;
                    }
                }
                else if (kv.Value.Type == JTokenType.Null)
                {
                    Console.WriteLine($"Delete {kv.Key}");
                    dist.Remove(kv.Key);
                }
                else
                {
                    dist[kv.Key] = kv.Value;
                }
            }
            return dist;
        }
    }
}
