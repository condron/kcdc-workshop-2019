using System;
using Infrastructure;
using Registration.Blueprint.Commands;
using Registration.Components.EventWriters;

namespace Registration.Components.CommandHandlers{
    public class UserSvc :
        IHandleCommand<RegisterUser>,
        IHandleCommand<ChangeName>
    {
        private readonly IRepository _repo;

        public UserSvc(IRepository repo)
        {
            _repo = repo;
        }
        public bool Handle(RegisterUser cmd)
        {
            try
            {
                var user = new User(
                    cmd.UserId,
                    cmd.FirstName,
                    cmd.LastName,
                    cmd.Email);
                _repo.Save(user);
            }
            catch (Exception _)//todo:try harder
            {
                return false;
            }
            return true;
        }
        public bool Handle(ChangeName cmd){
            try{
                var user = _repo.Load<User>(cmd.UserId);
                user.ChangeName(cmd.FirstName,cmd.LastName);
                _repo.Save(user);
            }
            catch (Exception _)//todo:try harder
            {
                return false;
            }
            return true;
        }
       
       

       
    }
}