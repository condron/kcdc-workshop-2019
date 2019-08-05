using System;

namespace Registration.Blueprint.ReadModels{
    public class UserDisplayName
    {
        public Guid UserId;
        public string DisplayName;
        public UserDisplayName(Guid userId, string displayName)
        {
            UserId = userId;
            DisplayName = displayName;
        }
    }
}