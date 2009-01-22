using System;
using Context = OpenFAST.Context;
using Message = OpenFAST.Message;
using MessageInputStream = OpenFAST.MessageInputStream;
using MessageOutputStream = OpenFAST.MessageOutputStream;
using QName = OpenFAST.QName;
using ErrorCode = OpenFAST.Error.ErrorCode;
using ErrorHandler = OpenFAST.Error.ErrorHandler;
using FastException = OpenFAST.Error.FastException;
using MessageTemplate = OpenFAST.Template.MessageTemplate;
using TemplateRegistry = OpenFAST.Template.TemplateRegistry;
using OpenFAST;

namespace OpenFAST.Session
{
	public class Session : ErrorHandler
	{
		private class SessionThread : IThreadRunnable
		{
			public SessionThread(Session enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(Session enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private Session enclosingInstance;
			public Session Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public virtual void  Run()
			{
				while (Enclosing_Instance.listening)
				{
					try
					{
						Message message = Enclosing_Instance.MessageInputStream.readMessage();
						
						if (message == null)
						{
							Enclosing_Instance.listening = false;
							break;
						}
						if (Enclosing_Instance.protocol.IsProtocolMessage(message))
						{
							Enclosing_Instance.protocol.HandleMessage(Enclosing_Instance, message);
						}
						else if (Enclosing_Instance.messageListener != null)
						{
							Enclosing_Instance.messageListener.OnMessage(message);
						}
						else
						{
							throw new System.SystemException("Received non-protocol message without a message listener.");
						}
					}
					catch (System.Exception e)
					{
						System.Exception cause = e.InnerException;
						
						if (cause != null && cause.GetType().Equals(typeof(System.Net.Sockets.SocketException)) && cause.Message.Equals("Socket closed"))
						{
							Enclosing_Instance.listening = false;
						}
						else if (e is FastException)
						{
							FastException fastException = ((FastException) e);
							Enclosing_Instance.errorHandler.Error(fastException.Code, fastException.Message);
						}
						else
						{
							Enclosing_Instance.errorHandler.Error(OpenFAST.Error.FastConstants.GENERAL_ERROR, e.Message, e);
						}
					}
				}
			}
		}
		virtual public Client Client
		{
			get
			{
				return client;
			}
			
			set
			{
				this.client = value;
			}
			
		}
		virtual public ErrorHandler ErrorHandler
		{
			get
			{
				return errorHandler;
			}
			
			set
			{
				if (value == null)
				{
					this.errorHandler = OpenFAST.Error.ErrorHandler_Fields.NULL;
				}
				
				this.errorHandler = value;
			}
			
		}
		virtual public Connection Connection
		{
			get
			{
				return connection;
			}
			
		}
		virtual public MessageListener MessageHandler
		{
			set
			{
				this.messageListener = value;
				Listening = true;
			}
			
		}
		virtual public bool Listening
		{
			set
			{
				this.listening = value;
				if (value)
					ListenForMessages();
			}
			
		}
		virtual public SessionListener SessionListener
		{
			set
			{
				this.SessionListener = value;
			}
			
		}
        private MessageInputStream in_stream;
        public MessageInputStream MessageInputStream
        {
            get
            {
                return in_stream;
            }
            set
            {
                in_stream = value;
            }
        }
        private MessageOutputStream out_stream;

        public MessageOutputStream MessageOutputStream
        {
            get
            {
                return out_stream;
            }
            set
            {
                out_stream = value;
            }
        }
		private SessionProtocol protocol;
		private Connection connection;
		private Client client;
		private MessageListener messageListener;
		private bool listening;
		private SupportClass.ThreadClass listeningThread;
		private ErrorHandler errorHandler = OpenFAST.Error.ErrorHandler_Fields.DEFAULT;
		private SessionListener sessionListener = OpenFAST.Session.SessionListener_Fields.NULL;
		
		public Session(Connection connection, SessionProtocol protocol)
		{
			Context inContext = new Context();
			Context outContext = new Context();
			inContext.ErrorHandler = this;
			
			this.connection = connection;
			this.protocol = protocol;
			try
			{
				this.in_stream = new MessageInputStream(connection.InputStream.BaseStream, inContext);
				this.out_stream = new MessageOutputStream(connection.OutputStream.BaseStream, outContext);
			}
			catch (System.IO.IOException e)
			{
				errorHandler.Error(null, "Error occurred in connection.", e);
				throw new IllegalStateException(e);
			}
			
			protocol.ConfigureSession(this);
		}
		
		// INITIATOR
		public virtual void  Close()
		{
			listening = false;
			out_stream.WriteMessage(protocol.CloseMessage);
			in_stream.Close();
			out_stream.Close();
		}
		
		// RESPONDER
		public virtual void  Close(ErrorCode alertCode)
		{
			listening = false;
			in_stream.Close();
			out_stream.Close();
			sessionListener.OnClose();
		}
		
		public virtual void  Error(ErrorCode code, string message)
		{
			if (code.Equals(OpenFAST.Error.FastConstants.D9_TEMPLATE_NOT_REGISTERED))
			{
				code = OpenFAST.Session.SessionConstants.TEMPLATE_NOT_SUPPORTED;
				message = "Template Not Supported";
			}
			protocol.OnError(this, code, message);
			errorHandler.Error(code, message);
		}
		
		public virtual void  Error(ErrorCode code, string message, System.Exception t)
		{
			protocol.OnError(this, code, message);
			errorHandler.Error(code, message, t);
		}
		
		public virtual void  Reset()
		{
			out_stream.Reset();
			in_stream.Reset();
			out_stream.WriteMessage(protocol.ResetMessage);
		}
		
		private void  ListenForMessages()
		{
			if (listeningThread == null)
			{
				IThreadRunnable messageReader = new SessionThread(this);
				listeningThread = new SupportClass.ThreadClass(new System.Threading.ThreadStart(messageReader.Run), "FAST Session Message Reader");
			}
			if (listeningThread.IsAlive)
				return ;
			listeningThread.Start();
		}
		
		public virtual void  SendTemplates(TemplateRegistry registry)
		{
			if (!protocol.SupportsTemplateExchange())
			{
				throw new System.NotSupportedException("The procotol " + protocol + " does not support template exchange.");
			}
			MessageTemplate[] templates = registry.Templates;
			for (int i = 0; i < templates.Length; i++)
			{
				MessageTemplate template = templates[i];
				out_stream.WriteMessage(protocol.CreateTemplateDefinitionMessage(template));
				out_stream.WriteMessage(protocol.CreateTemplateDeclarationMessage(template, registry.GetId(template)));
				if (!out_stream.GetTemplateRegistry().IsRegistered(template))
					out_stream.RegisterTemplate(registry.GetId(template), template);
			}
		}
		
		public virtual void  AddDynamicTemplateDefinition(MessageTemplate template)
		{
			in_stream.GetTemplateRegistry().Define(template);
			out_stream.GetTemplateRegistry().Define(template);
		}
		
		public virtual void  RegisterDynamicTemplate(QName templateName, int id)
		{
			if (!in_stream.GetTemplateRegistry().IsDefined(templateName))
			{
				throw new System.SystemException("Template " + templateName + " has not been defined.");
			}
			in_stream.GetTemplateRegistry().Register(id, templateName);
			if (!out_stream.GetTemplateRegistry().IsDefined(templateName))
			{
				throw new System.SystemException("Template " + templateName + " has not been defined.");
			}
			out_stream.GetTemplateRegistry().Register(id, templateName);
		}
	}
}