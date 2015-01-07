/*******************************************************************************************************************************
 * AK.Chore.Presentation.Api.NotificationHub
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

using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

#endregion

namespace AK.Chore.Presentation.Api
{
    /// <summary>
    /// Sends out notifications through SignalR.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class NotificationHub : Hub
    {
        public Task JoinGroup()
        {
            var userId = this.Context.GetUserId();
            var groupName = string.Format("User_{0}", userId);

            return this.Groups.Add(this.Context.ConnectionId, groupName);
        }

        public void NotifyReload()
        {
            var userId = this.Context.GetUserId();
            var groupName = string.Format("User_{0}", userId);

            this.Clients.OthersInGroup(groupName).notifyReload();
        }
    }
}