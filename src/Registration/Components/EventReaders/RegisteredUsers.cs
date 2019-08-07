using System;
using System.Collections.Generic;
using System.Threading;
using EventStore.ClientAPI;
using Infrastructure;
using Registration.Blueprint.Events;
using Registration.Blueprint.ReadModels;

namespace Registration.Components.EventReaders
{
    internal class RegisteredUsers :
        Reader<List<UserDisplayName>>
    {
        public RegisteredUsers(
            IEventStoreConnection conn,
            Func<ResolvedEvent, object> deserializer) : base(conn, deserializer)
        { }
        public void Apply(UserRegistered @event)
        {
            try {
                Model.Add(new UserDisplayName(@event.UserId, $"{@event.LastName}, {@event.FirstName}"));
            }
            catch (Exception) {
                //  read models don't throw
            }
        }
        public void Apply(NameChanged @event)
        {
            try {
                foreach (var user in Model) {
                    if (user.UserId == @event.UserId) {
                        user.DisplayName = $"{@event.LastName}, {@event.FirstName}";
                        break;
                    }
                }
            }
            catch (Exception) {
                //  read models don't throw
            }
        }


    }
}