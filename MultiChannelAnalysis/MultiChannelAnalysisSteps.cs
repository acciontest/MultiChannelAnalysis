using System;
using System.Collections.Generic;
using AlteryxGalleryAPIWrapper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace MultiChannelAnalysis
{
    [Binding]
    public class MultiChannelAnalysisSteps
    {

        private string alteryxurl;
        private string _sessionid;
        private string _appid;
        private string _userid;
        private string _appName;
        private string jobid;
        private string outputid;
        private string validationId;
        private string _appActualName;
       

        private Client Obj = new Client("https://gallery.alteryx.com/api");


        private RootObject jsString = new RootObject();
       
        [Given(@"alteryx running at""(.*)""")]
        public void GivenAlteryxRunningAt(string SUT_url)
        {
            alteryxurl = Environment.GetEnvironmentVariable(SUT_url);
        }

        [Given(@"I am logged in using ""(.*)"" and ""(.*)""")]
        public void GivenIAmLoggedInUsingAnd(string user, string password)
        {
            _sessionid = Obj.Authenticate(user, password).sessionId;
        }
        
        [When(@"I run the application """"(.*)"""" with the all the Customer Segment and Product Categories and Market Area """"(.*)""""")]
        public void WhenIRunTheApplicationWithTheAllTheCustomerSegmentAndProductCategoriesAndMarketArea(string app, string p1)
        {
            //url + "/apps/gallery/?search=" + appName + "&limit=20&offset=0"
            //Search for App & Get AppId & userId 
            string response = Obj.SearchAppsGallery(app);
            var appresponse =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    response);
            int count = appresponse["recordCount"];
            if (count == 1)
            {
                _appid = appresponse["records"][0]["id"];
                _userid = appresponse["records"][0]["owner"]["id"];
                _appName = appresponse["records"][0]["primaryApplication"]["fileName"];
            }
            else
            {
                for (int i = 0; i <= count - 1; i++)
                {

                    _appActualName = appresponse["records"][i]["primaryApplication"]["metaInfo"]["name"];
                    if (_appActualName == app)
                    {
                        _appid = appresponse["records"][i]["id"];
                        _userid = appresponse["records"][i]["owner"]["id"];
                        _appName = appresponse["records"][i]["primaryApplication"]["fileName"];
                        break;
                    }
                }

            }

            jsString.appPackage.id = _appid;
            jsString.userId = _userid;
            jsString.appName = _appName;

            //url +"/apps/" + appPackageId + "/interface/
            //Get the app interface - not required
            string appinterface = Obj.GetAppInterface(_appid);
            dynamic interfaceresp = JsonConvert.DeserializeObject(appinterface);

            //Construct the payload to be posted.
            //List<JsonPayload.Question> questionAnsls = new List<JsonPayload.Question>();
            //questionAnsls.Add(new JsonPayload.Question("CustomerSegment", "true"));
            //questionAnsls.Add(new JsonPayload.Question("Drive Time Selection", "false"));
            //jsString.questions.AddRange(questionAnsls);

            var cs = new List<JsonPayload.datac>();
            cs.Add(new JsonPayload.datac() { key = "Consumer", value = "true" });
            cs.Add(new JsonPayload.datac() { key = "Corporate", value = "true" });
            cs.Add(new JsonPayload.datac() { key = "Home Office", value = "true" });
            cs.Add(new JsonPayload.datac() { key = "Small Business", value = "true" });
            string csegment = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(cs);


            var pc = new List<JsonPayload.datac>();
            pc.Add(new JsonPayload.datac() { key = "Furniture", value = "true" });
            pc.Add(new JsonPayload.datac() { key = "Office Supplies", value = "true" });
            pc.Add(new JsonPayload.datac() { key = "Technology", value = "true" });
            string prodcategory = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(pc);
            
            var loc = new List<JsonPayload.datac>();
            loc.Add(new JsonPayload.datac() { key = "Aurora", value = "true" });
            loc.Add(new JsonPayload.datac() { key = "Broomfield", value = "false" });
            loc.Add(new JsonPayload.datac() { key = "Arvada", value = "false" });
            loc.Add(new JsonPayload.datac() { key = "Denver", value = "false" });
            string loca = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(loc);

            for (int i = 0; i < 3; i++)
            {

                if (i == 0)
                {
                    JsonPayload.Question questionAns = new JsonPayload.Question();
                    questionAns.name = "CustomerSegment";
                    questionAns.answer = csegment;
                    jsString.questions.Add(questionAns);
                }
                else if (i == 1)
                {
                    JsonPayload.Question questionAns = new JsonPayload.Question();
                    questionAns.name = "ProductCategory";
                    questionAns.answer = prodcategory;
                    jsString.questions.Add(questionAns);
                }
                else if (i == 2)
                {
                    JsonPayload.Question questionAns = new JsonPayload.Question();
                    questionAns.name = "Location";
                    questionAns.answer = loca;
                    jsString.questions.Add(questionAns);
                }
            }
            jsString.jobName = "Job Name";

            // Make Call to run app

            var postData = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(jsString);
            string postdata = postData.ToString();
            string resjobqueue = Obj.QueueJob(postdata);

            var jobqueue =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    resjobqueue);
            jobid = jobqueue["id"];

            //Get the job status

            string status = "";
            while (status != "Completed")
            {
                string jobstatusresp = Obj.GetJobStatus(jobid);
                var statusresp =
                    new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                        jobstatusresp);
                status = statusresp["status"];
            }


        }
        
        [Then(@"I see output ""(.*)""")]
        public void ThenISeeOutput(string result)
        {
            //url + "/apps/jobs/" + jobId + "/output/"
            string getmetadata = Obj.GetOutputMetadata(jobid);
            dynamic metadataresp = JsonConvert.DeserializeObject(getmetadata);

            // outputid = metadataresp[0]["id"];
            int count = metadataresp.Count;
            for (int j = 0; j <= count - 1; j++)
            {
                outputid = metadataresp[j]["id"];
            }

            string getjoboutput = Obj.GetJobOutput(jobid, outputid, "html");
            string htmlresponse = getjoboutput;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlresponse);
            string output = doc.DocumentNode.SelectSingleNode("//div[@class='DefaultText']").InnerHtml;
            StringAssert.Contains(result,output);
           
        }
    }
}
