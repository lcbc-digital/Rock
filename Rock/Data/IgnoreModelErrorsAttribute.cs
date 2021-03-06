﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Data
{
    /// <summary>
    /// derived from http://mrbigglesworth79.blogspot.in/2011/12/partial-validation-with-data.html
    /// </summary>
    public class IgnoreModelErrorsAttribute : System.Attribute
    {
        /// <summary>
        /// The keys string
        /// </summary>
        public string[] Keys { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreModelErrorsAttribute"/> class.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public IgnoreModelErrorsAttribute( string[] keys )
            : base()
        {
            this.Keys = keys;
        }
    }
}
