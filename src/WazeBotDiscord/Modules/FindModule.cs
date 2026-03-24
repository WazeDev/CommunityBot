using Discord.Interactions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WazeBotDiscord.Find;

namespace WazeBotDiscord.Modules
{
    [Group("find", "Waze map find commands")]
    public class FindModule : InteractionModuleBase<SocketInteractionContext>
    {
        readonly FindService _findSvc;

        public FindModule(FindService findSvc)
        {
            _findSvc = findSvc;
        }

        [SlashCommand("segment", "Find a Waze segment by ID")]
        public async Task FindSegment([Summary("id", "The segment ID")] string id)
        {
            Regex regURL = new Regex(@"^\d{1,10}");
            if (regURL.Matches(id).Count != 1)
            {
                await RespondAsync("Incorrect segment ID format.", ephemeral: true);
                return;
            }

            await DeferAsync();
            var reply = new StringBuilder();

            var urlNA = "https://www.waze.com/Descartes/app/HouseNumbers?ids=" + id.Trim();
            var result = await _findSvc.GetWebRequest(urlNA, "application/json; charset=utf-8");
            var hnResult = JsonConvert.DeserializeObject<HouseNumberResult>(result);
            if (hnResult.editAreas.objects.Count > 0)
            {
                var centroid = _findSvc.GetCentroid(hnResult.editAreas.objects[0].geometry.coordinates[0]);
                reply.AppendLine($"<https://www.waze.com/en-US/editor/?env=usa&lon={centroid.x}&lat={centroid.y}&zoom=6&segments={id}>");
            }

            var urlROW = "https://www.waze.com/row-Descartes/app/HouseNumbers?ids=" + id.Trim();
            result = await _findSvc.GetWebRequest(urlROW, "application/json; charset=utf-8");
            hnResult = JsonConvert.DeserializeObject<HouseNumberResult>(result);
            if (hnResult.editAreas.objects.Count > 0)
            {
                var centroid = _findSvc.GetCentroid(hnResult.editAreas.objects[0].geometry.coordinates[0]);
                reply.AppendLine($"<https://www.waze.com/editor/?env=row&lon={centroid.x}&lat={centroid.y}&zoom=6&segments={id}>");
            }

            var urlIL = "https://www.waze.com/il-Descartes/app/HouseNumbers?ids=" + id.Trim();
            result = await _findSvc.GetWebRequest(urlIL, "application/json; charset=utf-8");
            hnResult = JsonConvert.DeserializeObject<HouseNumberResult>(result);
            if (hnResult.editAreas.objects.Count > 0)
            {
                var centroid = _findSvc.GetCentroid(hnResult.editAreas.objects[0].geometry.coordinates[0]);
                reply.AppendLine($"<https://www.waze.com/editor/?env=il&lon={centroid.x}&lat={centroid.y}&zoom=6&segments={id}>");
            }

            await FollowupAsync(reply.Length > 0 ? reply.ToString() : "Segment not found.");
        }

        [SlashCommand("place", "Find a Waze place by ID")]
        public async Task FindPlace([Summary("id", "The place ID")] string placeId)
        {
            Regex regURL = new Regex(@"^\d*\.\d*.\d*");
            if (regURL.Matches(placeId).Count != 1)
            {
                await RespondAsync("Incorrect Place ID format.", ephemeral: true);
                return;
            }

            await DeferAsync();
            var reply = new StringBuilder();

            var urls = new[]
            {
                ("https://www.waze.com/SearchServer/mozi?max_distance_kms=&lon=-84.22637&lat=39.61097&format=PROTO_JSON_FULL&venue_id=" + placeId.Trim(), "usa"),
                ("https://www.waze.com/row-SearchServer/mozi?max_distance_kms=&lon=-84.22637&lat=39.61097&format=PROTO_JSON_FULL&venue_id=" + placeId.Trim(), "row"),
                ("https://www.waze.com/il-SearchServer/mozi?max_distance_kms=&lon=-84.22637&lat=39.61097&format=PROTO_JSON_FULL&venue_id=" + placeId.Trim(), "il")
            };

            foreach (var (url, env) in urls)
            {
                try
                {
                    var result = await _findSvc.GetWebRequest(url, "application/json; charset=utf-8");
                    var placeResult = JsonConvert.DeserializeObject<PlaceResponse>(result);
                    if (placeResult.venue != null)
                    {
                        var centroid = new GeoPoint(placeResult.venue.location.x, placeResult.venue.location.y);
                        var editorBase = env == "usa" ? "https://www.waze.com/en-US/editor" : "https://www.waze.com/editor";
                        reply.AppendLine($"<{editorBase}/?env={env}&lon={centroid.x}&lat={centroid.y}&zoom=6&venues={placeId}>");
                    }
                }
                catch { }
            }

            await FollowupAsync(reply.Length > 0 ? reply.ToString() : "Place not found.");
        }

        #region Segment Helper Classes

        public class HouseNumberResult
        {
            public SegmentHouseNumber segmentHouseNumbers { get; set; }
            public EditAreas editAreas { get; set; }
            public Users users { get; set; }
        }

        public class SegmentHouseNumber
        {
            public List<HouseNumber> objects { get; set; }
        }

        public class EditAreas
        {
            public List<EditArea> objects { get; set; }
        }

        public class Users
        {
            public List<User> objects { get; set; }
        }

        public class EditArea
        {
            public int id { get; set; }
            public EditAreaGeometry geometry { get; set; }
        }

        public class HouseNumber
        {
            public string id { get; set; }
            public string number { get; set; }
            public int side { get; set; }
            public Geometry geometry { get; set; }
            public FractionPoint fractionPoint { get; set; }
            public bool valid { get; set; }
            public bool forced { get; set; }
            public string segID { get; set; }
        }

        public class Geometry
        {
            public string type { get; set; }
            public List<double> coordinates { get; set; }
        }

        public class EditAreaGeometry
        {
            public string type { get; set; }
            public List<List<List<double>>> coordinates { get; set; }
        }

        public class FractionPoint
        {
            public string type { get; set; }
            public List<double> coordinates { get; set; }
        }

        public class User
        {
            public int id { get; set; }
            public string userName { get; set; }
            public int rank { get; set; }
        }
        #endregion

        #region Place Helper Classes
        public class PlaceResponse
        {
            public string id { get; set; }
            public Venue venue { get; set; }
            public string info_url { get; set; }
            public bool info_url_append_client_data { get; set; }
            public string provider { get; set; }
            public bool updateable { get; set; }
        }

        public class Venue
        {
            public InternalVenueID internal_venue_id { get; set; }
            public List<string> categories { get; set; }
            public string name { get; set; }
            public Point location { get; set; }
            public List<Point> polygon { get; set; }
            public string house_number { get; set; }
            public string street { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string country { get; set; }
            public int cityId { get; set; }
            public int street_id { get; set; }
            public List<ExternalProvider> external_providers { get; set; }
            public double area { get; set; }
            public List<Image> images { get; set; }
            public List<EntryExitPoint> entry_exit_points { get; set; }
            public ulong creation_date { get; set; }
            public ulong created_by { get; set; }
            public ulong last_update_date { get; set; }
            public List<ulong> last_update_by { get; set; }
            public int lock_level { get; set; }
            public bool approved { get; set; }
            public bool residential { get; set; }
            public string currency { get; set; }
            public string venue_id { get; set; }
            public int country_id { get; set; }
            public EditorInfo created_by_info { get; set; }
            public EditorInfo last_updated_by_info { get; set; }
            public bool has_more_data { get; set; }
        }
        
        public class EntryExitPoint
        {
            public Point point { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public bool primary { get; set; }
        }

        public class Point
        {
            public double x { get; set; }
            public double y { get; set; }
        }

        public class Image
        {
            public EditorInfo created_by_info { get; set; }
            public bool by_me { get; set; }
            public string id { get; set; }
            public ulong date { get; set; }
            public Point location { get; set; }
            public int creatorUserId { get; set; }
            public bool street { get; set; }
            public bool approved { get; set; }
            public int likes { get; set; }
            public bool liked { get; set; }
        }

        public class ExternalProvider
        {
            public string provider { get; set; }
            public string id { get; set; }
        }

        public class InternalVenueID
        {
            public int t10 { get; set; }
            public int t1 { get; set; }
            public string id { get; set; }
        }
        
        public class EditorInfo
        {
            public ulong id { get; set; }
            public string name { get; set; }
            public int rank { get; set; }
            public int points { get; set; }
            public int mood { get; set; }
            public bool is_staff { get; set; }
            public bool is_registered { get; set; }
            public bool is_ad_operator { get; set; }
            public bool is_system_trusted { get; set; }
        }
        #endregion
    }

}
