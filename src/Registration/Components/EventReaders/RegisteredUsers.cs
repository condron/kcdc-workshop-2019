using System;
using System.Collections.Generic;
using Infrastructure;
using Registration.Blueprint.Events;
using Registration.Blueprint.ReadModels;

namespace Registration.Components.EventReaders
{
    internal class RegisteredUsers :
        IReadModel<IRegisteredUsers>,
        IHandle<UserRegistered>,
        IHandle<NameChanged>
    {
        public Tuple<CheckPoint, IRegisteredUsers> GetCurrent(Action<Tuple<CheckPoint, IRegisteredUsers>> target = null)
        {
            throw new NotImplementedException();
        }

        public void SubscribeToChanges(Action<Tuple<CheckPoint, IRegisteredUsers>> target)
        {
            throw new NotImplementedException();
        }
        private object readlock = new object();
        UserDisplayNameDTO[] UserDisplayNames
        {
            get {
                int count;
                Guid[] userIds ;
                lock (readlock) {
                     count = _users.Keys.Count;
                     userIds = new Guid[count];
                    _users.Keys.CopyTo(userIds, 0);
                }
                var users = new UserDisplayNameDTO[count];
                for (int i = 0; i < count; i++) {
                    var user = _users[userIds[i]];
                    users[i] = new UserDisplayNameDTO(userIds[i], user);
                }

                return users;
            }
        }

        private readonly Dictionary<Guid, string> _users = new Dictionary<Guid, string>();

        public void Handle(UserRegistered @event)
        {
            try
            {
                lock (readlock) {
                    _users.Add(@event.UserId, $"{@event.LastName}, {@event.FirstName}");
                }
            }
            catch (Exception)
            {
                //  read models don't throw
            }
        }
        public void Handle(NameChanged @event)
        {
            try
            {
                _users[@event.UserId] = $"{@event.LastName}, {@event.FirstName}";
            }
            catch (Exception)
            {
                //  read models don't throw
            }
        }

        
    }
}