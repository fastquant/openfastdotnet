using System;
using BitVectorBuilder = OpenFAST.BitVectorBuilder;
using BitVectorReader = OpenFAST.BitVectorReader;
using Context = OpenFAST.Context;
using Dictionary = OpenFAST.Dictionary;
using FieldValue = OpenFAST.FieldValue;
using Global = OpenFAST.Global;
using QName = OpenFAST.QName;
using ScalarValue = OpenFAST.ScalarValue;
using FastException = OpenFAST.Error.FastException;
using Operator = OpenFAST.Template.operator_Renamed.Operator;
using OperatorCodec = OpenFAST.Template.operator_Renamed.OperatorCodec;
using FASTType = OpenFAST.Template.Type.FASTType;
using TypeCodec = OpenFAST.Template.Type.Codec.TypeCodec;
using RecordingInputStream = OpenFAST.util.RecordingInputStream;

namespace OpenFAST.Template
{
	
	[Serializable]
	public class Scalar:Field
	{
		private void  InitBlock()
		{
			defaultValue = ScalarValue.UNDEFINED;
		}

		virtual public FASTType Type
		{
			get
			{
				return type;
			}
			
		}

		virtual public OperatorCodec OperatorCodec
		{
			get
			{
				return operatorCodec;
			}
			
		}

		virtual public Operator Operator
		{
			get
			{
				return operator_Renamed;
			}
			
		}

		virtual public string Dictionary
		{
			get
			{
				return dictionary;
			}
			
			set
			{
				if (value == null)
					throw new System.NullReferenceException();
				this.dictionary = value;
			}
			
		}

		virtual public ScalarValue DefaultValue
		{
			get
			{
				return defaultValue;
			}
			
		}

		override public System.Type ValueType
		{
			get
			{
				return typeof(ScalarValue);
			}
			
		}

		override public string TypeName
		{
			get
			{
				return "scalar";
			}
			
		}

		virtual public ScalarValue BaseValue
		{
			get
			{
				return initialValue;
			}
			
		}

		virtual public TypeCodec TypeCodec
		{
			get
			{
				return typeCodec;
			}
			
		}
		private const long serialVersionUID = 1L;
		private Operator operator_Renamed;
		private OperatorCodec operatorCodec;
		private FASTType type;
		private TypeCodec typeCodec;
		private string dictionary;
		private ScalarValue defaultValue;
		private ScalarValue initialValue;
		
		public Scalar(string name, FASTType type, Operator operator_Renamed, ScalarValue defaultValue, bool optional):this(new QName(name), type, operator_Renamed, defaultValue, optional)
		{
		}
		public Scalar(QName name, FASTType type, Operator operator_Renamed, ScalarValue defaultValue, bool optional):base(name, optional)
		{
			InitBlock();
			this.operator_Renamed = operator_Renamed;
			this.operatorCodec = operator_Renamed.GetCodec(type);
			this.dictionary = OpenFAST.Dictionary_Fields.GLOBAL;
			this.defaultValue = (defaultValue == null)?ScalarValue.UNDEFINED:defaultValue;
			this.type = type;
			this.typeCodec = type.GetCodec(operator_Renamed, optional);
			this.initialValue = ((defaultValue == null) || defaultValue.Undefined)?this.type.DefaultValue:defaultValue;
			operator_Renamed.Validate(this);
		}

		public Scalar(QName name, FASTType type, OperatorCodec operatorCodec, ScalarValue defaultValue, bool optional):base(name, optional)
		{
			InitBlock();
			this.operator_Renamed = operatorCodec.Operator;
			this.operatorCodec = operatorCodec;
			this.dictionary = "global";
			this.defaultValue = (defaultValue == null)?ScalarValue.UNDEFINED:defaultValue;
			this.type = type;
			this.typeCodec = type.GetCodec(operator_Renamed, optional);
			this.initialValue = ((defaultValue == null) || defaultValue.Undefined)?this.type.DefaultValue:defaultValue;
			operator_Renamed.Validate(this);
		}

		public override byte[] Encode(FieldValue fieldValue, Group template, Context context, BitVectorBuilder presenceMapBuilder)
		{
			ScalarValue priorValue = (ScalarValue) context.Lookup(Dictionary, template, Key);
			ScalarValue value_Renamed = (ScalarValue) fieldValue;
			if (!operatorCodec.CanEncode(value_Renamed, this))
			{
				Global.HandleError(OpenFAST.Error.FastConstants.D3_CANT_ENCODE_VALUE, "The scalar " + this + " cannot encode the value " + value_Renamed);
			}
			ScalarValue valueToEncode = operatorCodec.GetValueToEncode((ScalarValue) value_Renamed, priorValue, this, presenceMapBuilder);
			if (operator_Renamed.ShouldStoreValue(value_Renamed))
			{
				context.Store(Dictionary, template, Key, (ScalarValue) value_Renamed);
			}
			if (valueToEncode == null)
			{
				return new byte[0];
			}
			byte[] encoding = typeCodec.Encode(valueToEncode);
			if (context.TraceEnabled && encoding.Length > 0)
			{
				context.GetEncodeTrace().Field(this, fieldValue, valueToEncode, encoding, presenceMapBuilder.Index);
			}
			return encoding;
		}

		public virtual ScalarValue DecodeValue(ScalarValue newValue, ScalarValue previousValue)
		{
			return operatorCodec.DecodeValue(newValue, previousValue, this);
		}

		public virtual ScalarValue Decode(ScalarValue previousValue)
		{
			return operatorCodec.DecodeEmptyValue(previousValue, this);
		}

		public override bool UsesPresenceMapBit()
		{
			return operatorCodec.UsesPresenceMapBit(optional);
		}

		public override bool IsPresenceMapBitSet(byte[] encoding, FieldValue fieldValue)
		{
			return operatorCodec.IsPresenceMapBitSet(encoding, fieldValue);
		}

		public override FieldValue Decode(System.IO.Stream in_Renamed, Group template, Context context, BitVectorReader presenceMapReader)
		{
			try
			{
				ScalarValue previousValue = null;
				if (operator_Renamed.UsesDictionary())
				{
					previousValue = context.Lookup(Dictionary, template, Key);
					ValidateDictionaryTypeAgainstFieldType(previousValue, this.type);
				}
				ScalarValue value_Renamed;
				int pmapIndex = presenceMapReader.Index;
				if (IsPresent(presenceMapReader))
				{
					if (context.TraceEnabled)
						in_Renamed = new RecordingInputStream(in_Renamed);
					if (!operatorCodec.ShouldDecodeType())
					{
						return operatorCodec.DecodeValue(null, null, this);
					}
					ScalarValue decodedValue = typeCodec.Decode(in_Renamed);
					value_Renamed = DecodeValue(decodedValue, previousValue);
					if (context.TraceEnabled)
						context.DecodeTrace.Field(this, value_Renamed, decodedValue, ((RecordingInputStream) in_Renamed).Buffer, pmapIndex);
				}
				else
				{
					value_Renamed = Decode(previousValue);
				}
				ValidateDecodedValueIsCorrectForType(value_Renamed, type);
				if (!((Operator == Operator.DELTA) && (value_Renamed == null)))
				{
					context.Store(Dictionary, template, Key, value_Renamed);
				}
				return value_Renamed;
			}
			catch (FastException e)
			{
				throw new FastException("Error occurred while decoding " + this, e.Code, e);
			}
		}

		private void  ValidateDecodedValueIsCorrectForType(ScalarValue value_Renamed, FASTType type)
		{
			if (value_Renamed == null)
				return ;
			type.ValidateValue(value_Renamed);
		}

		private void  ValidateDictionaryTypeAgainstFieldType(ScalarValue previousValue, FASTType type)
		{
			if (previousValue == null || previousValue.Undefined)
				return ;
			if (!type.IsValueOf(previousValue))
			{
				Global.HandleError(OpenFAST.Error.FastConstants.D4_INVALID_TYPE, "The value \"" + previousValue + "\" is not valid for the type " + type);
			}
		}

		public override string ToString()
		{
			return "Scalar [name=" + name.Name + ", operator=" + operator_Renamed + ", type=" + type + ", dictionary=" + dictionary + "]";
		}

		public override FieldValue CreateValue(string value_Renamed)
		{
			return type.GetValue(value_Renamed);
		}
		public virtual string Serialize(ScalarValue value_Renamed)
		{
			return type.Serialize(value_Renamed);
		}
		public  override bool Equals(System.Object other)
		{
			if (other == this)
				return true;
			if (other == null || !(other is Scalar))
				return false;
			return Equals((Scalar) other);
		}
		internal bool Equals(Scalar other)
		{
			return name.Equals(other.name) && type.Equals(other.type) && typeCodec.Equals(other.typeCodec) && operator_Renamed.Equals(other.operator_Renamed) && operatorCodec.Equals(other.operatorCodec) && initialValue.Equals(other.initialValue) && dictionary.Equals(other.dictionary);
		}
		public override int GetHashCode()
		{
			return name.GetHashCode() + type.GetHashCode() + typeCodec.GetHashCode() + operator_Renamed.GetHashCode() + operatorCodec.GetHashCode() + initialValue.GetHashCode() + dictionary.GetHashCode();
		}
	}
}