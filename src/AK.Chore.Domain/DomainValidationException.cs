/*******************************************************************************************************************************
 * AK.Chore.Domain.DomainValidationException
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
using AK.Commons.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

#endregion

namespace AK.Chore.Domain
{
    /// <summary>
    /// Represents a domain-level validation exception.
    /// </summary>
    /// <author>Aashish Koirala</author>
    public class DomainValidationException : ReasonedException<DomainValidationErrorCode>
    {
        private readonly IDictionary<string, object> additionalData;
        private readonly string description;

        public DomainValidationException(
            DomainValidationErrorCode reason, object additionalData = null) : base(reason)
        {
            this.additionalData = BuildAdditionalData(additionalData);
            this.description = this.BuildDescription();
        }

        public DomainValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IDictionary<string, object> AdditionalData
        {
            get { return this.additionalData; }
        }

        protected override string GetReasonDescription(DomainValidationErrorCode reason)
        {
            return this.description;
        }

        private static IDictionary<string, object> BuildAdditionalData(object additionalData)
        {
            if (additionalData == null) return null;

            return additionalData
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead)
                .Select(x => new {Key = x.Name, Value = x.GetValue(additionalData)})
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private string BuildDescription()
        {
            var builder = new StringBuilder();
            builder.AppendFormat(this.Reason.Describe());

            if (this.additionalData == null) return builder.ToString();

            foreach (var pair in this.additionalData)
            {
                builder.AppendLine();
                builder.AppendFormat("{0} = {1}", pair.Key, pair.Value);
            }

            return builder.ToString();
        }
    }
}