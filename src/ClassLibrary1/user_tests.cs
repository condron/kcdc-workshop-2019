using System;
using System.Runtime.Serialization;
using Infrastructure;
using Registration.Blueprint.Events;
using Registration.Components.EventWriters;
using Xunit;
// ReSharper disable InconsistentNaming

namespace Registration.Tests
{
    public class user_tests
    {
        [Fact]
        public void can_register_user()
        {
            //given

            //when
            var guestId = Guid.NewGuid();
            var firstName = "mike";
            var lastName = "Jones";
            var email = "mike@jones.com";
            var user = new User(guestId, firstName, lastName, email);

            //then
            var events = ((IEventSource)user).TakeEvents();
            Assert.Collection(
                events,
                (evt) => {
                    var registered = evt as UserRegistered;
                    Assert.NotNull(registered);
                    Assert.Equal(guestId, registered.UserId);
                    Assert.Equal(firstName, registered.FirstName);
                    Assert.Equal(lastName, registered.LastName);
                    Assert.Equal(email, registered.Email);
                });
        }
        [Fact]
        public void can_change_name()
        {
            //given
            var user = (User)FormatterServices.GetUninitializedObject(typeof(User));
            var userRegistered = new UserRegistered(Guid.NewGuid(), "mike", "Jones", "mike@jones.com");
            ((IEventSource)user).Hydrate(new[] { userRegistered });

            //when
            var firstName = "William";
            var lastName = "Jones";
            user.ChangeName(firstName, lastName);

            //then
            var events = ((IEventSource)user).TakeEvents();
            Assert.Collection(
                events,
                (evt) => {
                    var nameChanged = evt as NameChanged;
                    Assert.NotNull(nameChanged);
                    Assert.Equal(userRegistered.UserId, nameChanged.UserId);
                    Assert.Equal(firstName, nameChanged.FirstName);
                    Assert.Equal(lastName, nameChanged.LastName);
                });
        }
    }
}
