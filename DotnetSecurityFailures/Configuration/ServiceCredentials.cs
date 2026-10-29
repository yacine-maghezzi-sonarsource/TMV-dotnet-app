namespace DotnetSecurityFailures.Configuration;

/// <summary>
/// INTENTIONALLY VULNERABLE: Hardcoded secrets (CWE-798, OWASP A04/A07)
///
/// Every value below is a FAKE, randomly generated credential that follows the
/// real provider format and has real entropy, so that secret scanners detect them.
/// None of them grant access to anything. NEVER do this in real code.
/// </summary>
public static class ServiceCredentials
{
    // AWS IAM user credentials
    public const string AwsAccessKeyId = "AKIA57TQN5BQL62RKPWK";
    public const string AwsSecretAccessKey = "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";

    // GitHub personal access token (classic)
    public const string GitHubToken = "ghp_HPqih1pUWU3OkmR4mlHG8J0Sw95z4ZAOFalO";

    // Slack bot token
    public const string SlackBotToken = "xoxb-654614380943-3514370938730-SuJuohr96FQh1c1oL5rG3Yes";

    // Stripe live secret key
    public const string StripeSecretKey = "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";

    // Google Cloud API key
    public const string GoogleApiKey = "AIzaP-xvsduaa2gIqZm4eiCZDNelRDpu3Dxns5K";

    // SendGrid API key
    public const string SendGridApiKey = "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";

    // Twilio API key SID
    public const string TwilioApiKey = "SK3f320d4b6264f67f8950e21744bb2289";

    // OpenAI project key
    public const string OpenAiApiKey = "sk-proj-eWObMJ5SlOoYFBMh82oXqOWYQX6p8ury06uBIJOHJxn4_Vi3MLpA0Li8HDGCct3RT3BlbkFJo9-3Zv6IkZw-HClE6j26lLLwIR6oRifPA8RDGNAvZ9fcxsRJv2zZcUY2xBm1UZjv";

    // Azure Storage connection string
    public const string AzureStorageConnectionString =
        "DefaultEndpointsProtocol=https;AccountName=nwpaystorage;AccountKey=3s4UXgfWisjSy70+qo0ern1pwVKaLM1LJSvaVeAtJjWzYfpvPJFEF8MttmRu3egFr7vhdJOKsmkne781x3Ljxw==;EndpointSuffix=core.windows.net";

    // PostgreSQL production connection string with inline password
    public const string PostgresConnectionString =
        "Host=prod-db.northwindpay.internal;Port=5432;Database=payments;Username=payments_admin;Password=DdMHuuQWMJ7fhlMi3rN2j#Vs";

    // JWT HMAC signing secret
    public const string JwtSigningKey = "+nyA2xZeEaU6KTlkohIM394fjvRtkf+90ibYomq2jvy/mV8bShJdE+494vbJnkaK";

    // AES-256 key and static IV (hex)
    public const string AesKeyHex = "a277e2cb32e608952fb1737eb3eac9e7287f151a13a12f57b303e964a215cca8";
    public const string AesIvHex = "0fc344e3acfeb8480b7b34d7c1d5958c";

    // npm, SonarQube and Docker Hub tokens used by CI
    public const string NpmPublishToken = "npm_qWJSde4q2z2FQtOgLFhLAmDZlChglN3XQTy2";
    public const string SonarToken = "sqp_cc0a9d301a0844eb3eaa5aa1383fd78b453aa082";
    public const string DockerHubToken = "dckr_pat_X8FJLubRSFQJ1DvV6Nx4EZj4xAg";
}
