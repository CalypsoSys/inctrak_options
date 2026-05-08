using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using IncTrak.Models;
using IncTrak.Data;
using IncTrak.data;
using Microsoft.Extensions.Options;

namespace IncTrak.Controllers
{
    [ApiController]
    public class ParticipantController : IncTrakController
    {
        public ParticipantController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object QueryParticipants(inctrakContext context, LoginRights rights, string searchString, string searchType)
        {
            string part1, part2, part3, part4, part5, part6;
            SearchType st = SearchParts(searchString, searchType, out part1, out part2, out part3, out part4, out part5, out part6);

            IQueryable<Participants> query;
            if (st == SearchType.None || (st == SearchType.InLine && searchString == "_____"))
            {
                if ((searchString == "_____" && searchType == "_____") ||  (st == SearchType.InLine && searchString == "_____"))
                    query = (from p in context.Participants
                             where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                             select p).Take(10);
                else
                    query = from p in context.Participants
                            where 1 == 2 && (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                            select p;
            }
            else if (st == SearchType.Any || st == SearchType.InLine)
            {
                query = from p in context.Participants
                        where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) && 
                        (
                            p.Name.Contains(part1, StringComparison.InvariantCultureIgnoreCase) ||
                            (part2 != null && p.Name.Contains(part2, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part3 != null && p.Name.Contains(part3, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part4 != null && p.Name.Contains(part4, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part5 != null && p.Name.Contains(part5, StringComparison.InvariantCultureIgnoreCase)) ||
                            (part6 != null && p.Name.Contains(part6, StringComparison.InvariantCultureIgnoreCase))
                        )
                        select p;
            }
            else
            {
                query = from p in context.Participants
                        where (p.GroupFk == rights.GroupKey || p.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey)) &&
                        (
                            p.Name.Contains(part1) &&
                            (part2 == null || p.Name.Contains(part2, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part3 == null || p.Name.Contains(part3, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part4 == null || p.Name.Contains(part4, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part5 == null || p.Name.Contains(part5, StringComparison.InvariantCultureIgnoreCase)) &&
                            (part6 == null || p.Name.Contains(part6, StringComparison.InvariantCultureIgnoreCase))
                        )
                        select p;
            }

            if (st == SearchType.InLine)
            {
                return query.ToArray().Select(p => new PARTICIPANT_UI(rights.GroupKeyCheck) { SetFromParticipant = p });
            }
            else
            {
                var existingParticipant = query.ToArray().Select(p => new PARTICIPANT_UI(rights.GroupKeyCheck) { SetFromParticipant = p, TOTAL_GRANTS = p.Grants.Count, GRANTED_SHARES = (Decimal?)p.Grants.Sum(g => g.Shares) ?? 0, HAS_USER = p.UserFk != null });
                if (rights.IsAdmin)
                {
                    var newParticipant = new PARTICIPANT_UI[1] { new PARTICIPANT_UI(rights.GroupKeyCheck, new Participants { ParticipantPk = Guid.Empty, GroupFk = rights.GroupKeyCheck, Name = "<Create New Participant>" }) };
                    return newParticipant.Union(existingParticipant).ToArray();
                }

                return existingParticipant.ToArray();
            }
        }

        [Route("api/company/participants/{searchString}/{searchType}/")]
        public object GetParticipants(string searchString, string searchType)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QueryParticipants(context, rights, searchString, searchType);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get participants");
                return new { success = false, message = message };
            }
        }

        [Route("api/company/participant/{participantKey}")]
        public object GetParticipant(Guid participantKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    Participants participant;
                    if (participantKey == Guid.Empty)
                        participant = new Participants() { GroupFk = rights.GroupKeyCheck };
                    else
                        participant = (from s in context.Participants
                                    where s.ParticipantPk == participantKey && (s.GroupFk == rights.GroupKey || s.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select s).FirstOrDefault();
                    string userName=null, emailAddress=null;
                    if (rights.IsAdmin && participant.UserFk.HasValue )
                    {
                        var user = (from u in context.Users
                                    where u.UserPk == participant.UserFk.Value && u.GroupFk == rights.GroupKey
                                    select u).FirstOrDefault();
                        if ( user != null )
                        {
                            userName = user.UserName;
                            emailAddress = user.EmailAddress;
                        }
                    }

                    var partTypes = (from pt in context.ParticipantTypes
                                         select new PARTICIPANT_TYPES_UI() { PartType = pt }).ToArray();

                    return new { success = true, Participant = new PARTICIPANT_UI(rights.GroupKeyCheck, participant) { USER_NAME = userName, EMAIL_ADDRESS = emailAddress }, PartTypes =partTypes };
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get a participant");
                return new { success = false, message = message };
            }
        }

        [Route("api/company/participant/")]
        [HttpPost]
        public ActionResult SaveParticipant(SaveData<PARTICIPANT_UI> saveParticipant)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." });
                    if (saveParticipant.Key != saveParticipant.Data.PARTICIPANT_PK)
                        return Ok(new { success = false, message = "Invalid participant, keys do not match - thx" });


                    Participants participant;
                    if (saveParticipant.Key == Guid.Empty)
                    {
                        participant = saveParticipant.Data.GetParticipant(rights.GroupKey);
                        context.Participants.Add(participant);
                    }
                    else
                    {
                        participant = (from s in context.Participants
                                             where s.ParticipantPk == saveParticipant.Key && s.GroupFk == rights.GroupKey
                                     select s).FirstOrDefault();
                        if (participant == null)
                            return Ok(new { success = false, message = "Cannot find this participant - thx" });
                        saveParticipant.Data.SetToParticipant(participant, rights.GroupKey);
                    }

                    if (saveParticipant.Data.USER_ACTION == "create_user" || saveParticipant.Data.USER_ACTION == "update_user") {
                        var newUser = (from u in context.Users
                                      where (u.EmailAddress == saveParticipant.Data.EMAIL_ADDRESS || u.UserName == saveParticipant.Data.USER_NAME) &&
                                      u.UserPk != participant.UserFk
                                      select u).Count();
                        if ( newUser > 0 )
                            return Ok(new { success = false, message = "Invalid user, username or email address already exist - thx" });

                        if ( saveParticipant.Data.USER_ACTION == "update_user" )
                        {
                            participant.UserFkNavigation.UserName = saveParticipant.Data.USER_NAME;
                            participant.UserFkNavigation.EmailAddress = saveParticipant.Data.EMAIL_ADDRESS;
                        }
                        else
                        {
                            Users user = new Users();
                            user.Administrator = false;
                            user.EmailAddress = saveParticipant.Data.EMAIL_ADDRESS;
                            user.UserName = saveParticipant.Data.USER_NAME;
                            user.GroupFk = rights.GroupKey;
                            context.Users.Add(user);

                            participant.UserFkNavigation = user;
                        }
                    }
                    else if (saveParticipant.Data.USER_ACTION == "delete_user" && participant.UserFk.HasValue)
                    {
                        if (participant.UserFkNavigation != null)
                            context.Users.Remove(participant.UserFkNavigation);
                        participant.UserFk = null;
                    }

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Participant saved.", key=participant.ParticipantPk });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save participant");
                return Ok(new { success = false, message = message });
            }
        }


        [Route("api/company/participant/{participantKey}")]
        [HttpDelete]
        public object DeleteParticipant(Guid participantKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrakContext context = new OptionsContext(_options.Value))
                {
                    rights = GetLoginUser(context);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (participantKey != Guid.Empty)
                    {
                        Participants participant = (from s in context.Participants
                                             where s.ParticipantPk == participantKey && s.GroupFk == rights.GroupKey
                                             select s).FirstOrDefault();
                        if (participant != null)
                        {
                            context.Participants.Remove(participant);
                            context.SaveChanges();
                            return new { success = true, Participants = QueryParticipants(context, rights, "", "") };
                        }
                    }
                }
                throw new Exception("Cannot find participant");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save participant");
                return new { success = false, message = message }; ;
            }
        }
    }
}
