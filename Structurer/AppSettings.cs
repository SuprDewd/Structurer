using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace Structurer
{
    [XmlRoot("AppSettings", IsNullable = false)]
    public class AppSettings
    {
        [XmlElement("Templates", typeof(SerializableDictionary<string, string>))]
        public SerializableDictionary<string, string> Templates { get; set; }

        public AppSettings() { this.Templates = new SerializableDictionary<string, string>(); }

        public static AppSettings Load(string fileName = null)
        {
            try
            {
                if (fileName != null && File.Exists(fileName))
                {
                    XmlSerializer s = new XmlSerializer(typeof(AppSettings));
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        return (AppSettings)s.Deserialize(fs);
                    }
                }

                return new AppSettings();
            }
            catch (Exception)
            {
                return new AppSettings();
            }
        }

        public bool Save(string fileName)
        {
            try
            {
                XmlSerializer s = new XmlSerializer(typeof(AppSettings));
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    s.Serialize(fs, this);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}