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
using System.Text;
using OpenFAST.Error;
using OpenFAST.Template.Type;

namespace OpenFAST.Template.Operator
{
    [Serializable]
    internal sealed class TailOperatorCodec : OperatorCodec
    {
        internal TailOperatorCodec(Operator op, FASTType[] types)
            : base(op, types)
        {
        }

        public override ScalarValue GetValueToEncode(ScalarValue value, ScalarValue priorValue, Scalar field)
        {
            if (value == null)
            {
                if (priorValue == null)
                    return null;
                if (priorValue.Undefined && field.DefaultValue.Undefined)
                    return null;
                return ScalarValue.NULL;
            }

            if (priorValue == null)
            {
                return value;
            }

            if (priorValue.Undefined)
            {
                priorValue = field.BaseValue;
            }

            int index = 0;

            byte[] val = value.Bytes;
            byte[] prior = priorValue.Bytes;

            if (val.Length > prior.Length)
                return value;
            if (val.Length < prior.Length)
            {
                Global.HandleError(FastConstants.D3_CANT_ENCODE_VALUE,
                                   "The value " + val + " cannot be encoded by a tail operator with previous value " +
                                   priorValue);
            }

            while (index < val.Length && val[index] == prior[index])
                index++;
            if (val.Length == index)
                return null;

            return (ScalarValue) field.CreateValue(Encoding.UTF8.GetString(val, index, val.Length - index));
        }

        public override ScalarValue DecodeValue(ScalarValue newValue, ScalarValue previousValue, Scalar field)
        {
            StringValue baseValue;

            if ((previousValue == null) && !field.Optional)
            {
                Global.HandleError(FastConstants.D6_MNDTRY_FIELD_NOT_PRESENT, "");
                return null;
            }
            if ((previousValue == null) || previousValue.Undefined)
            {
                baseValue = (StringValue) field.BaseValue;
            }
            else
            {
                baseValue = (StringValue) previousValue;
            }

            if (newValue == null || newValue.Null)
            {
                if (field.Optional)
                {
                    return null;
                }
                throw new ArgumentException("");
            }

            string delta = ((StringValue) newValue).Value;
            int length = Math.Max(baseValue.Value.Length - delta.Length, 0);
            string root = baseValue.Value.Substring(0, (length) - (0));

            return new StringValue(root + delta);
        }

        public override ScalarValue DecodeEmptyValue(ScalarValue previousValue, Scalar field)
        {
            ScalarValue value = previousValue;
            if (value != null && value.Undefined)
                value = (field.DefaultValue.Undefined) ? null : field.DefaultValue;
            if (value == null && !field.Optional)
            {
                Global.HandleError(FastConstants.D6_MNDTRY_FIELD_NOT_PRESENT,
                                   "The field " + field + " was not present.");
            }
            return value;
        }

        public override bool Equals(object obj) //POINTP
        {
            return obj != null && obj.GetType() == GetType();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}