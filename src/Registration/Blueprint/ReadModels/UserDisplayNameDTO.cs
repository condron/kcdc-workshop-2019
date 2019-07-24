using System;

namespace Registration.Blueprint.ReadModels{
    public class UserDisplayNameDTO
    {
        public Guid UserId;
        public string DisplayName;
        public UserDisplayNameDTO(Guid userId, string displayName)
        {
            UserId = userId;
            DisplayName = displayName;
        }
    }
}