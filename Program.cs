using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using System.Reflection;

//TODO: add your LinkedIn auth token here
//you can create a token at https://www.linkedin.com/developers/tools/oauth
var authToken = "";

//configure your Azure OpenAI backend
var key = "";
var endpoint = "";
var model = "";


var sk = Kernel.Builder.Configure(c => c.AddAzureChatCompletionService(model, endpoint, key, true)).Build();
sk.ImportSkill(new LinkedInSkill(), "LI");
sk.CreateSemanticFunction(Assembly.GetEntryAssembly().LoadEmbeddedResource("sk_announce.skprompt.txt"), 
    "CreateAnnouncement", 
    "Announcer", 
    maxTokens:2048);

//get the LinkedIn PersonUPN
var contextVariables = new ContextVariables();
contextVariables.Set(LinkedInSkill.Parameters.AuthToken, authToken);
var personUpnResult = await sk.RunAsync(contextVariables, sk.Skills.GetFunction("LI", "GetPersonUpn"));
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

var announcementResult = await sk.RunAsync(contextVariables, sk.Skills.GetFunction("Announcer", "CreateAnnouncement"));
var announcementText = announcementResult.Result;

//post the article with commentary
contextVariables = new ContextVariables();
contextVariables.Set(LinkedInSkill.Parameters.AuthToken, authToken);
contextVariables.Set(LinkedInSkill.Parameters.PersonURN, personUpn);
contextVariables.Set(LinkedInSkill.Parameters.ArticleUri, announcementUri);

await sk.RunAsync(contextVariables, sk.Skills.GetFunction("LI", "PostArticle"));


//below is an example of creating content (text + image) and posting it to LI

//var imagePath = "";

//contextVariables = new ContextVariables();
//contextVariables.Set(LinkedInSkill.Parameters.AuthToken, authToken);
//contextVariables.Set(LinkedInSkill.Parameters.PersonURN, personUpn);
//contextVariables.Set(LinkedInSkill.Parameters.ImagePath, imagePath);
//var uploadImageResult = await sk.RunAsync(contextVariables, sk.Skills.GetFunction("LI", "UploadImage"));

////write the LI post which has the reference to the image included
//contextVariables = new ContextVariables("<text for the post goes here>");
//contextVariables.Set(LinkedInSkill.Parameters.AuthToken, authToken);
//contextVariables.Set(LinkedInSkill.Parameters.PersonURN, personUpn);
//contextVariables.Set(LinkedInSkill.Parameters.ImageAsset, uploadImageResult.Result); //the image asset is optional, if it's not included it will just be text

//await sk.RunAsync(contextVariables, sk.Skills.GetFunction("LI", "PostContent"));