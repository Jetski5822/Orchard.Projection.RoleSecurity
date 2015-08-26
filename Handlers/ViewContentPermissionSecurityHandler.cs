using System.Collections.Generic;
using Orchard.Projection.RoleSecurity.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Projection.RoleSecurity.Handlers {
    public class ViewContentPermissionSecurityHandler : ContentHandler {
        private readonly IRepository<RoleSecurityRecord> _roleSecurityRepository;
        private readonly IRepository<PermissionRecord> _permissionRepository;
        private readonly IRoleService _roleService;
        private readonly IAuthorizationService _authorizationService;

        private readonly Permission _permission = Orchard.Core.Contents.Permissions.ViewContent;

        public ViewContentPermissionSecurityHandler(IRepository<RoleSecurityRecord> roleSecurityRepository,
            IRepository<PermissionRecord> permissionRepository,
            IRoleService roleService,
            IAuthorizationService authorizationService) {
            _roleSecurityRepository = roleSecurityRepository;
            _permissionRepository = permissionRepository;
            _roleService = roleService;
            _authorizationService = authorizationService;

            OnPublished<ContentItem>(UpdateContentItemRoleSecurityRecords);
        }

        private void UpdateContentItemRoleSecurityRecords(PublishContentContext context, ContentItem contentItem) {
            var roles = _roleService.GetRoles();

            var allowedRoles = new List<RoleRecord>();

            bool canAnonView = _authorizationService.TryCheckAccess(
                _permission, 
                null /* null signifies anon user */,
                contentItem);

            foreach (var role in roles) {
                var user = UserSimulation.Create(role.Name);

                if (_authorizationService.TryCheckAccess(
                        _permission, 
                        user,
                        contentItem)
                    ) {

                    allowedRoles.Add(role);
                }
            }

            /* Delete and maintain a fresh one on each publish */
            var currentRecord = _roleSecurityRepository
                .Get(o => o.ContentItemRecord == contentItem.Record);

            if (currentRecord != null)
                _roleSecurityRepository.Delete(currentRecord);

            PermissionRecord permissionRecord = _permissionRepository
                .Get(x => x.Name == _permission.Name);

            var entity = new RoleSecurityRecord{
                ContentItemRecord = contentItem.Record,
                AnonymousCanView = canAnonView,
                Permission = permissionRecord};

            _roleSecurityRepository.Create(entity);

            foreach (var allowedRole in allowedRoles) {
                entity.RolesSecurities.Add(
                    new RolesSecuritiesRecord{
                        Role = allowedRole,
                        RoleSecurity = entity });
            }
        }
    }

}