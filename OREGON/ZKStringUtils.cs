using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cz.zk.OREGON
{
    class ZKStringUtils
    {
        public ZKStringUtils()
        {

        }

        /// <summary>
        /// Method is supposed to divide the provided string into substrings of the
        /// length nearest greater than the param minLength. If the input string
        /// sInp is shorter than minLength, the returned ArrayList contains just
        /// this string. Otherwise the returned ArrayList contains more strings
        /// </summary>
        /// <param name="sInp"></param>
        /// <param name="minLength"></param>
        /// <returns></returns>
        public ArrayList DivideString(String sInp, int minLength)
        {
            String sPom;
            ArrayList al = new ArrayList();

            if (sInp.Length <= minLength)
            {
                al.Add(sInp);
                return (al);
            }
            else
            {
                sPom = "";
                String[] Phrases = sInp.Split(' ');
                foreach (String sent in Phrases)
                {
                    sPom += sent;
                    sPom += " ";
                    if (sPom.Length > minLength)
                    {
                        al.Add(sPom);
                        sPom = "";
                    }
                }
                if (sPom.Length > 0) al.Add(sPom);
                return (al);
            }
        }
    }
}
