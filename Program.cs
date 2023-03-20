using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.Orchestration;
using System.Reflection;

//TODO: add your LinkedIn auth token here
//you can create a token at https://www.linkedin.com/developers/tools/oauth
var authToken = "";

//configure your Azure OpenAI backend
var key = "";
var endpoint = "";
var model = "";


var sk = Kernel.Builder.Configure(c => c.AddAzureOpenAICompletionBackend(model, model, endpoint, key)).Build();
sk.ImportSkill(new LinkedInSkill(), "LI");
sk.CreateSemanticFunction(Assembly.GetEntryAssembly().LoadEmbeddedResource("sk_announce.skprompt.txt"), 
    "CreateAnnouncement", 
    "Announcer", 
    maxTokens:2048);

//get the LinkedIn PersonUPN
var contextVariables = new ContextVariables();
contextVariables.Set(LinkedInSkill.Parameters.AuthToken, authToken);
var personUpnResult = await sk.RunAsync(contextVariables, sk.Skills.GetNativeFunction("LI", "GetPersonUpn"));
var personUpn = personUpnResult.Result;

var announcementUri = "https://devblogs.microsoft.com/semantic-kernel";
var announcementMessage = "📢 Introducing Semantic Kernel—a new open-source project that helps developers integrate cutting-edge large language model technology like GPT-4 quickly and easily into their apps. Learn more at https://devblogs.microsoft.com/semantic-kernel";
var announcementTopic = "Semantic Kernel";

//TODO: Would be better to get this directly from the users LI profile, unfortunately this requires special permissions that I didn't have time to get
var myRole = "";
var myProfile = "";
var githubRepo = "https://github.com/craigomatic/sk-announce";

contextVariables = new ContextVariables(announcementMessage);
contextVariables.Set("TOPIC", announcementTopic);
contextVariables.Set("ROLE", myRole);
contextVariables.Set("BACKGROUND", myProfile);
contextVariables.Set("SOURCECODE", githubRepo);

var announcementResult = await sk.RunAsync(contextVariables, sk.Skills.GetSemanticFunction("Announcer", "CreateAnnouncement"));
var announcementText = announcementResult.Result;

//write the LI post which has the reference to the image included
contextVariables = new ContextVariables(announcementText);
contextVariables.Set(LinkedInSkill.Parameters.AuthToken, authToken);
contextVariables.Set(LinkedInSkill.Parameters.PersonURN, personUpn);
contextVariables.Set(LinkedInSkill.Parameters.ArticleUri, announcementUri);

await sk.RunAsync(contextVariables, sk.Skills.GetNativeFunction("LI", "PostArticle"));