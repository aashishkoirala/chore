/*******************************************************************************************************************************
 * AK.Chore.Presentation.Properties.AssemblyInfo
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

using AK.Chore.Presentation;
using Microsoft.Owin;
using System.Reflection;
using System.Runtime.InteropServices;

#endregion

[assembly: AssemblyTitle("CHORE - Web Application")]
[assembly: AssemblyDescription("Presentation layer for Chore")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Aashish Koirala")]
[assembly: AssemblyProduct("Chore")]
[assembly: AssemblyCopyright("Copyright © 2014-2015, Aashish Koirala")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: OwinStartup(typeof(ChoreApplication))]
[assembly: ComVisible(false)]
[assembly: Guid("32b9e455-47b5-4628-821e-8455ec7621f2")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]