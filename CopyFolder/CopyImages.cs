using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopyImages
{
    public class CopyImages
    {
        public static void Copy(string root)
        {
            string original = root + "\\original";
            string processed = root + "\\processed";

            if (!System.IO.Directory.Exists(root) && !System.IO.Directory.Exists(original))
            {
                return;
            }

            System.IO.Directory.CreateDirectory(processed);
            string[] subDirs = null;
            try
            {
                subDirs = System.IO.Directory.GetDirectories(original);
            }
            catch (Exception e)
            {
                return;
            }

            foreach(string sD in subDirs)
            {
                string s = sD.Replace("original", "processed");
                System.IO.Directory.CreateDirectory(s);
                string[] subSubDirs = System.IO.Directory.GetDirectories(sD);

                foreach (string ssD in subSubDirs)
                {
                    string ss = ssD.Replace("original", "processed");
                    System.IO.Directory.CreateDirectory(ss);
                    string[] files = null;
                    try
                    {
                        files = System.IO.Directory.GetFiles(ssD);
                    }
                    catch (Exception e)
                    {
                        return;
                    }

                    foreach (string file in files)
                    {
                        try
                        {
                            //Preprocess file
                            System.IO.FileInfo fi = new System.IO.FileInfo(file);
                            System.IO.File.Copy(System.IO.Path.Combine(ssD, fi.Name), System.IO.Path.Combine(ss, fi.Name), true);
                        }
                        catch (System.IO.FileNotFoundException e)
                        {}
                    }
                }
                
            }
        }
    }
}
