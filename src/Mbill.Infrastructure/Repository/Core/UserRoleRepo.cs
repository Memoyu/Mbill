﻿using FreeSql;
using Mbill.Core.Interface.IRepositories.Core;
using Mbill.Infrastructure.Repository.Base;
using Mbill.Core.Security;
using Mbill.Core.Domains.Entities.Core;

namespace Mbill.Infrastructure.Repository.Core
{
    public class UserRoleRepo : AuditBaseRepo<UserRoleEntity>, IUserRoleRepo
    {
        private readonly ICurrentUser _currentUser;
        public UserRoleRepo(UnitOfWorkManager unitOfWorkManager, ICurrentUser currentUser) : base(unitOfWorkManager, currentUser)
        {
            _currentUser = currentUser;
        }
    }
}
