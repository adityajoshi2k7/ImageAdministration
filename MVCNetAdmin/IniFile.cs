using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace MVCNetAdmin
{
    public class IniFile
    {
        string _iniFile;

        public IniFile(string fileName) //constructor to initialize file name and path
        {
            _iniFile = fileName;
        }

        public void WriteSetting(string section, string key, string value)
        {
            if (NativeMethods.WritePrivateProfileString(section, key, value, _iniFile) == false)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    class NativeMethods   //contains windows api functions to read and write from ini files
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool WritePrivateProfileString(
            string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint GetPrivateProfileString(
            string lpAppName, string lpKeyName, string lpDefault, string lpReturnedString,
            uint nSize, string lpFileName);
    }



}
