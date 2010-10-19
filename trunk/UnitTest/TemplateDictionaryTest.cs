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
using NUnit.Framework;
using OpenFAST;
using OpenFAST.Error;
using OpenFAST.Template;
using OpenFAST.Template.Operator;
using OpenFAST.Template.Type;

namespace UnitTest
{
    [TestFixture]
    public class TemplateDictionaryTest
    {
        [Test]
        public void TestExistingTemplateValueLookup()
        {
            IDictionary dictionary = new TemplateDictionary();
            Group template = new MessageTemplate("Position",
                                                 new Field[]
                                                     {
                                                         new Scalar("exchange", FASTType.STRING, Operator.COPY,
                                                                    ScalarValue.Undefined, false)
                                                     });
            ScalarValue value = new StringValue("NYSE");
            dictionary.Store(template, FastConstants.ANY_TYPE, new QName("exchange"), value);

            Assert.AreEqual(ScalarValue.Undefined, dictionary.Lookup(template, new QName("bid"), FastConstants.ANY_TYPE));
        }

        [Test]
        public void TestLookupMultipleValuesForTemplate()
        {
            IDictionary dictionary = new TemplateDictionary();
            Group template = new MessageTemplate("Position",
                                                 new Field[]
                                                     {
                                                         new Scalar("exchange", FASTType.STRING, Operator.COPY,
                                                                    ScalarValue.Undefined, false)
                                                     });
            ScalarValue value = new StringValue("NYSE");
            ScalarValue marketValue = new DecimalValue(100000.00);
            dictionary.Store(template, FastConstants.ANY_TYPE, new QName("exchange"), value);
            dictionary.Store(template, FastConstants.ANY_TYPE, new QName("marketValue"), marketValue);

            Assert.IsFalse(value.Equals(ScalarValue.Undefined));
            Assert.AreEqual(value, dictionary.Lookup(template, new QName("exchange"), FastConstants.ANY_TYPE));
            Assert.AreEqual(marketValue, dictionary.Lookup(template, new QName("marketValue"), FastConstants.ANY_TYPE));
        }

        [Test]
        public void TestReset()
        {
            IDictionary dictionary = new TemplateDictionary();
            Group template = new MessageTemplate("Position",
                                                 new Field[]
                                                     {
                                                         new Scalar("exchange", FASTType.STRING, Operator.COPY,
                                                                    ScalarValue.Undefined, false)
                                                     });
            ScalarValue value = new StringValue("NYSE");
            dictionary.Store(template, FastConstants.ANY_TYPE, new QName("exchange"), value);

            Assert.AreEqual(value, dictionary.Lookup(template, new QName("exchange"), FastConstants.ANY_TYPE));
            dictionary.Reset();
            Assert.AreEqual(ScalarValue.Undefined,
                            dictionary.Lookup(template, new QName("exchange"), FastConstants.ANY_TYPE));
        }

        [Test]
        public void TestTemplateValueLookup()
        {
            IDictionary dictionary = new TemplateDictionary();
            Group template = new MessageTemplate("Position",
                                                 new Field[]
                                                     {
                                                         new Scalar("exchange", FASTType.STRING, Operator.COPY,
                                                                    ScalarValue.Undefined, false)
                                                     });
            ScalarValue value = new StringValue("NYSE");
            dictionary.Store(template, FastConstants.ANY_TYPE, new QName("exchange"), value);

            Assert.AreEqual(value, dictionary.Lookup(template, new QName("exchange"), FastConstants.ANY_TYPE));

            Group quoteTemplate = new MessageTemplate("Quote",
                                                      new Field[]
                                                          {
                                                              new Scalar("bid", FASTType.DECIMAL, Operator.DELTA,
                                                                         ScalarValue.Undefined, false)
                                                          });
            Assert.AreEqual(ScalarValue.Undefined,
                            dictionary.Lookup(quoteTemplate, new QName("exchange"), FastConstants.ANY_TYPE));
        }
    }
}