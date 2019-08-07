using System;
using Registration.Blueprint.Events;
using Registration.Components.EventReaders;
using Xunit;
// ReSharper disable InconsistentNaming

namespace Registration.Tests
{
    public class registered_users_tests
    {
        [Fact]
        public void can_list_users()
        {
            //given
            var user1 = new UserRegistered(Guid.NewGuid(), "Mike", "smith", "mike@smithtown.com");
            var user2 = new UserRegistered(Guid.NewGuid(), "Robert", "Jones", "Robert@jonestown.com");
            var user3 = new UserRegistered(Guid.NewGuid(), "Ben", "smith", "Ben@smithtown.com");
            
            var usersRM = new RegisteredUsers(() => null, null);

            //when
            usersRM.Apply(user1);
            usersRM.Apply(user2);
            usersRM.Apply(user3);
         
            //then
            var displayNames = usersRM.Current;
            Assert.Collection(
                displayNames,
                displayName => {
                    Assert.Equal(user1.UserId, displayName.UserId);
                    Assert.Equal($"{user1.LastName}, {user1.FirstName}", displayName.DisplayName);}, 
                displayName => {
                    Assert.Equal(user2.UserId, displayName.UserId);
                    Assert.Equal($"{user2.LastName}, {user2.FirstName}", displayName.DisplayName);}, 
                displayName => {
                    Assert.Equal(user3.UserId, displayName.UserId);
                    Assert.Equal($"{user3.LastName}, {user3.FirstName}", displayName.DisplayName);
                });

        }        [Fact]
        public void can_apply_name_changes()
        {
            //given
            var user1 = new UserRegistered(Guid.NewGuid(), "Mike", "smith", "mike@smithtown.com");
            var user2 = new UserRegistered(Guid.NewGuid(), "Robert", "Jones", "Robert@jonestown.com");
            var user3 = new UserRegistered(Guid.NewGuid(), "Ben", "smith", "Ben@smithtown.com");
            var nameChange = new NameChanged(user2.UserId, "John", "Doe");
            var usersRM = new RegisteredUsers(() => null, null);

            //when
            usersRM.Apply(user1);
            usersRM.Apply(user2);
            usersRM.Apply(user3);
            usersRM.Apply(nameChange);

            //then
            var displayNames = usersRM.Current;
            Assert.Collection(
                displayNames,
                displayName => {
                    Assert.Equal(user1.UserId, displayName.UserId);
                    Assert.Equal($"{user1.LastName}, {user1.FirstName}", displayName.DisplayName);}, 
                displayName => {
                    Assert.Equal(user2.UserId, displayName.UserId);
                    Assert.Equal($"{nameChange.LastName}, {nameChange.FirstName}", displayName.DisplayName);}, 
                displayName => {
                    Assert.Equal(user3.UserId, displayName.UserId);
                    Assert.Equal($"{user3.LastName}, {user3.FirstName}", displayName.DisplayName);
                });

        }


    }
}
