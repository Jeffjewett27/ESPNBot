using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ESPNBot
{
    /// <summary>
    /// References LoginInfo.txt for login credentials
    /// </summary>
    static class LoginInfo
    {
        private const string path = @"C:\Users\jeffr\source\repos\ESPNBot\ESPNBot\LoginInfo.txt";
        private static string username;
        private static string password;

        public static void LoadLogin()
        {
            using (var fileStream = File.OpenRead(path))
            {
                using (var streamReader = new StreamReader(fileStream))
                {
                    username = streamReader.ReadLine();
                    password = streamReader.ReadLine();
                }
            }
        }
        public static string GetUsername()
        {
            if (username == null)
            {
                LoadLogin();
            }
            return username;
        }

        public static string GetPassword()
        {
            if (password == null)
            {
                LoadLogin();
            }
            return password;
        }
    }
}
