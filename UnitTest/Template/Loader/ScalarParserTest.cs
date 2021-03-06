﻿/*

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
using System.Xml;
using NUnit.Framework;
using OpenFAST.Error;
using OpenFAST.Template;
using OpenFAST.Template.Loader;
using OpenFAST.Template.Operators;
using OpenFAST.Template.Types;
using OpenFAST.UnitTests.Test;

namespace OpenFAST.UnitTests.Template.Loader
{
    [TestFixture]
    public class ScalarParserTest : OpenFastTestCase
    {
        #region Setup/Teardown

        [SetUp]
        protected void SetUp()
        {
            _parser = new ScalarParser();
            _context = XmlMessageTemplateLoader.CreateInitialContext();
        }

        #endregion

        private ScalarParser _parser;
        private ParsingContext _context;

        [Test]
        public void TestInvalidType()
        {
            XmlElement invalidDef = Document("<array name='set'/>");
            try
            {
                _parser.Parse(invalidDef, _context);
            }
            catch (StatErrorException e)
            {
                Assert.AreEqual(StaticError.InvalidType, e.Error);
                Assert.IsTrue(e.Message.StartsWith("The type array is not defined.  Possible types: "));
            }
        }

        [Test]
        public void TestParseCopyInt()
        {
            XmlElement int32Def = Document("<int32 name='value'><copy/></int32>");
            Assert.IsTrue(_parser.CanParse(int32Def, _context));
            var int32 = (Scalar) _parser.Parse(int32Def, _context);
            AssertScalarField(int32, FastType.I32, "value", null, "", DictionaryFields.Global, "value",
                              Operator.Copy, ScalarValue.Undefined, false);
        }

        [Test]
        public void TestParseDeltaDecimal()
        {
            XmlElement decimalDef =
                Document(
                    "<decimal name='price'><delta value='1.2' key='decimalValue' dictionary='marketData'/></decimal>");
            Assert.IsTrue(_parser.CanParse(decimalDef, _context));
            var decimalt = (Scalar) _parser.Parse(decimalDef, _context);
            AssertScalarField(decimalt, FastType.Decimal, "price", null, "", "marketData", "decimalValue",
                              Operator.Delta, new DecimalValue(1.2), false);
        }

        [Test]
        public void TestParseStringDefaultWithNamespace()
        {
            XmlElement stringDef =
                Document("<string name='text' ns='http://openfast.org/data/' presence='optional'><default/></string>");
            Assert.IsTrue(_parser.CanParse(stringDef, _context));
            var stringt = (Scalar) _parser.Parse(stringDef, _context);
            AssertScalarField(stringt, FastType.String, "text", null, "http://openfast.org/data/",
                              DictionaryFields.Global, "text", Operator.Default,
                              ScalarValue.Undefined, true);
        }

        [Test]
        public void TestParseWithOperatorNamespace()
        {
            XmlElement uintDef =
                Document("<uInt32 name='uint'><copy key='values' ns='http://openfast.org/data/'/></uInt32>");
            var uintt = (Scalar) _parser.Parse(uintDef, _context);
            AssertScalarField(uintt, FastType.U32, "uint", null, "", DictionaryFields.Global, "values",
                              "http://openfast.org/data/", Operator.Copy,
                              ScalarValue.Undefined, false);
        }
    }
}