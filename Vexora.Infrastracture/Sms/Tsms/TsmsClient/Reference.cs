namespace Vexora.Infrastracture.Sms.Tsms.TsmsClient;

[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "8.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace = "http://sms.tsms.ir/", ConfigurationName = "Tsms.Client.tsmsService")]
public interface tsmsService
{

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    System.Threading.Tasks.Task<int[]> sendSmsAsync(string username, string password, string[] sms_number, string[] mobile, string[] msg, string[] mclass, string messageid);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    System.Threading.Tasks.Task<int[]> sendSmsExtAsync(string username, string password, string[] sms_number, string[] mobile, string[] msg, string[] mclass, string messageid, int[] send_dates);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "sendSmsResult")]
    System.Threading.Tasks.Task<int[]> sendSmsGroupAsync(string username, string password, string sms_number, string mobile, string msg, int mclass, string messageid);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    System.Threading.Tasks.Task<int[]> GetDeliverySmsAsync(string username, string password, int[] recipientIds);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    System.Threading.Tasks.Task<int[]> WsdlCheckSendAsync(string username, string password, string messageid);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    System.Threading.Tasks.Task<int[]> DeleteSendSMSQueueAsync(string username, string password, string messageid);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    System.Threading.Tasks.Task<int[]> sendSoundOfTextAsync(string username, string password, string phones, string msg, string speaker, int gainLevel, int pitchLevel, int punctuationLevel, int speechSpeedLevel, int toneLevel, int tryCount, int MinuteBetweenTry, string messageid);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    System.Threading.Tasks.Task<int[]> sendSoundAsync(string username, string password, string phones, string title, int soundid, int soundIdType, int tryCount, int MinuteBetweenTry, string messageid);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "uploadResponse")]
    System.Threading.Tasks.Task<int[]> uploadAsync(string username, string password, string title, [System.Xml.Serialization.SoapElementAttribute(DataType = "base64Binary")] byte[] binaryfile);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(objectInfo))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object3))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object2))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object1))]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "GetDeliverySoundResponse")]
    System.Threading.Tasks.Task<Tsms.TsmsClient.object1[]> GetDeliverySoundAsync(string username, string password, string phones, int idsendsound);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(objectInfo))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object3))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object2))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object1))]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "GetDeliverySmsGroupResponse")]
    System.Threading.Tasks.Task<Tsms.TsmsClient.object2[]> GetDeliverySmsGroupAsync(string username, string password, string phones, int idsendsms);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(objectInfo))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object3))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object2))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object1))]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "GetSmsReceivedResponse")]
    System.Threading.Tasks.Task<Tsms.TsmsClient.object3[]> GetSmsReceivedAsync(string username, string password);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(objectInfo))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object3))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object2))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object1))]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "GetSmsReceivedIdResponse")]
    System.Threading.Tasks.Task<Tsms.TsmsClient.object3[]> GetSmsReceivedIdAsync(string username, string password, int id);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(objectInfo))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object3))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object2))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object1))]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "sendSoundOfPatternResponse")]
    System.Threading.Tasks.Task<int[]> sendSoundOfPatternAsync(string username, string password, string phone, string title, int patternid, string[] patternVariables, int tryCount, int MinuteBetweenTry, string messageid);

    [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(Style = System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults = true, Use = System.ServiceModel.OperationFormatUse.Encoded)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(objectInfo))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object3))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object2))]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(object1))]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfoResponse")]
    System.Threading.Tasks.Task<Tsms.TsmsClient.objectInfo[]> UserInfoAsync(string username, string password);
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "8.0.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://sms.tsms.ir/")]
public partial class object1
{

    private int statusField;

    private bool statusFieldSpecified;

    private string phoneField;

    private int trycountField;

    private bool trycountFieldSpecified;

    private int durationField;

    private bool durationFieldSpecified;

    /// <remarks/>
    public int status
    {
        get
        {
            return this.statusField;
        }
        set
        {
            this.statusField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.SoapIgnoreAttribute()]
    public bool statusSpecified
    {
        get
        {
            return this.statusFieldSpecified;
        }
        set
        {
            this.statusFieldSpecified = value;
        }
    }

    /// <remarks/>
    public string phone
    {
        get
        {
            return this.phoneField;
        }
        set
        {
            this.phoneField = value;
        }
    }

    /// <remarks/>
    public int trycount
    {
        get
        {
            return this.trycountField;
        }
        set
        {
            this.trycountField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.SoapIgnoreAttribute()]
    public bool trycountSpecified
    {
        get
        {
            return this.trycountFieldSpecified;
        }
        set
        {
            this.trycountFieldSpecified = value;
        }
    }

    /// <remarks/>
    public int duration
    {
        get
        {
            return this.durationField;
        }
        set
        {
            this.durationField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.SoapIgnoreAttribute()]
    public bool durationSpecified
    {
        get
        {
            return this.durationFieldSpecified;
        }
        set
        {
            this.durationFieldSpecified = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "8.0.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://sms.tsms.ir/")]
public partial class objectInfo
{

    private int creditField;

    private bool creditFieldSpecified;

    private string[] sms_numebrsField;

    /// <remarks/>
    public int credit
    {
        get
        {
            return this.creditField;
        }
        set
        {
            this.creditField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.SoapIgnoreAttribute()]
    public bool creditSpecified
    {
        get
        {
            return this.creditFieldSpecified;
        }
        set
        {
            this.creditFieldSpecified = value;
        }
    }

    /// <remarks/>
    public string[] sms_numebrs
    {
        get
        {
            return this.sms_numebrsField;
        }
        set
        {
            this.sms_numebrsField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "8.0.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://sms.tsms.ir/")]
public partial class object3
{

    private int statusField;

    private bool statusFieldSpecified;

    private int idField;

    private bool idFieldSpecified;

    private string phoneField;

    private string sms_numberField;

    private string msgField;

    private string dateField;

    /// <remarks/>
    public int status
    {
        get
        {
            return this.statusField;
        }
        set
        {
            this.statusField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.SoapIgnoreAttribute()]
    public bool statusSpecified
    {
        get
        {
            return this.statusFieldSpecified;
        }
        set
        {
            this.statusFieldSpecified = value;
        }
    }

    /// <remarks/>
    public int id
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.SoapIgnoreAttribute()]
    public bool idSpecified
    {
        get
        {
            return this.idFieldSpecified;
        }
        set
        {
            this.idFieldSpecified = value;
        }
    }

    /// <remarks/>
    public string phone
    {
        get
        {
            return this.phoneField;
        }
        set
        {
            this.phoneField = value;
        }
    }

    /// <remarks/>
    public string sms_number
    {
        get
        {
            return this.sms_numberField;
        }
        set
        {
            this.sms_numberField = value;
        }
    }

    /// <remarks/>
    public string msg
    {
        get
        {
            return this.msgField;
        }
        set
        {
            this.msgField = value;
        }
    }

    /// <remarks/>
    public string date
    {
        get
        {
            return this.dateField;
        }
        set
        {
            this.dateField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "8.0.0")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.Xml.Serialization.SoapTypeAttribute(Namespace = "http://sms.tsms.ir/")]
public partial class object2
{

    private int statusField;

    private bool statusFieldSpecified;

    private string phoneField;

    /// <remarks/>
    public int status
    {
        get
        {
            return this.statusField;
        }
        set
        {
            this.statusField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.SoapIgnoreAttribute()]
    public bool statusSpecified
    {
        get
        {
            return this.statusFieldSpecified;
        }
        set
        {
            this.statusFieldSpecified = value;
        }
    }

    /// <remarks/>
    public string phone
    {
        get
        {
            return this.phoneField;
        }
        set
        {
            this.phoneField = value;
        }
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "8.0.0")]
public interface tsmsServiceChannel : Tsms.TsmsClient.tsmsService, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "8.0.0")]
public partial class tsmsServiceClient : System.ServiceModel.ClientBase<Tsms.TsmsClient.tsmsService>, Tsms.TsmsClient.tsmsService
{

    /// <summary>
    /// Implement this partial method to configure the service endpoint.
    /// </summary>
    /// <param name="serviceEndpoint">The endpoint to configure</param>
    /// <param name="clientCredentials">The client credentials</param>
    static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);

    public tsmsServiceClient() :
            base(tsmsServiceClient.GetDefaultBinding(), tsmsServiceClient.GetDefaultEndpointAddress())
    {
        this.Endpoint.Name = EndpointConfiguration.tsms_Port.ToString();
        ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
    }

    public tsmsServiceClient(EndpointConfiguration endpointConfiguration) :
            base(tsmsServiceClient.GetBindingForEndpoint(endpointConfiguration), tsmsServiceClient.GetEndpointAddress(endpointConfiguration))
    {
        this.Endpoint.Name = endpointConfiguration.ToString();
        ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
    }

    public tsmsServiceClient(EndpointConfiguration endpointConfiguration, string remoteAddress) :
            base(tsmsServiceClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
    {
        this.Endpoint.Name = endpointConfiguration.ToString();
        ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
    }

    public tsmsServiceClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) :
            base(tsmsServiceClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
    {
        this.Endpoint.Name = endpointConfiguration.ToString();
        ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
    }

    public tsmsServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
    {
    }

    public System.Threading.Tasks.Task<int[]> sendSmsAsync(string username, string password, string[] sms_number, string[] mobile, string[] msg, string[] mclass, string messageid)
    {
        return base.Channel.sendSmsAsync(username, password, sms_number, mobile, msg, mclass, messageid);
    }

    public System.Threading.Tasks.Task<int[]> sendSmsExtAsync(string username, string password, string[] sms_number, string[] mobile, string[] msg, string[] mclass, string messageid, int[] send_dates)
    {
        return base.Channel.sendSmsExtAsync(username, password, sms_number, mobile, msg, mclass, messageid, send_dates);
    }

    public System.Threading.Tasks.Task<int[]> sendSmsGroupAsync(string username, string password, string sms_number, string mobile, string msg, int mclass, string messageid)
    {
        return base.Channel.sendSmsGroupAsync(username, password, sms_number, mobile, msg, mclass, messageid);
    }

    public System.Threading.Tasks.Task<int[]> GetDeliverySmsAsync(string username, string password, int[] recipientIds)
    {
        return base.Channel.GetDeliverySmsAsync(username, password, recipientIds);
    }

    public System.Threading.Tasks.Task<int[]> WsdlCheckSendAsync(string username, string password, string messageid)
    {
        return base.Channel.WsdlCheckSendAsync(username, password, messageid);
    }

    public System.Threading.Tasks.Task<int[]> DeleteSendSMSQueueAsync(string username, string password, string messageid)
    {
        return base.Channel.DeleteSendSMSQueueAsync(username, password, messageid);
    }

    public System.Threading.Tasks.Task<int[]> sendSoundOfTextAsync(string username, string password, string phones, string msg, string speaker, int gainLevel, int pitchLevel, int punctuationLevel, int speechSpeedLevel, int toneLevel, int tryCount, int MinuteBetweenTry, string messageid)
    {
        return base.Channel.sendSoundOfTextAsync(username, password, phones, msg, speaker, gainLevel, pitchLevel, punctuationLevel, speechSpeedLevel, toneLevel, tryCount, MinuteBetweenTry, messageid);
    }

    public System.Threading.Tasks.Task<int[]> sendSoundAsync(string username, string password, string phones, string title, int soundid, int soundIdType, int tryCount, int MinuteBetweenTry, string messageid)
    {
        return base.Channel.sendSoundAsync(username, password, phones, title, soundid, soundIdType, tryCount, MinuteBetweenTry, messageid);
    }

    public System.Threading.Tasks.Task<int[]> uploadAsync(string username, string password, string title, byte[] binaryfile)
    {
        return base.Channel.uploadAsync(username, password, title, binaryfile);
    }

    public System.Threading.Tasks.Task<Tsms.TsmsClient.object1[]> GetDeliverySoundAsync(string username, string password, string phones, int idsendsound)
    {
        return base.Channel.GetDeliverySoundAsync(username, password, phones, idsendsound);
    }

    public System.Threading.Tasks.Task<Tsms.TsmsClient.object2[]> GetDeliverySmsGroupAsync(string username, string password, string phones, int idsendsms)
    {
        return base.Channel.GetDeliverySmsGroupAsync(username, password, phones, idsendsms);
    }

    public System.Threading.Tasks.Task<Tsms.TsmsClient.object3[]> GetSmsReceivedAsync(string username, string password)
    {
        return base.Channel.GetSmsReceivedAsync(username, password);
    }

    public System.Threading.Tasks.Task<Tsms.TsmsClient.object3[]> GetSmsReceivedIdAsync(string username, string password, int id)
    {
        return base.Channel.GetSmsReceivedIdAsync(username, password, id);
    }

    public System.Threading.Tasks.Task<int[]> sendSoundOfPatternAsync(string username, string password, string phone, string title, int patternid, string[] patternVariables, int tryCount, int MinuteBetweenTry, string messageid)
    {
        return base.Channel.sendSoundOfPatternAsync(username, password, phone, title, patternid, patternVariables, tryCount, MinuteBetweenTry, messageid);
    }

    public System.Threading.Tasks.Task<Tsms.TsmsClient.objectInfo[]> UserInfoAsync(string username, string password)
    {
        return base.Channel.UserInfoAsync(username, password);
    }

    public virtual System.Threading.Tasks.Task OpenAsync()
    {
        return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
    }

#if !NET6_0_OR_GREATER
        public virtual System.Threading.Tasks.Task CloseAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
#endif

    private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
    {
        if ((endpointConfiguration == EndpointConfiguration.tsms_Port))
        {
            System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding();
            result.MaxBufferSize = int.MaxValue;
            result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
            result.MaxReceivedMessageSize = int.MaxValue;
            result.AllowCookies = true;
            return result;
        }
        throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
    }

    private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
    {
        if ((endpointConfiguration == EndpointConfiguration.tsms_Port))
        {
            return new System.ServiceModel.EndpointAddress("http://www.tsms.ir/soapWSDL/");
        }
        throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
    }

    private static System.ServiceModel.Channels.Binding GetDefaultBinding()
    {
        return tsmsServiceClient.GetBindingForEndpoint(EndpointConfiguration.tsms_Port);
    }

    private static System.ServiceModel.EndpointAddress GetDefaultEndpointAddress()
    {
        return tsmsServiceClient.GetEndpointAddress(EndpointConfiguration.tsms_Port);
    }

    public enum EndpointConfiguration
    {

        tsms_Port,
    }
}