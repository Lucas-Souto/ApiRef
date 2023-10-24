using System;
using System.IO;
using System.Xml;

namespace ApiRef.Core
{
    public static class ApiReference
    {
        public static void Generate(string path, string outputDirectory, bool filterPublic)
        {
            NestedNamespace namespaces = DLLImporter.Import(path, filterPublic);
            XmlDocument docs = new XmlDocument();
            string xmlPath = Path.GetFileNameWithoutExtension(path) + ".xml";

            if (File.Exists(xmlPath)) docs.Load(xmlPath);
        }
    }
}