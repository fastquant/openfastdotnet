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
using System;
using System.IO;
using System.Text;
using OpenFAST.Error;

namespace OpenFAST.Template.Type.Codec
{
    [Serializable]
    public sealed class NullableByteVector : NotStopBitEncodedTypeCodec
    {
        public static ScalarValue DefaultValue
        {
            get { return new ByteVectorValue(new byte[] {}); }
        }

        public override ScalarValue Decode(Stream inStream)
        {
            ScalarValue decode = NULLABLE_UNSIGNED_INTEGER.Decode(inStream);
            if (decode == null)
                return null;
            int length = decode.ToInt();
            var encoding = new byte[length];

            for (int i = 0; i < length; i++)
                try
                {
                    encoding[i] = (byte) inStream.ReadByte();
                }
                catch (IOException e)
                {
                    Global.HandleError(FastConstants.IO_ERROR,
                                       "An error occurred while decoding a nullable byte vector.", e);
                }
            return new ByteVectorValue(encoding);
        }

        public override byte[] EncodeValue(ScalarValue value)
        {
            if (value.Null)
                return NULLABLE_UNSIGNED_INTEGER.EncodeValue(ScalarValue.NULL);
            var byteVectorValue = (ByteVectorValue) value;
            int lengthSize = IntegerCodec.GetUnsignedIntegerSize(byteVectorValue.Value.Length);
            var encoding = new byte[byteVectorValue.Value.Length + lengthSize];
            byte[] length = NULLABLE_UNSIGNED_INTEGER.Encode(new IntegerValue(byteVectorValue.Value.Length));
            Array.Copy(length, 0, encoding, 0, lengthSize);
            Array.Copy(byteVectorValue.Value, 0, encoding, lengthSize, byteVectorValue.Value.Length);
            return encoding;
        }

        public static ScalarValue FromString(string value)
        {
            return new ByteVectorValue(Encoding.UTF8.GetBytes(value));
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj.GetType() == GetType();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}