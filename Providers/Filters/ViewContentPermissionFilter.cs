using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.ProjectionFilters.Filters;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;

namespace Orchard.Projection.RoleSecurity.Providers.Filters {
    public class ViewContentPermissionFilter : IFilterProvider {
        private readonly IAuthenticationService _authenticationService;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRoleService _roleService;

        public ViewContentPermissionFilter(IAuthenticationService authenticationService,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IWorkContextAccessor workContextAccessor,
            IRoleService roleService) {
            _authenticationService = authenticationService;
            _userRolesRepository = userRolesRepository;
            _workContextAccessor = workContextAccessor;
            _roleService = roleService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("ViewContentPermissionFilter", T("ViewContentPermissionFilter"), T("ViewContentPermissionFilter"))
                .Element("ViewContentPermissionFilter", T("View Content Permission Filter"), T("Filter by content by role, based on if the user has the View Content Permission."),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter
                );
        }

        public void ApplyFilter(dynamic context) {
            var user = _authenticationService.GetAuthenticatedUser();

            if (user != null) {
                if (!String.IsNullOrEmpty(_workContextAccessor.GetContext().CurrentSite.SuperUser) &&
                    String.Equals(user.UserName, _workContextAccessor.GetContext().CurrentSite.SuperUser,
                        StringComparison.Ordinal)) {
                    return;
                }
            }

            var queryManager = (DefaultHqlQuery)context.Query;

            var fiJoins = typeof(DefaultHqlQuery).GetField("_joins", BindingFlags.Instance | BindingFlags.NonPublic);
            var joins = fiJoins.GetValue(queryManager) as List<Tuple<IAlias, Join>>;

            joins.Add(new Tuple<IAlias, Join>(new Alias("Orchard.Projection.RoleSecurity.Models"),
                                              new Join("RoleSecurityRecord", "rsrd", ",")));

            var query = ((IHqlQuery) context.Query)
                .Where(alias => alias.Named("rsrd").Property("ContentItemRecord", "cird"),
                       expression => expression.EqProperty("Id", "ci.Id"));

            /* Short cut if anon*/
            if (user == null) {
                query
                    .Where(alias => alias.Named("rsrd"), expression => expression.Eq("AnonymousCanView", true));

                return;
            }
            
            joins.Add(new Tuple<IAlias, Join>(new Alias("Orchard.Projection.RoleSecurity.Models"),
                                              new Join("RolesSecuritysRecord", "rssrd", ",")));
            
            /* TODO (ngm) : Move to one call to the DB rather than multiple. 
             * I dont think Roles for users are cached so not a good move to do this.
             * SysCache may cache the query however. */
            var roleIds = _userRolesRepository
                .Fetch(x => x.UserId == user.Id)
                .Select(x => x.Role.Id)
                .ToList();
            /* TODO (ngm) : Move this too. Caching Roles could be dangerous. */
            roleIds.Add(_roleService.GetRoleByName("Authenticated").Id);

            query
                .Where(alias => alias.Named("rssrd").Property("RoleSecurity", "rssrdRoleSecurity"),
                       expression => expression.EqProperty("Id", "rsrd.Id"))
                .Where(alias => alias.Named("rssrd").Property("Role", "rssrdRole"), 
                       expression => expression.In("Id", roleIds));
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Filter by content by role, based on if the user has the View Content Permission.");
        }
    }

}