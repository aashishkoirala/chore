/*******************************************************************************************************************************
 * AK.Chore.Tests.Integration.Initializer
 * Copyright © 2014-2015 Aashish Koirala <http://aashishkoirala.github.io>
 * 
 * This file is part of CHORE.
 *  
 * CHORE is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * CHORE is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with CHORE.  If not, see <http://www.gnu.org/licenses/>.
 * 
 *******************************************************************************************************************************/

#region Namespace Imports

using AK.Commons;
using AK.Commons.Configuration;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

#endregion

namespace AK.Chore.Tests.Integration
{
    /// <summary>
    /// Initializes environment for integration tests.
    /// </summary>
    /// <author>Aashish Koirala</author>
    internal static class Initializer
    {
        private static bool isInitialized;
        private static readonly object initializeLock = new object();

        public static void Initialize()
        {
            lock (initializeLock)
            {
                if (isInitialized) return;

                var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Debug.Assert(directory != null);

                var keyFolder = Path.Combine(directory, "Keys");
                if (Directory.Exists(keyFolder)) Directory.Delete(keyFolder, true);
                Directory.CreateDirectory(keyFolder);

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                AppEnvironment.Initialize("ChoreTests", new InitializationOptions
                    {
                        ConfigStore = config.GetConfigStore(),
                        EnableLogging = true,
                        GenerateServiceClients = false
                    });

                isInitialized = true;
            }
        }

        public static void ShutDown()
        {
            lock (initializeLock)
            {
                if (!isInitialized) return;
                AppEnvironment.ShutDown();
                isInitialized = false;
            }
        }
    }
}