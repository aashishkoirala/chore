/*******************************************************************************************************************************
 * AK.Chore.Application.Services.ServiceBase
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
using AK.Commons.DataAccess;
using AK.Commons.DomainDriven;
using AK.Commons.Logging;
using System;

#endregion

namespace AK.Chore.Application.Services
{
    /// <summary>
    /// Base class for all service implementations.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public abstract class ServiceBase
    {
        private readonly IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider;
        private readonly IAppDataAccess appDataAccess;
        private readonly string databaseKey;

        private IUnitOfWorkFactory db;
        private IEntityIdGenerator<int> idGenerator;

        protected ServiceBase(
            IAppDataAccess appDataAccess,
            IAppConfig appConfig,
            IAppLogger logger,
            IProviderSource<IEntityIdGeneratorProvider> entityIdGeneratorProvider)
        {
            this.databaseKey = appConfig.Get("DatabaseKey", "Chore");
            this.appDataAccess = appDataAccess;
            this.Logger = logger;
            this.entityIdGeneratorProvider = entityIdGeneratorProvider;
        }

        protected IAppLogger Logger { get; private set; }

        protected IUnitOfWorkFactory Db
        {
            get { return this.db ?? (this.db = this.appDataAccess[this.databaseKey]); }
        }

        protected IEntityIdGenerator<int> IdGenerator
        {
            get
            {
                return this.idGenerator ??
                       (this.idGenerator = this.entityIdGeneratorProvider[this.databaseKey].Get<int>());
            }
        }

        protected static TEnum ParseEnum<TEnum>(string value)
        {
            return (TEnum) Enum.Parse(typeof (TEnum), value);
        }
    }
}