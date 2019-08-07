using System;
using System.Collections.Generic;
using System.Threading;
using EventStore.ClientAPI;
using Infrastructure;
using Registration.Blueprint.Events;
using Registration.Blueprint.ReadModels;

namespace Registration.Components.EventReaders
{
    public class RegisteredUsers :
        Reader<List<UserDisplayName>>
    {
        public RegisteredUsers(
            Func<IEventStoreConnection> conn,
            Func<ResolvedEvent, object> deserializer) : base(conn, deserializer)
        { }
        private void Apply(UserRegistered @event)
        {
            try {
                Model.Add(new UserDisplayName(@event.UserId, $"{@event.LastName}, {@event.FirstName}"));
            }
            catch (Exception) {
                //  read models don't throw
            }
        }
        private void Apply(NameChanged @event)
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