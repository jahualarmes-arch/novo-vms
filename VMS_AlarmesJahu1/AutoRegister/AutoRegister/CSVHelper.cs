using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AutoRegister
{
    public class CSVHelper
    {
        public CSVHelper()
        {
        }

        public void WriteToCSVFile(List<DEVICE_INFO> ls, string path, bool append)
        {
            using(StreamWriter sw = new StreamWriter(path, append, Encoding.Default))
            {
                foreach (var item in ls)
                {
                    sw.WriteLine(string.Format("{0},{1},{2}", item.ID, item.UserName, item.Password));
                }
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
        }

        public List<DEVICE_INFO> ReadCSV(string path)
        {
            List<DEVICE_INFO> ls = new List<DEVICE_INFO>();
            using (StreamReader sr = new StreamReader(path, Encoding.Default))
            {
                while (true)
                {
                    string res = sr.ReadLine();
                    if (res != null && res.Length > 0)
                    {
                        string[] items = res.Split(new char[] { ',' });
                        DEVICE_INFO info = new DEVICE_INFO();
                        if (items.Length == 3)
                        {
                            info.ID = items[0];
                            info.UserName = items[1];
                            info.Password = items[2];
                            ls.Add(info);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                sr.Close();
                sr.Dispose();
            }
            return ls;
        }
    }
}
