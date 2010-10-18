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
using System.IO;
using OpenFAST.Error;
using OpenFAST.Template.Type;

namespace OpenFAST.Template
{
    [Serializable]
    public sealed class MessageTemplate : Group, IFieldSet, IEquatable<MessageTemplate>
    {
        public MessageTemplate(QName name, Field[] fields) : base(name, AddTemplateIdField(fields), false)
        {
        }

        public MessageTemplate(string name, Field[] fields) : this(new QName(name), fields)
        {
        }

        public new static System.Type ValueType
        {
            get { return typeof (Message); }
        }

        public Field[] TemplateFields
        {
            get
            {
                var f = new Field[Fields.Length - 1];
                Array.Copy(Fields, 1, f, 0, Fields.Length - 1);
                return f;
            }
        }

        #region IFieldSet Members

        Field IFieldSet.GetField(int index)
        {
            return Fields[index];
        }

        #endregion

        public override bool UsesPresenceMap()
        {
            return true;
        }

        private static Field[] AddTemplateIdField(Field[] fields)
        {
            var newFields = new Field[fields.Length + 1];
            newFields[0] = new Scalar("templateId", FASTType.U32, Operator.Operator.COPY, ScalarValue.Undefined, false);
            Array.Copy(fields, 0, newFields, 1, fields.Length);
            return newFields;
        }

        public byte[] Encode(Message message, Context context)
        {
            int id;
            if (context.TemplateRegistry.TryGetId(message.Template, out id))
            {
                message.SetInteger(0, id);
                return Encode(message, this, context);
            }

            throw new FastException(
                "Cannot encode message: The template " + message.Template + " has not been registered.",
                FastConstants.D9_TEMPLATE_NOT_REGISTERED);
        }

        public Message Decode(Stream inStream, int templateId, BitVectorReader presenceMapReader, Context context)
        {
            try
            {
                if (context.TraceEnabled)
                    context.DecodeTrace.GroupStart(this);
                IFieldValue[] fieldValues = DecodeFieldValues(inStream, this, presenceMapReader, context);
                fieldValues[0] = new IntegerValue(templateId);
                var message = new Message(this, fieldValues);
                if (context.TraceEnabled)
                    context.DecodeTrace.GroupEnd();
                return message;
            }
            catch (FastException e)
            {
                throw new FastException("An error occurred while decoding " + this, e.Code, e);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override IFieldValue CreateValue(string value)
        {
            return new Message(this);
        }

        #region Equals

        public bool Equals(MessageTemplate other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as MessageTemplate);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}