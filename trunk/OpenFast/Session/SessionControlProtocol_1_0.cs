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
using OpenFAST.Codec;
using OpenFAST.Error;
using OpenFAST.Template;
using OpenFAST.Template.Operator;
using Type = OpenFAST.Template.Type.FASTType;

namespace OpenFAST.Session
{
    public class SessionControlProtocol_1_0 : AbstractSessionControlProtocol
    {
        internal const int FAST_HELLO_TEMPLATE_ID = 16000;
        internal const int FAST_ALERT_TEMPLATE_ID = 16001;
        public static readonly MessageTemplate FAST_ALERT_TEMPLATE;
        public static readonly MessageTemplate FAST_HELLO_TEMPLATE;
        private static readonly IMessageHandler RESET_HANDLER;

        static SessionControlProtocol_1_0()
        {
            FAST_ALERT_TEMPLATE = new MessageTemplate("",
                                                      new Field[]
                                                          {
                                                              new Scalar("Severity", Type.U32, Operator.NONE,
                                                                         ScalarValue.UNDEFINED, false),
                                                              new Scalar("Code", Type.U32, Operator.NONE,
                                                                         ScalarValue.UNDEFINED, false),
                                                              new Scalar("Value", Type.U32, Operator.NONE,
                                                                         ScalarValue.UNDEFINED, true),
                                                              new Scalar("Description", Type.ASCII, Operator.NONE,
                                                                         ScalarValue.UNDEFINED, false)
                                                          });
            FAST_HELLO_TEMPLATE = new MessageTemplate("",
                                                      new Field[]
                                                          {
                                                              new Scalar("SenderName", Type.ASCII, Operator.NONE,
                                                                         ScalarValue.UNDEFINED, false)
                                                          });
            RESET_HANDLER = new RESETMessageHandler();
        }

        public override Message CloseMessage
        {
            get { return CreateFastAlertMessage(SessionConstants.CLOSE); }
        }

        public override Session OnNewConnection(string serverName, IConnection connection)
        {
            var session = new Session(connection, this, TemplateRegistryFields.Null, TemplateRegistryFields.Null);
            Message message = session.MessageInputStream.ReadMessage();
            session.MessageOutputStream.WriteMessage(CreateHelloMessage(serverName));
            string clientName = message.GetString(1);
            session.Client = new BasicClient(clientName, "unknown");
            return session;
        }

        public override Session Connect(string senderName, IConnection connection, ITemplateRegistry inboundRegistry,
                                        ITemplateRegistry outboundRegistry, IMessageListener messageListener,
                                        ISessionListener sessionListener)
        {
            var session = new Session(connection, this, TemplateRegistryFields.Null, TemplateRegistryFields.Null);
            session.MessageOutputStream.WriteMessage(CreateHelloMessage(senderName));
            Message message = session.MessageInputStream.ReadMessage();
            string serverName = message.GetString(1);
            session.Client = new BasicClient(serverName, "unknown");
            return session;
        }

        public override void OnError(Session session, ErrorCode code, string message)
        {
            session.MessageOutputStream.WriteMessage(CreateFastAlertMessage(code));
        }

        public virtual void RegisterSessionTemplates(ITemplateRegistry registry)
        {
            registry.Register(FAST_HELLO_TEMPLATE_ID, FAST_HELLO_TEMPLATE);
            registry.Register(FAST_ALERT_TEMPLATE_ID, FAST_ALERT_TEMPLATE);
            registry.Register(FAST_RESET_TEMPLATE_ID, FAST_RESET_TEMPLATE);
        }

        public override void ConfigureSession(Session session)
        {
            RegisterSessionTemplates(session.MessageInputStream.GetTemplateRegistry());
            RegisterSessionTemplates(session.MessageOutputStream.GetTemplateRegistry());
            session.MessageInputStream.AddMessageHandler(FAST_RESET_TEMPLATE, RESET_HANDLER);
            session.MessageOutputStream.AddMessageHandler(FAST_RESET_TEMPLATE, RESET_HANDLER);
        }

        public static Message CreateFastAlertMessage(ErrorCode code)
        {
            var alert = new Message(FAST_ALERT_TEMPLATE);
            alert.SetInteger(1, code.Severity.Code);
            alert.SetInteger(2, code.Code);
            alert.SetString(4, code.Description);
            return alert;
        }

        public static Message CreateHelloMessage(string name)
        {
            var message = new Message(FAST_HELLO_TEMPLATE);
            message.SetString(1, name);
            return message;
        }

        public override void HandleMessage(Session session, Message message)
        {
            if (message.Template.Equals(FAST_ALERT_TEMPLATE))
            {
                ErrorCode alertCode = ErrorCode.GetAlertCode(message);
                if (alertCode.Equals(SessionConstants.CLOSE))
                {
                    session.Close(alertCode);
                }
                else
                {
                    session.ErrorHandler.Error(alertCode, message.GetString(4));
                }
            }
        }

        public override bool IsProtocolMessage(Message message)
        {
            if (message == null)
                return false;
            return (message.Template.Equals(FAST_ALERT_TEMPLATE)) || (message.Template.Equals(FAST_HELLO_TEMPLATE)) ||
                   (message.Template.Equals(FAST_RESET_TEMPLATE));
        }

        public override bool SupportsTemplateExchange()
        {
            return false;
        }

        public override Message CreateTemplateDeclarationMessage(MessageTemplate messageTemplate, int templateId)
        {
            return null;
        }

        public override Message CreateTemplateDefinitionMessage(MessageTemplate messageTemplate)
        {
            return null;
        }

        #region Nested type: RESETMessageHandler

        public class RESETMessageHandler : IMessageHandler
        {
            #region MessageHandler Members

            public virtual void HandleMessage(Message readMessage, Context context, ICoder coder)
            {
                coder.Reset();
            }

            #endregion
        }

        #endregion
    }
}