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
                Yuri Astrakhan <FirstName><LastName>@gmail.com
*/
using System;
using System.Text;
using OpenFAST.Template.Type.Codec;

namespace OpenFAST.Template.Type
{
    [Serializable]
    internal sealed class ByteVectorType : SimpleType
    {
        internal ByteVectorType() : base("byteVector", TypeCodec.BYTE_VECTOR, TypeCodec.NULLABLE_BYTE_VECTOR_TYPE)
        {
        }

        public override ScalarValue DefaultValue
        {
            get { return new ByteVectorValue(ByteUtil.EmptyByteArray); }
        }

        public override ScalarValue GetVal(string value)
        {
            return new ByteVectorValue(Encoding.UTF8.GetBytes(value));
        }

        public override bool IsValueOf(ScalarValue priorValue)
        {
            return priorValue is ByteVectorValue;
        }

        public override ScalarValue GetValue(byte[] bytes)
        {
            return new ByteVectorValue(bytes);
        }
    }
}