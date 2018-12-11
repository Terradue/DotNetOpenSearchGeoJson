using System;
using System.IO;
using System.Xml.Linq;
using Terradue.OpenSearch.GeoJson.Result;
using Terradue.OpenSearch.Result;


namespace Terradue.Opensearch.GeoJson.Client {

    internal class Client {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static Version version = typeof(Client).Assembly.GetName().Version;
        private static int GENERAL_ERROR_EXIT_STATUS_CODE = 1;
        
        public static void Main(string[] args) {
            
            
            try {

                // check if file path is provided
                if (args == null || args.Length == 0) {
                    throw new Exception("Please provide a file to convert");
                } 
                
                // check is file exists
                if (!File.Exists(args[0])) {
                    throw new FileNotFoundException();
                }
                string file = args[0];
                
                // check is file is a json
                if (!file.EndsWith(".json")) {
                    throw new Exception("Please provide a json file");
                }
                
                var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                var fc = FeatureCollectionResult.DeserializeFromStream(fs);
                fs.Close();

                var feed = AtomFeed.CreateFromOpenSearchResultCollection(fc);
                log.Info(FormatXml(feed.SerializeToString()));

                
            } catch (Exception e) {
                if (e is FileNotFoundException || e is DirectoryNotFoundException) {
                    log.Error("File not found. " + e.Message);
                } else {
                    log.Error(e);
                }
                Environment.ExitCode = GENERAL_ERROR_EXIT_STATUS_CODE;
                return;
            }
            
            
            
        }
        
        
        /// <summary>
        /// Xml pretty print
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        static string FormatXml(string xml) {
            try {
                XDocument doc = XDocument.Parse(xml);
                return doc.ToString();
            } catch (Exception) {
                return xml;
            }
        }

    }

}