﻿/* Copyright 2011 the OpenDMS.NET Project (http://sites.google.com/site/opendmsnet/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

namespace Common.NetworkPackage
{
    /// <summary>
    /// Provides a method of packaging and unpackaging a collection of <see cref="MetaFormProperty"/> 
    /// objects.
    /// </summary>
    public class MetaForm : ListBase<MetaFormProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaForm"/> class.
        /// </summary>
        public MetaForm()
            : base("MetaForm")
        {
        }

        /// <summary>
        /// Gets the a default instantiation of this class.
        /// </summary>
        /// <returns>The instantiated object.</returns>
        public static MetaForm GetDefault()
        {
            MetaForm mf = new MetaForm();

            mf.Add(new MetaFormProperty(typeof(Guid), "$guid", "Guid", Guid.Empty, true));
            mf.Add(new MetaFormProperty(typeof(string), "$lockedby", "Locked by User", null, true));
            mf.Add(new MetaFormProperty(typeof(DateTime), "$lockedat", "Locked at Date/Time", DateTime.MinValue, true));
            mf.Add(new MetaFormProperty(typeof(string), "$creator", "Created by", null, true));
            mf.Add(new MetaFormProperty(typeof(ulong), "$length", "Size", 0, true));
            mf.Add(new MetaFormProperty(typeof(string), "$md5", "MD5 Hashvalue", null, true));
            mf.Add(new MetaFormProperty(typeof(string), "$extension", "Extension", null, true));
            mf.Add(new MetaFormProperty(typeof(DateTime), "$created", "Created at Date/Time", DateTime.MinValue, true));
            mf.Add(new MetaFormProperty(typeof(DateTime), "$modified", "Modified at Date/Time", DateTime.MinValue, true));
            mf.Add(new MetaFormProperty(typeof(DateTime), "$modified", "Last accessed at Date/Time", DateTime.MinValue, true));
            mf.Add(new MetaFormProperty(typeof(string), "$title", "Title", null, false));
            mf.Add(new MetaFormProperty(typeof(System.Collections.Generic.List<string>), "$tags", "Tags", 
                new System.Collections.Generic.List<string>(), false));

            return mf;
        }
    }
}