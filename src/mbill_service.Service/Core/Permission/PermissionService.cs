﻿using mbill_service.Core.Domains.Entities.Core;
using mbill_service.Core.Interface.IRepositories.Core;
using mbill_service.Service.Base;
using mbill_service.Service.Core.Permission.Input;
using mbill_service.Service.Core.Permission.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mbill_service.Service.Core.Permission
{
    public class PermissionService : ApplicationService, IPermissionService
    {
        private readonly IPermissionRepo _permissionRepo;
        private readonly IRolePermissionRepo _rolePermissionRepo;

        public PermissionService(IPermissionRepo permissionRepo, IRolePermissionRepo rolePermissionRepo)
        {
            _permissionRepo = permissionRepo;
            _rolePermissionRepo = rolePermissionRepo;
        }

        public async Task<List<TreePermissionDto>> GetAllTreeAsync()
        {
            var permissions = await _permissionRepo.Select.ToListAsync();
            int index = 1;
            List<TreePermissionDto> treePermissionDtos = permissions.GroupBy(r => r.Module).Select(r =>
                      new TreePermissionDto
                      {
                          Rowkey = index++.ToString(),
                          Children = permissions.Where(u => u.Module == r.Key)
                                                .Select(r => new TreePermissionDto
                                                {
                                                    Rowkey = index++.ToString(),
                                                    Name = r.Name,
                                                    Router = r.Router,
                                                    CreateTime = r.CreateTime
                                                })
                                                .ToList(),
                          Name = r.Key,
                      }).ToList();
            return treePermissionDtos;
        }

        public async Task<IDictionary<string, IEnumerable<PermissionDto>>> GetAllStructualAsync()
        {
            return (await _permissionRepo.Select.ToListAsync())
                   .GroupBy(r => r.Module)
                   .ToDictionary(
                       group => group.Key,
                       group =>
                           Mapper.Map<IEnumerable<PermissionDto>>(group.ToList())
                   );
        }

        public async Task<bool> CheckAsync(string permission)
        {
            long[] roleIds = CurrentUser.Roles;
            PermissionEntity permissionEntity = await _permissionRepo.Where(r => r.Name == permission).FirstAsync();
            bool existPermission = await _rolePermissionRepo.Select
                .AnyAsync(r => roleIds.Contains(r.RoleId) && r.PermissionId == permissionEntity.Id);
            return existPermission;
        }
    }
}
