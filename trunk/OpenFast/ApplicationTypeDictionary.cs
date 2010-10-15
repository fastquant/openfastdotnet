/*

The contents of this file are subject to the Mozilla Public License
Version 1.1 (the "License"); you may not use this file except in
compliance with the License. You may obtain a copy of the License at
http://www.mozilla.org/MPL/

Software distributed under the License is distributed on an "AS IS"
basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
License for the specific language governing rights and limitations
under the License.

The Original Code is OpenFAST.

The Initial Developer of the Original Code is The LaSalle Technology
Group, LLC.  Portions created by Shariq Muhammad
are Copyright (C) Shariq Muhammad. All Rights Reserved.

Contributor(s): Shariq Muhammad <shariq.muhammad@gmail.com>

*/
using System.Collections.Generic;
using Group = OpenFAST.Template.Group;
using System.Text;

namespace OpenFAST
{
	public sealed class ApplicationTypeDictionary : Dictionary
	{
        private Dictionary<QName, Dictionary<QName, ScalarValue>> dictionary = new Dictionary<QName, Dictionary<QName, ScalarValue>>();
		
		public ScalarValue Lookup(Group template, QName key, QName applicationType)
		{
		    Dictionary<QName, ScalarValue> value;
		    if (dictionary.TryGetValue(template.TypeReference, out value))
			{
			    ScalarValue value2;
			    if (value.TryGetValue(key, out value2))
					return value2;
			}
			return ScalarValue.UNDEFINED;
		}
		
		public void  Reset()
		{
            dictionary = new Dictionary<QName, Dictionary<QName, ScalarValue>>();
		}

        public void Store(Group group, QName applicationType, QName key, ScalarValue value_Renamed)
        {
            Dictionary<QName, ScalarValue> value;
            
            if (!dictionary.TryGetValue(group.TypeReference, out value))
                dictionary[group.TypeReference] = value = new Dictionary<QName, ScalarValue>();

            value[key] = value_Renamed;
        }

	    public override string ToString()
		{
			var builder = new StringBuilder();
            foreach (var val in dictionary)
            {
                builder.Append("Dictionary: Type=").Append(val.Key);

                foreach (var val2 in val.Value)
                {
                    builder.Append(val2.Key).Append("=").Append(val2.Value).Append("\n");
                }
            }
			return builder.ToString();
		}
	}
}