using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace KGySoft.Libraries
{
    /// <summary>
    /// Szabv�nyos Ini file-ok kezel�se
    /// </summary>
    public static class IniFiles
    {
        #region .INI file tools

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);

        /// <summary>
        /// �rt�k beolvas�sa .INI file-b�l (hiba eset�n kiv�telt dob)
        /// </summary>
        /// <param name="fileName">File n�v (�tvonallal is lehet)</param>
        /// <param name="section">Tartom�ny</param>
        /// <param name="key">Tartom�nyon bel�li kulcsn�v</param>
        /// <returns>A kulcshoz tartoz� �rt�k</returns>
        static public string IniReadValue(string fileName, string section, string key)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", temp, 255, fileName);
            return temp.ToString();                        
        }

        /// <summary>
        /// �rt�k beolvas�sa .INI file-b�l (hiba eset�n a defaultValue �rt�kkel t�r vissza)
        /// </summary>
        /// <param name="fileName">File n�v (�tvonallal is lehet)</param>
        /// <param name="section">Tartom�ny</param>
        /// <param name="key">Tartom�nyon bel�li kulcsn�v</param>
        /// <param name="defaultValue">Hiba eset�n ezt kapjuk vissza</param>
        /// <returns>A kulcshoz tartoz� �rt�k</returns>
        static public string IniReadValue(string fileName, string section, string key, string defaultValue)
        {
            try
            {
                StringBuilder temp = new StringBuilder(255);
                GetPrivateProfileString(section, key, defaultValue, temp, 255, fileName);
                return temp.ToString();
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// �rt�k �r�sa .INI file-ba
        /// </summary>
        /// <param name="fileName">File n�v (�tvonallal is lehet)</param>
        /// <param name="section">Tartom�ny</param>
        /// <param name="key">Tartom�nyon bel�li kulcsn�v</param>
        /// <param name="value">A ki�rand� kulcs�rt�k</param>
        /// <returns>Sikeress�g</returns>
        static public bool IniWriteValue(string fileName, string section, string key, string value)
        {
            try
            {
                WritePrivateProfileString(section, key, value, fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
