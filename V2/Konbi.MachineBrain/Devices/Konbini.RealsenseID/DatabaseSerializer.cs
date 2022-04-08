using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Konbini.RealsenseID
{
    internal class DatabaseSerializer
    {
        public static string Serialize(IList<(rsid.Faceprints, string)> users, int db_version, string filename)
        {
            string facePrintString = "";
            try
            {
                DbObj json_root = new DbObj();
                List<rsid.UserFaceprints> jsonstring = new List<rsid.UserFaceprints>();
                foreach (var (faceprintsDb, userIdDb) in users)
                {
                    jsonstring.Add(new rsid.UserFaceprints()
                    {
                        userID = userIdDb,
                        faceprints = faceprintsDb
                    });
                }
                json_root.db = jsonstring;
                json_root.version = db_version;
                facePrintString = JsonConvert.SerializeObject(json_root);
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.WriteLine(JsonConvert.SerializeObject(json_root));//.Replace("\\\"", ""));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed serializing database: " + e.Message);
                return facePrintString;
            }
            return facePrintString;
        }


        public static List<(rsid.Faceprints, string)> Deserialize(string filename, out int db_version)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    DbObj obj = JsonConvert.DeserializeObject<DbObj>(reader.ReadToEnd());
                    var usr_array = new List<(rsid.Faceprints, string)>();
                    foreach (var uf in obj.db)
                    {
                        usr_array.Add((uf.faceprints, uf.userID));
                    }
                    db_version = obj.version;
                    return usr_array;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed deserializing database: " + e.Message);
                db_version = -1;
                return null;
            }
        }
    }
    internal class DbObj
    {
        public List<rsid.UserFaceprints> db { get; set; }
        public int version { get; set; }
    }
}
