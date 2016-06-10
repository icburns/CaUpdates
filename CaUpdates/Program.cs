using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using CaUpdates.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CaUpdates
{
    class Program
    {
        static void Main(string[] args)
        {
            const string stateResultsUrl = "http://api.sos.ca.gov/api/president/party/democratic?format=json";
            DateTime pulse = DateTime.Now;
            StateResultsModel stateResults = null;
            CandidateModel bernieResult = null;
            CandidateModel clintonResult = null;

            Console.WriteLine("Querying for updated results at: " + pulse);

            stateResults = GetStateResults(stateResultsUrl);

            bernieResult = stateResults.Candidates.Single(c => c.Name == "Bernie Sanders");
            clintonResult = stateResults.Candidates.Single(c => c.Name == "Hillary Clinton");

            int bernieVoteDiff = (bernieResult.Votes);
            int clintonVoteDiff = (clintonResult.Votes);
            Console.WriteLine("New votes found!");
            Console.WriteLine("Bernie +" + bernieVoteDiff);
            Console.WriteLine("Clinton +" + clintonVoteDiff);
            Console.WriteLine("Distribution of added votes:");
            Console.WriteLine("B: " + (double) bernieVoteDiff/(bernieVoteDiff + clintonVoteDiff));
            Console.WriteLine("C: " + (double) clintonVoteDiff/(bernieVoteDiff + clintonVoteDiff));

            LogChanges(stateResults, bernieResult, clintonResult);

            while (true)
            {
                pulse = DateTime.Now;

                Console.WriteLine("Querying for updated results at: " + pulse);

                StateResultsModel newResults = GetStateResults(stateResultsUrl);

                CandidateModel newBernieResult = newResults.Candidates.Single(c => c.Name == "Bernie Sanders");
                CandidateModel newClintonResult = newResults.Candidates.Single(c => c.Name == "Hillary Clinton");


                if (bernieResult.Votes != newBernieResult.Votes || clintonResult.Votes != newClintonResult.Votes)
                {
                    bernieVoteDiff = (newBernieResult.Votes - bernieResult.Votes);
                    clintonVoteDiff = (newClintonResult.Votes - clintonResult.Votes);
                    Console.WriteLine("New votes found!");
                    Console.WriteLine("Bernie +" + bernieVoteDiff);
                    Console.WriteLine("Clinton +" + clintonVoteDiff);
                    Console.WriteLine("Distribution of added votes:");
                    Console.WriteLine("B: " + (double) bernieVoteDiff/(bernieVoteDiff + clintonVoteDiff));
                    Console.WriteLine("C: " + (double) clintonVoteDiff/(bernieVoteDiff + clintonVoteDiff));

                    stateResults = newResults;
                    bernieResult = newBernieResult;
                    clintonResult = newClintonResult;

                    LogChanges(stateResults, bernieResult, clintonResult);
                }


                Thread.Sleep(300000);
            }
        }


        public static void LogChanges(StateResultsModel stateResults, CandidateModel bernieResult,
            CandidateModel clintonResult)
        {
            using (
                SqlConnection sqlCon =
                    new SqlConnection(
                        System.Configuration.ConfigurationManager.ConnectionStrings["local"].ConnectionString))
            {
                sqlCon.Open();

                using (
                    SqlCommand sqlCmd1 = new SqlCommand
                    {
                        CommandText =
                            "INSERT INTO dbo.[CaStateResults] ([BernieVotes], [ClintonVotes], [UpdatedAt]) VALUES (@BernieVotes, @ClintonVotes, @UpdatedAt)",
                        Connection = sqlCon
                    })
                {
                    sqlCmd1.Parameters.AddWithValue("@BernieVotes", bernieResult.Votes);
                    sqlCmd1.Parameters.AddWithValue("@ClintonVotes", clintonResult.Votes);
                    sqlCmd1.Parameters.AddWithValue("@UpdatedAt", stateResults.UpdatedAt);
                    sqlCmd1.ExecuteNonQuery();
                }

                sqlCon.Close();
            }

            Console.WriteLine("New votes recorded");
        }

        public static StateResultsModel GetStateResults(string url)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.ContentType = "application/json";
            request.KeepAlive = true;
            request.UserAgent =
                "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.8) Gecko/2009021910 Firefox/3.0.7 (.NET CLR 3.5.30729)";
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StateResultsModel results = null;
            JsonSerializer serializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            if (responseStream != null)
            {
                StreamReader reader = new StreamReader(responseStream);
                JsonTextReader jReader = new JsonTextReader(reader);

                results = serializer.Deserialize<StateResultsModel>(jReader);
                //"June 9, 2016, 10:32 a.m."
                DateTime updatedAt;
                bool couldParseTimestamp =
                    DateTime.TryParseExact(results.Timestamp.Replace("a.m.", "AM").Replace("p.m.", "PM"),
                        "MMMM d, yyyy, H:mm tt",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out updatedAt);
                if (!couldParseTimestamp)
                {
                    updatedAt = DateTime.Now;
                }
                results.UpdatedAt = updatedAt;
            }

            return results;
        }
    }
}