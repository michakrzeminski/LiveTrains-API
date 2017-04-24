using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiveTrains
{
    class Program
    {
        //local database connection
        static SqlConnection conn;
        static HttpClient client;
        static string apikey = "382ec2fc-692c-4ef1-ad39-893065d6fad8";
        static Dictionary<string, List<string>> tramDict = new Dictionary<string, List<string>>();
        private static void databaseConnect()
        {
            //connection
            string connStr = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;";
            string path = AppDomain.CurrentDomain.BaseDirectory;
            //remove \bin\Debug from path
            path = path.Remove(path.IndexOf("bin"), 10);
            connStr += @"AttachDbFilename=" + path + "TrainsDatabase.mdf" + ";";
            conn = new SqlConnection(connStr);
            conn.Open();
        }
        static void databaseDisconnect()
        {
            //disconnect 
            conn.Close();
            conn.Dispose();
        }

        //GET request to API returning online trams in JSON
        private static async Task GetTramsOnlineOld()
        {
            Console.WriteLine("GetTramOnlineOld");
            string uri = "https://api.um.warszawa.pl/api/action/wsstore_get/?";
            string resource_id = "c7238cfe-8b1f-4c38-bb4a-de386db7e776";
            uri += "id=" + resource_id;
            uri += "&";
            uri += "apikey=" + apikey;

            try
            {
                string responseBody = await client.GetStringAsync(uri);
                var json = JObject.Parse(responseBody);
                var jsonDeserialized = JsonConvert.DeserializeObject(responseBody);

                foreach (var item in json["result"])
                {
                    //one record to database
                    Console.WriteLine(item);

                    var comm = "INSERT INTO " + "TrainsOld";
                    comm += " VALUES (";
                    foreach (var child in item.Children())
                    {
                        Console.WriteLine(child.First);
                        comm += "'" + child.First + "',";
                    }
                    comm = comm.Remove(comm.Length - 1);
                    comm += ")";

                    Console.WriteLine(comm);
                    SqlCommand command = new SqlCommand(comm, conn);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        int statusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        private static async Task GetTramsOnline()
        {
            Console.WriteLine("GetTramsOnline");
            string uri = "https://api.um.warszawa.pl/api/action/busestrams_get/?";
            string resource_id = "f2e5503e- 927d-4ad3-9500-4ab9e55deb59";
            uri += "resource_id=" + resource_id;
            uri += "&";
            uri += "type=2";
            uri += "&";
            uri += "apikey=" + apikey;

            try
            {
                string responseBody = await client.GetStringAsync(uri);
                var json = JObject.Parse(responseBody);
                var jsonDeserialized = JsonConvert.DeserializeObject(responseBody);

                var onlineComm = "TRUNCATE TABLE Trains; INSERT INTO " + "Trains";
                var historyComm = "INSERT INTO " + "TrainsHistory";
                var comm = " VALUES ";
                foreach (var item in json["result"])
                {
                    //one record to database
                    comm += "(";

                    foreach (var child in item.Children())
                    {
                        Console.WriteLine(child.First);
                        comm += "'" + child.First + "',";
                    }
                    //removing last comma
                    comm = comm.Remove(comm.Length - 1);
                    comm += "), ";
                }
                //removing last comma
                comm = comm.Remove(comm.Length - 2);
                historyComm += comm;
                onlineComm += comm;

                SqlCommand command = new SqlCommand(onlineComm, conn);
                SqlCommand command2 = new SqlCommand(historyComm, conn);
                try
                {
                    command.ExecuteNonQuery();
                    command2.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("TrainsOnline exception");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }


        private static async Task getTramStops()
        {
            Console.WriteLine("GetTramStops");
            string uri = "https://api.um.warszawa.pl/api/action/dbstore_get/?";
            string resource_id = "ab75c33d-3a26-4342-b36a-6e5fef0a3ac3";
            uri += "id=" + resource_id;
            uri += "&";
            uri += "apikey=" + apikey;
            try
            {
                string responseBody = await client.GetStringAsync(uri);
                var json = JObject.Parse(responseBody);
                var jsonDeserialized = JsonConvert.DeserializeObject(responseBody);

                foreach (var item in json["result"])
                {
                    //one record to database
                    var comm = "INSERT INTO " + "Stops";
                    comm += " VALUES (";
                    foreach (var field in item["values"])
                    {
                        if(field["key"].ToString() == "zespol")
                        {
                            comm += "'" + field["value"].ToString();
                        }
                        else if(field["key"].ToString() == "slupek")
                        {
                            comm += field["value"].ToString() + "',";
                        }
                        else
                        {
                            comm += "'" + field["value"].ToString() + "',";
                        }
                    }
                    comm = comm.Remove(comm.Length - 1);
                    comm += ")";

                    SqlCommand command = new SqlCommand(comm, conn);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("getTramStops exception");
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        private static async Task getLinesOnStops(string przyst_id)
        {
            Console.WriteLine("GetLinesOnStops: "+przyst_id);
            List<string> linesList = new List<string>();
            string uri = "https://api.um.warszawa.pl/api/action/dbtimetable_get/?";
            string resource_id = "88cd555f-6f31-43ca-9de4-66c479ad5942";
            uri += "id=" + resource_id;
            uri += "&";
            uri += "busstopId=" + przyst_id.Substring(0,4);
            uri += "&";
            uri += "busstopNr=" + przyst_id.Substring(4, 2);
            uri += "&";
            uri += "apikey=" + apikey;
            try
            {
                string responseBody = await client.GetStringAsync(uri);
                var json = JObject.Parse(responseBody);
                var jsonDeserialized = JsonConvert.DeserializeObject(responseBody);

                foreach (var item in json["result"])
                {
                    //one record to database
                    var comm = "INSERT INTO " + "Lines";
                    comm += " (stop_id, no_line)";
                    comm += " VALUES ("+"'"+ przyst_id + "',";
                    foreach (var field in item["values"])
                    {
                        //if value not in range 1-44 that means it is not tram
                        int no_line;
                        if(Int32.TryParse(field["value"].ToString(), out no_line))
                        {
                            if (no_line <= 44)
                            {
                                comm += "'" + field["value"].ToString() + "',";
                                linesList.Add(field["value"].ToString());
                            }
                            else
                            {
                                comm += "'0',";
                            }
                        }
                    }
                    comm = comm.Remove(comm.Length - 1);
                    comm += ")";

                    SqlCommand command = new SqlCommand(comm, conn);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine("getLinesOnStops exception: "+ex);
                    }
                }
                if (!tramDict.ContainsKey(przyst_id) && linesList.Count != 0)
                {
                    tramDict.Add(przyst_id, linesList);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        private static async Task getTimeTable(string przyst_id, string nr_linii)
        {
            Console.WriteLine("GetTimeTable: " + przyst_id + " " + nr_linii);
            string uri = "https://api.um.warszawa.pl/api/action/dbtimetable_get/?";
            string resource_id = "e923fa0e-d96c-43f9-ae6e-60518c9f3238";
            uri += "id=" + resource_id;
            uri += "&";
            uri += "busstopId=" + przyst_id.Substring(0, 4);
            uri += "&";
            uri += "busstopNr=" + przyst_id.Substring(4, 2);
            uri += "&";
            uri += "line=" + nr_linii;
            uri += "&";
            uri += "apikey=" + apikey;
            try
            {
                string responseBody = await client.GetStringAsync(uri);
                var json = JObject.Parse(responseBody);
                var jsonDeserialized = JsonConvert.DeserializeObject(responseBody);

                foreach (var item in json["result"])
                {
                    //one record to database
                    var comm = "INSERT INTO " + "Timetable";
                    comm += " VALUES (" + "'" + przyst_id + "','"+nr_linii+"',";
                    foreach (var field in item["values"])
                    {
                        if (field["key"].ToString() == "brygada" ||
                            field["key"].ToString() == "kierunek" ||
                            field["key"].ToString() == "trasa" ||
                            field["key"].ToString() == "czas")
                        {
                            comm += "'" + field["value"].ToString() + "',";
                        }
                    }
                    comm = comm.Remove(comm.Length - 1);
                    comm += ")";

                    SqlCommand command = new SqlCommand(comm, conn);
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("getTimeTable exception: "+ ex);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        private static List<string> getStopIds()
        {
            List<string> ids = new List<string>();
            var comm = "SELECT stop_id FROM Stops";
            SqlCommand command = new SqlCommand(comm, conn);

            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ids.Add(reader.GetString(0));
                }
            }
            reader.Close();
            return ids;
        }

        private static void timer_tick(object source, ElapsedEventArgs e)
        {
            GetTramsOnline().Wait();
        }

        private static void clearDatabase()
        {
            var comm = "DELETE FROM Stops; DELETE FROM Lines; DELETE FROM Timetable";
            SqlCommand command = new SqlCommand(comm, conn);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        static void init()
        {
            //clear database before insertions
            clearDatabase();

            getTramStops().Wait();
            List<string> przyst_ids = new List<string>();

            przyst_ids = getStopIds();

            foreach(var przyst_id in przyst_ids)
            {
                getLinesOnStops(przyst_id).Wait();
            }

            //TODO method to drop not tram stops

            foreach(var przyst in tramDict)
            {
                for(int i = 0; i < przyst.Value.Count; ++i)
                {
                    getTimeTable(przyst.Key,przyst.Value[i]).Wait();
                }
            }
        }
        static void Main(string[] args)
        {
            databaseConnect();
            client = new HttpClient();

            //init();

            System.Timers.Timer timer;
            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(timer_tick);
            timer.Interval = 10000;
            timer.Enabled = true;
            timer.Start();

            Console.ReadLine();
            databaseDisconnect();
            client.Dispose();
        }
    }
}
